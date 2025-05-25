using NextPassAPI.Data.Models;
using NextPassAPI.Data.Models.Query;
using NextPassAPI.Data.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPassAPI.Data.Repositories.Interfaces
{
    public interface ICredentialRepository
    {
        Task<CredenatialResponse> GetCredentialAsync(GetCredentialQuery query);
        Task<Credential> CreateCredentialAsync(Credential newCredential);
        Task<bool> UpdateCredentialAsync(string id,Credential updatedCredential);
        Task<bool> DeleteCredentialAsync(string credentialId);
        Task<bool> CanUserEditCredentialAsync(string credentialId, string currentUserId);
        Task<bool> InviteUserAsync(string ownerId, string invitedUserId, string credentialId);
        Task<bool> RevokeUserAccessAsync(string ownerId, string credentialId, string revokeUserId);
        Task<Credential> GetCredentialByIdAsync(string credentialId);
    }
}
