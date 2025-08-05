using System.ComponentModel.DataAnnotations;

namespace Fundo.Applications.WebApi.Models
{
    public class Loan
    {
        public int Id { get; set; }
        [Required]
        public decimal Amount { get; set; }
        public decimal CurrentBalance { get; set; }
        [Required]
        public string ApplicantName { get; set; }
        [Required]
        public string Status { get; set; } // "active" or "paid"
    }
}