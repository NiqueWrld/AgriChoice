using AgriChoice.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;
using System.ComponentModel.DataAnnotations;

namespace AgriChoice.Models
{
     public class Purchase
    {
        [Key]
        public int PurchaseId { get; set; }

        [Required]
        public string UserId { get; set; }
        public IdentityUser User { get; set; }

        [Required]
        public int CowId { get; set; }
        public Cow Cow { get; set; }

        [Required]
        public DateTime PurchaseDate { get; set; }

        [Required]
        public Paymentstatus PaymentStatus { get; set; }

        public enum Paymentstatus
        {
            Pending,
            Completed,
            Refunded
        }

        [Required]
        public string DeliveryStatus { get; set; } // "Scheduled", "In Transit", "Delivered"
    }
}