using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using WebApplication1.Class.Models;

namespace WebApplication1.Class.Data
{
    public class SalesData
    {
        /// <summary>
        /// Inserta una venta con sus items y descuenta stock de Products.
        /// </summary>
        public void InsertSale(Sale sale, List<SaleItem> items)
        {
            using (var conn = Db.GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Insertar venta
                        using (var cmd = new SqlCommand(@"
                            INSERT INTO Sales (CashierUserId, Subtotal, Tax, Total)
                            OUTPUT INSERTED.Id
                            VALUES (@CashierUserId, @Subtotal, @Tax, @Total);", conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@CashierUserId", sale.CashierUserId);
                            cmd.Parameters.AddWithValue("@Subtotal", sale.Subtotal);
                            cmd.Parameters.AddWithValue("@Tax", sale.Tax);
                            cmd.Parameters.AddWithValue("@Total", sale.Total);
                            sale.Id = (int)cmd.ExecuteScalar();
                        }

                        // Insertar items de la venta
                        foreach (var item in items)
                        {
                            using (var cmd = new SqlCommand(@"
                                INSERT INTO SaleItems (SaleId, ProductId, Quantity, UnitPrice, LineTotal)
                                VALUES (@SaleId, @ProductId, @Quantity, @UnitPrice, @LineTotal);", conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@SaleId", sale.Id);
                                cmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                                cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                                cmd.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                                cmd.Parameters.AddWithValue("@LineTotal", item.LineTotal);
                                cmd.ExecuteNonQuery();
                            }

                            // Descontar stock
                            using (var cmd = new SqlCommand(@"
                                UPDATE Products
                                SET Stock = Stock - @Quantity
                                WHERE Id = @ProductId;", conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                                cmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Obtiene ventas cuyo campo de fecha está en el rango [fromUtc, toUtc).
        /// Cambia el nombre de la columna de fecha si en tu DB se llama distinto (por ejemplo SaleDate).
        /// </summary>
        public static List<Sale> GetByDateRange(DateTime fromUtc, DateTime toUtc)
        {
            var list = new List<Sale>();

            using (var conn = Db.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    SELECT Id, CashierUserId, Subtotal, Tax, Total, CreatedAt
                    FROM Sales
                    WHERE CreatedAt >= @FromUtc AND CreatedAt < @ToUtc
                    ORDER BY CreatedAt ASC;", conn))
                {
                    cmd.Parameters.AddWithValue("@FromUtc", fromUtc);
                    cmd.Parameters.AddWithValue("@ToUtc", toUtc);

                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            var sale = new Sale
                            {
                                Id = (int)r["Id"],
                                CashierUserId = (int)r["CashierUserId"],
                                Subtotal = (decimal)r["Subtotal"],
                                Tax = (decimal)r["Tax"],
                                Total = (decimal)r["Total"]
                            };

                            // Si tu modelo Sale tiene CreatedAt, descomenta esta línea:
                            // sale.CreatedAt = (DateTime)r["CreatedAt"];

                            list.Add(sale);
                        }
                    }
                }
            }

            return list;
        }
    }
}
