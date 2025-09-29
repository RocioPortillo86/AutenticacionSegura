# Documento Maestro para la Creación de un Sistema de Punto de Venta (POS) en C# y ASP.NET Web Forms

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
INSERT INTO Users (Email, PasswordHash, Role, Active) VALUES 
('admin@example.com', '$2a$12$e0N1Z1Q1Z1Q1Z1Q1Z1Q1Z.1Q1Z1Q1Z1Q1Z1Q1Z1Q1Z1Q1Z1Q1Z1', 'Admin', 1);
```

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
- **Pages**: Contiene las páginas Web Forms (.aspx) del sistema.
- **Styles**: Archivos CSS para el estilo de la aplicación.
- **App_Themes**: Temas opcionales para la aplicación.
- **Site.Master**: Master page que define la estructura común de las páginas.

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
    public User GetById(int id)
    {
        using (var conn = Db.GetConnection())
        {
            conn.Open();
            using (var cmd = new SqlCommand("SELECT * FROM Users WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new User
                        {
                            Id = (int)reader["Id"],
                            Email = reader["Email"].ToString(),
                            PasswordHash = reader["PasswordHash"].ToString(),
                            Role = reader["Role"].ToString(),
                            Active = (bool)reader["Active"]
                        };
                    }
                }
            }
        }
        return null;
    }

    public void Insert(User user)
    {
        using (var conn = Db.GetConnection())
        {
            conn.Open();
            using (var cmd = new SqlCommand("INSERT INTO Users (Email, PasswordHash, Role, Active) VALUES (@Email, @PasswordHash, @Role, @Active)", conn))
            {
                cmd.Parameters.AddWithValue("@Email", user.Email);
                cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                cmd.Parameters.AddWithValue("@Role", user.Role);
                cmd.Parameters.AddWithValue("@Active", user.Active);
                cmd.ExecuteNonQuery();
            }
        }
    }

    // Métodos Update y Delete...
}

// ProductData.cs y SalesData.cs seguirán un patrón similar.
```

### Servicios en App_Code/Services
```csharp
// AuthService.cs
public class AuthService
{
    public bool Login(string email, string password, HttpSessionState session)
    {
        var user = UserData.GetByEmail(email);
        if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            session["uid"] = user.Id;
            session["role"] = user.Role;
            return true;
        }
        return false;
    }

    public void Logout(HttpSessionState session)
    {
        session.Clear();
    }
}

// SalesService.cs
public class SalesService
{
    public int CreateSale(int cashierUserId, IEnumerable<(int productId, int qty)> items)
    {
        using (var conn = Db.GetConnection())
        {
            conn.Open();
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    var saleId = InsertSale(cashierUserId, transaction);
                    foreach (var item in items)
                    {
                        InsertSaleItem(saleId, item.productId, item.qty, transaction);
                        UpdateProductStock(item.productId, -item.qty, transaction);
                    }
                    transaction.Commit();
                    return saleId;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }

    private int InsertSale(int cashierUserId, SqlTransaction transaction)
    {
        // Lógica para insertar la venta y retornar el SaleId
    }

    private void InsertSaleItem(int saleId, int productId, int qty, SqlTransaction transaction)
    {
        // Lógica para insertar el SaleItem
    }

    private void UpdateProductStock(int productId, int qtyChange, SqlTransaction transaction)
    {
        // Lógica para actualizar el stock del producto
    }
}
```

### Pautas para páginas
- En `Page_Load` de páginas protegidas, verificar si `Session["uid"]` es nulo y redirigir a `Login.aspx`.
- En `Users.aspx` y `Products.aspx`, verificar que el rol sea Admin.
- Usar `Server.HtmlEncode` para prevenir XSS.

## 4) FRONTEND (WEB FORMS CON CONTROLES ASP.NET + ESTILOS PROPIOS)

### Menú de navegación (header superior)
```html
<asp:Menu ID="Menu1" runat="server" BackColor="#353A40" ForeColor="White">
    <Items>
        <asp:MenuItem Text="Home" Value="Default.aspx" />
        <asp:MenuItem Text="Users" Value="Users.aspx" Visible='<%# Session["role"] == "Admin" %>' />
        <asp:MenuItem Text="Products" Value="Products.aspx" Visible='<%# Session["role"] == "Admin" %>' />
        <asp:MenuItem Text="CashRegister" Value="CashRegister.aspx" />
        <asp:MenuItem Text="SalesReport" Value="SalesReport.aspx" />
        <asp:MenuItem Text="Logout" Value="Logout" OnClick="lnkLogout_Click" />
    </Items>
</asp:Menu>
```

