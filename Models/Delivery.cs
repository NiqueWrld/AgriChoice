using System.ComponentModel.DataAnnotations;

namespace AgriChoice.Models
{
    public class Delivery
    {
        [Key]
        public int DeliveryId { get; set; }

        [Required]
        public int PurchaseId { get; set; }
        public Purchase Purchase { get; set; }

       
        public DateTime ScheduledDate { get; set; }

        public string TrackingUrl { get; set; } // Optional tracking link

        [Required]
        public Deliverystatus DeliveryStatus { get; set; }
        public enum Deliverystatus
        {
            Pending,
            InProgress,
            Completed
        }

    }
}