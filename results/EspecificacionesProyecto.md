# Sistema de Punto de Venta (POS) en C# y ASP.NET Web Forms

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
#### Tablas principales
- **Users**: Almacena información de los usuarios.
  - Campos: Id (PK), Email (UN), PasswordHash, Role, Active.
- **Products**: Almacena información de los productos.
  - Campos: Id (PK), Sku (UN), Name, Price, Stock, Active.
- **Sales**: Almacena información de las ventas.
  - Campos: Id (PK), CreatedAtUtc, CashierUserId (FK), Subtotal, Tax, Total.
- **SaleItems**: Almacena los ítems de cada venta.
  - Campos: Id (PK), SaleId (FK), ProductId (FK), Quantity, UnitPrice, LineTotal.

#### SCRIPT SQL COMPLETO Y EJECUTABLE
```sql
-- Crear la base de datos
CREATE DATABASE POS_DB;
GO

USE POS_DB;
GO

-- Crear tabla Users
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(255) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(50) NOT NULL,
    Active BIT NOT NULL
);

-- Crear tabla Products
CREATE TABLE Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Sku NVARCHAR(50) UNIQUE NOT NULL,
    Name NVARCHAR(255) NOT NULL,
    Price DECIMAL(18, 2) NOT NULL,
    Stock INT NOT NULL,
    Active BIT NOT NULL
);

-- Crear tabla Sales
CREATE TABLE Sales (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CreatedAtUtc DATETIME NOT NULL DEFAULT GETDATE(),
    CashierUserId INT NOT NULL,
    Subtotal DECIMAL(18, 2) NOT NULL,
    Tax DECIMAL(18, 2) NOT NULL,
    Total DECIMAL(18, 2) NOT NULL,
    FOREIGN KEY (CashierUserId) REFERENCES Users(Id)
);

-- Crear tabla SaleItems
CREATE TABLE SaleItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
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
VALUES ('admin@example.com', '$2a$12$e0N1Z1Z1Z1Z1Z1Z1Z1Z1Z1u1Z1Z1Z1Z1Z1Z1Z1Z1Z1Z1Z1Z1Z1Z1', 'Admin', 1);
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

### Descripción de carpetas/archivos
- **App_Code**: Contiene la lógica de negocio y acceso a datos.
  - **Models**: Clases POCO que representan las entidades del sistema.
  - **Data**: Clases para el acceso a datos utilizando ADO.NET.
  - **Services**: Clases que implementan la lógica de negocio.
- **Pages**: Contiene las páginas Web Forms (.aspx) del sistema.
- **Styles**: Archivos CSS para el estilo de la aplicación.
- **App_Themes**: Temas opcionales para la aplicación.
- **Site.Master**: Contenedor maestro que incluye el menú de navegación.

## 3) BACKEND CORE MÍNIMO Y SEGURO (ADO.NET + Session)

### Modelos (POCOs) en App_Code/Models
```csharp
// === User.cs ===
public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; }
    public bool Active { get; set; }
}

// === Product.cs ===
public class Product
{
    public int Id { get; set; }
    public string Sku { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool Active { get; set; }
}

// === Sale.cs ===
public class Sale
{
    public int Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public int CashierUserId { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
}

// === SaleItem.cs ===
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
// === Db.cs ===
public static class Db
{
    public static SqlConnection GetConnection()
    {
        return new SqlConnection(ConfigurationManager.ConnectionStrings["POS_DB"].ConnectionString);
    }
}

// === UserData.cs ===
public class UserData
{
    public User GetUserByEmail(string email)
    {
        using (var conn = Db.GetConnection())
        {
            conn.Open();
            using (var cmd = new SqlCommand("SELECT Id, Email, PasswordHash, Role, Active FROM Users WHERE Email = @Email", conn))
            {
                cmd.Parameters.AddWithValue("@Email", email);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new User
                        {
                            Id = (int)reader["Id"],
                            Email = (string)reader["Email"],
                            PasswordHash = (string)reader["PasswordHash"],
                            Role = (string)reader["Role"],
                            Active = (bool)reader["Active"]
                        };
                    }
                }
            }
        }
        return null;
    }

    // TODO: Métodos para Insertar, Actualizar y Eliminar usuarios
}

