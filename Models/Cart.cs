using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AgriChoice.Models
{
    public class Cart
    {
        [Key]
        public int CartId { get; set; }

        public decimal ShippingCost { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TotalCost { get; set; }

        [Required]
        public string UserId { get; set; } // Links to the user
        public IdentityUser User { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(255)]
        public string DeliveryAddress { get; set; }

        // Navigation property to items
        public ICollection<CartItem> Items { get; set; }
    }

    public class CartItem
    {
        [Key]
        public int CartItemId { get; set; }

        [Required]
        public int CartId { get; set; }
        public Cart Cart { get; set; }

        [Required]
        public int CowId { get; set; }
        public Cow Cow { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    }

}