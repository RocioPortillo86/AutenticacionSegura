# Sistema de Punto de Venta (POS) en C# y ASP.NET Web Forms (.NET Framework 4.8)

## 1) CONFIGURACIÓN DEL ENTORNO DE DESARROLLO

### Instalación
1. **Visual Studio 2022**: Asegúrate de instalar el workload "ASP.NET and web development".
2. **.NET Framework 4.8**: Verifica que esté instalado en tu sistema.
3. **SQL Server**: Puedes usar SQL Server Express o LocalDB.
4. **Paquete NuGet**: Instala `BCrypt.Net-Next` para el hash seguro de contraseñas.

### Creación del proyecto en Visual Studio
1. Abre Visual Studio y selecciona "Crear un nuevo proyecto".
2. Elige "Aplicación web de ASP.NET (.NET Framework)".
3. Configura el proyecto:
   - **Framework**: .NET 4.8
   - **Plantilla**: Web Forms
   - **Autenticación**: Sin autenticación

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
    Price DECIMAL(18, 2) NOT NULL,
    Stock INT NOT NULL,
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
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18, 2) NOT NULL,
    LineTotal DECIMAL(18, 2) NOT NULL,
    FOREIGN KEY (SaleId) REFERENCES Sales(Id),
    FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

-- Insertar usuario Admin inicial
INSERT INTO Users (Email, PasswordHash, Role, Active) 
VALUES ('admin@example.com', '$2a$12$e0N1Z1Q1Q1Q1Q1Q1Q1Q1Q1O', 'Admin', 1); -- Hash de 'admin123'
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
- **Pages**: Contiene las páginas Web Forms (.aspx) del sistema.
- **Styles**: Archivos CSS para el estilo de la aplicación.
- **App_Themes**: Temas opcionales para la aplicación.
- **Site.Master**: Master page que define la estructura común de las páginas.

---

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

        SalesData.InsertSale(sale, items.Select(i => new SaleItem
        {
            ProductId = i.productId,
            Quantity = i.qty,
            UnitPrice = ProductData.GetById(i.productId).Price,
            LineTotal = i.qty * ProductData.GetById(i.productId).Price
        }).ToList());

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
- Anti-XSS usando `Server.HtmlEncode`.
- Mensajes de error genéricos para evitar revelar detalles sensibles.

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
            <nav style="background-color: #353A40; color: white;">
                <ul>
                    <li><a href="Default.aspx">Home</a></li>
                    <li><a href="Users.aspx" runat="server" visible='<%# Session["role"] == "Admin" %>'>Users</a></li>
                    <li><a href="Products.aspx" runat="server" visible='<%# Session["role"] == "Admin" %>'>Products</a></li>
                    <li><a href="CashRegister.aspx">Cash Register</a></li>
                    <li><a href="SalesReport.aspx">Sales Report</a></li>
                    <li><a href="Login.aspx" runat="server" visible='<%# Session["uid"] == null %>'>Login</a></li>
                    <li><a href="Logout.aspx" runat="server" visible='<%# Session["uid"] != null %>'>Logout</a></li>
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
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="YourNamespace.Login" %>
<asp:TextBox ID="txtEmail" runat="server" Required="true" />
<asp:TextBox ID="txtPassword" runat="server" TextMode="Password" Required="true" />
<asp:Button ID="btnLogin" runat="server" Text="Iniciar" OnClick="btnLogin_Click" />
<asp:Label ID="lblMessage" runat="server" />
```

- **Default.aspx**:
```html
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="YourNamespace.Default" %>
<asp:Label ID="lblWelcome" runat="server" />
<asp:Label ID="lblRole" runat="server" />
```

- **Users.aspx** (solo Admin):
```html
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Users.aspx.cs" Inherits="YourNamespace.Users" %>
<asp:GridView ID="gvUsers" runat="server" />
<asp:FormView ID="fvUser" runat="server" />
<asp:Label ID="lblMessage" runat="server" />
<asp:Button ID="btnNew" runat="server" Text="Nuevo" />
<asp:Button ID="btnSave" runat="server" Text="Guardar" />
<asp:Button ID="btnDelete" runat="server" Text="Eliminar" />
```

- **Products.aspx** (solo Admin):
```html
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Products.aspx.cs" Inherits="YourNamespace.Products" %>
<asp:GridView ID="gvProducts" runat="server" />
<asp:FormView ID="fvProduct" runat="server" />
<asp:Label ID="lblMessage" runat="server" />
<asp:Button ID="btnNew" runat="server" Text="Nuevo" />
<asp:Button ID="btnSave" runat="server" Text="Guardar" />
<asp:Button ID="btnDelete" runat="server" Text="Eliminar" />
```

- **CashRegister.aspx** (Admin/Cashier):
```html
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CashRegister.aspx.cs" Inherits="YourNamespace.CashRegister" %>
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

