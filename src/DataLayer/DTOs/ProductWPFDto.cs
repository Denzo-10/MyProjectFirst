using System;
using System.IO;

namespace DataLayer.DTOs
{
    public class ProductWPFDto
    {
        public int ProductId { get; set; }
        public string Article { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public byte? Discount { get; set; }
        public bool HasDiscount => Discount > 0;
        public decimal DiscountedPrice => HasDiscount ? Price * (1 - (Discount.Value / 100m)) : Price;
        public int? StockQuantity { get; set; }
        public bool InStock => StockQuantity > 0;
        public string Unit { get; set; }
        public string Description { get; set; }
        public string Photo { get; set; } // Здесь хранится имя файла (например "1.jpg", "A112T4.jpg")
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int ManufacturerId { get; set; }
        public string ManufacturerName { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }

        // Путь к папке с изображениями будет установлен позже
        private string _imagesFolderPath;

        public void SetImagesFolderPath(string path)
        {
            _imagesFolderPath = path;
        }

        // Путь к изображению-заглушке
        public string DefaultImagePath
        {
            get
            {
                if (!string.IsNullOrEmpty(_imagesFolderPath))
                {
                    string defaultImage = Path.Combine(_imagesFolderPath, "picture.png");
                    if (File.Exists(defaultImage))
                    {
                        return defaultImage;
                    }
                }
                return string.Empty;
            }
        }

        // Проверяем, есть ли фото
        public bool HasPhoto
        {
            get
            {
                if (string.IsNullOrEmpty(Photo) || string.IsNullOrEmpty(_imagesFolderPath))
                    return false;

                // Если Photo содержит путь (например "1.jpg"), берем только имя файла
                string fileName = Path.GetFileName(Photo);
                var fullPath = Path.Combine(_imagesFolderPath, fileName);
                return File.Exists(fullPath);
            }
        }

        // Полный путь к фото
        public string PhotoPath
        {
            get
            {
                if (!string.IsNullOrEmpty(Photo) && !string.IsNullOrEmpty(_imagesFolderPath))
                {
                    // Если Photo содержит путь (например "1.jpg"), берем только имя файла
                    string fileName = Path.GetFileName(Photo);
                    string fullPath = Path.Combine(_imagesFolderPath, fileName);
                    if (File.Exists(fullPath))
                    {
                        return fullPath;
                    }
                }

                // Возвращаем путь к изображению-заглушке
                return DefaultImagePath;
            }
        }
    }
}