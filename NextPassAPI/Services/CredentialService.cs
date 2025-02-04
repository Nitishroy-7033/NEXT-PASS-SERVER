using NextPassAPI.Data.Models;
using NextPassAPI.Data.Models.Query;
using NextPassAPI.Data.Models.Requests;
using NextPassAPI.Data.Models.Responses;
using NextPassAPI.Data.Repositories.Interfaces;
using NextPassAPI.Services.Interfaces;

namespace NextPassAPI.Services
{
    public class CredentialService : ICredentialService
    {
        private readonly ICredentialRepository _credentialRepository;
        public CredentialService(ICredentialRepository credentialRepository)
        {
            _credentialRepository = credentialRepository;
        }


        public async Task<CredenatialResponse> GetCredentialAsync(GetCredentialQuery query)
        {
            return await _credentialRepository.GetCredentialAsync(query);
        }


        public async Task<Credential> CreateCredentialAsnyc(CredentialRequest credentialRequest)
        {
            var newCredential = new Credential
            {
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
