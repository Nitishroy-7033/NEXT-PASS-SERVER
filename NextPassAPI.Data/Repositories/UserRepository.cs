using System.Text.RegularExpressions;
using MongoDB.Driver;
using NextPassAPI.Data.DbContexts;
using NextPassAPI.Data.Models;
using NextPassAPI.Data.Models.Requests;
using NextPassAPI.Data.Repositories.Interfaces;

namespace NextPassAPI.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _user;

        public UserRepository(MongoDbContext<User> database)
        {
            _user = database.GetCollection();
        }

        public async Task<User> GetUserByEmail(string email)
        {
          return await _user.Find(user => Regex.IsMatch(user.Email, $"^{email}$", RegexOptions.IgnoreCase))
                      .FirstOrDefaultAsync();
        }
        public async Task<User> GetUserById(string id)
        {
            return await _user.Find(user => user.Id == id).FirstOrDefaultAsync();
        }
        public async Task<List<User>> GetAllUser()
        {
            return await _user.Find(user => true).ToListAsync();
        }
        public async Task<User> CreateUser(User newUser)
        {
            await _user.InsertOneAsync(newUser);
            return newUser;
        }

        public async Task<bool> UpdateUser(User updatedUser)
        {
            var result = await _user.ReplaceOneAsync(u => u.Id == updatedUser.Id, updatedUser);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteUser(string userId)
        {
            var result = await _user.DeleteOneAsync(u => u.Id == userId);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }

        public async Task<User> UpdateDatabaseSettings(string id, DatabaseUpdateRequest databaseUpdateRequest)
        {
            var update = Builders<User>.Update
                .Set(u => u.DatabaseString, databaseUpdateRequest.DatabaseString)
                .Set(u => u.DataBaseType, databaseUpdateRequest.DataBaseType);

            var options = new FindOneAndUpdateOptions<User>
            {
                ReturnDocument = ReturnDocument.After 
            };

            return await _user.FindOneAndUpdateAsync(u => u.Id == id, update, options);
        }

    }
}
