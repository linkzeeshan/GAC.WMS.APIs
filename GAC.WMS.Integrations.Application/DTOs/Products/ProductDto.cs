using System.ComponentModel.DataAnnotations;

namespace GAC.WMS.Integrations.Application.DTOs.Products
{
    public class ProductDto
    {
        [Required]
        [StringLength(50)]
        public string ProductId { get; set; } = Guid.CreateVersion7().ToString();
        
        [Required]
        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [StringLength(50)]
        public string? Category { get; set; }
        
        [StringLength(20)]
        public string? UnitOfMeasure { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal? Weight { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal? Length { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal? Width { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal? Height { get; set; }
        
        [StringLength(50)]
        public string? Barcode { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Additional attributes as dictionary
        public Dictionary<string, object>? Attributes { get; set; }
    }
}
