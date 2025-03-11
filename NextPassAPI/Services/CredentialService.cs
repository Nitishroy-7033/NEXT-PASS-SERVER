using NextPassAPI.Data.Enums;
using NextPassAPI.Data.Models;
using NextPassAPI.Data.Models.Query;
using NextPassAPI.Data.Models.Requests;
using NextPassAPI.Data.Models.Responses;
using NextPassAPI.Data.Repositories.Interfaces;
using NextPassAPI.Identity.Utils;
using NextPassAPI.Services.Interfaces;
using System.Security.Claims;

namespace NextPassAPI.Services
{
    public class CredentialService : ICredentialService
    {
        private readonly ICredentialRepository _credentialRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserRepository _userRepository;

        public CredentialService(ICredentialRepository credentialRepository, IHttpContextAccessor httpContextAccessor,
            IUserRepository userRepository)
        {
            _credentialRepository = credentialRepository;
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
        }


        public async Task<CredenatialResponse> GetCredentialAsync(GetCredentialQuery query)
        {
            return await _credentialRepository.GetCredentialAsync(query);
        }

        public async Task<Credential> CreateCredentialAsnyc(CredentialRequest credentialRequest)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User ID not found in token.");
            var user = await _userRepository.GetUserById(userId);
            var passwordHelper = new EncryptionHelper(user.EncyptionKey);
            var encryptedPassword = passwordHelper.Encrypt(credentialRequest.Password);
             
            var newCredential = new Credential
            {
                UserId = userId,
                Title = credentialRequest.Title ?? credentialRequest.EmailId,
                PasswordChangeReminder = credentialRequest.PasswordChangeReminder,
                PasswordStrength = credentialRequest.PasswordStrength ?? "weak",
                SiteUrl = credentialRequest.SiteUrl,
                Category=credentialRequest.Category,
                EmailId = credentialRequest.EmailId,
                Password = encryptedPassword,
                PhoneNumber = credentialRequest.PhoneNumber,
                IsPasswordCompromised = false,
                UserName = credentialRequest.UserName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
                
            };

            return await _credentialRepository.CreateCredentialAsync(newCredential);
        }

        public async Task<bool> UpdateCredentialAsync(CredentialRequest credentialRequest)
        {

            var updatedCredential = new Credential
            {
                SiteUrl = credentialRequest.SiteUrl,
                EmailId = credentialRequest.EmailId,
                Password = credentialRequest.Password,
                PhoneNumber = credentialRequest.PhoneNumber,
                IsPasswordCompromised = false,
                UserName = credentialRequest.UserName,
                UpdatedAt = DateTime.UtcNow
            };

            return await _credentialRepository.UpdateCredentialAsync(updatedCredential);
        }

        public async Task<bool> DeleteCredentialAsync(string credentialId)
        {
            return await _credentialRepository.DeleteCredentialAsync(credentialId);
        }


    }
}
