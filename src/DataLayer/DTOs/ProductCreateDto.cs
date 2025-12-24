using System.ComponentModel.DataAnnotations;

namespace DataLayer.DTOs
{
    public class ProductCreateDto
    {
        [Required]
        [StringLength(50)]
        public string Article { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public decimal Price { get; set; }

        public int? StockQuantity { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int ManufacturerId { get; set; }

        [Required]
        public int SupplierId { get; set; }

        [StringLength(50)]
        public string Unit { get; set; } = "шт.";

        public byte? Discount { get; set; } = 0;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [StringLength(255)]
        public string Photo { get; set; }
    }
}
