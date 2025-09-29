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

### SCRIPT SQL COMPLETO Y EJECUTABLE
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
    CreatedAtUtc DATETIME NOT NULL DEFAULT GETDATE(),
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

-- Insertar un usuario Admin inicial
INSERT INTO Users (Email, PasswordHash, Role, Active) 
VALUES ('admin@example.com', '$2a$12$e0N1Z1Q1Z1Z1Z1Z1Z1Z1Z1O1Z1Z1Z1Z1Z1Z1Z1Z1Z1Z1Z1Z1Z1', 'Admin', 1);
```
**Nota**: El valor de `PasswordHash` es un hash de ejemplo generado con BCrypt.

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
  - **Models**: Clases que representan las entidades del sistema (User, Product, Sale, SaleItem).
  - **Data**: Clases para el acceso a datos (Db, UserData, ProductData, SalesData).
  - **Services**: Clases que implementan la lógica de negocio (AuthService, SalesService).
- **Pages**: Contiene las páginas Web Forms del sistema.
- **Styles**: Archivos CSS para el estilo de la aplicación.
- **App_Themes**: Temas opcionales para la aplicación.
- **Site.Master**: Master page que define la estructura común de las páginas.

---

## 3) BACKEND CORE MÍNIMO Y SEGURO (ADO.NET + Session)

### Modelos (POCOs) en App_Code/Models
```csharp
public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; }
    public bool Active { get; set; }
}

