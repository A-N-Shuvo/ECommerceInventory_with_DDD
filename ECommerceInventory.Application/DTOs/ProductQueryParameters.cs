using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ECommerceInventory.Application.DTOs
{
    public class ProductQueryParameters
    {
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        [JsonPropertyName("search")]
        public string? Search { get; set; } // search keyword
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;

        // 🔽 Sorting এর জন্য নতুন ফিল্ড
        public string? SortBy { get; set; }  // উদাহরণ: "Name", "Price"
        public bool Desc { get; set; } = false;
    }
}
