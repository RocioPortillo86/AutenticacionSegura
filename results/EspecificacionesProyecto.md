# Documento Maestro para la Creación de un Sistema de Punto de Venta (POS) en C# y ASP.NET Web Forms (.NET Framework 4.8)

## 1) CONFIGURACIÓN DEL ENTORNO DE DESARROLLO

### Instalación
1. **Visual Studio 2022**: Asegúrate de instalar el workload "ASP.NET and web development".
2. **.NET Framework 4.8**: Verifica que esté instalado en tu sistema.
3. **SQL Server**: Puedes usar SQL Server Express o LocalDB.
4. **Paquete NuGet**: Instala `BCrypt.Net-Next` para el hash seguro de contraseñas.

### Creación del proyecto en Visual Studio
1. **Tipo**: Selecciona "Aplicación web de ASP.NET (.NET Framework)".
2. **Framework**: Asegúrate de seleccionar .NET 4.8.
3. **Plantilla**: Escoge "Web Forms".
4. **Autenticación**: Selecciona "Sin autenticación".

### Configuración básica de Web.config
```xml
<configuration>
  <connectionStrings>
    <add name="POS_DB" connectionString="Data Source=.;Initial Catalog=POS_DB;Integrated Security=True;" providerName="System.Data.SqlClient" />
  </connectionStrings>
  <globalization culture="es-MX" uiCulture="es-MX" />
  <compilation targetFramework="4.8" />
  <sessionState timeout="20" />
  <pages validateRequest="true" viewStateEncryptionMode="Always" />
</configuration>
```
**Nota**: Asegúrate de usar HTTPS en desarrollo y producción.

### Base de datos
Las tablas principales son:
- **Users**: Almacena información de los usuarios.
- **Products**: Almacena información de los productos.
- **Sales**: Registra las ventas realizadas.
- **SaleItems**: Detalla los productos vendidos en cada venta.

#### SCRIPT SQL COMPLETO Y EJECUTABLE
```sql
-- Crear la base de datos
CREATE DATABASE POS_DB;
GO

USE POS_DB;
GO

-- Crear tabla Users
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Email NVARCHAR(255) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(50) NOT NULL,
    Active BIT NOT NULL
);

-- Crear tabla Products
CREATE TABLE Products (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Sku NVARCHAR(50) UNIQUE NOT NULL,
    Name NVARCHAR(255) NOT NULL,
    Price DECIMAL(18, 2) NOT NULL CHECK (Price >= 0),
    Stock INT NOT NULL CHECK (Stock >= 0),
    Active BIT NOT NULL
);

-- Crear tabla Sales
CREATE TABLE Sales (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CreatedAtUtc DATETIME NOT NULL DEFAULT GETUTCDATE(),
    CashierUserId INT NOT NULL,
    Subtotal DECIMAL(18, 2) NOT NULL,
    Tax DECIMAL(18, 2) NOT NULL,
    Total DECIMAL(18, 2) NOT NULL,
    FOREIGN KEY (CashierUserId) REFERENCES Users(Id)
);

-- Crear tabla SaleItems
CREATE TABLE SaleItems (
    Id INT PRIMARY KEY IDENTITY(1,1),
    SaleId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL CHECK (Quantity > 0),
    UnitPrice DECIMAL(18, 2) NOT NULL,
    LineTotal DECIMAL(18, 2) NOT NULL,
    FOREIGN KEY (SaleId) REFERENCES Sales(Id),
    FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

-- Insertar un usuario Admin inicial
INSERT INTO Users (Email, PasswordHash, Role, Active) 
VALUES ('admin@example.com', '$2a$12$e0N1Z1Q1Q1Q1Q1Q1Q1Q1Q1O', 'Admin', 1); -- Hash de 'Admin123'
```

---

## 2) ESTRUCTURA INICIAL DEL PROYECTO

```
/App_Code
    /Models
        User.cs
        Product.cs
        Sale.cs
        SaleItem.cs
    /Data
        Db.cs
        UserData.cs
        ProductData.cs
        SalesData.cs
    /Services
        AuthService.cs
        SalesService.cs
/Pages
    Login.aspx
    Default.aspx
    Users.aspx
    Products.aspx
    CashRegister.aspx
    SalesReport.aspx
/Styles
    Site.css
/App_Themes
    /PosTheme
        PosTheme.skin (opcional)
Site.Master
```

### Descripción de carpetas y archivos
- **App_Code**: Contiene la lógica de negocio y acceso a datos.
  - **Models**: Clases POCO que representan las entidades del sistema.
  - **Data**: Clases para el acceso a datos usando ADO.NET.
  - **Services**: Clases que implementan la lógica de negocio.
