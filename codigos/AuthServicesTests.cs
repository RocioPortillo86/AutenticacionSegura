using System;
using Xunit;
using Moq;
using WebApplication1.Class.Services;
using WebApplication1.Class.Models;
using WebApplication1.Class.Data;

namespace WebApplication1.Tests
{
    public class AuthServicesTests
    {
        private const string OkEmail = "ok@test.com";
        private const string OkPassword = "Pwd!234";
        private const string ClientIp = "127.0.0.1";

        // 🔧 SIEMPRE usar este helper para crear Mock<UserData>
        private static Mock<UserData> MakeUserDataMock()
        {
            // connectionString dummy solo para satisfacer el ctor de UserData
            return new Mock<UserData>("Server=(local);Database=Fake;Trusted_Connection=True;");
        }

        private static User MakeUser(string email, string plainPassword, bool active = true)
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(plainPassword);
            return new User
            {
                Email = email,
                Active = active,
                PasswordHash = hash
            };
        }

        // 🔧 Usa SIEMPRE este builder: internamente ya crea el Mock<UserData> con ctor correcto
        private static AuthServices BuildWithUser(User user)
        {
            var userData = MakeUserDataMock();
            userData
                .Setup(u => u.GetByEmail(It.IsAny<string>()))
                .Returns<string>(email =>
                {
                    if (user == null) return null;
                    return string.Equals(email, user.Email, StringComparison.OrdinalIgnoreCase) ? user : null;
                });

            return new AuthServices(userData.Object);
        }

        [Fact]
        public void ValidateUser_CredencialesValidas_RetornaTrueYReseteaIntentos()
        {
            var user = MakeUser(OkEmail, OkPassword, active: true);
            var svc = BuildWithUser(user);

            var ok1 = svc.ValidateUser(OkEmail, OkPassword, ClientIp);
            var ok2 = svc.ValidateUser(OkEmail, OkPassword, ClientIp);

            Assert.True(ok1);
            Assert.True(ok2);
        }

        [Fact]
        public void ValidateUser_ContrasenaIncorrecta_RetornaFalse()
        {
            var user = MakeUser(OkEmail, OkPassword, active: true);
            var svc = BuildWithUser(user);

            var ok = svc.ValidateUser(OkEmail, "Clave-Incorrecta", ClientIp);

            Assert.False(ok);
        }

        [Fact]
        public void ValidateUser_UsuarioInactivo_RetornaFalse()
        {
            var user = MakeUser(OkEmail, OkPassword, active: false);
            var svc = BuildWithUser(user);

            var ok = svc.ValidateUser(OkEmail, OkPassword, ClientIp);

            Assert.False(ok);
        }

        [Fact]
        public void ValidateUser_UsuarioNoExiste_RetornaFalse()
        {
            var svc = BuildWithUser(user: null);

            var ok = svc.ValidateUser("noexiste@test.com", "Algo123!", ClientIp);

            Assert.False(ok);
        }

        [Fact]
        public void ValidateUser_SuperaMaximosIntentos_BloqueaAunqueLaClaveSeaCorrecta()
        {
            var user = MakeUser(OkEmail, OkPassword, active: true);
            var svc = BuildWithUser(user);

            for (int i = 0; i < 5; i++)
                Assert.False(svc.ValidateUser(OkEmail, "mala" + i, ClientIp));

            var okAhora = svc.ValidateUser(OkEmail, OkPassword, ClientIp);
            Assert.False(okAhora);
        }

        [Fact]
        public void ValidateUser_ExcepcionEnRepositorio_RetornaFalse()
        {
            // 👇 ¡AQUÍ TAMBIÉN pasa el connectionString al Mock!
            var userData = MakeUserDataMock();
            userData.Setup(u => u.GetByEmail(It.IsAny<string>()))
                    .Throws(new Exception("Falla de BD"));

            var svc = new AuthServices(userData.Object);

            var ok = svc.ValidateUser(OkEmail, OkPassword, ClientIp);

            Assert.False(ok);
        }

        [Fact]
        public void ValidateUser_RateLimitIndependientePorIp_NoInterfiereConOtraIp()
        {
            var user = MakeUser(OkEmail, OkPassword, active: true);
            var svc = BuildWithUser(user);

            for (int i = 0; i < 5; i++)
                Assert.False(svc.ValidateUser(OkEmail, "mala", "10.0.0.1"));

            var okDesdeOtraIp = svc.ValidateUser(OkEmail, OkPassword, "10.0.0.2");
            Assert.True(okDesdeOtraIp);
        }


    }
}

