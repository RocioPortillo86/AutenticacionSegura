using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebApplication1.Class.Data;
using WebApplication1.Class.Models;

namespace WebApplication1.Pages
{
    public partial class Products : System.Web.UI.Page
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
            lblMessage.Text = "Producto guardado correctamente";
            lblMessage.Style["display"] = "block";
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "Eliminado correctamente";
            lblMessage.Style["display"] = "block";
        }

        protected void btnNew_Click(object sender, EventArgs e)
        {
            fvProduct.ChangeMode(FormViewMode.Insert);
            fvProduct.DataBind();
            lblMessage.Text = "Producto agregado correctamente";
            lblMessage.Style["display"] = "block";
        }
    }
}