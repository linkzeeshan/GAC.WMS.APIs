using System.ComponentModel.DataAnnotations;

namespace GAC.WMS.Integrations.Application.DTOs.Common
{
    public class BatchRequestDto<T> where T : class
    {
        [Required]
        [StringLength(50)]
        public string RequestId { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        [MinLength(1, ErrorMessage = "At least one item is required")]
        public List<T> Items { get; set; } = new List<T>();
    }
}
