using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPassAPI.Data.Models.Responses
{
    public class CredenatialResponse
    {
        public List<Credential> Credentials { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalCount { get; set; }
    }
}
