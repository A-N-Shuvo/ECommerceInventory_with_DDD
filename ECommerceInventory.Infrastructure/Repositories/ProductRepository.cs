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
    public class ProductRepository : RepositoryBase<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _context;
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        // existing methods...

        public IQueryable<Product> Query()
        {
            // include Category so consumers get Category navigation populated
            return _context.Products.Include(p => p.Category).AsQueryable();
        }
    }
}
