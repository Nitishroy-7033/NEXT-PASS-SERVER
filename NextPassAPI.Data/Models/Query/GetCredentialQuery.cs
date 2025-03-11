using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPassAPI.Data.Models.Query
{
    public class GetCredentialQuery
    {
        public string? CredenatialId { get; set; }
        public string? Title { get; set; }
        public string? SortBy { get; set; }
        public string? SiteUrl { get; set; }
        public string? EmailId { get; set; }
        public int? CurrentPage { get; set; }
        public int? PageSize { get; set; } = 30;
    }
}
