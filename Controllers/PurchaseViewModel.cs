using AgriChoice.Models;
using System.ComponentModel.DataAnnotations;

namespace AgriChoice.Controllers
{
    public class PurchaseViewModel
    {
        public Cow Cow { get; set; }

        // Payment Details
        [Required]
        public string FullName { get; set; }

        [Required]
        [CreditCard]
        public string CreditCardNumber { get; set; }

        [Required]
        public DateTime ExpirationDate { get; set; }

        [Required]
        [Range(100, 999, ErrorMessage = "CVV must be a 3-digit number.")]
        public int CVV { get; set; }
    }
}