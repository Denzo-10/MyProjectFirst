using System.Security.Claims;
using DataLayer.Context;
using DataLayer.DTOs;
using DataLayer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Pages.Products
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public List<ProductViewDto> Products { get; set; } = new List<ProductViewDto>();

        [BindProperty(SupportsGet = true)]
        public string SearchDescription { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? ManufacturerId { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool OnlyDiscounted { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool InStockOnly { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; }

        public List<Manufacturer> Manufacturers { get; set; }

        // Свойства для отображения информации о пользователе
        public bool IsAuthenticated { get; set; }
        public bool IsClient { get; set; }
        public string UserFullName { get; set; }

        public async Task OnGetAsync()
        {
            IsAuthenticated = User.Identity.IsAuthenticated;
            UserFullName = HttpContext.Session.GetString("UserFullName") ?? "";
            IsClient = User.IsInRole("Клиент");

            await LoadManufacturers();
            await LoadProducts();
        }

        private async Task LoadManufacturers()
        {
            Manufacturers = await _context.Manufacturers
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        private async Task LoadProducts()
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Manufacturer)
                .Include(p => p.Supplier)
                .AsQueryable();

            if (!string.IsNullOrEmpty(SearchDescription))
            {
                query = query.Where(p => p.Description != null &&
                    p.Description.ToLower().Contains(SearchDescription.ToLower()));
            }

            if (ManufacturerId.HasValue)
            {
                query = query.Where(p => p.ManufacturerId == ManufacturerId.Value);
            }

            if (MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= MaxPrice.Value);
            }

            if (OnlyDiscounted)
            {
                query = query.Where(p => p.Discount > 0);
            }

            if (InStockOnly)
            {
                query = query.Where(p => p.StockQuantity > 0);
            }

            query = SortBy switch
            {
                "name" => query.OrderBy(p => p.Name),
                "name_desc" => query.OrderByDescending(p => p.Name),
                "supplier" => query.OrderBy(p => p.Supplier.Name),
                "supplier_desc" => query.OrderByDescending(p => p.Supplier.Name),
                "price" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                _ => query.OrderBy(p => p.Name)
            };

            var products = await query.ToListAsync();

            Products = products.Select(p => new ProductViewDto
            {
                ProductId = p.ProductId,
                Article = p.Article,
                Name = p.Name,
                Description = p.Description ?? "Нет описания",
                Price = p.Price,
                Discount = p.Discount ?? 0,
                StockQuantity = p.StockQuantity ?? 0,
                Unit = p.Unit,
                Photo = p.Photo,
                CategoryName = p.Category.Name,
                ManufacturerName = p.Manufacturer.Name,
                SupplierName = p.Supplier.Name,
                DiscountedPrice = p.Price * (100 - (p.Discount ?? 0)) / 100,
                HasDiscount = (p.Discount ?? 0) > 0,
                HighDiscount = (p.Discount ?? 0) > 15,
                InStock = (p.StockQuantity ?? 0) > 0
            }).ToList();
        }
    }
}