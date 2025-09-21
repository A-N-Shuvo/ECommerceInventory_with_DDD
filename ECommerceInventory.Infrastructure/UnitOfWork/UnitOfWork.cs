using ECommerceInventory.Core.Interfaces;
using ECommerceInventory.Infrastructure.Data;
using System.Threading.Tasks;

namespace ECommerceInventory.Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUserRepository _userRepository;

        public UnitOfWork(
            ApplicationDbContext context,
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IUserRepository userRepository)
        {
            _context = context;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _userRepository = userRepository;
        }

        public IProductRepository Products => _productRepository;
        public ICategoryRepository Categories => _categoryRepository;
        public IUserRepository Users => _userRepository;

        public void Dispose()
        {
            _context.Dispose();
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
