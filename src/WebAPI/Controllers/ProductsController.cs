using DataLayer.DTOs;
using DataLayer.Intarfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        [HttpGet("article/{article}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductByArticle(string article)
        {
            var product = await _productService.GetProductByArticleAsync(article);

            if (product == null)
                return NotFound($"Товар с артикулом '{article}' не найден");

            return Ok(product);
        }

        [HttpPost]
        [Authorize(Roles = "Администратор, Менеджер")]
        public async Task<IActionResult> AddProduct([FromBody] ProductCreateDto productDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _productService.AddProductAsync(productDto);
                return Ok(new { Message = "Товар успешно добавлен" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Администратор, Менеджер")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductCreateDto productDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _productService.UpdateProductAsync(id, productDto);
                return Ok(new { Message = "Товар успешно обновлен" });
            }
            catch (ArgumentException ex)
            {
                if (ex.Message.Contains("не найден"))
                    return NotFound(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Администратор, Менеджер")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
                return Ok(new { Message = "Товар успешно удален" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}