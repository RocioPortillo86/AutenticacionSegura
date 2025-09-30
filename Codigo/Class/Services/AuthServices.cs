using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using WebApplication1.Class.Data;
namespace WebApplication1.Class.Services
{
    public class AuthService
    {
        public bool Login(string email, string password, HttpSessionState session)
        {


            UserData data = new UserData();
            var user = data.GetByEmail(email);
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                session["uid"] = user.Id;
                session["role"] = user.Role;
                return true;
            }
            return false;
        }

        public void Logout(HttpSessionState session)
        {
            session.Clear();
        }
    }
}