using ECommerceInventory.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceInventory.Application.Interfaces
{
    public interface IProductService
    {
        Task<(IEnumerable<ProductDto> Items, int Total)> GetAllAsync(ProductQueryParameters qp);
        Task<ProductDto?> GetByIdAsync(int id);
        Task<ProductDto> CreateAsync(ProductCreateDto dto);
        Task<bool> UpdateAsync(int id, ProductUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
