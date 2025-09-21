using ECommerceInventory.Application.DTOs;
using ECommerceInventory.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceInventory.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Get all products with optional filters, search, sorting, pagination.
        /// </summary>
        /// <param name="qp">Query parameters for filtering, search, pagination</param>
        /// <returns>List of products with total count in header</returns>`
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ProductQueryParameters qp)
        {
            var (items, total) = await _productService.GetAllAsync(qp);
            var response = new
            {
                Total = total,
                Page = qp.Page,
                Limit = qp.Limit,
                Items = items
            };
            return Ok(response);
        }

        /// <summary>
        /// Search products by keyword (name/description).
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery(Name = "search")] string search, [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var qp = new ProductQueryParameters
            {
                Search = search,
                Page = Math.Max(page, 1),
                Limit = Math.Max(limit, 1)
            };

            var (items, total) = await _productService.GetAllAsync(qp);
            return Ok(new { Total = total, Page = qp.Page, Limit = qp.Limit, Items = items });
        }

        /// <summary>
        /// Get a product by ID
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Product details</returns>
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            return product == null ? NotFound() : Ok(product);
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromForm] ProductCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = await _productService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromForm] ProductUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _productService.UpdateAsync(id, dto);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _productService.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}