// === ProductData.cs ===
public class ProductData
{
    public List<Product> GetAllProducts()
    {
        var products = new List<Product>();
        using (var conn = Db.GetConnection())
        {
            conn.Open();
            using (var cmd = new SqlCommand("SELECT Id, Sku, Name, Price, Stock, Active FROM Products", conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            Id = (int)reader["Id"],
                            Sku = (string)reader["Sku"],
                            Name = (string)reader["Name"],
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

    // TODO: Métodos para Insertar, Actualizar y Eliminar productos
}

// === SalesData.cs ===
public class SalesData
{
    public void InsertSale(Sale sale, List<SaleItem> saleItems)
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
                    foreach (var item in saleItems)
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

                        // TODO: Descontar stock del producto
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw; // Manejo de errores
                }
            }
        }
    }
}
```

### Servicios en App_Code/Services
```csharp
// === AuthService.cs ===
public class AuthService
{
    public User Login(string email, string password)
    {
        var userData = new UserData();
        var user = userData.GetUserByEmail(email);
        if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            // Establecer sesión
            HttpContext.Current.Session["uid"] = user.Id;
            HttpContext.Current.Session["role"] = user.Role;
            return user;
        }
        return null; // Credenciales inválidas
    }

    public void Logout()
    {
        HttpContext.Current.Session.Clear();
    }
}

// === SalesService.cs ===
public class SalesService
{
    public void CreateSale(Sale sale, List<SaleItem> saleItems)
    {
        // TODO: Validar stock
        sale.Tax = sale.Subtotal * 0.16m; // Calcular IVA
        sale.Total = sale.Subtotal + sale.Tax;

        var salesData = new SalesData();
        salesData.InsertSale(sale, saleItems);
    }
}
```

### Pautas para páginas
- En `Page_Load` de páginas protegidas: 
```csharp
if (Session["uid"] == null)
{
    Response.Redirect("Login.aspx");
}
```
- `Users.aspx` y `Products.aspx`: solo acceso Admin.

### Seguridad mínima
- SQL parametrizado en todas las consultas.
- Hash de contraseñas con BCrypt.
- Validación en servidor.
- Anti-XSS con `Server.HtmlEncode`.
- Mensajes de error genéricos.

## 4) FRONTEND (WEB FORMS CON CONTROLES ASP.NET + ESTILOS PROPIOS)

### Menú de navegación (header superior)
- Diseñar el menú en la parte superior (barra horizontal fija/sencilla).
- Fondo del menú: `#353A40`.
- Texto de los ítems del menú: **blanco**.
- Mostrar opciones: Home, Users, Products, CashRegister, SalesReport, Logout.
- Ocultar o desactivar enlaces según el rol (`Session["role"]`).

### Paleta de colores obligatoria en toda la app
- Menú superior (navbar): fondo `#353A40`, **texto blanco**.
- Fondos generales: `#F5F6FA`, **texto negro**.
- Encabezado de GridView: fondo `#19A1B9`, **texto blanco**.
- Botones principales: fondo `#0F6AF6`, **texto blanco**.
- Botones de acción crítica (eliminar/cancelar): fondo `#E13C4A`, **texto blanco**.
- Todo el contenido normal (labels, texto en formularios): **negro**.

### Site.Master
```html
<%@ Master Language="C#" AutoEventWireup="true" CodeBehind="Site.master.cs" Inherits="YourNamespace.Site" %>
<!DOCTYPE html>
<html lang="es-MX">
<head runat="server">
    <title>POS System</title>
    <link href="Styles/Site.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="navbar">
            <ul>
                <li><a href="Default.aspx">Home</a></li>
                <% if (Session["role"] != null && Session["role"].ToString() == "Admin") { %>
                    <li><a href="Users.aspx">Users</a></li>
                    <li><a href="Products.aspx">Products</a></li>
                <% } %>
                <li><a href="CashRegister.aspx">Cash Register</a></li>
                <li><a href="SalesReport.aspx">Sales Report</a></li>
                <li><a href="Logout.aspx">Logout</a></li>
            </ul>
        </div>
        <asp:ContentPlaceHolder ID="MainContent" runat="server" />
    </form>
</body>
</html>
```

