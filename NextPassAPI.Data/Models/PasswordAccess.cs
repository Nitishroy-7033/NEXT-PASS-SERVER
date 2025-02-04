using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPassAPI.Data.Models
{
    public class PasswordAccess
    {
        public string DeviceName { get; set; }
        public string UserId { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
        public string IpAddress { get; set; }
        public string DeviceOS { get; set; }
        public string DeviceType { get; set; }
    }
}
