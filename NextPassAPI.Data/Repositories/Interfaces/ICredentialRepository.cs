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
        Task<CredenatialResponse> GetCredentialAsync(GetCredentialQuery query,string userId);
        Task<Credential> CreateCredentialAsnyc(Credential newCredential);
        Task<bool> UpdateCredentialAsync(Credential updatedCredential);
        Task<bool> DeleteCredentialAsync(string credentialId);

    }
}
