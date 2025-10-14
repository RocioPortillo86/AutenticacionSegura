using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace WebApplication1
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            var jwtCookie = HttpContext.Current.Request.Cookies["pv_jwt"];
            if (jwtCookie != null && !string.IsNullOrEmpty(jwtCookie.Value))
            {
                try
                {
                    var principal = JwtHelpers.ValidateJwt(jwtCookie.Value);
                    HttpContext.Current.User = principal;
                }
                catch
                {
                    // Token inválido o expirado: eliminar cookie
                    var expiredCookie = new HttpCookie("pv_jwt", "")
                    {
                        Expires = DateTime.UtcNow.AddDays(-1),
                        HttpOnly = true,
                        Secure = Request.IsSecureConnection,
                        SameSite = SameSiteMode.Strict
                    };
                    HttpContext.Current.Response.Cookies.Add(expiredCookie);
                }
            }
        }
    }
}