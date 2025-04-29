using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace AgriChoice.Models
{
    public class Document
    {
        [Key]
        public int DocumentId { get; set; }

        [Required]
        public int PurchaseId { get; set; }
        public Purchase Purchase { get; set; }

        [Required]
        public Documenttype DocumentType { get; set; }
        public enum Documenttype
        {
            Certificate,
            Receipt,
            Agreement
        }

        [Required]
        public string DocumentUrl { get; set; } // URL to the document file
    }
}