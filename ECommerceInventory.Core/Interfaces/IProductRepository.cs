using ECommerceInventory.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceInventory.Core.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        //Task<IEnumerable<Product>> GetProductsWithCategoryAsync();
        IQueryable<Product> Query();
    }
}