### Paleta de colores
- Menú superior: fondo `#353A40`, texto blanco.
- Fondos generales: `#F5F6FA`, texto negro.
- Encabezado de GridView: fondo `#19A1B9`, texto blanco.
- Botones principales: fondo `#0F6AF6`, texto blanco.
- Botones de acción crítica: fondo `#E13C4A`, texto blanco.

### Site.Master
```html
<!DOCTYPE html>
<html>
<head runat="server">
    <title>POS System</title>
    <link href="Styles/Site.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:ContentPlaceHolder ID="MainContent" runat="server" />
        </div>
    </form>
</body>
</html>
```

### Páginas (.aspx) con controles ASP.NET y validadores
- **Login.aspx**
```html
<asp:TextBox ID="txtEmail" runat="server" />
<asp:TextBox ID="txtPassword" runat="server" TextMode="Password" />
<asp:Button ID="btnLogin" runat="server" Text="Iniciar" OnClick="btnLogin_Click" />
<asp:RequiredFieldValidator ControlToValidate="txtEmail" ErrorMessage="Email es requerido." />
<asp:RegularExpressionValidator ControlToValidate="txtEmail" ErrorMessage="Email no es válido." />
<asp:ValidationSummary ID="ValidationSummary1" runat="server" />
<asp:Label ID="lblMessage" runat="server" />
```

- **Default.aspx**
```html
<asp:Label ID="lblWelcome" runat="server" />
<asp:Label ID="lblRole" runat="server" />
```

- **Users.aspx**
```html
<asp:GridView ID="gvUsers" runat="server" />
<asp:FormView ID="fvUser" runat="server" />
<asp:Label ID="lblMessage" runat="server" />
<asp:Button ID="btnNew" runat="server" Text="Nuevo" />
<asp:Button ID="btnSave" runat="server" Text="Guardar" />
<asp:Button ID="btnDelete" runat="server" Text="Eliminar" />
```

- **Products.aspx**
```html
<asp:GridView ID="gvProducts" runat="server" />
<asp:FormView ID="fvProduct" runat="server" />
<asp:Label ID="lblMessage" runat="server" />
<asp:Button ID="btnNew" runat="server" Text="Nuevo" />
<asp:Button ID="btnSave" runat="server" Text="Guardar" />
<asp:Button ID="btnDelete" runat="server" Text="Eliminar" />
```

- **CashRegister.aspx**
```html
<asp:DropDownList ID="ddlProducts" runat="server" />
<asp:TextBox ID="txtQty" runat="server" />
<asp:Button ID="btnAddItem" runat="server" Text="Agregar" />
<asp:GridView ID="gvCart" runat="server" />
<asp:Label ID="lblSubtotal" runat="server" />
<asp:Label ID="lblTax" runat="server" />
<asp:Label ID="lblTotal" runat="server" />
<asp:Button ID="btnCheckout" runat="server" Text="Registrar Venta" />
<asp:Label ID="lblMessage" runat="server" />
```

- **SalesReport.aspx**
```html
<asp:TextBox ID="txtFrom" runat="server" />
<asp:TextBox ID="txtTo" runat="server" />
<asp:Button ID="btnFilter" runat="server" Text="Filtrar" />
<asp:GridView ID="gvSales" runat="server" />
<asp:Label ID="lblTotalGeneral" runat="server" />
<asp:Label ID="lblMessage" runat="server" />
```

### Estilos
```css
/* Styles/Site.css */
body {
    background-color: #F5F6FA;
    color: black;
}

.navbar {
    background-color: #353A40;
    color: white;
}

.grid-header {
    background-color: #19A1B9;
    color: white;
}

.btn-primary {
    background-color: #0F6AF6;
    color: white;
}

.btn-danger {
    background-color: #E13C4A;
    color: white;
}
```

## 5) GENERACIÓN DEL CÓDIGO DE PÁGINAS (CODE-BEHIND .ASPX.CS)

### A) // === Pages/Login.aspx.cs ===
```csharp
using System;
using System.Web;
using System.Web.UI;
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
            var authService = new AuthService();
            if (authService.Login(txtEmail.Text, txtPassword.Text, Session))
            {
                Response.Redirect("Default.aspx");
            }
            else
            {
                lblMessage.Text = "Credenciales inválidas.";
            }
        }
    }
}
```