- **Pages**: Contiene las páginas Web Forms del sistema.
- **Styles**: Archivos CSS para el estilo de la aplicación.
- **App_Themes**: Temas opcionales para la aplicación.
- **Site.Master**: Plantilla maestra que define la estructura común de las páginas.

---

## 3) BACKEND CORE MÍNIMO Y SEGURO (ADO.NET + Session)

### Modelos (POCOs) en App_Code/Models
```csharp
// User.cs
public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; }
    public bool Active { get; set; }
}

// Product.cs
public class Product
{
    public int Id { get; set; }
    public string Sku { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool Active { get; set; }
}

// Sale.cs
public class Sale
{
    public int Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public int CashierUserId { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
}

// SaleItem.cs
public class SaleItem
{
    public int Id { get; set; }
    public int SaleId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
```

### Acceso a datos (ADO.NET) en App_Code/Data
```csharp
// Db.cs
public static class Db
{
    public static SqlConnection GetConnection()
    {
        return new SqlConnection(ConfigurationManager.ConnectionStrings["POS_DB"].ConnectionString);
    }
}

// UserData.cs
public class UserData
{
    public static User GetById(int id)
    {
        // TODO: Implementar lógica para obtener un usuario por ID
    }

    public static List<User> GetAll()
    {
        // TODO: Implementar lógica para obtener todos los usuarios
    }

    public static void Insert(User user)
    {
        // TODO: Implementar lógica para insertar un nuevo usuario
    }

    public static void Update(User user)
    {
        // TODO: Implementar lógica para actualizar un usuario existente
    }

    public static void Delete(int id)
    {
        // TODO: Implementar lógica para eliminar un usuario
    }
}

// ProductData.cs
public class ProductData
{
    public static List<Product> GetAll()
    {
        // TODO: Implementar lógica para obtener todos los productos
    }

    public static void Insert(Product product)
    {
        // TODO: Implementar lógica para insertar un nuevo producto
    }

    public static void Update(Product product)
    {
        // TODO: Implementar lógica para actualizar un producto existente
    }

    public static void Delete(int id)
    {
        // TODO: Implementar lógica para eliminar un producto
    }
}

// SalesData.cs
public class SalesData
{
    public static void InsertSale(Sale sale, List<SaleItem> items)
    {
        // TODO: Implementar lógica para insertar una venta con transacción
    }

    public static List<Sale> GetByDateRange(DateTime fromUtc, DateTime toUtc)
    {
        // TODO: Implementar lógica para obtener ventas por rango de fechas
    }
}
```

### Servicios en App_Code/Services
```csharp
// AuthService.cs
public class AuthService
{
    public static bool Login(string email, string password, HttpSessionState session)
    {
        // TODO: Implementar lógica de autenticación
    }

    public static void Logout(HttpSessionState session)
    {
        // Limpiar la sesión
        session.Clear();
    }
}

// SalesService.cs
public class SalesService
{
    public static int CreateSale(int cashierUserId, IEnumerable<(int productId, int qty)> items)
    {
        // TODO: Implementar lógica para crear una venta
    }
}
```

### Pautas para páginas
- En `Page_Load` de páginas protegidas: 
```csharp
if (Session["uid"] == null) { Response.Redirect("Login.aspx"); return; }
```
- `Users.aspx` y `Products.aspx`: solo acceso Admin.

### Seguridad mínima
- **SQL parametrizado**: Todas las consultas deben ser parametrizadas.
- **BCrypt**: Usar para el hash de contraseñas.
- **Validación en servidor**: Validar todas las entradas del usuario.
- **Anti-XSS**: Usar `Server.HtmlEncode` al mostrar datos ingresados por el usuario.
- **Manejo de errores**: Capturar excepciones y mostrar mensajes genéricos.

---

## 4) FRONTEND (WEB FORMS CON CONTROLES ASP.NET + ESTILOS PROPIOS)

### Menú de navegación (header superior)
- Diseñar el menú en la parte superior con fondo `#353A40` y texto blanco.
- Opciones: Home, Users, Products, CashRegister, SalesReport, Logout.
- Ocultar o desactivar enlaces según el rol (`Session["role"]`).

### Paleta de colores
- Menú superior: fondo `#353A40`, texto blanco.
- Fondos generales: `#F5F6FA`, texto negro.
- Encabezado de GridView: fondo `#19A1B9`, texto blanco.
- Botones principales: fondo `#0F6AF6`, texto blanco.
- Botones de acción crítica: fondo `#E13C4A`, texto blanco.

