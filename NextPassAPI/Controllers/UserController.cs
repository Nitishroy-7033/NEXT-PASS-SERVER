using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NextPassAPI.Data.Models.Requests;
using NextPassAPI.Data.Models.Responses;
using NextPassAPI.Data.Models;
using NextPassAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace NextPassAPI.Controllers
{
    [Route("User")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }


        [Authorize]
        [HttpGet("{email}")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            try
            {
                var user = await _userService.GetUserByEmail(email);
                if (user == null)
                {
                    var emptyResponse = new ApiResponse<User>(false, "User not found", null );
                    return Ok(emptyResponse);
                }
                var response = new ApiResponse<User>(true, "User retrieved successfully", user);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<string>(false, "An error occurred while retrieving the user", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }
        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody] User updatedUser)
        {
            try
            {
                var success = await _userService.UpdateUser(updatedUser);
                if (!success)
                {
                    var errorResponse = new ApiResponse<bool>(false, "Failed to update user", false);
                    return BadRequest(errorResponse);
                }
                var response = new ApiResponse<bool>(true, "User updated successfully", true);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<string>(false, "An error occurred while updating the user", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            try
            {
                var success = await _userService.DeleteUser(userId);
                if (!success)
                {
                    var errorResponse = new ApiResponse<bool>(false, "Failed to delete user", false);
                    return BadRequest(errorResponse);
                }
                var response = new ApiResponse<bool>(true, "User deleted successfully", true);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<string>(false, "An error occurred while deleting the user", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        [Authorize]
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUser();
                var response = new ApiResponse<List<User>>(true, "User get successfully", users);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<string>(false, "An error occurred while geting the user", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        [Authorize]
        [HttpPut("database-settings")]
        public async Task<IActionResult> UpdateDatabaseSettings([FromBody] DatabaseUpdateRequest databaseUpdateRequest)
        {
            try
            {
                if (databaseUpdateRequest != null && databaseUpdateRequest.DataBaseType != null)
                {
                    var response = await _userService.UpdateDatabaseSettings(databaseUpdateRequest);
                      return Ok(new ApiResponse<User>(true, "Database settings updated successfully", data:response));
                 }
                else
                {
                    var errorResponse = new ApiResponse<string>(false, "Database settings not updated", "Database type is required");
                    return BadRequest(errorResponse);
                }
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<string>(false, "An error occurred while updating the user", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }
    }
}
