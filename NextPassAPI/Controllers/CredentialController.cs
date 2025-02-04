using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NextPassAPI.Data.Models.Query;
using NextPassAPI.Data.Models.Requests;
using NextPassAPI.Data.Models.Responses;
using NextPassAPI.Data.Models;
using NextPassAPI.Services.Interfaces;

namespace NextPassAPI.Controllers
{
    [Route("Credential")]
    [ApiController]
    [Authorize]
    public class CredentialController : ControllerBase
    {
        private readonly ICredentialService _credentialService;

        public CredentialController(ICredentialService credentialService)
        {
            _credentialService = credentialService;
        }

        
        [HttpGet]
        public async Task<IActionResult> GetCredentials([FromQuery] GetCredentialQuery query)
        {
            try
            {
                var credentials = await _credentialService.GetCredentialAsync(query);
                var response = new ApiResponse<CredenatialResponse>(true, "Credentials retrieved successfully", credentials);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<string>(false, "An error occurred while retrieving credentials", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCredential([FromBody] CredentialRequest credentialRequest)
        {
            try
            {
                var credential = await _credentialService.CreateCredentialAsnyc(credentialRequest);
                var response = new ApiResponse<Credential>(true, "Credential created successfully", credential);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<string>(false, "An error occurred while creating the credential", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCredential([FromBody] CredentialRequest credentialRequest)
        {
            try
            {
                var success = await _credentialService.UpdateCredentialAsync(credentialRequest);
                if (!success)
                {
                    var errorResponse = new ApiResponse<bool>(false, "Failed to update credential", false);
                    return BadRequest(errorResponse);
                }
                var response = new ApiResponse<bool>(true, "Credential updated successfully", true);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<string>(false, "An error occurred while updating the credential", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        [HttpDelete("{credentialId}")]
        public async Task<IActionResult> DeleteCredential(string credentialId)
        {
            try
            {
                var success = await _credentialService.DeleteCredentialAsync(credentialId);
                if (!success)
                {
                    var errorResponse = new ApiResponse<bool>(false, "Failed to delete credential", false);
                    return BadRequest(errorResponse);
                }
                var response = new ApiResponse<bool>(true, "Credential deleted successfully", true);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<string>(false, "An error occurred while deleting the credential", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }
    }
}