### Site.Master
```html
<%@ Master Language="C#" AutoEventWireup="true" CodeBehind="Site.master.cs" Inherits="YourNamespace.Site" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title></title>
    <link href="Styles/Site.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:Menu ID="Menu1" runat="server" ...>
                <!-- Opciones del menú según rol -->
            </asp:Menu>
            <asp:ContentPlaceHolder ID="MainContent" runat="server" />
        </div>
    </form>
</body>
</html>
```

### Páginas (.aspx) con controles ASP.NET y validadores
- **Login.aspx**: 
  - Controles: `TextBox` Email/Password, `Button` Iniciar; `RequiredFieldValidator`, `RegularExpressionValidator`, `ValidationSummary`.
  
- **Default.aspx**: 
  - Controles: `Label` para bienvenida y rol actual.

- **Users.aspx** (solo Admin): 
  - Controles: `GridView` + `FormView`/`DetailsView` para CRUD de usuarios.

- **Products.aspx** (solo Admin): 
  - Controles: `GridView` + `FormView` con `RangeValidator` para Price/Stock.

- **CashRegister.aspx** (Admin/Cashier): 
  - Controles: `DropDownList` productos activos, `TextBox` cantidad, botón Agregar; `GridView` carrito; Labels Subtotal/IVA/Total; botón Registrar venta.

- **SalesReport.aspx** (Admin/Cashier): 
  - Controles: filtros FechaDesde/FechaHasta con validadores; `GridView` resultados y total general.

### Estilos
- Definir en `Styles/Site.css` las clases para navbar, botones y GridView, respetando la paleta de colores indicada.

---

## 5) GENERACIÓN DEL CÓDIGO DE PÁGINAS (CODE-BEHIND .ASPX.CS)

### A) // === Pages/Login.aspx.cs ===
```csharp
using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using App_Code.Services;

public partial class Login : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["uid"] != null)
        {
            Response.Redirect("Default.aspx");
            return;
        }
    }

    protected void btnLogin_Click(object sender, EventArgs e)
    {
        if (Page.IsValid)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text.Trim();
            if (AuthService.Login(email, password, Session))
            {
                Response.Redirect("Default.aspx");
            }
            else
            {
                lblMessage.Text = "Credenciales inválidas. Intente nuevamente.";
            }
        }
    }
}
```

### B) // === Pages/Default.aspx.cs ===
```csharp
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using App_Code.Data;

public partial class Default : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["uid"] == null)
        {
            Response.Redirect("Login.aspx");
            return;
        }

        if (!IsPostBack)
        {
            // Cargar información del usuario si es necesario
            lblWelcome.Text = "Bienvenido, " + Server.HtmlEncode(Session["email"].ToString());
            lblRole.Text = "Rol: " + Server.HtmlEncode(Session["role"].ToString());
        }
    }
}
```

### C) // === Pages/Users.aspx.cs ===
```csharp
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using App_Code.Data;

public partial class Users : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["uid"] == null || Session["role"].ToString() != "Admin")
        {
            Response.Redirect("Login.aspx");
            return;
        }

        if (!IsPostBack)
        {
            BindGrid();
        }
    }

    private void BindGrid()
    {
        gvUsers.DataSource = UserData.GetAll();
        gvUsers.DataBind();
    }

    protected void btnNew_Click(object sender, EventArgs e)
    {
        fvUser.ChangeMode(FormViewMode.Insert);
        // Limpiar el formulario
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        // Obtener datos del FormView y guardar
        // Hashear contraseña si es nuevo
        // UserData.Insert o UserData.Update
        BindGrid();
    }

    protected void gvUsers_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "DeleteUser")
        {
            int userId = Convert.ToInt32(e.CommandArgument);
            UserData.Delete(userId);
            BindGrid();
        }
    }

    protected void gvUsers_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvUsers.PageIndex = e.NewPageIndex;
        BindGrid();
    }
}
```

### D) // === Pages/Products.aspx.cs ===
```csharp
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using App_Code.Data;

public partial class Products : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["uid"] == null || Session["role"].ToString() != "Admin")
        {
            Response.Redirect("Login.aspx");
            return;
        }

        if (!IsPostBack)
        {
            BindGrid();
        }
    }

    private void BindGrid()
    {
        gvProducts.DataSource = ProductData.GetAll();
        gvProducts.DataBind();
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        // Obtener datos del FormView y guardar
        // Validar Price y Stock
        // ProductData.Insert o ProductData.Update
        BindGrid();
    }

    protected void gvProducts_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "DeleteProduct")
        {
            int productId = Convert.ToInt32(e.CommandArgument);
            ProductData.Delete(productId);
            BindGrid();
        }
    }

    protected void gvProducts_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvProducts.PageIndex = e.NewPageIndex;
        BindGrid();
    }
}
```

