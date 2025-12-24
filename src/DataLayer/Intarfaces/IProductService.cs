using DataLayer.DTOs;

namespace DataLayer.Intarfaces
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetAllProductsAsync();
        Task<ProductDto> GetProductByArticleAsync(string article);
        Task AddProductAsync(ProductCreateDto productDto);
        Task UpdateProductAsync(int id, ProductCreateDto productDto);
        Task DeleteProductAsync(int id);
    }
}
