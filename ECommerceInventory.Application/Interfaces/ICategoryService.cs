using ECommerceInventory.Application.DTOs;
using ECommerceInventory.Application.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceInventory.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllAsync();
        Task<CategoryDto?> GetByIdAsync(int id);
        Task<CategoryDto> CreateAsync(CategoryCreateDto dto);
        Task<bool> UpdateAsync(int id, CategoryCreateDto dto);
        Task<DeleteResult> DeleteAsync(int id);
    }

}
