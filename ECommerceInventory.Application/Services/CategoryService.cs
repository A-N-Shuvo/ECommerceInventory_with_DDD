using AutoMapper;
using ECommerceInventory.Application.DTOs;
using ECommerceInventory.Application.Enums;
using ECommerceInventory.Application.Interfaces;
using ECommerceInventory.Core.Entities;
using ECommerceInventory.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceInventory.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public CategoryService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllAsync()
        {
            var cats = await _uow.Categories.GetAllAsync();
            return _mapper.Map<IEnumerable<CategoryDto>>(cats);
        }

        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            var cat = await _uow.Categories.GetByIdAsync(id);
            if (cat == null) return null;
            return _mapper.Map<CategoryDto>(cat);
        }

        public async Task<CategoryDto> CreateAsync(CategoryCreateDto dto)
        {
            var entity = _mapper.Map<Category>(dto);
            await _uow.Categories.AddAsync(entity);
            await _uow.CompleteAsync();
            return _mapper.Map<CategoryDto>(entity);
        }

        public async Task<bool> UpdateAsync(int id, CategoryCreateDto dto)
        {
            var cat = await _uow.Categories.GetByIdAsync(id);
            if (cat == null) return false;
            cat.Name = dto.Name;
            _uow.Categories.Update(cat);
            await _uow.CompleteAsync();
            return true;
        }

        // Updated DeleteAsync method
        //public async Task<DeleteResult> DeleteAsync(int id)
        //{
        //    // use repository method that includes products
        //    var cat = await _uow.Categories.GetCategoryWithProductsAsync(id);
        //    if (cat == null) return DeleteResult.NotFound;

        //    if (cat.Products != null && cat.Products.Any())
        //        return DeleteResult.Conflict;

        //    _uow.Categories.Remove(cat);
        //    await _uow.CompleteAsync();
        //    return DeleteResult.Success;
        //}

        public async Task<DeleteResult> DeleteAsync(int id)
        {
            // Products সহ ক্যাটেগরি লোড
            var cat = await _uow.Categories.GetCategoryWithProductsAsync(id);
            if (cat == null)
                return DeleteResult.NotFound;

            if (cat.Products.Any())
                return DeleteResult.Conflict;

            _uow.Categories.Remove(cat);
            await _uow.CompleteAsync();
            return DeleteResult.Success;
        }
    }
}
