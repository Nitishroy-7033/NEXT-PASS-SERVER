using NextPassAPI.Data.Models;
using NextPassAPI.Data.Models.Query;
using NextPassAPI.Data.Models.Requests;
using NextPassAPI.Data.Models.Responses;
using NextPassAPI.Data.Repositories.Interfaces;
using NextPassAPI.Services.Interfaces;
using System.Security.Claims;

namespace NextPassAPI.Services
{
    public class CredentialService : ICredentialService
    {
        private readonly ICredentialRepository _credentialRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CredentialService(ICredentialRepository credentialRepository, IHttpContextAccessor httpContextAccessor)
        {
            _credentialRepository = credentialRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<CredenatialResponse> GetCredentialAsync(GetCredentialQuery query)
        {
            var userId = _httpContextAccessor.HttpContext?.User
               .FindFirst("UserId")?.Value;
            return await _credentialRepository.GetCredentialAsync(query,userId);
        }


        public async Task<Credential> CreateCredentialAsnyc(CredentialRequest credentialRequest)
        {
            var userId = _httpContextAccessor.HttpContext?.User
                .FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token.");
            }
            var newCredential = new Credential
            {
                UserId = userId,
                SiteUrl = credentialRequest.SiteUrl,
                EmailId = credentialRequest.EmailId,
                Password = credentialRequest.Password,
                PhoneNumber = credentialRequest.PhoneNumber,
                IsPasswordCompromised = false,
                UserName = credentialRequest.UserName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            return await _credentialRepository.CreateCredentialAsnyc(newCredential);
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
