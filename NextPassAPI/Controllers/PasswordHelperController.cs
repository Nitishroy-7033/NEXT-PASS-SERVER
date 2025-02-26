using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NextPassAPI.Identity.Utils;

namespace NextPassAPI.Controllers
{
    [Route("Password")]
    [ApiController]
    public class PasswordHelperController : ControllerBase
    {
        private readonly EncryptionHelper _encryptionHelper;

        public PasswordHelperController()
        {
            string secretKey = "bGz9H@pL!vXkQw3Y7dN#z2TfGmUoC6Rs";
            _encryptionHelper = new EncryptionHelper(secretKey);
        }

        [HttpGet("encrypt")]
        public IActionResult GetEncryptedPassword([FromQuery] string password)
        {
            var response = _encryptionHelper.Encrypt(password);
            return Ok(response);
        }

        [HttpGet("decrypt")]
        public IActionResult GetDecryptedPassword([FromQuery] string encryptedPassword)
        {
            var response = _encryptionHelper.Decrypt(encryptedPassword);
            return Ok(response);
        }

        [HttpGet("generate-key")]
        public IActionResult GenerateUserKey()
        {
            var key = EncryptionHelper.GenerateSecureKey();
            return Ok(key);
        }
    }
}
