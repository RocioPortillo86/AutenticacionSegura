A continuación, se presenta el código en C# para los eventos de los botones en las páginas .aspx que has proporcionado. Este código incluye la lógica para guardar la información en la base de datos y mostrarla en las respectivas páginas.

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
            // Limpiar el formulario para nuevo usuario
            fvUser.ChangeMode(FormViewMode.Insert);
            fvUser.DataBind();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            // Guardar usuario
            var user = new User
            {
                Email = ((TextBox)fvUser.FindControl("txtEmail")).Text,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(((TextBox)fvUser.FindControl("txtPassword")).Text),
                Role = ((DropDownList)fvUser.FindControl("ddlRole")).SelectedValue,
                Active = ((CheckBox)fvUser.FindControl("chkActive")).Checked
            };

            if (fvUser.CurrentMode == FormViewMode.Insert)
            {
                userData.Insert(user);
            }
            else if (fvUser.CurrentMode == FormViewMode.Edit)
            {
                user.Id = Convert.ToInt32(gvUsers.SelectedDataKey.Value);
                userData.Update(user);
            }

            LoadUsers();
        }

        protected void btnDelete_Command(object sender, CommandEventArgs e)
        {
            int userId = Convert.ToInt32(e.CommandArgument);
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
                Price = Convert.ToDecimal(((TextBox)fvProduct.FindControl("txtPrice")).Text),
                Stock = Convert.ToInt32(((TextBox)fvProduct.FindControl("txtStock")).Text),
                Active = ((CheckBox)fvProduct.FindControl("chkActive")).Checked
            };

            if (fvProduct.CurrentMode == FormViewMode.Insert)
            {
                productData.Insert(product);
            }
            else if (fvProduct.CurrentMode == FormViewMode.Edit)
            {
                product.Id = Convert.ToInt32(gvProducts.SelectedDataKey.Value);
                productData.Update(product);
            }

            LoadProducts();
        }

        protected void btnDelete_Command(object sender, CommandEventArgs e)
        {
            int productId = Convert.ToInt32(e.CommandArgument);
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
            int productId = Convert.ToInt32(ddlProducts.SelectedValue);
            int qty = Convert.ToInt32(txtQty.Text);
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

            lblSubtotal.Text = "Subtotal: " + cart.Sum(item => item.qty * productData.GetById(item.productId).Price).ToString("C");
            lblTax.Text = "IVA: " + (cart.Sum(item => item.qty * productData.GetById(item.productId).Price) * 0.16m).ToString("C");
            lblTotal.Text = "Total: " + (cart.Sum(item => item.qty * productData.GetById(item.productId).Price) * 1.16m).ToString("C");
        }

        protected void btnCheckout_Click(object sender, EventArgs e)
        {
            int cashierUserId = (int)Session["uid"];
            salesService.CreateSale(cashierUserId, cart);
            lblMessage.Text = "Venta registrada con éxito.";
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
            DateTime fromDate = DateTime.Parse(txtFrom.Text);
            DateTime toDate = DateTime.Parse(txtTo.Text);
            var sales = salesData.GetByDateRange(fromDate, toDate);
            gvSales.DataSource = sales;
            gvSales.DataBind();

            lblTotalGeneral.Text = "Total General: " + sales.Sum(s => s.Total).ToString("C");
        }
    }
}
```

### Notas sobre seguridad
- Se utiliza **hash y salt** para las contraseñas mediante BCrypt, evitando el almacenamiento de contraseñas en texto plano.
- Se implementan **consultas parametrizadas** en todas las interacciones con la base de datos para prevenir **SQL Injection**.
- Se valida la sesión en cada página protegida, redirigiendo a la página de login si no hay sesión activa.
- Se maneja la entrada del usuario y se asegura que los datos se procesen de manera segura y adecuada.