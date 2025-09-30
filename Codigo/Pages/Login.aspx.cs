using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.ApplicationServices;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebApplication1.Class.Services;
using WebApplication1.Class.Models;
namespace WebApplication1.Pages
{
    public partial class Login : System.Web.UI.Page
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
                AuthService authService = new AuthService();
                //var authService = new AuthService();
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