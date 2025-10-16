using System;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebApplication1.Class.Data;
using WebApplication1.Class.Services;
using WebApplication1; // para JwtHelpers

namespace WebApplication1.Pages
{
    public partial class Login : Page
    {
        // ========= Ganchos (sobrescribibles en tests) =========

        internal Func<UserData> UserDataFactory =
            () => new UserData(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);

        internal Func<UserData, AuthServices> AuthServiceFactory = ud => new AuthServices(ud);

        internal Func<string, string, string> JwtGenerator =
            (email, role) => JwtHelpers.GenerateJwt(email, role);

        internal Action<string, bool> FormsAuthRedirect =
            (user, isPersistent) => FormsAuthentication.RedirectFromLoginPage(user, isPersistent);

        // Gancho para validar credenciales (evita moquear métodos no virtuales)
        internal Func<string, string, string, bool> ValidateUserFunc = null;

        // =====================================================

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        private string GetChildText(string id)
        {
            var tb = LoginForm?.FindControl(id) as TextBox;
            return tb?.Text ?? string.Empty;
        }

        protected void LoginForm_Authenticate(object sender, AuthenticateEventArgs e)
        {
            // 1) Entradas (con fallback a los TextBox internos cuando no hay postback real)
            string email = LoginForm.UserName ?? string.Empty;
            string password = LoginForm.Password ?? string.Empty;

            if (string.IsNullOrEmpty(email))
                email = GetChildText("UserName");
            if (string.IsNullOrEmpty(password))
                password = GetChildText("Password");

            email = email.Trim();
            password = password.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                e.Authenticated = false;
                LoginForm.FailureText = "Credenciales inválidas.";
                return;
            }

            // 2) Obtener Request/Response desde HttpContext.Current (seguro en tests unitarios)
            var ctx = HttpContext.Current;
            var req = ctx?.Request;
            var resp = ctx?.Response;

            string clientIp = null;
            if (req != null)
            {
                clientIp = req.UserHostAddress;
                if (string.IsNullOrEmpty(clientIp) && req.ServerVariables != null)
                    clientIp = req.ServerVariables["REMOTE_ADDR"];
            }

            // 3) Servicios
            var userData = UserDataFactory();
            var auth = AuthServiceFactory(userData);

            bool isValid = false;
            try
            {
                if (ValidateUserFunc != null)
                    isValid = ValidateUserFunc(email, password, clientIp);
                else
                    isValid = auth.ValidateUser(email, password, clientIp);
            }
            catch
            {
                isValid = false;
            }

            if (!isValid)
            {
                e.Authenticated = false;
                LoginForm.FailureText = "Credenciales inválidas.";
                return;
            }

            // 4) Reglas de negocio: usuario activo
            var user = userData.GetByEmail(email);
            if (user == null || !user.Active)
            {
                e.Authenticated = false;
                LoginForm.FailureText = "Credenciales inválidas.";
                return;
            }

            // 5) Emitir JWT en cookie HttpOnly
            string jwt = JwtGenerator(user.Email, user.Role);

            // *** CORRECCIÓN DEFINITIVA: detectar HTTPS correctamente ***
            bool isHttps = false;

            if (req != null)
            {
                // 1️⃣ Intenta con Request.IsSecureConnection
                isHttps = req.IsSecureConnection;

                // 2️⃣ Fallback: revisa el esquema de la URL
                if (!isHttps && req.Url != null)
                    isHttps = string.Equals(req.Url.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);

                // 3️⃣ Último recurso: header X-Forwarded-Proto (usado por proxies o tests)
                if (!isHttps && !string.IsNullOrEmpty(req.Headers["X-Forwarded-Proto"]))
                    isHttps = req.Headers["X-Forwarded-Proto"].Equals("https", StringComparison.OrdinalIgnoreCase);
            }

            var cookie = new HttpCookie("pv_jwt", jwt)
            {
                HttpOnly = true,
                Secure = isHttps,                 // <- ya se marcará true en tests con https
                SameSite = SameSiteMode.Strict
            };

            int minutes;
            if (!int.TryParse(ConfigurationManager.AppSettings["JwtExpiryMinutes"], out minutes))
                minutes = 20;
            cookie.Expires = DateTime.UtcNow.AddMinutes(minutes);

            // Usa Response desde HttpContext.Current
            resp?.Cookies.Add(cookie);

            // 6) Señalar autenticación correcta y redirigir
            e.Authenticated = true;
            FormsAuthRedirect(email, false);
        }
    }
}