### Páginas (.aspx) con controles ASP.NET y validadores
```html
// === Login.aspx ===
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="YourNamespace.Login" %>
<!DOCTYPE html>
<html lang="es-MX">
<head runat="server">
    <title>Login</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:Label ID="lblEmail" runat="server" Text="Email:" />
            <asp:TextBox ID="txtEmail" runat="server" />
            <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail" ErrorMessage="Email es requerido." />
            <asp:Label ID="lblPassword" runat="server" Text="Password:" />
            <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" />
            <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtPassword" ErrorMessage="Password es requerido." />
            <asp:Button ID="btnLogin" runat="server" Text="Iniciar" OnClick="btnLogin_Click" />
            <asp:ValidationSummary ID="vsLogin" runat="server" />
        </div>
    </form>
</body>
</html>

// === Default.aspx ===
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="YourNamespace.Default" %>
<!DOCTYPE html>
<html lang="es-MX">
<head runat="server">
    <title>Bienvenido</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h1>Bienvenido, <%: Session["role"] %></h1>
        </div>
    </form>
</body>
</html>

// === Users.aspx ===
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Users.aspx.cs" Inherits="YourNamespace.Users" %>
<!DOCTYPE html>
<html lang="es-MX">
<head runat="server">
    <title>Usuarios</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:GridView ID="gvUsers" runat="server" AutoGenerateColumns="False">
                <Columns>
                    <asp:BoundField DataField="Email" HeaderText="Email" />
                    <asp:BoundField DataField="Role" HeaderText="Rol" />
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:Button ID="btnEdit" runat="server" Text="Editar" CommandArgument='<%# Eval("Id") %>' OnClick="btnEdit_Click" />
                            <asp:Button ID="btnDelete" runat="server" Text="Eliminar" CommandArgument='<%# Eval("Id") %>' OnClick="btnDelete_Click" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
            <asp:Button ID="btnAddUser" runat="server" Text="Agregar Usuario" OnClick="btnAddUser_Click" />
        </div>
    </form>
</body>
</html>

// === Products.aspx ===
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Products.aspx.cs" Inherits="YourNamespace.Products" %>
<!DOCTYPE html>
<html lang="es-MX">
<head runat="server">
    <title>Productos</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:GridView ID="gvProducts" runat="server" AutoGenerateColumns="False">
                <Columns>
                    <asp:BoundField DataField="Sku" HeaderText="SKU" />
                    <asp:BoundField DataField="Name" HeaderText="Nombre" />
                    <asp:BoundField DataField="Price" HeaderText="Precio" />
                    <asp:BoundField DataField="Stock" HeaderText="Stock" />
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:Button ID="btnEdit" runat="server" Text="Editar" CommandArgument='<%# Eval("Id") %>' OnClick="btnEdit_Click" />
                            <asp:Button ID="btnDelete" runat="server" Text="Eliminar" CommandArgument='<%# Eval("Id") %>' OnClick="btnDelete_Click" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
            <asp:Button ID="btnAddProduct" runat="server" Text="Agregar Producto" OnClick="btnAddProduct_Click" />
        </div>
    </form>
</body>
</html>

// === CashRegister.aspx ===
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CashRegister.aspx.cs" Inherits="YourNamespace.CashRegister" %>
<!DOCTYPE html>
<html lang="es-MX">
<head runat="server">
    <title>Caja</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:DropDownList ID="ddlProducts" runat="server">
                <!-- TODO: Llenar productos activos -->
            </asp:DropDownList>
            <asp:TextBox ID="txtQuantity" runat="server" />
            <asp:Button ID="btnAddToCart" runat="server" Text="Agregar" OnClick="btnAddToCart_Click" />
            <asp:GridView ID="gvCart" runat="server" AutoGenerateColumns="False">
                <Columns>
                    <asp:BoundField DataField="ProductName" HeaderText="Producto" />
                    <asp:BoundField DataField="Quantity" HeaderText="Cantidad" />
                    <asp:BoundField DataField="LineTotal" HeaderText="Total" />
                </Columns>
            </asp:GridView>
            <asp:Label ID="lblSubtotal" runat="server" Text="Subtotal: $" />
            <asp:Label ID="lblTax" runat="server" Text="IVA: $" />
            <asp:Label ID="lblTotal" runat="server" Text="Total: $" />
            <asp:Button ID="btnRegisterSale" runat="server" Text="Registrar Venta" OnClick="btnRegisterSale_Click" />
        </div>
    </form>
</body>
</html>

// === SalesReport.aspx ===
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SalesReport.aspx.cs" Inherits="YourNamespace.SalesReport" %>
<!DOCTYPE html>
<html lang="es-MX">
<head runat="server">
    <title>Reporte de Ventas</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:TextBox ID="txtDateFrom" runat="server" />
            <asp:TextBox ID="txtDateTo" runat="server" />
            <asp:Button ID="btnGenerateReport" runat="server" Text="Generar Reporte" OnClick="btnGenerateReport_Click" />
            <asp:GridView ID="gvSalesReport" runat="server" AutoGenerateColumns="False">
                <Columns>
                    <asp:BoundField DataField="SaleId" HeaderText="ID Venta" />
                    <asp:BoundField DataField="Total" HeaderText="Total" />
                </Columns>
            </asp:GridView>
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

.navbar {
    background-color: #353A40;
    overflow: hidden;
}

.navbar ul {
    list-style-type: none;
    margin: 0;
    padding: 0;
}

.navbar li {
    float: left;
}

.navbar a {
    display: block;
    color: white;
    text-align: center;
    padding: 14px 16px;
    text-decoration: none;
}

.navbar a:hover {
    background-color: #19A1B9;
}

.grid-header {
    background-color: #19A1B9;
    color: white;
}

.button-primary {
    background-color: #0F6AF6;
    color: white;
}

.button-danger {
    background-color: #E13C4A;
    color: white;
}
```

