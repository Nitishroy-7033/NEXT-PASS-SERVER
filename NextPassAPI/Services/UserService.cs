using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NextPassAPI.Data.DbContexts;
using NextPassAPI.Data.Models;
using NextPassAPI.Data.Models.Requests;
using NextPassAPI.Data.Repositories.Interfaces;
using NextPassAPI.Identity.AuthHandler;
using NextPassAPI.Services.Interfaces;
using System.Security.Cryptography;
using System.Globalization;

namespace NextPassAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOptions<MongoDbConfigs> _mongoDBSettings;
        private readonly AuthHandler _authHandler;
        private readonly IConfiguration _configuration;

        public UserService(
            IUserRepository userRepository,
            AuthHandler authHandler,
            IHttpContextAccessor httpContextAccessor,
            IOptions<MongoDbConfigs> mongoDBSettings,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _authHandler = authHandler;
            _httpContextAccessor = httpContextAccessor;
            _mongoDBSettings = mongoDBSettings;
            _configuration = configuration;
        }

        public async Task<User> GetUserByEmail(string email) => await _userRepository.GetUserByEmail(email);
        public async Task<User> GetUserById(string id) => await _userRepository.GetUserById(id);
        public async Task<List<User>> GetAllUser() => await _userRepository.GetAllUser();
        public async Task<bool> DeleteUser(string userId) => await _userRepository.DeleteUser(userId);

        public async Task<bool> UpdateUser(User updatedUser)
        {
            updatedUser.UpdatedAt = DateTime.UtcNow;
            return await _userRepository.UpdateUser(updatedUser);
        }

        public async Task<User> UpdateDatabaseSettings(DatabaseUpdateRequest databaseUpdateRequest)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User ID not found.");
            }

            if (databaseUpdateRequest.DataBaseType == "NEXT_PASS")
            {
                databaseUpdateRequest.DatabaseString = _mongoDBSettings.Value.ConnectionString;
            }
            else if (databaseUpdateRequest.DataBaseType == "USER")
            {
                if (!IsValidMongoDbConnection(databaseUpdateRequest.DatabaseString))
                {
                    throw new Exception("Invalid MongoDB connection string.");
                }
            }
            return await _userRepository.UpdateDatabaseSettings(userId, databaseUpdateRequest);
        }

        public async Task<bool> UpdatePassword(string oldPassword, string newPassword)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User ID not found.");
            }

            var user = await _userRepository.GetUserById(userId);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            if (!_authHandler.VerifyPassword(oldPassword, user.HashedPassword))
            {
                return false; // Incorrect old password
            }

            var newPasswordHash = _authHandler.HashPassword(newPassword);
            return await _userRepository.ChangePassword(userId, newPasswordHash) != null;
        }

        private string GetUserIpAddress()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return "Unknown";
            }

            // Check for proxy headers first (e.g., if behind a reverse proxy or load balancer)
            var forwardedHeader = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedHeader))
            {
                return forwardedHeader.Split(',').First().Trim(); // Get the first IP in the list
            }

            // Fallback to direct connection IP
            return httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }


        private async Task<bool> WhitelistUserIp(string userIp)
        {
            try
            {
                var publicKey = _configuration["MongoDB:PublicApiKey"];
                var privateKey = _configuration["MongoDB:PrivateApiKey"];
                var projectId = _configuration["MongoDB:ProjectId"];

                if (string.IsNullOrEmpty(publicKey) || string.IsNullOrEmpty(privateKey) || string.IsNullOrEmpty(projectId))
                {
                    throw new Exception("MongoDB API credentials are missing.");
                }

                var apiUrl = $"https://cloud.mongodb.com/api/atlas/v1.0/groups/{projectId}/accessList";

                var jsonBody = JsonSerializer.Serialize(new
                {
                    ipAddress = userIp,
                    comment = "User's dynamic IP"
                });

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Generate HMAC-SHA1 Signature
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                var signature = GenerateHmacSignature(privateKey, timestamp);

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("MongoDB-HMAC-SHA1",
                    $"public_key={publicKey}, timestamp={timestamp}, signature={signature}");

                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(apiUrl, content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error whitelisting IP: {ex.Message}");
                return false;
            }
        }

        // Generate HMAC-SHA1 Signature
        private static string GenerateHmacSignature(string privateKey, string timestamp)
        {
            using var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(privateKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(timestamp));
            return BitConverter.ToString(hash).Replace("-", "").ToLower(CultureInfo.InvariantCulture);
        }

        private bool IsValidMongoDbConnection(string connectionString)
        {
            try
            {
                var client = new MongoClient(connectionString);
                client.ListDatabaseNames();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
