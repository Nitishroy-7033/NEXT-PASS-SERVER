using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPassAPI.Data.Models.Requests
{
    public class DatabaseUpdateRequest
    {
        public string? DatabaseString { get; set; }
        public string? DataBaseType { get; set; } = "NEXT_PASS";
    }
}
