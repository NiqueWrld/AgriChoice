using System.ComponentModel.DataAnnotations;

namespace AgriChoice.Models
{
    public class Cow
    {
        [Key]
        public int CowId { get; set; }

        [Required]
        public string Breed { get; set; }
        [Required]
        public string Name { get; set; }

        [Required]
        public string Gender { get; set; }

        public int Age { get; set; }

        public double Weight { get; set; } 

        [Required]
        public decimal Price { get; set; }

        public string Description { get; set; }

        public string ImageUrl { get; set; }

        [Required]
        public bool IsAvailable { get; set; } 
    }

}