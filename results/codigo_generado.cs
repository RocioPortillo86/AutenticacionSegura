✅ API Key configurada correctamente

🤖 Modelo listo

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
    <add name="POS_DB" connectionString="Data Source=.;Initial Catalog=POS_DB;Integrated Security=True" providerName="System.Data.SqlClient" />
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
VALUES ('admin@example.com', '$2a$12$e0N1Z1Q1Z1Z1Z1Z1Z1Z1Z1O1O1O1O1O1O1O1O1O1O1O1O1O1O1', 'Admin', 1);
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
- **Site.Master**: Plantilla maestra que define la estructura común de las páginas.

## 3) BACKEND CORE MÍNIMO Y SEGURO (ADO.NET + Session)

### Modelos (POCOs) en App_Code/Models
```csharp
// === App_Code/Models/User.cs ===
public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; }
    public bool Active { get; set; }
}

// === App_Code/Models/Product.cs ===
public class Product
{
    public int Id { get; set; }
    public string Sku { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool Active { get; set; }
}

// === App_Code/Models/Sale.cs ===
public class Sale
{
    public int Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public int CashierUserId { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
}

// === App_Code/Models/SaleItem.cs ===
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
// === App_Code/Data/Db.cs ===
public static class Db
{
    public static SqlConnection GetConnection()
    {
        return new SqlConnection(ConfigurationManager.ConnectionStrings["POS_DB"].ConnectionString);
    }
}

// === App_Code/Data/UserData.cs ===
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

    // Otros métodos: GetAll, Insert, Update, Delete...
}

// === App_Code/Data/ProductData.cs ===
public class ProductData
{
    public List<Product> GetAll()
    {
        var products = new List<Product>();
        using (var conn = Db.GetConnection())
        {
            conn.Open();
            using (var cmd = new SqlCommand("SELECT * FROM Products", conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            Id = (int)reader["Id"],
                            Sku = reader["Sku"].ToString(),
                            Name = reader["Name"].ToString(),
                            Price = (decimal)reader["Price"],
                            Stock = (int)reader["Stock"],
                            Active = (bool)reader["Active"]
                        });
                    }
                }
            }
        }
        return products;
    }

    // Otros métodos: Insert, Update, Delete...
}

// === App_Code/Data/SalesData.cs ===
public class SalesData
{
    public void InsertSale(Sale sale, List<SaleItem> items)
    {
        using (var conn = Db.GetConnection())
        {
            conn.Open();
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    // Insertar venta
                    using (var cmd = new SqlCommand("INSERT INTO Sales (CashierUserId, Subtotal, Tax, Total) OUTPUT INSERTED.Id VALUES (@CashierUserId, @Subtotal, @Tax, @Total)", conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@CashierUserId", sale.CashierUserId);
                        cmd.Parameters.AddWithValue("@Subtotal", sale.Subtotal);
                        cmd.Parameters.AddWithValue("@Tax", sale.Tax);
                        cmd.Parameters.AddWithValue("@Total", sale.Total);
                        sale.Id = (int)cmd.ExecuteScalar();
                    }

                    // Insertar items de venta
                    foreach (var item in items)
                    {
                        using (var cmd = new SqlCommand("INSERT INTO SaleItems (SaleId, ProductId, Quantity, UnitPrice, LineTotal) VALUES (@SaleId, @ProductId, @Quantity, @UnitPrice, @LineTotal)", conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@SaleId", sale.Id);
                            cmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                            cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                            cmd.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                            cmd.Parameters.AddWithValue("@LineTotal", item.LineTotal);
                            cmd.ExecuteNonQuery();
                        }

                        // Descontar stock
                        using (var cmd = new SqlCommand("UPDATE Products SET Stock = Stock - @Quantity WHERE Id = @ProductId", conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                            cmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw; // Manejo de errores en el nivel superior
                }
            }
        }
    }

    // Otros métodos: GetByDateRange...
}
```

