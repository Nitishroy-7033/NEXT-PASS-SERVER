using NextPassAPI.Data.Models.Requests;
using NextPassAPI.Data.Models;
using NextPassAPI.Data.Repositories.Interfaces;
using NextPassAPI.Identity.Utils;
using NextPassAPI.Services.Interfaces;

namespace NextPassAPI.Services
{
    public class PasswordHelperService : IPasswordHelperService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserRepository _userRepository;

        public PasswordHelperService(IHttpContextAccessor httpContextAccessor,IUserRepository userRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _userRepository = userRepository;
        }

        public async Task<string>  GetEncryptedPassword(string password)
        {
            var userId = _httpContextAccessor.HttpContext?.User
               .FindFirst("UserId")?.Value;
            User user = await _userRepository.GetUserById(userId);
            var passwordHelper = new EncryptionHelper(user.EncyptionKey);
            var encryptedPassword = passwordHelper.Encrypt(password);
            return encryptedPassword;
        }

        public async Task<string> GetDecryptedPassword(string encryptedPassword)
        {
            var userId = _httpContextAccessor.HttpContext?.User
               .FindFirst("UserId")?.Value;
            User user = await _userRepository.GetUserById(userId);
            var passwordHelper = new EncryptionHelper(user.EncyptionKey);
            var password = passwordHelper.Decrypt(encryptedPassword);
            return password;
        }
    }
}
