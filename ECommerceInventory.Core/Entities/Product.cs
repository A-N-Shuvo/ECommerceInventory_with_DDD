using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace ECommerceInventory.Core.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Precision(18, 2)] 
        public decimal Price { get; set; }
        public int Stock { get; set; }

        // Relationship
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public string? ImageUrl { get; set; }
    }
}
