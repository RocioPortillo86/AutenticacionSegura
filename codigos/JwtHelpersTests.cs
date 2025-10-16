using System;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using WebApplication1;
using Xunit;

namespace WebApplication1.Tests
{
    public class JwtHelpersTests
    {
        // Helpers para leer claves de App.config del proyecto de tests
        private static string Conf(string key) => ConfigurationManager.AppSettings[key];

        // Builder flexible: permite fijar notBefore (nbf) para evitar IDX12401 en tokens expirados
        private static string BuildToken(
            string secret,
            string issuer,
            string audience,
            int expMinutes,
            string email = "user@test.com",
            string role = "User",
            int? notBeforeMinutes = null
        )
        {
            var now = DateTime.UtcNow;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            DateTime? nbf = null;
            if (notBeforeMinutes.HasValue)
                nbf = now.AddMinutes(notBeforeMinutes.Value);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: nbf,                               // puede ser null
                expires: now.AddMinutes(expMinutes),          // expiración relativa
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static bool HasEmailClaim(ClaimsPrincipal principal, string expectedEmail)
        {
            // Dependiendo de la versión, "sub" puede mapearse a NameIdentifier o quedar como "sub"
            return principal.Claims.Any(c =>
                (c.Type == ClaimTypes.NameIdentifier || c.Type == JwtRegisteredClaimNames.Sub) &&
                string.Equals(c.Value, expectedEmail, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void GenerateJwt_Y_ValidateJwt_DevuelvenPrincipalConClaimsEsperados()
        {
            var issuer = Conf("JwtIssuer");
            var audience = Conf("JwtAudience");
            var email = "ok@test.com";
            var role = "Admin";

            var token = JwtHelpers.GenerateJwt(email, role);
            var principal = JwtHelpers.ValidateJwt(token);

            Assert.NotNull(principal);
            Assert.True(HasEmailClaim(principal, email));
            Assert.Contains(principal.Claims, c => c.Type == ClaimTypes.Role && c.Value == role);
        }

        [Fact]
        public void ValidateJwt_FirmaInvalida_LanzaExcepcion()
        {
            // Token firmado con una clave distinta
            var badSecret = "otra-clave-secreta-distinta-que-debe-fallar";
            var issuer = Conf("JwtIssuer");
            var audience = Conf("JwtAudience");
            var tokenConFirmaMala = BuildToken(badSecret, issuer, audience, expMinutes: 10);

            // En distintas versiones puede lanzar InvalidSignature o SignatureKeyNotFound.
            Assert.ThrowsAny<SecurityTokenException>(() =>
            {
                JwtHelpers.ValidateJwt(tokenConFirmaMala);
            });
        }

        [Fact]
        public void ValidateJwt_AudienceIncorrecto_LanzaExcepcion()
        {
            var secret = Conf("JwtSecret");
            var issuer = Conf("JwtIssuer");
            var wrongAudience = "audience-equivocada";
            var tokenAudMala = BuildToken(secret, issuer, wrongAudience, expMinutes: 10);

            Assert.Throws<SecurityTokenInvalidAudienceException>(() =>
            {
                JwtHelpers.ValidateJwt(tokenAudMala);
            });
        }

        [Fact]
        public void ValidateJwt_IssuerIncorrecto_LanzaExcepcion()
        {
            var secret = Conf("JwtSecret");
            var audience = Conf("JwtAudience");
            var wrongIssuer = "issuer-equivocado";
            var tokenIssuerMalo = BuildToken(secret, wrongIssuer, audience, expMinutes: 10);

            Assert.Throws<SecurityTokenInvalidIssuerException>(() =>
            {
                JwtHelpers.ValidateJwt(tokenIssuerMalo);
            });
        }

        [Fact]
        public void ValidateJwt_TokenExpirado_LanzaExcepcion()
        {
            var secret = Conf("JwtSecret");
            var issuer = Conf("JwtIssuer");
            var audience = Conf("JwtAudience");

            // nbf = ahora -10, exp = ahora -1  (exp > nbf, pero ya expirado)
            var tokenExpirado = BuildToken(
                secret, issuer, audience,
                expMinutes: -1,
                notBeforeMinutes: -10
            );

            Assert.Throws<SecurityTokenExpiredException>(() =>
            {
                JwtHelpers.ValidateJwt(tokenExpirado);
            });
        }
    }
}
