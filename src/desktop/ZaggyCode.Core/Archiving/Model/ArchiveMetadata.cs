namespace ZaggyCode.Core.Archiving.Model;

[XmlRoot("zaggy-code-file-metadata")]
public class ArchiveMetadata
{
    [XmlElement("name")]
    public required string Name { get; set; }
    
    [XmlElement("description")]
    public string? Description { get; set; }
    
    [XmlElement("created-at-version")]
    public required Version CreatedAtVersion { get; set; }
}