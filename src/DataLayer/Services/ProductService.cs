using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.Context;
using DataLayer.DTOs;
using DataLayer.Intarfaces;
using DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductDto>> GetAllProductsAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Manufacturer)
                .Include(p => p.Supplier)
                .Select(p => new ProductDto
                {
                    ProductId = p.ProductId,
                    Article = p.Article,
                    Name = p.Name,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    CategoryName = p.Category.Name,
                    ManufacturerName = p.Manufacturer.Name,
                    SupplierName = p.Supplier.Name
                })
                .ToListAsync();
        }

        public async Task<ProductDto> GetProductByArticleAsync(string article)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Manufacturer)
                .Include(p => p.Supplier)
                .Where(p => p.Article == article)
                .Select(p => new ProductDto
                {
                    ProductId = p.ProductId,
                    Article = p.Article,
                    Name = p.Name,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    CategoryName = p.Category.Name,
                    ManufacturerName = p.Manufacturer.Name,
                    SupplierName = p.Supplier.Name
                })
                .FirstOrDefaultAsync();
        }

        public async Task AddProductAsync(ProductCreateDto productDto)
        {
            var exists = await _context.Products
                .AnyAsync(p => p.Article == productDto.Article);

            if (exists)
                throw new ArgumentException($"Товар с артикулом '{productDto.Article}' уже существует");

            var product = new Product
            {
                Article = productDto.Article,
                Name = productDto.Name,
                Price = productDto.Price,
                StockQuantity = productDto.StockQuantity,
                CategoryId = productDto.CategoryId,
                ManufacturerId = productDto.ManufacturerId,
                SupplierId = productDto.SupplierId,
                Unit = productDto.Unit,
                Discount = productDto.Discount,
                Description = productDto.Description,
                Photo = productDto.Photo
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProductAsync(int id, ProductCreateDto productDto)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                throw new ArgumentException($"Товар с ID {id} не найден");

            if (product.Article != productDto.Article)
            {
                var articleExists = await _context.Products
                    .AnyAsync(p => p.Article == productDto.Article && p.ProductId != id);

                if (articleExists)
                    throw new ArgumentException($"Товар с артикулом '{productDto.Article}' уже существует");
            }

            product.Article = productDto.Article;
            product.Name = productDto.Name;
            product.Price = productDto.Price;
            product.StockQuantity = productDto.StockQuantity;
            product.CategoryId = productDto.CategoryId;
            product.ManufacturerId = productDto.ManufacturerId;
            product.SupplierId = productDto.SupplierId;
            product.Unit = productDto.Unit;
            product.Discount = productDto.Discount;
            product.Description = productDto.Description;
            product.Photo = productDto.Photo;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                throw new ArgumentException($"Товар с ID {id} не найден");

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }
}