using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using WebApplication1.Class.Data;
using WebApplication1.Class.Models;
using BCrypt.Net;
using System.Collections.Concurrent;

namespace WebApplication1.Class.Services
{
    public class AuthServices
    {

        // Key: email+ip, Value: (failCount, firstFailUtc)
        private static readonly ConcurrentDictionary<string, (int failCount, DateTime firstFailUtc)> _rateLimit = new ConcurrentDictionary<string, (int, DateTime)>();
        private const int MaxAttempts = 5;
        private static readonly TimeSpan Window = TimeSpan.FromMinutes(15);

        private readonly UserData _userData;

        public AuthServices(UserData userData)
        {
            _userData = userData;
        }

        public bool ValidateUser(string email, string password, string clientIp)
        {
            string key = $"{email}:{clientIp}";
            var now = DateTime.UtcNow;

            // Rate limiting: bloquea si excede intentos en ventana
            if (_rateLimit.TryGetValue(key, out var entry))
            {
                if (entry.failCount >= MaxAttempts && now - entry.firstFailUtc < Window)
                    return false; // Bloqueo temporal
                if (now - entry.firstFailUtc >= Window)
                    _rateLimit.TryUpdate(key, (0, now), entry);
            }

            try
            {
                var user = _userData.GetByEmail(email);
                if (user == null || !user.Active)
                {
                    RegisterAttempt(key, false, now);
                    return false;
                }

                if (BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    _rateLimit.TryRemove(key, out _); // Reset en éxito
                    return true;
                }
                else
                {
                    RegisterAttempt(key, false, now);
                    return false;
                }
            }
            catch
            {
                RegisterAttempt(key, false, DateTime.UtcNow);
                return false;
            }
        }

        private void RegisterAttempt(string key, bool success, DateTime now)
        {
            if (!success)
            {
                _rateLimit.AddOrUpdate(key,
                    (1, now),
                    (k, v) =>
                    {
                        if (now - v.firstFailUtc >= Window)
                            return (1, now);
                        return (v.failCount + 1, v.firstFailUtc);
                    });
            }
        }
        
    }
}