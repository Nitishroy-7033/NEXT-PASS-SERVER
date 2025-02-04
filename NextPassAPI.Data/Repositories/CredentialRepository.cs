using MongoDB.Driver;
using NextPassAPI.Data.DbContexts;
using NextPassAPI.Data.Models;
using NextPassAPI.Data.Models.Query;
using NextPassAPI.Data.Models.Responses;
using NextPassAPI.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextPassAPI.Data.Repositories
{
    public class CredentialRepository : ICredentialRepository
    {
        private readonly IMongoCollection<Credential> _credential;
        public CredentialRepository(MongoDbContext<Credential> database)
        {
            _credential = database.GetCollection();
        }

        public async Task<CredenatialResponse> GetCredentialAsync(GetCredentialQuery query)
        {
            var filterBuilder = Builders<Credential>.Filter;
            var filters = new List<FilterDefinition<Credential>>();
            if (!string.IsNullOrEmpty(query.CredenatialId))
                filters.Add(filterBuilder.Eq(c => c.Id, query.CredenatialId));
            if (!string.IsNullOrEmpty(query.SiteUrl))
                filters.Add(filterBuilder.Eq(c => c.SiteUrl, query.SiteUrl));
            if (!string.IsNullOrEmpty(query.EmailId))
                filters.Add(filterBuilder.Eq(c => c.EmailId, query.EmailId));

            var filter = filters.Count > 0 ? filterBuilder.And(filters) : filterBuilder.Empty;

            var totalCount = await _credential.CountDocumentsAsync(filter);
            var credentials = await _credential.Find(filter)
                                               .SortByDescending(c => c.CreatedAt)
                                               .Skip((query.CurrentPage.GetValueOrDefault(1) - 1) * query.PageSize.GetValueOrDefault(30))
                                               .Limit(query.PageSize.GetValueOrDefault(10))
                                               .ToListAsync();

            return new CredenatialResponse
            {
                Credentials = credentials,
                PageSize = query.PageSize.GetValueOrDefault(30),
                CurrentPage = query.CurrentPage.GetValueOrDefault(1),
                TotalCount = (int)totalCount
            };
        }

        public async Task<Credential> CreateCredentialAsnyc(Credential newCredential)
        {
            await _credential.InsertOneAsync(newCredential);
            return newCredential;
        }

        public async Task<bool> UpdateCredentialAsync(Credential updatedCredential)
        {
            var result = await _credential.ReplaceOneAsync(c => c.Id == updatedCredential.Id, updatedCredential);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteCredentialAsync(string credentialId)
        {
            var result = await _credential.DeleteOneAsync(c => c.Id == credentialId);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }
    }
}
