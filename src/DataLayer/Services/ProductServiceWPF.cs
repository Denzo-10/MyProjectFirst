using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.Context;
using DataLayer.DTOs;
using DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Services
{
    public class ProductServiceWPF
    {
        private readonly AppDbContext _context;
        private readonly string _imagesFolder;

        public ProductServiceWPF(string imagesFolder)
        {
            _context = new AppDbContext();
            _imagesFolder = imagesFolder;

            // Создаем папку для изображений, если она не существует
            if (!Directory.Exists(_imagesFolder))
            {
                Directory.CreateDirectory(_imagesFolder);
            }
        }

        public string ImagesFolder => _imagesFolder;

        // Аутентификация пользователя
        public async Task<User> AuthenticateUserAsync(string login, string password)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Login == login && u.Password == password);

                return user;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка аутентификации: {ex.Message}", ex);
            }
        }

        // Получение всех товаров
        public async Task<List<ProductWPFDto>> GetProductsAsync()
        {
            try
            {
                var products = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Manufacturer)
                    .Include(p => p.Supplier)
                    .Select(p => new ProductWPFDto
                    {
                        ProductId = p.ProductId,
                        Article = p.Article,
                        Name = p.Name,
                        Price = p.Price,
                        Discount = p.Discount,
                        StockQuantity = p.StockQuantity,
                        Unit = p.Unit,
                        Description = p.Description,
                        Photo = p.Photo,
                        CategoryId = p.CategoryId,
                        CategoryName = p.Category != null ? p.Category.Name : "",
                        ManufacturerId = p.ManufacturerId,
                        ManufacturerName = p.Manufacturer != null ? p.Manufacturer.Name : "",
                        SupplierId = p.SupplierId,
                        SupplierName = p.Supplier != null ? p.Supplier.Name : ""
                    })
                    .ToListAsync();

                return products;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при загрузке товаров: {ex.Message}", ex);
            }
        }

        // Получение производителей
        public async Task<List<Manufacturer>> GetManufacturersAsync()
        {
            try
            {
                return await _context.Manufacturers.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при загрузке производителей: {ex.Message}", ex);
            }
        }

        // Получение категорий
        public async Task<List<Category>> GetCategoriesAsync()
        {
            try
            {
                return await _context.Categories.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при загрузке категорий: {ex.Message}", ex);
            }
        }

        // Сохранение товара (добавление или обновление)
        public async Task SaveProductAsync(Product product)
        {
            try
            {
                if (product.ProductId == 0)
                {
                    // Новый товар
                    _context.Products.Add(product);
                }
                else
                {
                    // Обновление существующего товара
                    var existingProduct = await _context.Products.FindAsync(product.ProductId);
                    if (existingProduct != null)
                    {
                        existingProduct.Article = product.Article;
                        existingProduct.Name = product.Name;
                        existingProduct.Price = product.Price;
                        existingProduct.Discount = product.Discount;
                        existingProduct.StockQuantity = product.StockQuantity;
                        existingProduct.Unit = product.Unit;
                        existingProduct.Description = product.Description;
                        existingProduct.Photo = product.Photo;
                        existingProduct.CategoryId = product.CategoryId;
                        existingProduct.ManufacturerId = product.ManufacturerId;
                        existingProduct.SupplierId = product.SupplierId;

                        _context.Products.Update(existingProduct);
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при сохранении товара: {ex.Message}", ex);
            }
        }

        // Удаление товара
        public async Task DeleteProductAsync(int productId)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product != null)
                {
                    // Удаляем изображение товара если оно есть
                    if (!string.IsNullOrEmpty(product.Photo))
                    {
                        string imagePath = Path.Combine(_imagesFolder, product.Photo);
                        if (File.Exists(imagePath))
                        {
                            File.Delete(imagePath);
                        }
                    }

                    _context.Products.Remove(product);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при удалении товара: {ex.Message}", ex);
            }
        }

        // Сохранение изображения товара
        public async Task<string> SaveProductImageAsync(string sourcePath, string article)
        {
            try
            {
                if (string.IsNullOrEmpty(sourcePath) || !File.Exists(sourcePath))
                    return null;

                // Определяем расширение файла
                string extension = Path.GetExtension(sourcePath).ToLower();

                // Создаем имя файла на основе артикула
                string fileName = $"{article}{extension}";
                string targetPath = Path.Combine(_imagesFolder, fileName);

                // Копируем файл
                File.Copy(sourcePath, targetPath, true);

                return fileName;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при сохранении изображения: {ex.Message}", ex);
            }
        }

        // Удаление изображения товара
        public async Task DeleteProductImageAsync(string photoFileName)
        {
            try
            {
                if (!string.IsNullOrEmpty(photoFileName))
                {
                    string imagePath = Path.Combine(_imagesFolder, photoFileName);
                    if (File.Exists(imagePath))
                    {
                        await Task.Run(() => File.Delete(imagePath));
                    }
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку, но не прерываем выполнение
                Console.WriteLine($"Error deleting image: {ex.Message}");
            }
        }

        // Получение поставщиков
        public async Task<List<Supplier>> GetSuppliersAsync()
        {
            try
            {
                return await _context.Suppliers.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при загрузке поставщиков: {ex.Message}", ex);
            }
        }

        // Получение единиц измерения
        public async Task<List<string>> GetUnitsAsync()
        {
            try
            {
                return await _context.Products
                    .Select(p => p.Unit)
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при загрузке единиц измерения: {ex.Message}", ex);
            }
        }
    }
}