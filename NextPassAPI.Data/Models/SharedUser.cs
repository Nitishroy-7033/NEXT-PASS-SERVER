using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NextPassAPI.Data.Models;

[BsonIgnoreExtraElements]
public class SharedUser
{   
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; }

    public string? Username { get; set; }

    public string? Profile { get; set; }
}
