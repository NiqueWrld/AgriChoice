using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AgriChoice.Models
{
    public class RefundRequest
    {
        public int RefundRequestId { get; set; }
        public int PurchaseId { get; set; } 
        public string UserId { get; set; }
        public IdentityUser? User { get; set; }
        public string Reason { get; set; }
        public string AdditionalComments { get; set; }
        [Required]
        public int PickOffPin { get; set; }
        [Required]
        public int DropOffPin { get; set; }
        public DateTime ScheduledDate { get; set; }
        public DateTime CollectionCompletedDate { get; set; }
        public bool? PickedUp { get; set; }
        public string? DriverId { get; set; }

        public string UploadedFileUrl { get; set; } 
        public DateTime RequestedAt { get; set; } 
        public Refundstatus Status { get; set; } 

        public enum Refundstatus
        {
            Pending,
            Approved,
            Returned,
            Rejected
        }
    }
}
