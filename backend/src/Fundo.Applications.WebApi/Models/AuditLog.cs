using System;

namespace Fundo.Applications.WebApi.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public int LoanId { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; }
        public string UserId { get; set; }
    }
}