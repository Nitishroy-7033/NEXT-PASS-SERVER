using System;

namespace NextPassAPI.Data.Models;

public class MetaData
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string DatabaseType { get; set; } = "NEXT-PASS";
    public string DatabaseString { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string[] Category { get; set; }
    public int Pin { get; set; }
}
