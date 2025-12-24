namespace DataLayer.DTOs
{
    public class ProductViewDto
    {
        public int ProductId { get; set; }
        public string Article { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public byte Discount { get; set; }
        public int StockQuantity { get; set; }
        public string Unit { get; set; }
        public string Photo { get; set; }
        public string CategoryName { get; set; }
        public string ManufacturerName { get; set; }
        public string SupplierName { get; set; }

        public decimal DiscountedPrice { get; set; }
        public bool HasDiscount { get; set; }
        public bool HighDiscount { get; set; }
        public bool InStock { get; set; }
    }
}
