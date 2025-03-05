using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NextPassAPI.Data.Enums;

namespace NextPassAPI.Data.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } 
        public string Email { get; set; } 
        public string HashedPassword { get; set; } 
        public string Salt { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastLogin { get; set; } = DateTime.UtcNow;
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? UserName { get; set; }
        public string EncyptionKey { get; set; } 
        public string? PhoneNumber { get; set; }
        public string? SecurityQuestion { get; set; }
        public string? SecurityAnswer { get; set; }
        public string? SecurityKey { get; set; }
        public bool IsVerified { get; set; } = false;
        public DateTime? VerificationDate { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ProfilePicture { get; set; } = null;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string Role { get; set; } 
        public bool IsDeleted { get; set; } = false;
        public bool IsTwoFactorEnabled { get; set; } = false; // new property
        public string? TwoFactorSecret { get; set; }   // new property
        public string? DatabaseString { get; set; } 
        public string? DataBaseType { get; set; } = "NEXT_PASS";

    }
}
