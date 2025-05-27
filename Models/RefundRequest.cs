using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace AgriChoice.Models
{
    public class RefundRequest
    {
        public int RefundRequestId { get; set; }
        public int PurchaseId { get; set; }
        public Purchase? Purchase { get; set; }
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
        public IdentityUser? Driver { get; set; }

        public string UploadedFileUrl { get; set; }
        public DateTime RequestedAt { get; set; }
        public Refundstatus Status { get; set; }

        // Collection of cows to be returned
        public ICollection<RefundRequestCow> RefundRequestCows { get; set; } = new List<RefundRequestCow>();

        public enum Refundstatus
        {
            Pending,
            Approved,
            Returned,
            Rejected
        }
    }

    // Join entity between RefundRequest and Cow
    public class RefundRequestCow
    {
        public int RefundRequestId { get; set; }
        public RefundRequest RefundRequest { get; set; }

        public int CowId { get; set; }
        public Cow Cow { get; set; }

        // Additional fields specific to the return of this cow
        public string? ReturnReason { get; set; }
        public string? Condition { get; set; }
        public bool Processed { get; set; } = false;
    }
}