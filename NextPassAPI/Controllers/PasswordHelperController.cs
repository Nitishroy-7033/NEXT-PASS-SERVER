using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NextPassAPI.Identity.Utils;
using NextPassAPI.Services.Interfaces;
using System.Security.Cryptography;

namespace NextPassAPI.Controllers
{
    [Route("Password")]
    [ApiController]
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
                var response = await _passwordHelperService.GetEncryptedPassword(password);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet("decrypt")]
        public async Task<IActionResult> GetDecryptedPassword([FromQuery] string encryptedPassword)
        {
            try
            {
                var response = await _passwordHelperService.GetDecryptedPassword(encryptedPassword);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
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
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] keyBytes = new byte[32]; // 32 bytes for AES-256
                rng.GetBytes(keyBytes);
                return Convert.ToBase64String(keyBytes);
            }
        }
    }
}
