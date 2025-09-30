using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Class.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Sku { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool Active { get; set; }
    }
}