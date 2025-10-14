using WebApplication1.Class.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace WebApplication1.Class.Data
{
    public class UserData
    {
        private readonly string _connectionString;

        public UserData(string connectionString)
        {
            _connectionString = connectionString;
        }

        public User GetByEmail(string email)
        {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand("SELECT Id, Email, PasswordHash, Role, Active FROM dbo.Users WHERE Email = @email AND Active = 1", conn))
                {
                    cmd.Parameters.Add("@email", SqlDbType.NVarChar, 256).Value = email;
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                Id = reader.GetInt32(0),
                                Email = reader.GetString(1),
                                PasswordHash = reader.GetString(2),
                                Role = reader.GetString(3),
                                Active = reader.GetBoolean(4)
                            };
                        }
                    }
                }
                return null;
           
        }
    }
}
