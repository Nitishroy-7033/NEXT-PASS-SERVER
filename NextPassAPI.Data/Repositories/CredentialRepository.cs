using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using NextPassAPI.Data.DbContexts;
using NextPassAPI.Data.Models;
using NextPassAPI.Data.Models.Query;
using NextPassAPI.Data.Models.Responses;
using NextPassAPI.Data.Repositories.Interfaces;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace NextPassAPI.Data.Repositories
{
    public class CredentialRepository : ICredentialRepository
    {
        private readonly MongoDbContext<Credential> _defaultDbContext;
        private readonly IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IMongoCollection<Credential> _credential;
        private User _user;
        public CredentialRepository(
            MongoDbContext<Credential> defaultDbContext,
            IUserRepository userRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _defaultDbContext = defaultDbContext;
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        private async Task GetCollectionAsync()
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token.");
            }

            var user = await _userRepository.GetUserById(userId);
            _user = user;
            if (user?.DataBaseType == "USER" && !string.IsNullOrEmpty(user.DatabaseString))
            {
                var mongoDbConfigs = Microsoft.Extensions.Options.Options.Create(new MongoDbConfigs
                {
                    ConnectionString = user.DatabaseString,
                    DatabaseName = "NEXT_PASS_DB"
                });
                var mongoDbContext = new MongoDbContext<Credential>(mongoDbConfigs);
                _credential = mongoDbContext.GetCollection();
            }
            else
            {
                _credential = _defaultDbContext.GetCollection();
            }
        }
        public async Task<Credential> GetCredentialByIdAsync(string credentialId)
        {
            await GetCollectionAsync();
            var credential = await _credential.Find(c => c.Id == credentialId).FirstOrDefaultAsync();
            return credential;
        }
        public async Task<CredenatialResponse> GetCredentialAsync(GetCredentialQuery query)
        {
            await GetCollectionAsync();

            var filterBuilder = Builders<Credential>.Filter;
            var filters = new List<FilterDefinition<Credential>>();

            if (!string.IsNullOrEmpty(query.CredenatialId))
                filters.Add(filterBuilder.Eq(c => c.Id, query.CredenatialId));
            if (!string.IsNullOrEmpty(query.Title))
                filters.Add(filterBuilder.Regex(c => c.Title, new BsonRegularExpression(query.Title, "i")));
            if (!string.IsNullOrEmpty(query.SiteUrl))
                filters.Add(filterBuilder.Regex(c => c.SiteUrl, new BsonRegularExpression(query.SiteUrl, "i")));
            if (!string.IsNullOrEmpty(query.EmailId))
                filters.Add(filterBuilder.Regex(c => c.EmailId, new BsonRegularExpression(query.EmailId, "i")));

            filters.Add(filterBuilder.Eq(e => e.UserId, _user.Id));

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

        public async Task<Credential> CreateCredentialAsync(Credential newCredential)
        {
            await GetCollectionAsync();
            await _credential.InsertOneAsync(newCredential);
            return newCredential;
        }

        public async Task<bool> UpdateCredentialAsync(string id ,Credential updatedCredential)
        {
            await GetCollectionAsync();
            var result = await _credential.ReplaceOneAsync(c => c.Id == id, updatedCredential);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteCredentialAsync(string credentialId)
        {
            await GetCollectionAsync();
            var result = await _credential.DeleteOneAsync(c => c.Id == credentialId);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }
        public async Task<bool> InviteUserAsync(string ownerId, string invitedUserId, string credentialId)
        {
            await GetCollectionAsync();

            if (ownerId == invitedUserId)
                return false; // User can't invite themselves

            var credential = await _credential.Find(c => c.Id == credentialId).FirstOrDefaultAsync();
            if (credential == null || credential.UserId != ownerId)
                return false; // Credential doesn't exist or requester is not the owner

            if (credential.SharedWith.Any(u => u.UserId == invitedUserId))
                return false; // Already invited

            var invitedUser = await _userRepository.GetUserById(invitedUserId);
            if (invitedUser == null)
                return false;

            var sharedUser = new SharedUser
            {
                UserId = invitedUserId,
                Username = invitedUser.FirstName + " " + invitedUser.LastName,
                Profile = invitedUser.ProfilePicture
            };
            var update = Builders<Credential>.Update.AddToSet(c => c.SharedWith, sharedUser);
            var result = await _credential.UpdateOneAsync(c => c.Id == credentialId, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }


        public async Task<bool> CanUserEditCredentialAsync(string credentialId, string currentUserId)
        {
            await GetCollectionAsync();
            var credential = await _credential.Find(c => c.Id == credentialId).FirstOrDefaultAsync();
            if (credential == null) return false;
            bool isOwner = credential.UserId == currentUserId;
            bool isShared = credential.SharedWith.Any(u => u.UserId == currentUserId);
            return isOwner || isShared;
        }
        public async Task<bool> RevokeUserAccessAsync(string ownerId, string credentialId, string revokeUserId)
        {
            await GetCollectionAsync();

            var credential = await _credential.Find(c => c.Id == credentialId).FirstOrDefaultAsync();

            if (credential == null || credential.UserId != ownerId)
                return false; // Only owner can revoke access

            var update = Builders<Credential>.Update.PullFilter(c => c.SharedWith, u => u.UserId == revokeUserId);
            var result = await _credential.UpdateOneAsync(c => c.Id == credentialId, update);

            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

    }
}
