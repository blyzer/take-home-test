using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Fundo.Applications.WebApi.Models;
using Fundo.Applications.WebApi.Attributes;
using System.Collections.Generic;
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
        public async Task<ActionResult<IEnumerable<Loan>>> GetLoans()
        {
            return await _context.Loans.ToListAsync();
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
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}