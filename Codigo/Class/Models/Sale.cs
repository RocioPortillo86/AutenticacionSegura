using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Class.Models
{
    public class Sale
    {
        public int Id { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public int CashierUserId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
    }
}