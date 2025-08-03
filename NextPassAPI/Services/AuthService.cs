using NextPassAPI.Data.Models;
using NextPassAPI.Data.Repositories.Interfaces;
using BCrypt.Net;
using NextPassAPI.Data.Models.Requests;
using NextPassAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.OAuth;
using NextPassAPI.Identity.AuthHandler;
using NextPassAPI.Data.Enums;
using System.Security.Cryptography;
namespace NextPassAPI.Services
{
    public class AuthService : IAuthService
    {

        private readonly IUserRepository _userRepository;
        private readonly AuthHandler _authHandler;
        private readonly INotificationService _notificationService;

        public AuthService(IUserRepository userRepository, AuthHandler authHandler, INotificationService notificationService)
        {
            _userRepository = userRepository;
            _authHandler = authHandler;
            _notificationService = notificationService;
        }

        public async Task<User?> LoginUser(string email, string password)
        {
            var user = await _userRepository.GetUserByEmail(email);
            if (user != null && _authHandler.VerifyPassword(password, user.HashedPassword))
            {
                // Log the login event
                await _notificationService.NotifyUserLoginAsync(user.Id);
                return user;
            }
            return null;
        }

        public async Task<User> RegisterUser(UserRequest request)
        {
            string key = GenerateSecureKey();
            var user = new User
            {
                Email = request.Email,
                HashedPassword = _authHandler.HashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
                EncyptionKey = key,
                CreatedAt = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow,
                IsVerified = false,
                IsDeleted = false,
                Role = "User",
                AccountSetupStatus = "AccountCreated",
                AccountStatus = "Active"
            };
            return await _userRepository.CreateUser(user);
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
