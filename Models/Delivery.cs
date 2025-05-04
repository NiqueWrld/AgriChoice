using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AgriChoice.Models
{
    public class Delivery
    {
        [Key]
        public int DeliveryId { get; set; }

        public string? CurrentLocation { get; set; }

        public string? DriverId { get; set; }

        [Required]
        public int PickUpPin { get; set; }

        [Required]
        public int DropOffPin { get; set; }

        public IdentityUser? User { get; set; }
        public bool? PickedUp { get; set; }

        public DateTime ScheduledDate { get; set; }

        public DateTime DeliveryCompletedDate { get; set; }

        public DateTime PickupDate { get; set; }


        

    }
}
