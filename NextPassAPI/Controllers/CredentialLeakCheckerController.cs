using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NextPassAPI.Services.Interfaces;

namespace NextPassAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CredentialLeakCheckerController : ControllerBase
    {
        private readonly ICredentialLeakChecker _credentialLeakChecker;

        public CredentialLeakCheckerController(ICredentialLeakChecker credentialLeakChecker)
        {
            _credentialLeakChecker = credentialLeakChecker;
        }

        [HttpPost("check")]
        public async Task<IActionResult> CheckPassword([FromBody] string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return BadRequest("Password cannot be null or empty.");
            }

            try
            {
                bool isLeaked = await _credentialLeakChecker.IsPasswordLeakedAsync(password);
                return Ok(new { IsLeaked = isLeaked });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }
    }
}
