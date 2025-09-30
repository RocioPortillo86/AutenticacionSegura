using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebApplication1.Class.Data;

namespace WebApplication1.Pages
{
    public partial class SalesReport : System.Web.UI.Page
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
                lblMessage.Text = "Filtro aplicado";
                lblMessage.Style["display"] = "block";
            }
            else
            {
                lblMessage.Text = "Fechas inválidas.";
                lblMessage.Style["display"] = "block";
            }
        }
    }
}