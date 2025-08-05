using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Fundo.Applications.WebApi.Models;
using Fundo.Applications.WebApi.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fundo.Applications.WebApi.Controllers
{
    [ApiController]
    [Route("/loans")]
    [AuthenticatedUser]
    public class LoanManagementController : ControllerBase
    {
        private readonly LoanContext _context;

        public LoanManagementController(LoanContext context)
        {
            _context = context;
        }

        // POST /loans - Only Managers and Admins can create loans
        [HttpPost]
        [ManagerOrAdmin]
        public async Task<ActionResult<Loan>> CreateLoan(Loan loan)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
                
            try
            {
                _context.Loans.Add(loan);
                await _context.SaveChangesAsync();

                var auditLog = new AuditLog
                {
                    LoanId = loan.Id,
                    Action = "Loan created",
                    Timestamp = DateTime.UtcNow,
                    UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                };
                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetLoan), new { id = loan.Id }, loan);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the loan", error = ex.Message });
            }
        }

        // GET /loans/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Loan>> GetLoan(int id)
        {
            var loan = await _context.Loans.FindAsync(id);
            if (loan == null)
                return NotFound();
            return loan;
        }

        // GET /loans
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Loan>>> GetLoans(int page = 1, int pageSize = 10, string filter = null, string sort = null)
        {
            var query = _context.Loans.AsQueryable();

            // Filtering
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(l => l.ApplicantName.Contains(filter) || l.Status.Contains(filter));
            }

            // Sorting
            if (!string.IsNullOrEmpty(sort))
            {
                switch (sort.ToLower())
                {
                    case "amount":
                        query = query.OrderBy(l => l.Amount);
                        break;
                    case "amount_desc":
                        query = query.OrderByDescending(l => l.Amount);
                        break;
                    case "status":
                        query = query.OrderBy(l => l.Status);
                        break;
                    case "status_desc":
                        query = query.OrderByDescending(l => l.Status);
                        break;
                    default:
                        query = query.OrderBy(l => l.Id);
                        break;
                }
            }

            var loans = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return Ok(new {
                Page = page,
                PageSize = pageSize,
                Total = await query.CountAsync(),
                Data = loans
            });
        }

        // POST /loans/{id}/payment
        [HttpPost("{id}/payment")]
        public async Task<IActionResult> MakePayment(int id, [FromBody] decimal paymentAmount)
        {
            var loan = await _context.Loans.FindAsync(id);
            if (loan == null)
                return NotFound();
            if (paymentAmount <= 0 || paymentAmount > loan.CurrentBalance)
                return BadRequest("Invalid payment amount.");
            loan.CurrentBalance -= paymentAmount;
            if (loan.CurrentBalance == 0)
                loan.Status = "paid";

            var auditLog = new AuditLog
            {
                LoanId = loan.Id,
                Action = $"Payment of {paymentAmount} made",
                Timestamp = DateTime.UtcNow,
                UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            };
            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync();
            return Ok();
        }

        // GET /loans/{id}/audit
        [HttpGet("{id}/audit")]
        public async Task<ActionResult<IEnumerable<AuditLog>>> GetAuditLogs(int id)
        {
            return await _context.AuditLogs.Where(a => a.LoanId == id).ToListAsync();
        }
    }
}