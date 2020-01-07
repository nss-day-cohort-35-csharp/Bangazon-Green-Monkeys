using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Models
{
    public class ProductTypeRevenue
    {
        public int Id { get; set; }
        public string ProductTypeName { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