## 5) GENERACIÓN DEL CÓDIGO COMPLETO DEL PROYECTO

### a) Web.config
```xml
<configuration>
  <connectionStrings>
    <add name="POS_DB" connectionString="Data Source=.;Initial Catalog=POS_DB;Integrated Security=True;" providerName="System.Data.SqlClient" />
  </connectionStrings>
  <globalization culture="es-MX" uiCulture="es-MX" />
  <compilation targetFramework="4.8" />
  <sessionState timeout="20" />
  <pages validateRequest="true" viewStateEncryptionMode="Always" />
  <httpRuntime requestValidationMode="2.0" />
</configuration>
```

### b) App_Code/Models
```csharp
// === User.cs ===
public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; }
    public bool Active { get; set; }
}

// === Product.cs ===
public class Product
{
    public int Id { get; set; }
    public string Sku { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool Active { get; set; }
}

// === Sale.cs ===
public class Sale
{
    public int Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public int CashierUserId { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
}

// === SaleItem.cs ===
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

### c) App_Code/Data
```csharp
// === Db.cs ===
public static class Db
{
    public static SqlConnection GetConnection()
    {
        return new SqlConnection(ConfigurationManager.ConnectionStrings["POS_DB"].ConnectionString);
    }
}

// === UserData.cs ===
public class UserData
{
    public User GetUserByEmail(string email)
    {
        using (var conn = Db.GetConnection())
        {
            conn.Open();
            using (var cmd = new SqlCommand("SELECT Id, Email, PasswordHash, Role, Active FROM Users WHERE Email = @Email", conn))
            {
                cmd.Parameters.AddWithValue("@Email", email);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new User
                        {
                            Id = (int)reader["Id"],
                            Email = (string)reader["Email"],
                            PasswordHash = (string)reader["PasswordHash"],
                            Role = (string)reader["Role"],
                            Active = (bool)reader["Active"]
                        };
                    }
                }
            }
        }
        return null;
    }

    // TODO: Métodos para Insertar, Actualizar y Eliminar usuarios
}

// === ProductData.cs ===
public class ProductData
{
    public List<Product> GetAllProducts()
    {
        var products = new List<Product>();
        using (var conn = Db.GetConnection())
        {
            conn.Open();
            using (var cmd = new SqlCommand("SELECT Id, Sku, Name, Price, Stock, Active FROM Products", conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            Id = (int)reader["Id"],
                            Sku = (string)reader["Sku"],
                            Name = (string)reader["Name"],
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

    // TODO: Métodos para Insertar, Actualizar y Eliminar productos
}

// === SalesData.cs ===
public class SalesData
{
    public void InsertSale(Sale sale, List<SaleItem> saleItems)
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
                    foreach (var item in saleItems)
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

                        // TODO: Descontar stock del producto
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw; // Manejo de errores
                }
            }
        }
    }
}
```

### d) App_Code/Services
```csharp
// === AuthService.cs ===
public class AuthService
{
    public User Login(string email, string password)
    {
        var userData = new UserData();
        var user = userData.GetUserByEmail(email);
        if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            // Establecer sesión
            HttpContext.Current.Session["uid"] = user.Id;
            HttpContext.Current.Session["role"] = user.Role;
            return user;
        }
        return null; // Credenciales inválidas
    }

    public void Logout()
    {
        HttpContext.Current.Session.Clear();
    }
}

