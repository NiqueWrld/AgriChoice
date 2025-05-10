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

        public int? DeliveryId { get; set; }
        public Delivery Delivery { get; set; }

        public List<PurchaseCow> PurchaseCows { get; set; }

        [Required]
        public DateTime PurchaseDate { get; set; }

        [Required]
        public string DeliveryAddress { get; set; }

        [Required]
        public decimal ShippingCost { get; set; }

        [Required]
        public decimal TotalPrice { get; set; }

        [Required]
        public Paymentstatus PaymentStatus { get; set; }

        public enum Paymentstatus
        {
            Pending,
            Completed,
            Refunded
        }

        [Required]
        public Deliverystatus DeliveryStatus { get; set; }

        public enum Deliverystatus
        {
            Scheduled,
            InTransit,
            Delivered
        }

        public int? ReviewId { get; set; }

    }

    public class PurchaseCow
    {
        [Key]
        public int PurchaseCowId { get; set; }

        [Required]
        public int PurchaseId { get; set; }
        public Purchase Purchase { get; set; }

        public int CowId { get; set; }
        public Cow Cow { get; set; }
    }
}