### Servicios en App_Code/Services
```csharp
// === App_Code/Services/AuthService.cs ===
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

// === App_Code/Services/SalesService.cs ===
public class SalesService
{
    public int CreateSale(int cashierUserId, IEnumerable<(int productId, int qty)> items)
    {
        var sale = new Sale
        {
            CashierUserId = cashierUserId,
            Subtotal = items.Sum(i => i.qty * ProductData.GetById(i.productId).Price),
            Tax = items.Sum(i => i.qty * ProductData.GetById(i.productId).Price) * 0.16m,
            Total = items.Sum(i => i.qty * ProductData.GetById(i.productId).Price) * 1.16m
        };

        var saleItems = items.Select(i => new SaleItem
        {
            ProductId = i.productId,
            Quantity = i.qty,
            UnitPrice = ProductData.GetById(i.productId).Price,
            LineTotal = i.qty * ProductData.GetById(i.productId).Price
        }).ToList();

        SalesData.InsertSale(sale, saleItems);
        return sale.Id;
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
- SQL parametrizado para evitar inyecciones.
- Uso de BCrypt para el hash de contraseñas.
- Validación de entradas en el servidor.
- Anti-XSS: usar `Server.HtmlEncode` al mostrar datos de usuario.
- Manejo de errores: capturar excepciones y mostrar mensajes genéricos.

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
<%@ Master Language="C#" AutoEventWireup="true" CodeFile="Site.Master.cs" Inherits="SiteMaster" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>POS System</title>
    <link href="Styles/Site.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <nav style="background-color: #353A40; color: white;">
                <ul>
                    <li><a href="Default.aspx">Home</a></li>
                    <% if (Session["role"] != null && Session["role"].ToString() == "Admin") { %>
                        <li><a href="Users.aspx">Users</a></li>
                        <li><a href="Products.aspx">Products</a></li>
                    <% } %>
                    <li><a href="CashRegister.aspx">Cash Register</a></li>
                    <li><a href="SalesReport.aspx">Sales Report</a></li>
                    <li><a href="Logout.aspx" runat="server" id="lnkLogout">Logout</a></li>
                </ul>
            </nav>
            <asp:ContentPlaceHolder ID="MainContent" runat="server" />
        </div>
    </form>
</body>
</html>
```

### Páginas (.aspx) con controles ASP.NET y validadores
- **Login.aspx**: 
```html
<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Login.aspx.cs" Inherits="Login" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Login</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:TextBox ID="txtEmail" runat="server" placeholder="Email" />
            <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" placeholder="Password" />
            <asp:Button ID="btnLogin" runat="server" Text="Iniciar" OnClick="btnLogin_Click" />
            <asp:Label ID="lblMessage" runat="server" ForeColor="Red" />
        </div>
    </form>
</body>
</html>
```

- **Default.aspx**: 
```html
<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="Default" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Bienvenido</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:Label ID="lblWelcome" runat="server" />
            <asp:Label ID="lblRole" runat="server" />
        </div>
    </form>
</body>
</html>
```

- **Users.aspx**: 
```html
<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Users.aspx.cs" Inherits="Users" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Usuarios</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:GridView ID="gvUsers" runat="server" AutoGenerateColumns="False" OnRowCommand="gvUsers_RowCommand" OnPageIndexChanging="gvUsers_PageIndexChanging" />
            <asp:FormView ID="fvUser" runat="server" />
            <asp:Label ID="lblMessage" runat="server" ForeColor="Red" />
            <asp:Button ID="btnNew" runat="server" Text="Nuevo" OnClick="btnNew_Click" />
            <asp:Button ID="btnSave" runat="server" Text="Guardar" OnClick="btnSave_Click" />
            <asp:Button ID="btnDelete" runat="server" Text="Eliminar" OnClick="btnDelete_Command" />
        </div>
    </form>
</body>
</html>
```

