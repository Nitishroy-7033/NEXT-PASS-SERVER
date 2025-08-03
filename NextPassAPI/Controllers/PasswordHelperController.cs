using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NextPassAPI.Identity.Utils;
using NextPassAPI.Services.Interfaces;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace NextPassAPI.Controllers
{
    [Route("Password")]
    [ApiController]
    [Authorize] // Require authentication for all password operations
    [EnableRateLimiting("GeneralPolicy")]
    public class PasswordHelperController : ControllerBase
    {
        private readonly IPasswordHelperService _passwordHelperService;
        public PasswordHelperController(IPasswordHelperService passwordHelperService)
        {
            _passwordHelperService = passwordHelperService;
        }

        [HttpGet("encrypt")]
        public async Task<IActionResult> GetEncryptedPassword([FromQuery] string password)
        {
            try
            {
                // Input validation
                if (string.IsNullOrWhiteSpace(password))
                {
                    return BadRequest("Password cannot be empty");
                }

                if (password.Length > 1000) // Prevent extremely long inputs
                {
                    return BadRequest("Password too long");
                }

                var response = await _passwordHelperService.GetEncryptedPassword(password);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while encrypting the password");
            }
        }

        [HttpGet("decrypt")]
        public async Task<IActionResult> GetDecryptedPassword([FromQuery] string encryptedPassword)
        {
            try
            {
                // Input validation
                if (string.IsNullOrWhiteSpace(encryptedPassword))
                {
                    return BadRequest("Encrypted password cannot be empty");
                }

                if (encryptedPassword.Length > 2000) // Prevent extremely long inputs
                {
                    return BadRequest("Encrypted password too long");
                }

                var response = await _passwordHelperService.GetDecryptedPassword(encryptedPassword);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while decrypting the password");
            }
        }

        [HttpGet("generate-key")]
        public IActionResult GenerateUserKey()
        {
            var key = GenerateSecureKey();
            return Ok(key);
        }
        public static string GenerateSecureKey()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] keyBytes = new byte[32]; // 32 bytes for AES-256
                rng.GetBytes(keyBytes);
                return Convert.ToBase64String(keyBytes);
            }
        }
    }
}
