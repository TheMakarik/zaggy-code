namespace ZaggyCode.Core.Archiving.Model;

public class ArchiveMetadata
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("created-at-version")]
    public required Version CreatedAtVersion { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is ArchiveMetadata other
               && Name == other.Name
               && Description == other.Description
               && CreatedAtVersion == other.CreatedAtVersion;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Description, CreatedAtVersion);
    }
}