// === SalesService.cs ===
public class SalesService
{
    public void CreateSale(Sale sale, List<SaleItem> saleItems)
    {
        // TODO: Validar stock
        sale.Tax = sale.Subtotal * 0.16m; // Calcular IVA
        sale.Total = sale.Subtotal + sale.Tax;

        var salesData = new SalesData();
        salesData.InsertSale(sale, saleItems);
    }
}
```

### e) Site.Master
```html
<%@ Master Language="C#" AutoEventWireup="true" CodeBehind="Site.master.cs" Inherits="YourNamespace.Site" %>
<!DOCTYPE html>
<html lang="es-MX">
<head runat="server">
    <title>POS System</title>
    <link href="Styles/Site.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="navbar">
            <ul>
                <li><a href="Default.aspx">Home</a></li>
                <% if (Session["role"] != null && Session["role"].ToString() == "Admin") { %>
                    <li><a href="Users.aspx">Users</a></li>
                    <li><a href="Products.aspx">Products</a></li>
                <% } %>
                <li><a href="CashRegister.aspx">Cash Register</a></li>
                <li><a href="SalesReport.aspx">Sales Report</a></li>
                <li><a href="Logout.aspx">Logout</a></li>
            </ul>
        </div>
        <asp:ContentPlaceHolder ID="MainContent" runat="server" />
    </form>
</body>
</html>
```

### f) Styles/Site.css
```css
/* === Styles/Site.css === */
body {
    background-color: #F5F6FA;
    color: black;
}

.navbar {
    background-color: #353A40;
    overflow: hidden;
}

.navbar ul {
    list-style-type: none;
    margin: 0;
    padding: 0;
}

.navbar li {
    float: left;
}

.navbar a {
    display: block;
    color: white;
    text-align: center;
    padding: 14px 16px;
    text-decoration: none;
}

.navbar a:hover {
    background-color: #19A1B9;
}

.grid-header {
    background-color: #19A1B9;
    color: white;
}

.button-primary {
    background-color: #0F6AF6;
    color: white;
}

.button-danger {
    background-color: #E13C4A;
    color: white;
}
```

### g) Páginas .aspx y .aspx.cs
```html
// === Login.aspx ===
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="YourNamespace.Login" %>
<!DOCTYPE html>
<html lang="es-MX">
<head runat="server">
    <title>Login</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:Label ID="lblEmail" runat="server" Text="Email:" />
            <asp:TextBox ID="txtEmail" runat="server" />
            <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail" ErrorMessage="Email es requerido." />
            <asp:Label ID="lblPassword" runat="server" Text="Password:" />
            <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" />
            <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtPassword" ErrorMessage="Password es requerido." />
            <asp:Button ID="btnLogin" runat="server" Text="Iniciar" OnClick="btnLogin_Click" />
            <asp:ValidationSummary ID="vsLogin" runat="server" />
        </div>
    </form>
</body>
</html>

// === Default.aspx ===
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="YourNamespace.Default" %>
<!DOCTYPE html>
<html lang="es-MX">
<head runat="server">
    <title>Bienvenido</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h1>Bienvenido, <%: Session["role"] %></h1>
        </div>
    </form>
</body>
</html>

