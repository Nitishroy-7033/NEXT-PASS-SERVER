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
using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;

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
        public AuthController(IAuthService authService, IConfiguration configuration, AuthHandler authHandler, IUserService userService)
        {
            _authService = authService;
            _configuration = configuration;
            _authHandler = authHandler;
            _userService = userService;
        }


        [HttpPost("login")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<IActionResult> LoginUser([FromBody] AuthRequest userLoginRequest)
        {
            try
            {
                // Input validation
                if (userLoginRequest == null || string.IsNullOrWhiteSpace(userLoginRequest.Email) || string.IsNullOrWhiteSpace(userLoginRequest.Password))
                {
                    var validationResponse = new ApiResponse<User>(false, "Email and password are required", null!);
                    return BadRequest(validationResponse);
                }

                // Validate email format
                if (!IsValidEmail(userLoginRequest.Email))
                {
                    var emailValidationResponse = new ApiResponse<User>(false, "Invalid email format", null!);
                    return BadRequest(emailValidationResponse);
                }

                var user = await _authService.LoginUser(userLoginRequest.Email, userLoginRequest.Password);
                if (user == null)
                {
                    var emptyResponse = new ApiResponse<User>(false, "Invalid email or password", null!);
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
        [EnableRateLimiting("AuthPolicy")]
        public async Task<IActionResult> RegisterUser([FromBody] UserRequest userRequest)
        {
            try
            {
                // Input validation
                if (userRequest == null || string.IsNullOrWhiteSpace(userRequest.Email) || string.IsNullOrWhiteSpace(userRequest.Password))
                {
                    var validationResponse = new ApiResponse<User>(false, "All fields are required", null!);
                    return BadRequest(validationResponse);
                }

                // Validate email format
                if (!IsValidEmail(userRequest.Email))
                {
                    var emailValidationResponse = new ApiResponse<User>(false, "Invalid email format", null!);
                    return BadRequest(emailValidationResponse);
                }

                // Validate password strength
                if (!IsStrongPassword(userRequest.Password))
                {
                    var passwordResponse = new ApiResponse<User>(false, "Password must be at least 8 characters with uppercase, lowercase, number and special character", null!);
                    return BadRequest(passwordResponse);
                }

                var existingUser = await _userService.GetUserByEmail(userRequest.Email);
                if (existingUser != null)
                {
                    var emptyResponse = new ApiResponse<User>(false, "User with this email already exists", null!);
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


        [HttpGet("healthcheck")]
        public IActionResult HealthCheck()
        {
            return Ok("API is running");
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var emailChecker = new EmailAddressAttribute();
                return emailChecker.IsValid(email);
            }
            catch
            {
                return false;
            }
        }

        private static bool IsStrongPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return false;

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }

    }
}
