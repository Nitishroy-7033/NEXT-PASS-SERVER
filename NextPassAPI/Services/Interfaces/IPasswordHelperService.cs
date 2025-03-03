namespace NextPassAPI.Services.Interfaces
{
    public interface IPasswordHelperService
    {
        Task<string> GetEncryptedPassword(string password);
        Task<string> GetDecryptedPassword(string encryptedPassword);
    }
}
