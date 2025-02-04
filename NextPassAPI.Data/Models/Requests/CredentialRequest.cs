using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPassAPI.Data.Models.Requests
{
    public class CredentialRequest
    {
        public string SiteUrl { get; set; }
        public string EmailId { get; set; }
        public string Password { get; set; }
        public string? UserName { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
