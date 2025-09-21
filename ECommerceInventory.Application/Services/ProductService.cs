using AutoMapper;
using ECommerceInventory.Application.DTOs;
using ECommerceInventory.Application.Interfaces;
using ECommerceInventory.Core.Entities;
using ECommerceInventory.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerceInventory.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        private readonly MemoryCacheEntryOptions _cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            SlidingExpiration = TimeSpan.FromMinutes(2)
        };

        private const int MaxPageSize = 100;

        public ProductService(IUnitOfWork uow, IMapper mapper, IMemoryCache cache)
        {
            _uow = uow;
            _mapper = mapper;
            _cache = cache;
        }

        // existing GetAllAsync & GetByIdAsync unchanged (kept from your earlier implementation)
        public async Task<(IEnumerable<ProductDto> Items, int Total)> GetAllAsync(ProductQueryParameters qp)
        {
            string cacheKey = $"products_{qp.CategoryId}_{qp.MinPrice}_{qp.MaxPrice}_{qp.Search}_{qp.Page}_{qp.Limit}_{qp.SortBy}_{qp.Desc}";

            if (_cache.TryGetValue(cacheKey, out (IEnumerable<ProductDto> Items, int Total) cachedResult))
                return cachedResult;

            var query = _uow.Products.Query().AsQueryable();

            if (qp.CategoryId.HasValue && qp.CategoryId.Value > 0)
                query = query.Where(p => p.CategoryId == qp.CategoryId.Value);

            if (qp.MinPrice.HasValue)
                query = query.Where(p => p.Price >= qp.MinPrice.Value);

            if (qp.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= qp.MaxPrice.Value);

            if (!string.IsNullOrWhiteSpace(qp.Search))
            {
                var q = qp.Search.Trim();
                query = query.Where(p =>
                    EF.Functions.Like(p.Name, $"%{q}%") ||
                    EF.Functions.Like(p.Description, $"%{q}%")
                );
            }

            var total = await query.CountAsync();

            query = !string.IsNullOrWhiteSpace(qp.SortBy)
                ? qp.SortBy.ToLower() switch
                {
                    "name" => qp.Desc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                    "price" => qp.Desc ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                    _ => query.OrderBy(p => p.Id)
                }
                : query.OrderBy(p => p.Id);

            int limit = Math.Min(Math.Max(qp.Limit, 1), MaxPageSize);
            int skip = (Math.Max(qp.Page, 1) - 1) * limit;

            var items = await query
                .Include(p => p.Category)
                .Skip(skip)
                .Take(limit)
                .ToListAsync();

            var mappedItems = _mapper.Map<IEnumerable<ProductDto>>(items);

            _cache.Set(cacheKey, (mappedItems, total), _cacheOptions);

            return (mappedItems, total);
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var product = await _uow.Products.Query()
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            return product == null ? null : _mapper.Map<ProductDto>(product);
        }

        // helper to save image and return relative url (or null)
        private async Task<string?> SaveImageAsync(IFormFile? imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;

            // Optional: validate file extension and size here

            var wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(wwwRootPath, "uploads");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            // Return relative path that can be served via static files middleware
            return $"/uploads/{fileName}";
        }

        public async Task<ProductDto> CreateAsync(ProductCreateDto dto)
        {
            var entity = _mapper.Map<Product>(dto);

            // handle image
            if (dto.Image != null)
            {
                entity.ImageUrl = await SaveImageAsync(dto.Image);
            }

            await _uow.Products.AddAsync(entity);
            await _uow.CompleteAsync();

            var created = await _uow.Products.Query()
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == entity.Id);

            return _mapper.Map<ProductDto>(created!);
        }

        public async Task<bool> UpdateAsync(int id, ProductUpdateDto dto)
        {
            if (dto.Price < 0 || dto.Stock < 0) return false;

            var product = await _uow.Products.GetByIdAsync(id);
            if (product == null) return false;

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.Stock = dto.Stock;
            product.CategoryId = dto.CategoryId;

            // if new image provided, save and replace ImageUrl (optionally delete old file)
            if (dto.Image != null)
            {
                var newUrl = await SaveImageAsync(dto.Image);
                if (!string.IsNullOrWhiteSpace(newUrl))
                {
                    // Optionally delete previous file from disk:
                    if (!string.IsNullOrWhiteSpace(product.ImageUrl) && product.ImageUrl.StartsWith("/uploads/"))
                    {
                        try
                        {
                            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                            if (File.Exists(oldPath))
                                File.Delete(oldPath);
                        }
                        catch
                        {
                            // ignore deletion errors (or log)
                        }
                    }

                    product.ImageUrl = newUrl;
                }
            }

            _uow.Products.Update(product);
            await _uow.CompleteAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _uow.Products.GetByIdAsync(id);
            if (product == null) return false;

            // Optionally delete image file
            if (!string.IsNullOrWhiteSpace(product.ImageUrl) && product.ImageUrl.StartsWith("/uploads/"))
            {
                try
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (File.Exists(oldPath))
                        File.Delete(oldPath);
                }
                catch
                {
                    // ignore
                }
            }

            _uow.Products.Remove(product);
            await _uow.CompleteAsync();
            return true;
        }
    }
}