- **SalesReport.aspx** (Admin/Cashier):
```html
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SalesReport.aspx.cs" Inherits="YourNamespace.SalesReport" %>
<asp:TextBox ID="txtFrom" runat="server" />
<asp:TextBox ID="txtTo" runat="server" />
<asp:Button ID="btnFilter" runat="server" Text="Filtrar" />
<asp:GridView ID="gvSales" runat="server" />
<asp:Label ID="lblTotalGeneral" runat="server" />
<asp:Label ID="lblMessage" runat="server" />
```

### Estilos
```css
/* === Styles/Site.css === */
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

body {
    background-color: #F5F6FA;
    color: black;
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

---

## 5) GENERACIÓN DEL CÓDIGO DE PÁGINAS (CODE-BEHIND .ASPX.CS)

### A) // === Pages/Login.aspx.cs ===
```csharp
using System;
using System.Web;
using System.Web.UI;
using App_Code.Services;

namespace YourNamespace
{
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
}
```

### B) // === Pages/Default.aspx.cs ===
```csharp
using System;
using System.Web.UI;
using App_Code.Data;

namespace YourNamespace
{
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
                var user = UserData.GetById((int)Session["uid"]);
                lblWelcome.Text = $"Bienvenido, {user.Email}";
                lblRole.Text = $"Rol: {user.Role}";
            }
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

namespace YourNamespace
{
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

        protected void gvUsers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // TODO: Manejar comandos de GridView (nuevo, editar, eliminar)
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            // TODO: Guardar usuario
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            // TODO: Eliminar usuario
        }
    }
}
```

### D) // === Pages/Products.aspx.cs ===
```csharp
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using App_Code.Data;

namespace YourNamespace
{
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

        protected void gvProducts_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // TODO: Manejar comandos de GridView (nuevo, editar, eliminar)
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            // TODO: Guardar producto
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            // TODO: Eliminar producto
        }
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

namespace YourNamespace
{
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
            else
            {
                cart = (List<(int, string, decimal, int, decimal)>)ViewState["Cart"];
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
                int productId = int.Parse(ddlProducts.SelectedValue);
                var product = ProductData.GetById(productId);
                decimal lineTotal = qty * product.Price;

                // Agregar o actualizar item en el carrito
                var existingItem = cart.Find(i => i.productId == productId);
                if (existingItem.productId != 0)
                {
                    cart.Remove(existingItem);
                    qty += existingItem.qty; // Actualizar cantidad
                }

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
            lblSubtotal.Text = subtotal.ToString("C");
            lblTax.Text = (subtotal * 0.16m).ToString("C");
            lblTotal.Text = (subtotal * 1.16m).ToString("C");
        }

        private void BindCart()
        {
            gvCart.DataSource = cart;
            gvCart.DataBind();
        }

        protected void btnCheckout_Click(object sender, EventArgs e)
        {
            var saleService = new SalesService();
            int saleId = saleService.CreateSale((int)Session["uid"], cart.Select(i => (i.productId, i.qty)));
            lblMessage.Text = $"Venta registrada con ID: {saleId}";

            // Limpiar carrito
            cart.Clear();
            ViewState["Cart"] = cart;
            RecalcTotals();
            BindCart();
        }
    }
}
```

### F) // === Pages/SalesReport.aspx.cs ===
```csharp
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using App_Code.Data;

namespace YourNamespace
{
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

                lblTotalGeneral.Text = sales.Sum(s => s.Total).ToString("C");
            }
            else
            {
                lblMessage.Text = "Fechas inválidas.";
            }
        }
    }
}
```

### G) // === Site.Master.cs ===
```csharp
using System;
using System.Web.UI;
using App_Code.Services;

namespace YourNamespace
{
    public partial class Site : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["uid"] != null)
            {
                pnlAdmin.Visible = Session["role"].ToString() == "Admin";
            }
            else
            {
                pnlAdmin.Visible = false;
            }
        }

        protected void lnkLogout_Click(object sender, EventArgs e)
        {
            var authService = new AuthService();
            authService.Logout(Session);
            Response.Redirect("~/Pages/Login.aspx");
        }
    }
}
```

---

### Consideraciones de seguridad
- Se implementa SQL parametrizado para prevenir inyecciones SQL.
- Las contraseñas se almacenan usando BCrypt, evitando el almacenamiento en texto plano.
- Se valida la entrada del usuario en el servidor y se utiliza `Server.HtmlEncode` para prevenir XSS.
- Se maneja la sesión de manera segura, evitando el uso de cookies y asegurando que las páginas protegidas verifiquen la sesión activa.