using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebApplication1.Class.Services;

namespace WebApplication1
{
    public partial class Site : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            /*if (Session["uid"] != null)
            {
                pnlAdmin.Visible = Session["role"].ToString() == "Admin";
            }
            else
            {
                pnlAdmin.Visible = false;
            }*/
        }

        protected void lnkLogout_Click(object sender, EventArgs e)
        {
            var authService = new AuthService();
            authService.Logout(Session);
            Response.Redirect("~/Pages/Login.aspx");
        }
    }
}