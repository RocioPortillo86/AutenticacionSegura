using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.ApplicationServices;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebApplication1.Class.Services;
using WebApplication1.Class.Data;
using WebApplication1.Class.Models;
using System.Web.Security;
using System.IdentityModel.Tokens.Jwt;

namespace WebApplication1.Pages
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            /*if (Session["uid"] != null)
            {
                Response.Redirect("Default.aspx");
            }*/
        }

        protected void LoginForm_Authenticate(object sender, EventArgs e)
        {
            string email = LoginForm.UserName?.Trim();
            string password = LoginForm.Password?.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                LoginForm.FailureText = "Credenciales inválidas.";
                return;
            }

            string clientIp = Request.UserHostAddress;
            var userData = new UserData(System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            var authService = new AuthServices(userData);

            bool isValid = authService.ValidateUser(email, password, clientIp);

            if (isValid)
            {
                // Generar JWT
                var user = userData.GetByEmail(email);
                string jwt = WebApplication1.JwtHelpers.GenerateJwt(user.Email, user.Role);

                var cookie = new HttpCookie("pv_jwt", jwt)
                {
                    HttpOnly = true,
                    Secure = Request.IsSecureConnection,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddMinutes(int.Parse(System.Configuration.ConfigurationManager.AppSettings["JwtExpiryMinutes"] ?? "20"))
                };
                Response.Cookies.Add(cookie);

                FormsAuthentication.RedirectFromLoginPage(email, false);
            }
            else
            {
                LoginForm.FailureText = "Credenciales inválidas.";
            }
            /* string email = LoginForm.UserName?.Trim();
             string password = LoginForm.Password?.Trim();

             if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
             {
                 LoginForm.FailureText = "Credenciales inválidas.";
                 return;
             }
            //UserData userData = new UserData();
             string clientIp = Request.UserHostAddress;
             var userData = new UserData(System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
             var authService = new AuthServices(userData);

             bool isValid = authService.ValidateUser(email, password, clientIp);

             if (isValid)
             {
                 FormsAuthentication.RedirectFromLoginPage(email, false);
             }
             else
             {
                 LoginForm.FailureText = "Credenciales inválidas.";
             }*/
        }
    }
}