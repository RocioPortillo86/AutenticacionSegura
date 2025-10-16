using System;
using System.IO;
using System;
using System.IO;
using System.Reflection;
using System.Security.Claims;
using System.Web;
using System.Web.UI.WebControls;
using Moq;
using WebApplication1.Class.Data;
using WebApplication1.Class.Models;
using Xunit;

// Alias para no chocar con el control WebForms "Menu"
using MenuPage = WebApplication1.Pages.Menu;

namespace WebApplication1.Tests
{
    public class DefaultPageTests
    {
        // ========= Helpers =========

        private static HttpContext MakeHttpContext(string url = "http://localhost/Pages/Default.aspx")
        {
            var req = new HttpRequest("", url, "");
            var res = new HttpResponse(new StringWriter());
            var ctx = new HttpContext(req, res);

            // Importante para WebForms/Page.ProcessRequest
            HttpContext.Current = ctx;
            return ctx;
        }

        private static ClaimsPrincipal MakePrincipal(string emailClaim = null)
        {
            var id = new ClaimsIdentity("jwt");
            if (!string.IsNullOrEmpty(emailClaim))
                id.AddClaim(new Claim(ClaimTypes.NameIdentifier, emailClaim));
            return new ClaimsPrincipal(id);
        }

        // Refleja y asigna los Label protegidos de la página
        private static void SetProtectedLabel(object page, string fieldName, Label label)
        {
            var f = page.GetType().GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (f == null) throw new InvalidOperationException($"No se encontró el campo '{fieldName}'.");
            f.SetValue(page, label);
        }

        // Mock<UserData> pasando el ctor requerido (connectionString)
        private static Mock<UserData> MakeUserDataMock()
        {
            return new Mock<UserData>("Server=(local);Database=Fake;Trusted_Connection=True;");
        }

        // ========= Tests =========

        [Fact]
        public void SinCookieJwt_RedireccionaALogin()
        {
            var ctx = MakeHttpContext();
            string redirected = null;

            var page = new MenuPage
            {
                // No hay cookie; de todas formas inyectamos para evitar nulos
                JwtValidator = _ => MakePrincipal("usuario@test.com"),
                UserDataFactory = () => MakeUserDataMock().Object,
                Redirector = (url, _) => redirected = url
            };

            page.ProcessRequest(ctx);

            Assert.Equal("~/Pages/Login.aspx", redirected);
        }

        [Fact]
        public void ConCookieJwtPeroSinEmailClaim_RedireccionaALogin()
        {
            var ctx = MakeHttpContext();
            ctx.Request.Cookies.Add(new HttpCookie("pv_jwt", "token-sin-claim"));
            string redirected = null;

            var page = new MenuPage
            {
                JwtValidator = _ => MakePrincipal(emailClaim: null),
                UserDataFactory = () => MakeUserDataMock().Object,
                Redirector = (url, _) => redirected = url
            };

            page.ProcessRequest(ctx);

            Assert.Equal("~/Pages/Login.aspx", redirected);
        }

        [Fact]
        public void ConEmailClaim_PeroUsuarioInexistenteOInactivo_RedireccionaALogin()
        {
            var userData = MakeUserDataMock();
            userData.Setup(u => u.GetByEmail("off@test.com")).Returns((User)null);
            userData.Setup(u => u.GetByEmail("inactive@test.com"))
                    .Returns(new User { Email = "inactive@test.com", Active = false });

            // Usuario no existe
            var ctx1 = MakeHttpContext();
            ctx1.Request.Cookies.Add(new HttpCookie("pv_jwt", "t"));
            string redirected1 = null;

            var page1 = new MenuPage
            {
                JwtValidator = _ => MakePrincipal("off@test.com"),
                UserDataFactory = () => userData.Object,
                Redirector = (url, _) => redirected1 = url
            };

            page1.ProcessRequest(ctx1);
            Assert.Equal("~/Pages/Login.aspx", redirected1);

            // Usuario inactivo
            var ctx2 = MakeHttpContext();
            ctx2.Request.Cookies.Add(new HttpCookie("pv_jwt", "t"));
            string redirected2 = null;

            var page2 = new MenuPage
            {
                JwtValidator = _ => MakePrincipal("inactive@test.com"),
                UserDataFactory = () => userData.Object,
                Redirector = (url, _) => redirected2 = url
            };

            page2.ProcessRequest(ctx2);
            Assert.Equal("~/Pages/Login.aspx", redirected2);
        }

        [Fact]
        public void UsuarioActivo_PueblaLabels_BienvenidoYRol()
        {
            var ctx = MakeHttpContext();
            ctx.Request.Cookies.Add(new HttpCookie("pv_jwt", "token"));

            var activo = new User { Email = "ok@test.com", Active = true, Role = "Admin" };
            var userData = MakeUserDataMock();
            userData.Setup(u => u.GetByEmail("ok@test.com")).Returns(activo);

            // No seteamos Redirector: no debe redirigir
            var page = new MenuPage
            {
                JwtValidator = _ => MakePrincipal("ok@test.com"),
                UserDataFactory = () => userData.Object
            };

            var lblWelcome = new Label();
            var lblRole = new Label();
            SetProtectedLabel(page, "lblWelcome", lblWelcome);
            SetProtectedLabel(page, "lblRole", lblRole);

            page.ProcessRequest(ctx);

            Assert.Equal("Bienvenido, ok@test.com", lblWelcome.Text);
            Assert.Equal("Rol: Admin", lblRole.Text);
        }
    }
}

