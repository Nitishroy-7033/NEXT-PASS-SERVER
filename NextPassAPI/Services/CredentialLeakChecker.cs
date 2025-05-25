using System;
using System.Security.Cryptography;
using System.Text;
using NextPassAPI.Services.Interfaces;

namespace NextPassAPI.Services;

public class CredentialLeakChecker : ICredentialLeakChecker
{
    private static readonly HttpClient _httpClient = new HttpClient();

    public async Task<bool> IsPasswordLeakedAsync(string password)
    {
        string hashedPassword = ComputeSHA1Hash(password);
        string prefix = hashedPassword.Substring(0, 5);
        string suffix = hashedPassword.Substring(5);

        string url = $"https://api.pwnedpasswords.com/range/{prefix}";
        var response = await _httpClient.GetStringAsync(url);

        return response.Split("\n").Any(line => line.StartsWith(suffix, StringComparison.OrdinalIgnoreCase));
    }

    private static string ComputeSHA1Hash(string input)
    {
        using (SHA1 sha1 = SHA1.Create())
        {
            byte[] hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToUpper();
        }
    }
    public async Task<List<string>> GetBreachedSitesAsync(string email)
    {
        string apiKey = "YOUR_HIBP_API_KEY";
        string url = $"https://haveibeenpwned.com/api/v3/breachedaccount/{email}";

        _httpClient.DefaultRequestHeaders.Add("hibp-api-key", apiKey);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "NextPassAPI");

        var response = await _httpClient.GetAsync(url);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return new List<string>(); // No breaches found

        var breachData = await response.Content.ReadAsStringAsync();
        var breachList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BreachInfo>>(breachData);

        return breachList.Select(b => b.Name).ToList();
    }

    public class BreachInfo
    {
        public string Name { get; set; }
    }
}
