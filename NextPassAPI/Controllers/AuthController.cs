using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NextPassAPI.Data.Models;
using NextPassAPI.Data.Models.Requests;
using NextPassAPI.Data.Models.Responses;
using NextPassAPI.Identity.AuthHandler;
using NextPassAPI.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NextPassAPI.Controllers
{
    [Route("Auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly AuthHandler _authHandler;
        public AuthController(IAuthService authService, IConfiguration configuration,AuthHandler authHandler, IUserService userService)
        {
            _authService = authService;
            _configuration = configuration;
            _authHandler = authHandler;
            _userService = userService;
        }


        [HttpPost("login")]
        public async Task<IActionResult> LoginUser([FromBody] AuthRequest userLoginRequest)
        {
            try
            {
                var user = await _authService.LoginUser(userLoginRequest.Email, userLoginRequest.Password);
                if (user == null)
                {
                    var emptyResponse = new ApiResponse<User>(false, "Invalid email or password", null);
                    return Unauthorized(emptyResponse);
                }
                
                var authReponse = _authHandler.GenerateToken(user);
                var response = new ApiResponse<AuthResponse>(true, "User logged in successfully", authReponse);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<string>(false, "An error occurred while logging in the user", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }




        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] UserRequest userRequest)
        {
            try
            {
                var existingUser = await _userService.GetUserByEmail(userRequest.Email);
                if (existingUser != null)
                {
                    var emptyResponse = new ApiResponse<User>(false, "User with this email already exists", null);
                    return BadRequest(emptyResponse);
                }
                var user = await _authService.RegisterUser(userRequest);
                var response = new ApiResponse<User>(true, "User created successfully", user);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<string>(false, "An error occurred while creating the user", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }




    }
}