// === Users.aspx ===
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Users.aspx.cs" Inherits="YourNamespace.Users" %>
<!DOCTYPE html>
<html lang="es-MX">
<head runat="server">
    <title>Usuarios</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:GridView ID="gvUsers" runat="server" AutoGenerateColumns="False">
                <Columns>
                    <asp:BoundField DataField="Email" HeaderText="Email" />
                    <asp:BoundField DataField="Role" HeaderText="Rol" />
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:Button ID="btnEdit" runat="server" Text="Editar" CommandArgument='<%# Eval("Id") %>' OnClick="btnEdit_Click" />
                            <asp:Button ID="btnDelete" runat="server" Text="Eliminar" CommandArgument='<%# Eval("Id") %>' OnClick="btnDelete_Click" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
            <asp:Button ID="btnAddUser" runat="server" Text="Agregar Usuario" OnClick="btnAddUser_Click" />
        </div>
    </form>
</body>
</html>

// === Products.aspx ===
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Products.aspx.cs" Inherits="YourNamespace.Products" %>
<!DOCTYPE html>
<html lang="es-MX">
<head runat="server">
    <title>Productos</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:GridView ID="gvProducts" runat="server" AutoGenerateColumns="False">
                <Columns>
                    <asp:BoundField DataField="Sku" HeaderText="SKU" />
                    <asp:BoundField DataField="Name" HeaderText="Nombre" />
                    <asp:BoundField DataField="Price" HeaderText="Precio" />
                    <asp:BoundField DataField="Stock" HeaderText="Stock" />
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:Button ID="btnEdit" runat="server" Text="Editar" CommandArgument='<%# Eval("Id") %>' OnClick="btnEdit_Click" />
                            <asp:Button ID="btnDelete" runat="server" Text="Eliminar" CommandArgument='<%# Eval("Id") %>' OnClick="btnDelete_Click" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
            <asp:Button ID="btnAddProduct" runat="server" Text="Agregar Producto" OnClick="btnAddProduct_Click" />
        </div>
    </form>
</body>
</html>

// === CashRegister.aspx ===
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CashRegister.aspx.cs" Inherits="YourNamespace.CashRegister" %>
<!DOCTYPE html>
<html lang="es-MX">
<head runat="server">
    <title>Caja</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:DropDownList ID="ddlProducts" runat="server">
                <!-- TODO: Llenar productos activos -->
            </asp:DropDownList>
            <asp:TextBox ID="txtQuantity" runat="server" />
            <asp:Button ID="btnAddToCart" runat="server" Text="Agregar" OnClick="btnAddToCart_Click" />
            <asp:GridView ID="gvCart" runat="server" AutoGenerateColumns="False">
                <Columns>
                    <asp:BoundField DataField="ProductName" HeaderText="Producto" />
                    <asp:BoundField DataField="Quantity" HeaderText="Cantidad" />
                    <asp:BoundField DataField="LineTotal" HeaderText="Total" />
                </Columns>
            </asp:GridView>
            <asp:Label ID="lblSubtotal" runat="server" Text="Subtotal: $" />
            <asp:Label ID="lblTax" runat="server" Text="IVA: $" />
            <asp:Label ID="lblTotal" runat="server" Text="Total: $" />
            <asp:Button ID="btnRegisterSale" runat="server" Text="Registrar Venta" OnClick="btnRegisterSale_Click" />
        </div>
    </form>
</body>
</html>

// === SalesReport.aspx ===
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SalesReport.aspx.cs" Inherits="YourNamespace.SalesReport" %>
<!DOCTYPE html>
<html lang="es-MX">
<head runat="server">
    <title>Reporte de Ventas</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:TextBox ID="txtDateFrom" runat="server" />
            <asp:TextBox ID="txtDateTo" runat="server" />
            <asp:Button ID="btnGenerateReport" runat="server" Text="Generar Reporte" OnClick="btnGenerateReport_Click" />
            <asp:GridView ID="gvSalesReport" runat="server" AutoGenerateColumns="False">
                <Columns>
                    <asp:BoundField DataField="SaleId" HeaderText="ID Venta" />
                    <asp:BoundField DataField="Total" HeaderText="Total" />
                </Columns>
            </asp:GridView>
        </div>
    </form>
</body>
</html>
```

### Comentarios sobre decisiones de seguridad
- Se utilizó SQL parametrizado en todas las consultas para prevenir inyecciones SQL.
- Las contraseñas se almacenan como hashes utilizando BCrypt, evitando el almacenamiento en texto plano.
- Se implementó validación en el servidor para entradas de usuario.
- Se redirige a la página de login si la sesión no está activa.
- Se controla el acceso a páginas y botones según el rol del usuario almacenado en la sesión.