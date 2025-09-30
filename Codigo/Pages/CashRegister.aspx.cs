using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebApplication1.Class.Data;
using WebApplication1.Class.Services;

namespace WebApplication1.Pages
{
    public partial class CashRegister : System.Web.UI.Page
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
                lblMessage.Text = "Se agrego Producto";
                lblMessage.Style["display"] = "block";
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
            lblMessage.Style["display"] = "block";

            // Limpiar carrito
            cart.Clear();
            ViewState["Cart"] = cart;
            RecalcTotals();
            BindCart();
        }
    }
}