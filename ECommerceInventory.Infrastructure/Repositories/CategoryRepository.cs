using ECommerceInventory.Core.Entities;
using ECommerceInventory.Core.Interfaces;
using ECommerceInventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceInventory.Infrastructure.Repositories
{
    public class CategoryRepository : RepositoryBase<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Category?> GetCategoryWithProductsAsync(int categoryId)
        {
            return await _context.Categories
                                 .Include(c => c.Products)
                                 .FirstOrDefaultAsync(c => c.Id == categoryId);
        }
    }
}
