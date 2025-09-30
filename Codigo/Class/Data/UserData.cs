using WebApplication1.Class.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace WebApplication1.Class.Data
{
    public class UserData
    {
        public static List<User> GetAll()
        {
            var users = new List<User>();

            using (var conn = Db.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT Id, Email, PasswordHash, Role, Active FROM Users ORDER BY Id DESC", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new User
                        {
                            Id = (int)reader["Id"],
                            Email = reader["Email"].ToString(),
                            PasswordHash = reader["PasswordHash"].ToString(),
                            Role = reader["Role"].ToString(),
                            Active = (bool)reader["Active"]
                        });
                    }
                }
            }
            return users;
        }

        public User GetById(int id)
        {
            using (var conn = Db.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT Id, Email, PasswordHash, Role, Active FROM Users WHERE Id = @Id", conn))
                {
                    cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                Id = (int)reader["Id"],
                                Email = reader["Email"].ToString(),
                                PasswordHash = reader["PasswordHash"].ToString(),
                                Role = reader["Role"].ToString(),
                                Active = (bool)reader["Active"]
                            };
                        }
                    }
                }
            }
            return null;
        }

        public User GetByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;

            using (var conn = Db.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    SELECT TOP 1 Id, Email, PasswordHash, Role, Active
                    FROM Users
                    WHERE Email = @Email;", conn))
                {
                    cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = email.Trim();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                Id = (int)reader["Id"],
                                Email = reader["Email"].ToString(),
                                PasswordHash = reader["PasswordHash"].ToString(),
                                Role = reader["Role"].ToString(),
                                Active = (bool)reader["Active"]
                            };
                        }
                    }
                }
            }
            return null;
        }

        // ---------- NUEVOS MÉTODOS ----------

        /// <summary>
        /// Verifica si ya existe un usuario con el mismo email.
        /// </summary>
        public bool EmailExists(string email, int? excludeId = null)
        {
            using (var conn = Db.GetConnection())
            {
                conn.Open();
                var sql = @"SELECT COUNT(1) 
                            FROM Users 
                            WHERE Email = @Email " + (excludeId.HasValue ? "AND Id <> @ExcludeId" : "");
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = email.Trim();
                    if (excludeId.HasValue)
                        cmd.Parameters.Add("@ExcludeId", SqlDbType.Int).Value = excludeId.Value;

                    var count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        /// <summary>
        /// Inserta un usuario y devuelve el Id generado.
        /// </summary>
        public int Insert(User u)
        {
            if (u == null) throw new ArgumentNullException(nameof(u));
            if (string.IsNullOrWhiteSpace(u.Email)) throw new ArgumentException("Email requerido.");
            if (string.IsNullOrWhiteSpace(u.PasswordHash)) throw new ArgumentException("PasswordHash requerido.");
            if (EmailExists(u.Email)) throw new InvalidOperationException("Ya existe un usuario con ese email.");

            using (var conn = Db.GetConnection())
            {
                conn.Open();
                var sql = @"INSERT INTO Users (Email, PasswordHash, Role, Active)
                            OUTPUT INSERTED.Id
                            VALUES (@Email, @PasswordHash, @Role, @Active);";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = u.Email.Trim();
                    cmd.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 255).Value = u.PasswordHash;
                    cmd.Parameters.Add("@Role", SqlDbType.NVarChar, 50).Value = u.Role ?? "";
                    cmd.Parameters.Add("@Active", SqlDbType.Bit).Value = u.Active;

                    try
                    {
                        var newId = (int)cmd.ExecuteScalar();
                        u.Id = newId;
                        return newId;
                    }
                    catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
                    {
                        // Violación de índice único (Email)
                        throw new InvalidOperationException("El email ya está registrado.", ex);
                    }
                }
            }
        }

        /// <summary>
        /// Actualiza el usuario. 
        /// Si PasswordHash es null o vacío, el password no se modifica.
        /// </summary>
        public void Update(User u)
        {
            if (u == null) throw new ArgumentNullException(nameof(u));
            if (u.Id <= 0) throw new ArgumentException("Id inválido.");
            if (string.IsNullOrWhiteSpace(u.Email)) throw new ArgumentException("Email requerido.");
            if (EmailExists(u.Email, excludeId: u.Id)) throw new InvalidOperationException("Ya existe un usuario con ese email.");

            using (var conn = Db.GetConnection())
            {
                conn.Open();

                // Armamos el UPDATE dinámico para no tocar el password si no viene
                var updateSql = @"UPDATE Users
                                  SET Email = @Email,
                                      Role = @Role,
                                      Active = @Active{0}
                                  WHERE Id = @Id;";

                var includePwd = !string.IsNullOrWhiteSpace(u.PasswordHash);
                var sql = string.Format(updateSql, includePwd ? ", PasswordHash = @PasswordHash" : "");

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = u.Email.Trim();
                    cmd.Parameters.Add("@Role", SqlDbType.NVarChar, 50).Value = u.Role ?? "";
                    cmd.Parameters.Add("@Active", SqlDbType.Bit).Value = u.Active;
                    cmd.Parameters.Add("@Id", SqlDbType.Int).Value = u.Id;

                    if (includePwd)
                        cmd.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 255).Value = u.PasswordHash;

                    try
                    {
                        var rows = cmd.ExecuteNonQuery();
                        if (rows == 0) throw new InvalidOperationException("No se encontró el usuario a actualizar.");
                    }
                    catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
                    {
                        throw new InvalidOperationException("El email ya está registrado.", ex);
                    }
                }
            }
        }

        /// <summary>
        /// Elimina físicamente. Si prefieres baja lógica, cambia por UPDATE Active = 0.
        /// </summary>
        public void Delete(int id)
        {
            using (var conn = Db.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("DELETE FROM Users WHERE Id = @Id;", conn))
                {
                    cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
