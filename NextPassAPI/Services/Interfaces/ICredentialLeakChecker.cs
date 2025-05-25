using System;

namespace NextPassAPI.Services.Interfaces;

public interface ICredentialLeakChecker
{
    Task<bool> IsPasswordLeakedAsync(string password);
}
