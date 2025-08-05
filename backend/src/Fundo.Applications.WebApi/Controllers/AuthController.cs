using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using Fundo.Applications.WebApi.Models;
using Fundo.Applications.WebApi.Services;
using Fundo.Applications.WebApi.Attributes;

namespace Fundo.Applications.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var response = await _authService.LoginAsync(request);
                if (response == null)
                {
                    return Unauthorized(new { message = "Invalid username or password" });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during login", error = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var response = await _authService.RegisterAsync(request);
                return CreatedAtAction(nameof(GetProfile), new { }, response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during registration", error = ex.Message });
            }
        }

        [HttpGet("profile")]
        [AuthenticatedUser]
        public async Task<ActionResult<UserDto>> GetProfile()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _authService.GetUserByIdAsync(userId.Value);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpPost("change-password")]
        [AuthenticatedUser]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var success = await _authService.ChangePasswordAsync(userId.Value, request);
                if (!success)
                {
                    return BadRequest(new { message = "Current password is incorrect" });
                }

                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while changing password", error = ex.Message });
            }
        }

        [HttpGet("users")]
        [ManagerOrAdmin]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            try
            {
                var users = await _authService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving users", error = ex.Message });
            }
        }

        [HttpGet("users/{userId}")]
        [ManagerOrAdmin]
        public async Task<ActionResult<UserDto>> GetUser(int userId)
        {
            try
            {
                var user = await _authService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound();
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving user", error = ex.Message });
            }
        }

        [HttpPut("users/{userId}/role")]
        [AdminOnly]
        public async Task<ActionResult> UpdateUserRole(int userId, [FromBody] UpdateRoleRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var success = await _authService.UpdateUserRoleAsync(userId, request.Role);
                if (!success)
                {
                    return BadRequest(new { message = "Invalid role or user not found" });
                }

                return Ok(new { message = "User role updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating user role", error = ex.Message });
            }
        }

        [HttpDelete("users/{userId}")]
        [AdminOnly]
        public async Task<ActionResult> DeactivateUser(int userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == userId)
                {
                    return BadRequest(new { message = "Cannot deactivate your own account" });
                }

                var success = await _authService.DeactivateUserAsync(userId);
                if (!success)
                {
                    return NotFound();
                }

                return Ok(new { message = "User deactivated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deactivating user", error = ex.Message });
            }
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    public class UpdateRoleRequest
    {
        public string Role { get; set; } = string.Empty;
    }
}
