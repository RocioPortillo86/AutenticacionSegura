/*using System;
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
                UserDataAnterior UserData = new UserDataAnterior();
                var user = UserData.GetById((int)Session["uid"]);
                lblWelcome.Text = $"Bienvenido, {user.Email}";
                lblRole.Text = $"Rol: {user.Role}";
            }


        }
    }
}*/

using System;
using System.Linq;
using System.Web.Security;
using WebApplication1.Class.Data;

namespace WebApplication1.Pages
{
    public partial class Menu : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {


            var jwtCookie = Request.Cookies["pv_jwt"];
            if (jwtCookie == null)
            {
                Response.Redirect("~/Pages/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            var claimsPrincipal = WebApplication1.JwtHelpers.ValidateJwt(jwtCookie.Value);

            foreach (var claim in claimsPrincipal.Claims)
            {
                System.Diagnostics.Debug.WriteLine($"Claim type: {claim.Type}, value: {claim.Value}");
            }

            var email = claimsPrincipal.Claims
         .FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                Response.Redirect("~/Pages/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                var data = new UserData(System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
                var user = data.GetByEmail(email);
                if (user == null || !user.Active)
                {
                    Response.Redirect("~/Pages/Login.aspx", false);
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }

                lblWelcome.Text = $"Bienvenido, {user.Email}";
                lblRole.Text = $"Rol: {user.Role}";
            }

        }
    }
}