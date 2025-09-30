using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebApplication1.Class.Data;

namespace WebApplication1.Pages
{
    public partial class Menu : System.Web.UI.Page
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
                UserData UserData = new UserData();
                var user = UserData.GetById((int)Session["uid"]);
                lblWelcome.Text = $"Bienvenido, {user.Email}";
                lblRole.Text = $"Rol: {user.Role}";
            }


        }
    }
}