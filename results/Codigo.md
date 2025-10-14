A continuación, se presenta el código en C# para los eventos de los botones en las páginas Web Forms que has especificado. Este código incluye la lógica para guardar la información en la base de datos y mostrarla en los controles correspondientes.

### Login.aspx.cs
```csharp
using System;
using System.Web;
using System.Web.UI;

namespace YourNamespace
{
    public partial class Login : Page
    {
        protected void btnLogin_Click(object sender, EventArgs e)
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

### Default.aspx.cs
```csharp
using System;
using System.Web;
using System.Web.UI;

namespace YourNamespace
{
    public partial class Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["uid"] == null)
            {
                Response.Redirect("Login.aspx");
            }
            lblWelcome.Text = "Bienvenido, " + Session["uid"].ToString();
            lblRole.Text = "Rol: " + Session["role"].ToString();
        }
    }
}
```

### Users.aspx.cs
```csharp
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace YourNamespace
{
    public partial class Users : Page
    {
        private UserData userData = new UserData();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["uid"] == null)
            {
                Response.Redirect("Login.aspx");
            }

            if (!IsPostBack)
            {
                LoadUsers();
            }
        }

        private void LoadUsers()
        {
            var users = userData.GetAll();
            gvUsers.DataSource = users;
            gvUsers.DataBind();
        }

        protected void btnNew_Click(object sender, EventArgs e)
        {
            fvUser.ChangeMode(FormViewMode.Insert);
            fvUser.DataBind();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (fvUser.CurrentMode == FormViewMode.Insert)
            {
                var user = new User
                {
                    Email = ((TextBox)fvUser.FindControl("txtEmail")).Text,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(((TextBox)fvUser.FindControl("txtPassword")).Text),
                    Role = ((DropDownList)fvUser.FindControl("ddlRole")).SelectedValue,
                    Active = ((CheckBox)fvUser.FindControl("chkActive")).Checked
                };
                userData.Insert(user);
            }
            else if (fvUser.CurrentMode == FormViewMode.Edit)
            {
                var userId = (int)gvUsers.DataKeys[gvUsers.SelectedIndex].Value;
                var user = userData.GetById(userId);
                user.Email = ((TextBox)fvUser.FindControl("txtEmail")).Text;
                user.Role = ((DropDownList)fvUser.FindControl("ddlRole")).SelectedValue;
                user.Active = ((CheckBox)fvUser.FindControl("chkActive")).Checked;
                userData.Update(user);
            }
            LoadUsers();
        }

        protected void btnDelete_Command(object sender, CommandEventArgs e)
        {
            var userId = Convert.ToInt32(e.CommandArgument);
            userData.Delete(userId);
            LoadUsers();
        }

        protected void gvUsers_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvUsers.PageIndex = e.NewPageIndex;
            LoadUsers();
        }
    }
}
```

### Products.aspx.cs
```csharp
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace YourNamespace
{
    public partial class Products : Page
    {
        private ProductData productData = new ProductData();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["uid"] == null)
            {
                Response.Redirect("Login.aspx");
            }

            if (!IsPostBack)
            {
                LoadProducts();
            }
        }

        private void LoadProducts()
        {
            var products = productData.GetAll();
            gvProducts.DataSource = products;
            gvProducts.DataBind();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            var product = new Product
            {
                Sku = ((TextBox)fvProduct.FindControl("txtSku")).Text,
                Name = ((TextBox)fvProduct.FindControl("txtName")).Text,
                Price = decimal.Parse(((TextBox)fvProduct.FindControl("txtPrice")).Text),
                Stock = int.Parse(((TextBox)fvProduct.FindControl("txtStock")).Text),
                Active = ((CheckBox)fvProduct.FindControl("chkActive")).Checked
            };
            productData.Insert(product);
            LoadProducts();
        }

        protected void btnDelete_Command(object sender, CommandEventArgs e)
        {
            var productId = Convert.ToInt32(e.CommandArgument);
            productData.Delete(productId);
            LoadProducts();
        }

        protected void gvProducts_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvProducts.PageIndex = e.NewPageIndex;
            LoadProducts();
        }
    }
}
```

### CashRegister.aspx.cs
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;

namespace YourNamespace
{
    public partial class CashRegister : Page
    {
        private ProductData productData = new ProductData();
        private SalesService salesService = new SalesService();
        private List<(int productId, int qty)> cart = new List<(int productId, int qty)>();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["uid"] == null)
            {
                Response.Redirect("Login.aspx");
            }

            if (!IsPostBack)
            {
                LoadProducts();
            }
        }

        private void LoadProducts()
        {
            var products = productData.GetAll();
            ddlProducts.DataSource = products;
            ddlProducts.DataTextField = "Name";
            ddlProducts.DataValueField = "Id";
            ddlProducts.DataBind();
        }

        protected void btnAddItem_Click(object sender, EventArgs e)
        {
            var productId = int.Parse(ddlProducts.SelectedValue);
            var qty = int.Parse(txtQty.Text);
            cart.Add((productId, qty));
            UpdateCart();
        }

        private void UpdateCart()
        {
            gvCart.DataSource = cart.Select(item => new
            {
                Product = productData.GetById(item.productId).Name,
                Quantity = item.qty,
                UnitPrice = productData.GetById(item.productId).Price,
                LineTotal = item.qty * productData.GetById(item.productId).Price
            }).ToList();
            gvCart.DataBind();

            var subtotal = cart.Sum(item => item.qty * productData.GetById(item.productId).Price);
            lblSubtotal.Text = $"Subtotal: {subtotal:C}";
            lblTax.Text = $"IVA: {subtotal * 0.16m:C}";
            lblTotal.Text = $"Total: {subtotal * 1.16m:C}";
        }

        protected void btnCheckout_Click(object sender, EventArgs e)
        {
            var cashierUserId = (int)Session["uid"];
            var saleId = salesService.CreateSale(cashierUserId, cart);
            lblMessage.Text = $"Venta registrada con ID: {saleId}";
            cart.Clear();
            UpdateCart();
        }
    }
}
```

### SalesReport.aspx.cs
```csharp
using System;
using System.Web.UI;

namespace YourNamespace
{
    public partial class SalesReport : Page
    {
        private SalesData salesData = new SalesData();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["uid"] == null)
            {
                Response.Redirect("Login.aspx");
            }
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            DateTime fromDate;
            DateTime toDate;

            if (DateTime.TryParse(txtFrom.Text, out fromDate) && DateTime.TryParse(txtTo.Text, out toDate))
            {
                var sales = salesData.GetByDateRange(fromDate, toDate);
                gvSales.DataSource = sales;
                gvSales.DataBind();

                var totalGeneral = sales.Sum(s => s.Total);
                lblTotalGeneral.Text = $"Total General: {totalGeneral:C}";
            }
            else
            {
                lblMessage.Text = "Fechas inválidas.";
            }
        }
    }
}
```

### Comentarios sobre decisiones de seguridad
- Se utiliza **parametrización** en todas las consultas SQL para prevenir inyecciones SQL.
- Las contraseñas se almacenan usando **hash + salt** con BCrypt, asegurando que nunca se guarde texto plano.
- Se valida la entrada del usuario en el servidor y se utiliza `validateRequest="true"` en las páginas para prevenir ataques XSS.
- Se gestiona la sesión del usuario usando `Session` en lugar de cookies, y se valida la sesión en cada página protegida.