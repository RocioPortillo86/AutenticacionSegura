using System;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Moq;
using WebApplication1.Class.Data;
using WebApplication1.Class.Models;
using WebApplication1.Class.Services;
using Xunit;

// ===== ALIASES IMPORTANTES =====
// Control de WebForms <asp:Login ... />
using WebLoginControl = System.Web.UI.WebControls.Login;
// Página WebForms WebApplication1.Pages.Login (code-behind de Login.aspx)
using LoginPageClass = WebApplication1.Pages.Login;

namespace WebApplication1.Tests
{
    public class LoginPageTests
    {
        // ===================== Helpers comunes =====================

        private static HttpContext MakeHttpContext(bool https)
        {
            var scheme = https ? "https" : "http";
            var url = scheme + "://localhost/Pages/Login.aspx";

            var request = new HttpRequest("", url, "");
            var response = new HttpResponse(new StringWriter());
            var ctx = new HttpContext(request, response);

            // Contexto global: Page usa HttpContext.Current internamente
            HttpContext.Current = ctx;
            return ctx;
        }

        private static void SetProtectedField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null)
                throw new InvalidOperationException("No se encontró el campo: " + fieldName);
            field.SetValue(target, value);
        }

        private static void InvokeLoginHandler(LoginPageClass page, WebLoginControl login)
        {
            // El control debe estar en el árbol de la página
            page.Controls.Add(login);

            // Asigna el field protegido 'LoginForm' que usa el code-behind
            SetProtectedField(page, "LoginForm", login);

            // Invoca el handler privado
            var method = typeof(LoginPageClass).GetMethod("LoginForm_Authenticate",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
                throw new InvalidOperationException("No se encontró LoginForm_Authenticate");

            var args = new AuthenticateEventArgs(false);
            method.Invoke(page, new object[] { login, args });
        }

        private static WebLoginControl MakeLogin()
        {
            // No agregamos EnsureChildControls ni asignamos Password (read-only)
            return new WebLoginControl
            {
                ID = "LoginForm",
                DisplayRememberMe = false,
                FailureText = string.Empty
            };
        }

        private static void SetLoginValues(WebLoginControl login, string user, string pwd)
        {
            // Crea los TextBox internos si no existen (sin EnsureChildControls)
            var userTb = login.FindControl("UserName") as TextBox;
            if (userTb == null)
            {
                userTb = new TextBox { ID = "UserName" };
                login.Controls.Add(userTb);
            }

            var pwdTb = login.FindControl("Password") as TextBox;
            if (pwdTb == null)
            {
                pwdTb = new TextBox { ID = "Password", TextMode = TextBoxMode.Password };
                login.Controls.Add(pwdTb);
            }

            userTb.Text = user ?? string.Empty;
            pwdTb.Text = pwd ?? string.Empty;

            // Esta propiedad sí tiene setter
            login.UserName = user ?? string.Empty;
            // login.Password es de solo lectura → NO se asigna
        }

        private static Mock<UserData> MakeUserDataMock()
        {
            return new Mock<UserData>("Server=(local);Database=Fake;Trusted_Connection=True;");
        }

        private static Mock<AuthServices> MakeAuthMock(UserData ud)
        {
            // No se hace Setup de ValidateUser (no es virtual)
            return new Mock<AuthServices>(ud);
        }

        // ===================== Tests =====================

        [Fact]
        public void CredencialesVacias_MuestraMensajeError_SinCookie_SinRedirect()
        {
            MakeHttpContext(false);
            var page = new LoginPageClass();

            page.UserDataFactory = () => MakeUserDataMock().Object;
            page.AuthServiceFactory = ud => MakeAuthMock(ud).Object;

            string redirected = null;
            page.FormsAuthRedirect = (u, p) => redirected = u;

            var login = MakeLogin();
            SetLoginValues(login, "", ""); // ambos vacíos

            InvokeLoginHandler(page, login);

            Assert.Equal("Credenciales inválidas.", login.FailureText);
            Assert.Null(redirected);
            Assert.Equal(0, HttpContext.Current.Response.Cookies.Count);
        }

        [Fact]
        public void CredencialesInvalidas_SetFailureText_SinCookie_SinRedirect()
        {
            MakeHttpContext(false);

            var ud = MakeUserDataMock();

            var page = new LoginPageClass();
            page.UserDataFactory = () => ud.Object;
            page.AuthServiceFactory = _ => MakeAuthMock(ud.Object).Object;

            // Forzamos resultado inválido vía gancho
            page.ValidateUserFunc = (email, pwd, ip) => false;

            string redirected = null;
            page.FormsAuthRedirect = (u, p) => redirected = u;

            var login = MakeLogin();
            SetLoginValues(login, "u@test.com", "badpwd");

            InvokeLoginHandler(page, login);

            Assert.Equal("Credenciales inválidas.", login.FailureText);
            Assert.Null(redirected);
            Assert.Equal(0, HttpContext.Current.Response.Cookies.Count);
        }

        [Fact]
        public void CredencialesValidas_GeneraCookieJwt_Y_Redirecciona()
        {
            MakeHttpContext(false);

            var ud = MakeUserDataMock();

            // Usuario activo retornado por UserData
            var user = new User { Email = "ok@test.com", Role = "Admin", Active = true };
            ud.Setup(u => u.GetByEmail("ok@test.com")).Returns(user);

            var page = new LoginPageClass();
            page.UserDataFactory = () => ud.Object;
            page.AuthServiceFactory = _ => MakeAuthMock(ud.Object).Object;

            // Validación OK vía gancho
            page.ValidateUserFunc = (email, pwd, ip) => (email == "ok@test.com" && pwd == "123");

            // JWT controlado
            page.JwtGenerator = (e, r) => "jwt_token_test";

            string redirected = null;
            page.FormsAuthRedirect = (u, p) => redirected = u;

            var login = MakeLogin();
            SetLoginValues(login, "ok@test.com", "123");

            InvokeLoginHandler(page, login);

            var cookie = HttpContext.Current.Response.Cookies["pv_jwt"];
            Assert.NotNull(cookie);
            Assert.Equal("jwt_token_test", cookie.Value);
            Assert.False(cookie.Secure);
            Assert.Equal("ok@test.com", redirected);
        }

        [Fact]
        public void CredencialesValidas_EnHttps_CookieSecureTrue()
        {
            MakeHttpContext(true);

            var ud = MakeUserDataMock();

            var user = new User { Email = "ok@test.com", Role = "Admin", Active = true };
            ud.Setup(u => u.GetByEmail("ok@test.com")).Returns(user);

            var page = new LoginPageClass();
            page.UserDataFactory = () => ud.Object;
            page.AuthServiceFactory = _ => MakeAuthMock(ud.Object).Object;

            page.ValidateUserFunc = (email, pwd, ip) => (email == "ok@test.com" && pwd == "123");
            page.JwtGenerator = (e, r) => "jwt_https";

            string redirected = null;
            page.FormsAuthRedirect = (u, p) => redirected = u;

            var login = MakeLogin();
            SetLoginValues(login, "ok@test.com", "123");

            InvokeLoginHandler(page, login);

            var cookie = HttpContext.Current.Response.Cookies["pv_jwt"];
            Assert.NotNull(cookie);
            Assert.Equal("jwt_https", cookie.Value);
            Assert.True(cookie.Secure); // por URL https
            Assert.Equal("ok@test.com", redirected);
        }

        [Fact]
        public void UsuarioInactivo_NoEmiteJwt_Y_MuestraError()
        {
            MakeHttpContext(false);

            var ud = MakeUserDataMock();

            // Usuario inactivo
            var user = new User { Email = "off@test.com", Role = "User", Active = false };
            ud.Setup(u => u.GetByEmail("off@test.com")).Returns(user);

            var page = new LoginPageClass();
            page.UserDataFactory = () => ud.Object;
            page.AuthServiceFactory = _ => MakeAuthMock(ud.Object).Object;

            // Validación OK pero usuario inactivo ⇒ no cookie, no redirect
            page.ValidateUserFunc = (email, pwd, ip) => (email == "off@test.com" && pwd == "123");
            page.JwtGenerator = (e, r) => "jwt_should_not_emit";

            string redirected = null;
            page.FormsAuthRedirect = (u, p) => redirected = u;

            var login = MakeLogin();
            SetLoginValues(login, "off@test.com", "123");

            InvokeLoginHandler(page, login);

            Assert.Equal(0, HttpContext.Current.Response.Cookies.Count);
            Assert.Null(redirected);
            Assert.Equal("Credenciales inválidas.", login.FailureText);
        }
    }
}
