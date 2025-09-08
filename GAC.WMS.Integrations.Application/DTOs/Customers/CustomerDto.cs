using System.ComponentModel.DataAnnotations;

namespace GAC.WMS.Integrations.Application.DTOs.Customers
{
    public class CustomerDto
    {
        [Required]
        [StringLength(50)]
        public string CustomerId { get; set; } = Guid.CreateVersion7().ToString();
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? ContactPerson { get; set; }
        
        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }
        
        [Phone]
        [StringLength(20)]
        public string? Phone { get; set; }
        
        // Address information
        [StringLength(200)]
        public string? Street { get; set; }
        
        [StringLength(100)]
        public string? City { get; set; }
        
        [StringLength(100)]
        public string? StateProvince { get; set; }
        
        [StringLength(20)]
        public string? PostalCode { get; set; }
        
        [StringLength(100)]
        public string? Country { get; set; }
        
        [StringLength(50)]
        public string? CustomerType { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        [StringLength(50)]
        public string? TaxIdentifier { get; set; }
    }
}