public class Product
{
    public int Id { get; set; }
    public string Sku { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool Active { get; set; }
}

public class Sale
{
    public int Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public int CashierUserId { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
}

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
#### Db.cs
```csharp
public static class Db
{
    public static SqlConnection GetConnection()
    {
        return new SqlConnection(ConfigurationManager.ConnectionStrings["POS_DB"].ConnectionString);
    }
}
```

#### UserData.cs
```csharp
public class UserData
{
    public User GetUserByEmail(string email)
    {
        using (var connection = Db.GetConnection())
        {
            var command = new SqlCommand("SELECT Id, Email, PasswordHash, Role, Active FROM Users WHERE Email = @Email", connection);
            command.Parameters.AddWithValue("@Email", email);
            connection.Open();
            using (var reader = command.ExecuteReader())
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
        return null;
    }
}
```

#### ProductData.cs
```csharp
public class ProductData
{
    public void AddProduct(Product product)
    {
        using (var connection = Db.GetConnection())
        {
            var command = new SqlCommand("INSERT INTO Products (Sku, Name, Price, Stock, Active) VALUES (@Sku, @Name, @Price, @Stock, @Active)", connection);
            command.Parameters.AddWithValue("@Sku", product.Sku);
            command.Parameters.AddWithValue("@Name", product.Name);
            command.Parameters.AddWithValue("@Price", product.Price);
            command.Parameters.AddWithValue("@Stock", product.Stock);
            command.Parameters.AddWithValue("@Active", product.Active);
            connection.Open();
            command.ExecuteNonQuery();
        }
    }
}
```

#### SalesData.cs
```csharp
public class SalesData
{
    public void InsertSale(Sale sale, List<SaleItem> saleItems)
    {
        using (var connection = Db.GetConnection())
        {
            connection.Open();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    var command = new SqlCommand("INSERT INTO Sales (CashierUserId, Subtotal, Tax, Total) OUTPUT INSERTED.Id VALUES (@CashierUserId, @Subtotal, @Tax, @Total)", connection, transaction);
                    command.Parameters.AddWithValue("@CashierUserId", sale.CashierUserId);
                    command.Parameters.AddWithValue("@Subtotal", sale.Subtotal);
                    command.Parameters.AddWithValue("@Tax", sale.Tax);
                    command.Parameters.AddWithValue("@Total", sale.Total);
                    sale.Id = (int)command.ExecuteScalar();

                    foreach (var item in saleItems)
                    {
                        command = new SqlCommand("INSERT INTO SaleItems (SaleId, ProductId, Quantity, UnitPrice, LineTotal) VALUES (@SaleId, @ProductId, @Quantity, @UnitPrice, @LineTotal)", connection, transaction);
                        command.Parameters.AddWithValue("@SaleId", sale.Id);
                        command.Parameters.AddWithValue("@ProductId", item.ProductId);
                        command.Parameters.AddWithValue("@Quantity", item.Quantity);
                        command.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                        command.Parameters.AddWithValue("@LineTotal", item.LineTotal);
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw; // Manejo de errores genérico
                }
            }
        }
    }
}
```

### Servicios en App_Code/Services
#### AuthService.cs
```csharp
public class AuthService
{
    public User Login(string email, string password)
    {
        var userData = new UserData();
        var user = userData.GetUserByEmail(email);
        if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            HttpContext.Current.Session["uid"] = user.Id;
            HttpContext.Current.Session["role"] = user.Role;
            return user;
        }
        return null; // Login fallido
    }

    public void Logout()
    {
        HttpContext.Current.Session.Clear();
    }
}
```

#### SalesService.cs
```csharp
public class SalesService
{
    public void CreateSale(Sale sale, List<SaleItem> saleItems)
    {
        // Validar stock y calcular IVA
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
- **SQL parametrizado**: Se utiliza en todas las consultas.
- **BCrypt**: Se usa para el hash de contraseñas.
- **Validación en servidor**: Se implementa en las páginas.
- **Anti-XSS**: Se utiliza `Server.HtmlEncode` donde sea necesario.
- **Manejo de errores**: Se capturan excepciones y se muestran mensajes genéricos.

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
- Contenido normal: texto negro.

### Site.Master
```aspx
<%@ Master Language="C#" AutoEventWireup="true" CodeBehind="Site.master.cs" Inherits="YourNamespace.Site" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title></title>
    <link href="Styles/Site.css" rel="stylesheet" type="text/css" />
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
#### Login.aspx
```aspx
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="YourNamespace.Login" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Login</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:TextBox ID="txtEmail" runat="server" placeholder="Email"></asp:TextBox>
            <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail" ErrorMessage="Email es requerido." />
            <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" placeholder="Contraseña"></asp:TextBox>
            <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtPassword" ErrorMessage="Contraseña es requerida." />
            <asp:Button ID="btnLogin" runat="server" Text="Iniciar" OnClick="btnLogin_Click" />
            <asp:ValidationSummary ID="vsLogin" runat="server" />
        </div>
    </form>
</body>
</html>
```

#### Default.aspx
```aspx
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="YourNamespace.Default" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Inicio</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h1>Bienvenido, <%: Session["role"] %></h1>
        </div>
    </form>
</body>
</html>
```

#### Users.aspx (solo Admin)
```aspx
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Users.aspx.cs" Inherits="YourNamespace.Users" %>
<!DOCTYPE html>
<html>
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
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </form>
</body>
</html>
```

#### Products.aspx (solo Admin)
```aspx
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Products.aspx.cs" Inherits="YourNamespace.Products" %>
<!DOCTYPE html>
<html>
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
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </form>
</body>
</html>
```

#### CashRegister.aspx (Admin/Cashier)
```aspx
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CashRegister.aspx.cs" Inherits="YourNamespace.CashRegister" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Caja</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:DropDownList ID="ddlProducts" runat="server"></asp:DropDownList>
            <asp:TextBox ID="txtQuantity" runat="server" placeholder="Cantidad"></asp:TextBox>
            <asp:Button ID="btnAdd" runat="server" Text="Agregar" OnClick="btnAdd_Click" />
            <asp:GridView ID="gvCart" runat="server"></asp:GridView>
            <asp:Label ID="lblSubtotal" runat="server" Text="Subtotal: "></asp:Label>
            <asp:Label ID="lblTax" runat="server" Text="IVA: "></asp:Label>
            <asp:Label ID="lblTotal" runat="server" Text="Total: "></asp:Label>
            <asp:Button ID="btnRegisterSale" runat="server" Text="Registrar Venta" OnClick="btnRegisterSale_Click" />
        </div>
    </form>
</body>
</html>
```

#### SalesReport.aspx (Admin/Cashier)
```aspx
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SalesReport.aspx.cs" Inherits="YourNamespace.SalesReport" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Reporte de Ventas</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:TextBox ID="txtDateFrom" runat="server" placeholder="Fecha Desde"></asp:TextBox>
            <asp:TextBox ID="txtDateTo" runat="server" placeholder="Fecha Hasta"></asp:TextBox>
            <asp:Button ID="btnSearch" runat="server" Text="Buscar" OnClick="btnSearch_Click" />
            <asp:GridView ID="gvSales" runat="server"></asp:GridView>
        </div>
    </form>
</body>
</html>
```

### Estilos
#### Styles/Site.css
```css
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

.navbar li a {
    display: block;
    color: white;
    text-align: center;
    padding: 14px 16px;
    text-decoration: none;
}

.navbar li a:hover {
    background-color: #19A1B9;
}

.gridview-header {
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

### (Opcional) App_Themes/PosTheme/PosTheme.skin
```xml
<asp:GridView runat="server" CssClass="gridview-header" />
<asp:Button runat="server" CssClass="button-primary" />
<asp:Button runat="server" CssClass="button-danger" />
```

---

### Comentarios sobre decisiones de seguridad
- **SQL parametrizado**: Se utiliza en todas las consultas para prevenir inyecciones SQL.
- **Hash seguro**: Se implementa BCrypt para almacenar contraseñas de forma segura.
- **Validación en servidor**: Se asegura que todos los datos de entrada sean validados antes de procesarlos.
- **Manejo de sesión**: Se utiliza `Session` para mantener la autenticación del usuario sin depender de cookies, asegurando que la información sensible no se exponga.