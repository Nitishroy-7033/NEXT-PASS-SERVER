using NextPassAPI.Data.Models.Requests;
using NextPassAPI.Data.Models;

namespace NextPassAPI.Services.Interfaces
{
    public interface IUserService
    {
        Task<User> GetUserByEmail(string email);
        Task<bool> UpdateUser(User updatedUser);
        Task<bool> DeleteUser(string userId);
        Task<List<User>> GetAllUser();
        Task<User> UpdateDatabaseSettings(DatabaseUpdateRequest databaseUpdateRequest);
    }
}