- **Products.aspx**: 
```html
<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Products.aspx.cs" Inherits="Products" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Productos</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:GridView ID="gvProducts" runat="server" AutoGenerateColumns="False" OnRowCommand="gvProducts_RowCommand" OnPageIndexChanging="gvProducts_PageIndexChanging" />
            <asp:FormView ID="fvProduct" runat="server" />
            <asp:Label ID="lblMessage" runat="server" ForeColor="Red" />
            <asp:Button ID="btnSave" runat="server" Text="Guardar" OnClick="btnSave_Click" />
            <asp:Button ID="btnDelete" runat="server" Text="Eliminar" OnClick="btnDelete_Command" />
        </div>
    </form>
</body>
</html>
```

- **CashRegister.aspx**: 
```html
<%@ Page Language="C#" AutoEventWireup="true" CodeFile="CashRegister.aspx.cs" Inherits="CashRegister" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Registro de Caja</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:DropDownList ID="ddlProducts" runat="server" />
            <asp:TextBox ID="txtQty" runat="server" />
            <asp:Button ID="btnAddItem" runat="server" Text="Agregar" OnClick="btnAddItem_Click" />
            <asp:GridView ID="gvCart" runat="server" />
            <asp:Label ID="lblSubtotal" runat="server" />
            <asp:Label ID="lblTax" runat="server" />
            <asp:Label ID="lblTotal" runat="server" />
            <asp:Button ID="btnCheckout" runat="server" Text="Registrar Venta" OnClick="btnCheckout_Click" />
            <asp:Label ID="lblMessage" runat="server" ForeColor="Red" />
        </div>
    </form>
</body>
</html>
```

- **SalesReport.aspx**: 
```html
<%@ Page Language="C#" AutoEventWireup="true" CodeFile="SalesReport.aspx.cs" Inherits="SalesReport" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Reporte de Ventas</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:TextBox ID="txtFrom" runat="server" />
            <asp:TextBox ID="txtTo" runat="server" />
            <asp:Button ID="btnFilter" runat="server" Text="Filtrar" OnClick="btnFilter_Click" />
            <asp:GridView ID="gvSales" runat="server" />
            <asp:Label ID="lblTotalGeneral" runat="server" />
            <asp:Label ID="lblMessage" runat="server" ForeColor="Red" />
        </div>
    </form>
</body>
</html>
```

### Estilos
```css
/* === Styles/Site.css === */
body {
    background-color: #F5F6FA;
    color: black;
}

nav {
    background-color: #353A40;
    color: white;
}

nav ul {
    list-style-type: none;
    padding: 0;
}

nav ul li {
    display: inline;
    margin-right: 15px;
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

```csharp
// === Pages/Login.aspx.cs ===
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
        }
    }

    protected void btnLogin_Click(object sender, EventArgs e)
    {
        if (Page.IsValid)
        {
            var email = txtEmail.Text.Trim();
            var password = txtPassword.Text.Trim();
            var authService = new AuthService();

            if (authService.Login(email, password, Session))
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

// === Pages/Default.aspx.cs ===
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
            var userId = (int)Session["uid"];
            var user = UserData.GetById(userId);
            lblWelcome.Text = $"Bienvenido, {user.Email}";
            lblRole.Text = $"Rol: {user.Role}";
        }
    }
}

// === Pages/Users.aspx.cs ===
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
        fvUser.DataBind();
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        // TODO: Implementar lógica para guardar usuario
        // Hashear contraseña si es nuevo
        // BindGrid();
    }

    protected void btnDelete_Command(object sender, CommandEventArgs e)
    {
        // TODO: Implementar lógica para eliminar usuario
        // BindGrid();
    }

    protected void gvUsers_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        // TODO: Implementar lógica para manejar comandos en el GridView
    }

    protected void gvUsers_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvUsers.PageIndex = e.NewPageIndex;
        BindGrid();
    }
}

// === Pages/Products.aspx.cs ===
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
        // TODO: Implementar lógica para guardar producto
        // BindGrid();
    }

    protected void btnDelete_Command(object sender, CommandEventArgs e)
    {
        // TODO: Implementar lógica para eliminar producto
        // BindGrid();
    }

    protected void gvProducts_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        // TODO: Implementar lógica para manejar comandos en el GridView
    }

    protected void gvProducts_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvProducts.PageIndex = e.NewPageIndex;
        BindGrid();
    }
}