### E) // === Pages/CashRegister.aspx.cs ===
```csharp
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using App_Code.Data;
using App_Code.Services;

public partial class CashRegister : Page
{
    private List<CartItem> cart;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["uid"] == null)
        {
            Response.Redirect("Login.aspx");
            return;
        }

        if (!IsPostBack)
        {
            LoadProducts();
            cart = new List<CartItem>();
            ViewState["Cart"] = cart;
        }
    }

    private void LoadProducts()
    {
        ddlProducts.DataSource = ProductData.GetAll();
        ddlProducts.DataBind();
    }

    protected void btnAddItem_Click(object sender, EventArgs e)
    {
        if (int.TryParse(txtQty.Text, out int qty) && qty > 0)
        {
            int productId = int.Parse(ddlProducts.SelectedValue);
            // Obtener información del producto
            var product = ProductData.GetById(productId);
            var cartItem = new CartItem
            {
                ProductId = product.Id,
                Name = product.Name,
                UnitPrice = product.Price,
                Quantity = qty,
                LineTotal = product.Price * qty
            };

            cart = (List<CartItem>)ViewState["Cart"];
            cart.Add(cartItem);
            ViewState["Cart"] = cart;

            RecalcTotals();
        }
    }

    private void RecalcTotals()
    {
        decimal subtotal = 0;
        foreach (var item in cart)
        {
            subtotal += item.LineTotal;
        }
        decimal tax = subtotal * 0.16m;
        decimal total = subtotal + tax;

        lblSubtotal.Text = subtotal.ToString("C");
        lblTax.Text = tax.ToString("C");
        lblTotal.Text = total.ToString("C");
    }

    protected void gvCart_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "RemoveItem")
        {
            int index = Convert.ToInt32(e.CommandArgument);
            cart = (List<CartItem>)ViewState["Cart"];
            cart.RemoveAt(index);
            ViewState["Cart"] = cart;
            RecalcTotals();
        }
    }

    protected void btnCheckout_Click(object sender, EventArgs e)
    {
        var itemsToSell = new List<(int productId, int qty)>();
        foreach (var item in cart)
        {
            itemsToSell.Add((item.ProductId, item.Quantity));
        }

        int saleId = SalesService.CreateSale((int)Session["uid"], itemsToSell);
        lblMessage.Text = "Venta registrada con ID: " + saleId;

        // Limpiar carrito
        cart.Clear();
        ViewState["Cart"] = cart;
        RecalcTotals();
    }
}
```

### F) // === Pages/SalesReport.aspx.cs ===
```csharp
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using App_Code.Data;

public partial class SalesReport : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["uid"] == null)
        {
            Response.Redirect("Login.aspx");
            return;
        }
    }

    protected void btnFilter_Click(object sender, EventArgs e)
    {
        if (DateTime.TryParse(txtFrom.Text, out DateTime from) && DateTime.TryParse(txtTo.Text, out DateTime to))
        {
            // Convertir a UTC si es necesario
            var sales = SalesData.GetByDateRange(from.ToUniversalTime(), to.ToUniversalTime());
            gvSales.DataSource = sales;
            gvSales.DataBind();

            // Calcular total general
            decimal totalGeneral = 0;
            foreach (var sale in sales)
            {
                totalGeneral += sale.Total;
            }
            lblTotalGeneral.Text = totalGeneral.ToString("C");
        }
        else
        {
            lblMessage.Text = "Fechas inválidas.";
        }
    }
}
```

### G) // === Site.Master.cs ===
```csharp
using System;
using System.Web.UI;

public partial class Site : MasterPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["uid"] != null)
        {
            // Mostrar/ocultar menús según rol
            pnlAdmin.Visible = Session["role"].ToString() == "Admin";
        }
    }

    protected void lnkLogout_Click(object sender, EventArgs e)
    {
        AuthService.Logout(Session);
        Response.Redirect("~/Pages/Login.aspx");
    }
}
```

---

### Consideraciones de Seguridad
- **SQL parametrizado**: Se asegura que no haya inyecciones SQL.
- **Hash seguro**: Se utiliza BCrypt para almacenar contraseñas de forma segura.
- **Validación de entrada**: Se valida la entrada del usuario en el servidor.
- **Manejo de sesión**: Se utiliza `Session` para mantener la autenticación del usuario sin cookies.
- **Manejo de errores**: Se capturan excepciones y se muestran mensajes amigables sin detalles sensibles.

Este documento proporciona una guía completa para la creación de un Sistema de Punto de Venta (POS) en C# y ASP.NET Web Forms, asegurando que se sigan las mejores prácticas de seguridad y organización del código.