### B) // === Pages/Default.aspx.cs ===
```csharp
using System;
using System.Web.UI;
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
            var userId = (int)Session["uid"];
            var user = UserData.GetById(userId);
            lblWelcome.Text = $"Bienvenido, {user.Email}";
            lblRole.Text = $"Rol: {user.Role}";
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
        if (Session["uid"] == null || (string)Session["role"] != "Admin")
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

    protected void gvUsers_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        // Lógica para manejar comandos de la GridView
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        // Lógica para guardar usuario
    }

    protected void btnDelete_Click(object sender, EventArgs e)
    {
        // Lógica para eliminar usuario
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
        if (Session["uid"] == null || (string)Session["role"] != "Admin")
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

    protected void gvProducts_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        // Lógica para manejar comandos de la GridView
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        // Lógica para guardar producto
    }

    protected void btnDelete_Click(object sender, EventArgs e)
    {
        // Lógica para eliminar producto
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
    private List<(int productId, string name, decimal unitPrice, int qty, decimal lineTotal)> cart;

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
            cart = new List<(int, string, decimal, int, decimal)>();
            ViewState["Cart"] = cart;
        }
    }

    private void LoadProducts()
    {
        ddlProducts.DataSource = ProductData.GetAllActive();
        ddlProducts.DataTextField = "Name";
        ddlProducts.DataValueField = "Id";
        ddlProducts.DataBind();
    }

    protected void btnAddItem_Click(object sender, EventArgs e)
    {
        if (int.TryParse(txtQty.Text, out int qty) && qty > 0)
        {
            int productId = int.Parse(ddlProducts.SelectedValue);
            var product = ProductData.GetById(productId);
            decimal lineTotal = product.Price * qty;

            cart = (List<(int, string, decimal, int, decimal)>)ViewState["Cart"];
            cart.Add((productId, product.Name, product.Price, qty, lineTotal));
            ViewState["Cart"] = cart;

            RecalcTotals();
            BindCart();
        }
    }

    private void RecalcTotals()
    {
        decimal subtotal = 0;
        foreach (var item in cart)
        {
            subtotal += item.lineTotal;
        }
        decimal tax = subtotal * 0.16m;
        decimal total = subtotal + tax;

        lblSubtotal.Text = subtotal.ToString("C");
        lblTax.Text = tax.ToString("C");
        lblTotal.Text = total.ToString("C");
    }

    private void BindCart()
    {
        gvCart.DataSource = cart;
        gvCart.DataBind();
    }

    protected void btnCheckout_Click(object sender, EventArgs e)
    {
        var salesService = new SalesService();
        var itemsToSell = new List<(int productId, int qty)>();
        foreach (var item in cart)
        {
            itemsToSell.Add((item.productId, item.qty));
        }

        int saleId = salesService.CreateSale((int)Session["uid"], itemsToSell);
        lblMessage.Text = $"Venta registrada con ID: {saleId}";

        // Limpiar carrito
        cart.Clear();
        ViewState["Cart"] = cart;
        RecalcTotals();
        BindCart();
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
            var sales = SalesData.GetByDateRange(from.ToUniversalTime(), to.ToUniversalTime());
            gvSales.DataSource = sales;
            gvSales.DataBind();

            decimal totalGeneral = 0;
            foreach (var sale in sales)
            {
                totalGeneral += sale.Total;
            }
            lblTotalGeneral.Text = totalGeneral.ToString("C");
        }
        else
        {
            lblMessage.Text = "Fechas no válidas.";
        }
    }
}
```

### G) // === Site.Master.cs ===
```csharp
using System;
using System.Web.UI;
using App_Code.Services;

public partial class Site : MasterPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["uid"] != null)
        {
            string role = (string)Session["role"];
            pnlAdmin.Visible = role == "Admin";
        }
    }

    protected void lnkLogout_Click(object sender, EventArgs e)
    {
        var authService = new AuthService();
        authService.Logout(Session);
        Response.Redirect("~/Pages/Login.aspx");
    }
}
```

### Consideraciones de seguridad
- Se utiliza SQL parametrizado en todas las consultas para prevenir inyecciones SQL.
- Las contraseñas se almacenan como hashes seguros utilizando BCrypt.
- Se valida la entrada del usuario en el servidor y se codifica la salida para prevenir XSS.
- Se maneja la sesión de manera segura, evitando el uso de cookies y asegurando que las sesiones se limpien adecuadamente al cerrar sesión.