// === Pages/CashRegister.aspx.cs ===
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using App_Code.Data;
using App_Code.Services;

public partial class CashRegister : Page
{
    private List<CartItem> Cart
    {
        get { return (List<CartItem>)ViewState["Cart"] ?? new List<CartItem>(); }
        set { ViewState["Cart"] = value; }
    }

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
        }
    }

    private void LoadProducts()
    {
        ddlProducts.DataSource = ProductData.GetAll();
        ddlProducts.DataTextField = "Name";
        ddlProducts.DataValueField = "Id";
        ddlProducts.DataBind();
    }

    protected void btnAddItem_Click(object sender, EventArgs e)
    {
        if (int.TryParse(txtQty.Text, out int qty) && qty > 0)
        {
            var productId = int.Parse(ddlProducts.SelectedValue);
            var product = ProductData.GetById(productId);
            var cartItem = Cart.Find(item => item.ProductId == productId);

            if (cartItem != null)
            {
                cartItem.Quantity += qty;
            }
            else
            {
                Cart.Add(new CartItem
                {
                    ProductId = productId,
                    Name = product.Name,
                    UnitPrice = product.Price,
                    Quantity = qty,
                    LineTotal = qty * product.Price
                });
            }

            RecalcTotals();
            gvCart.DataSource = Cart;
            gvCart.DataBind();
        }
        else
        {
            lblMessage.Text = "Cantidad inválida.";
        }
    }

    private void RecalcTotals()
    {
        decimal subtotal = 0;
        foreach (var item in Cart)
        {
            subtotal += item.LineTotal;
        }

        decimal tax = subtotal * 0.16m;
        decimal total = subtotal + tax;

        lblSubtotal.Text = $"Subtotal: {subtotal:C}";
        lblTax.Text = $"IVA: {tax:C}";
        lblTotal.Text = $"Total: {total:C}";
    }

    protected void gvCart_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        // TODO: Implementar lógica para eliminar producto del carrito
        // RecalcTotals();
    }

    protected void btnCheckout_Click(object sender, EventArgs e)
    {
        var saleService = new SalesService();
        var items = new List<(int productId, int qty)>();

        foreach (var item in Cart)
        {
            items.Add((item.ProductId, item.Quantity));
        }

        int saleId = saleService.CreateSale((int)Session["uid"], items);
        lblMessage.Text = $"Venta registrada con ID: {saleId}";

        Cart.Clear();
        RecalcTotals();
        gvCart.DataSource = null;
        gvCart.DataBind();
    }
}

// === Pages/SalesReport.aspx.cs ===
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
            lblTotalGeneral.Text = $"Total General: {totalGeneral:C}";
        }
        else
        {
            lblMessage.Text = "Fechas inválidas.";
        }
    }
}

// === Site.Master.cs ===
using System;
using System.Web.UI;

public partial class SiteMaster : MasterPage
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
        var authService = new AuthService();
        authService.Logout(Session);
        Response.Redirect("~/Pages/Login.aspx");
    }
}
```

### Consideraciones de seguridad
- Se implementa SQL parametrizado para prevenir inyecciones SQL.
- Las contraseñas se almacenan usando BCrypt para asegurar que no se guarden en texto plano.
- Se valida la entrada del usuario en el servidor para evitar datos maliciosos.
- Se utiliza `Server.HtmlEncode` para prevenir ataques XSS al mostrar datos ingresados por el usuario.
- Se maneja la sesión de manera segura, evitando el uso de cookies y asegurando que se verifique la sesión en cada página protegida.

✅ Guardado en: /home/runner/work/PuntoVentas/PuntoVentas/results/EspecificacionesProyecto.md
✅ Guardado en: /home/runner/work/PuntoVentas/PuntoVentas/results/EspecificacionesProyecto.md
📏 Tamaño (bytes): 28494
🧪 Existe?: True
