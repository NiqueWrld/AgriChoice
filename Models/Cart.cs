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
        public decimal Tax { get; set; }
        public decimal TotalCost { get; set; }

        [Required]
        public string UserId { get; set; } 
        public IdentityUser User { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(255)]
        public string DeliveryAddress { get; set; }

        // Navigation property to items
        public ICollection<CartItem> Items { get; set; }

        public async Task<decimal> CalculateShippingCostAsync()
        {
            //var distanceService = new AgriChoice.Services.DistanceService();
            //double fee = await distanceService.CalculateShippingFee("20 INANDA ROAD,NEWLANDS EAST Ntuzuma 4037", DeliveryAddress, 50);

            return Items.Sum(item => item.CalculateTotalPrice());
        }
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

        public int CalculateTotalPrice()
        {
            const int basePrice = 12000;

            int genderFee = 0;
            if (Cow.Gender == "Female")
            {
                genderFee = basePrice + (int)(0.3 * basePrice);
            }
            else if (Cow.Gender == "Male")
            {
                genderFee = (int)(0.4 * basePrice) + 1200;
            }

            int weightFee = (Cow.Weight >= 400 && Cow.Weight <= 550) ? 1000 : 1500;

            int ageFee = (Cow.Age >= 6 && Cow.Age <= 12) ? 1200 : 1500;

            return genderFee + weightFee + ageFee;
        }

    }

}