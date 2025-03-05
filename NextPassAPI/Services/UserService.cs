using NextPassAPI.Data.Models.Requests;
using NextPassAPI.Data.Models;
using NextPassAPI.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using NextPassAPI.Services.Interfaces;
using Microsoft.Extensions.Options;
using NextPassAPI.Data.DbContexts;

namespace NextPassAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOptions<MongoDbConfigs> _mongoDBSettings;
        public UserService(IUserRepository userRepository, IHttpContextAccessor httpContextAccessor, IOptions<MongoDbConfigs> mongoDBSettings) {

            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
            _mongoDBSettings = mongoDBSettings;
        }

        public async Task<User> GetUserByEmail(string email)
        {
            return await _userRepository.GetUserByEmail(email);
        }
        public async Task<User> GetUserById(string id)
        {
            return await _userRepository.GetUserById(id);
        }

        public async Task<bool> UpdateUser(User updatedUser)
        {
            updatedUser.UpdatedAt = DateTime.UtcNow;
            return await _userRepository.UpdateUser(updatedUser);
        }

        public async Task<bool> DeleteUser(string userId)
        {
            return await _userRepository.DeleteUser(userId);
        }

        public async Task<List<User>> GetAllUser()
        {
            return await _userRepository.GetAllUser();
        }

      public async  Task<User> UpdateDatabaseSettings(DatabaseUpdateRequest databaseUpdateRequest)
        {
            var userId = _httpContextAccessor.HttpContext?.User
               .FindFirst("UserId")?.Value;
            if(databaseUpdateRequest.DataBaseType=="NEXT_PASS")
            {
                databaseUpdateRequest.DatabaseString = _mongoDBSettings.Value.ConnectionString;
            }
                return await _userRepository.UpdateDatabaseSettings(userId, databaseUpdateRequest);
        }

    }
}
