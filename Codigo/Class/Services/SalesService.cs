using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication1.Class.Models;
using WebApplication1.Class.Data;


namespace WebApplication1.Class.Services
{
    public class SalesService
    {
        public int CreateSale(int cashierUserId, IEnumerable<(int productId, int qty)> items)
        {
            ProductData productData = new ProductData();
            SalesData SalesData = new SalesData();
            var sale = new Sale
            {
                CashierUserId = cashierUserId,
                Subtotal = items.Sum(i => i.qty * ProductData.GetById(i.productId).Price),
                Tax = items.Sum(i => i.qty * ProductData.GetById(i.productId).Price) * 0.16m,
                Total = items.Sum(i => i.qty * ProductData.GetById(i.productId).Price) * 1.16m
            };

            SalesData.InsertSale(sale, items.Select(i => new SaleItem
            {
                ProductId = i.productId,
                Quantity = i.qty,
                UnitPrice = ProductData.GetById(i.productId).Price,
                LineTotal = i.qty * ProductData.GetById(i.productId).Price
            }).ToList());

            return sale.Id;
        }
    }
}