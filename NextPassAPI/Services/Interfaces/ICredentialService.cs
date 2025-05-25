using NextPassAPI.Data.Models;
using NextPassAPI.Data.Models.Query;
using NextPassAPI.Data.Models.Requests;
using NextPassAPI.Data.Models.Responses;

namespace NextPassAPI.Services.Interfaces
{
    public interface ICredentialService
    {
        Task<CredenatialResponse> GetCredentialAsync(GetCredentialQuery query);
        Task<Credential> CreateCredentialAsnyc(CredentialRequest credentialRequest);
        Task<bool> UpdateCredentialAsync(string id, CredentialRequest credentialRequest);
        Task<bool> DeleteCredentialAsync(string credentialId);
        Task<bool> InviteUserAsync(string credentialId, string invitedUserId);
        Task<bool> RevokeUserAccessAsync(string credentialId, string revokeUserId);

    }
}
