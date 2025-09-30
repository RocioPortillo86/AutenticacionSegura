using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using WebApplication1.Class.Models;

namespace WebApplication1.Class.Data
{
    public class ProductData
    {
        public static List<Product> GetAll()
        {
            var products = new List<Product>();
            using (var conn = Db.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT Id, Sku, Name, Price, Stock, Active FROM Products", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        products.Add(Map(reader));
                }
            }
            return products;
        }

        public static Product GetById(int id)
        {
            using (var conn = Db.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT Id, Sku, Name, Price, Stock, Active FROM Products WHERE Id = @id", conn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            return Map(reader);
                        return null;
                    }
                }
            }
        }

        // Útil para tu cálculo de totales: trae varios productos de una sola vez
        public static List<Product> GetByIds(IEnumerable<int> ids)
        {
            var list = ids.Distinct().ToList();
            if (list.Count == 0) return new List<Product>();

            // Construye parámetros seguros para IN (@p0,@p1,...)
            var paramNames = list.Select((_, i) => "@p" + i).ToList();

            var sql = @"SELECT Id, Sku, Name, Price, Stock, Active 
            FROM Products 
            WHERE Id IN (" + string.Join(",", paramNames) + ")";
            var products = new List<Product>();
            using (var conn = Db.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    for (int i = 0; i < list.Count; i++)
                        cmd.Parameters.AddWithValue(paramNames[i], list[i]);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            products.Add(Map(reader));
                    }
                }
            }
            return products;
        }

        private static Product Map(IDataRecord r)
        {
            return new Product
            {
                Id = r.GetInt32(r.GetOrdinal("Id")),
                Sku = r["Sku"].ToString() ?? "",
                Name = r["Name"].ToString() ?? "",
                Price = r.GetDecimal(r.GetOrdinal("Price")),
                Stock = r.GetInt32(r.GetOrdinal("Stock")),
                Active = r.GetBoolean(r.GetOrdinal("Active"))
            };
        }
    }
}