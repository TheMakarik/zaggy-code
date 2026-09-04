using System.Text.Json.Serialization;

namespace ZaggyCode.Core.Archiving.Model;

public class ArchiveMetadata
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("created-at-version")]
    public required Version CreatedAtVersion { get; set; }
}