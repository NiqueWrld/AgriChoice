using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgriChoice.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Required]
        public string UserId { get; set; } // Foreign key to IdentityUser

        [ForeignKey("UserId")]
        public IdentityUser User { get; set; }

        [Required]
        public decimal Amount { get; set; } // Positive for credit, negative for debit

        [Required]
        public TransactionType Type { get; set; } // Enum for transaction type (e.g., Credit, Debit)

        [Required]
        public DateTime Date { get; set; } // Date of the transaction

        public string Description { get; set; } // Optional description of the transaction

        public int? PurchaseId { get; set; } // Optional link to a purchase
        [ForeignKey("PurchaseId")]
        public Purchase Purchase { get; set; }
    }

    public enum TransactionType
    {
        Credit, // Money added to the wallet
        Debit   // Money deducted from the wallet
    }
}
