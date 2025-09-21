using ECommerceInventory.Application.DTOs;
using ECommerceInventory.Application.Enums;
using ECommerceInventory.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceInventory.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _categoryService.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var cat = await _categoryService.GetByIdAsync(id);
            return cat == null ? NotFound() : Ok(cat);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CategoryCreateDto dto)
        {
            var cat = await _categoryService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = cat.Id }, cat);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, CategoryCreateDto dto)
        {
            var updated = await _categoryService.UpdateAsync(id, dto);
            return updated ? NoContent() : NotFound();
        }


        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _categoryService.DeleteAsync(id);

            return result switch
            {
                DeleteResult.Success => NoContent(),
                DeleteResult.NotFound => NotFound(),
                DeleteResult.Conflict => Conflict(new { message = "Category has linked products. Remove products first." }),
                _ => StatusCode(500, new { message = "Unexpected error" })
            };
        }
    }
}
