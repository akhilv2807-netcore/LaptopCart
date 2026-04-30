using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaptopCart.Models
{
    public class Product
    {   
        [Key] 
        public int Id { get; set; }
        [Required(ErrorMessage = "Product name is required.")]
        public string Name { get; set; }
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; }

        [Range(1, 1000000, ErrorMessage = "Price must be between 1 and 1000000")] //Date Annotation to validate price range
        public decimal Price { get; set; }
        public string? ImagePath { get; set; }
        public DateTime CreatedAt{ get; set; }
        [NotMapped]
        public IFormFile? ImageFile { get; set; }

    }
}
