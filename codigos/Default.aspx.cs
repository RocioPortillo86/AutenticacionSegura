using System;
using System.Linq;
using System.Web;
using System.Web.Security;
using WebApplication1.Class.Data;
using WebApplication1.Class.Models;

namespace WebApplication1.Pages
{
    public partial class Menu : System.Web.UI.Page
    {
        // Inyección para pruebas
        internal Func<string, System.Security.Claims.ClaimsPrincipal> JwtValidator
            = WebApplication1.JwtHelpers.ValidateJwt;

        internal Func<UserData> UserDataFactory
            = () => new UserData(System.Configuration.ConfigurationManager
                                 .ConnectionStrings["DefaultConnection"].ConnectionString);

        // Hook de redirección para tests (si es null, usa Response.Redirect)
        public Action<string, bool> Redirector;

        protected void Page_Load(object sender, EventArgs e)
        {
            var jwtCookie = Request.Cookies["pv_jwt"];
            if (jwtCookie == null)
            {
                RedirectOrResponse("~/Pages/Login.aspx");
                return;
            }

            // Validación del JWT
            var claimsPrincipal = JwtValidator(jwtCookie.Value);

            // (Solo debug)
            foreach (var claim in claimsPrincipal.Claims)
                System.Diagnostics.Debug.WriteLine($"Claim type: {claim.Type}, value: {claim.Value}");

            // Email desde NameIdentifier (o sub si estuviera mapeado así)
            var email = claimsPrincipal.Claims
                .FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                RedirectOrResponse("~/Pages/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                var data = UserDataFactory();
                var user = data.GetByEmail(email);

                if (user == null || !user.Active)
                {
                    RedirectOrResponse("~/Pages/Login.aspx");
                    return;
                }

                lblWelcome.Text = $"Bienvenido, {user.Email}";
                lblRole.Text = $"Rol: {user.Role}";
            }
        }

        private void RedirectOrResponse(string url, bool endResponse = false)
        {
            if (Redirector != null)
            {
                // En pruebas: capturamos la URL sin tocar el pipeline real
                Redirector(url, endResponse);
                return;
            }

            // En ejecución normal
            Response.Redirect(url, false);
            Context?.ApplicationInstance?.CompleteRequest();
        }
    }
}
