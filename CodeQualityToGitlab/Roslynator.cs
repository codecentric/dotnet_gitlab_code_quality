namespace CodeQualityToGitlab;

using System.Xml.Serialization;

[XmlRoot(ElementName = "Diagnostic")]
public class Diagnostic
{
    [XmlAttribute(AttributeName = "Id")]
    public required string Id { get; set; }

    [XmlAttribute(AttributeName = "Title")]
    public required string Title { get; set; }

    [XmlAttribute(AttributeName = "Count")]
    public required string Count { get; set; }

    [XmlElement(ElementName = "Severity")]
    public required string Severity { get; set; }

    [XmlElement(ElementName = "Message")]
    public required string Message { get; set; }

    [XmlElement(ElementName = "FilePath")]
    public string? FilePath { get; set; }

    [XmlElement(ElementName = "Location")]
    public Location? Location { get; set; }
}

[XmlRoot(ElementName = "Summary")]
public class Summary
{
    [XmlElement(ElementName = "Diagnostic")]
    public required List<Diagnostic> Diagnostic { get; set; } = [ ];
}

[XmlRoot(ElementName = "Location")]
public class Location
{
    [XmlAttribute(AttributeName = "Line")]
    public required string Line { get; set; }

    [XmlAttribute(AttributeName = "Character")]
    public required string Character { get; set; }
}

[XmlRoot(ElementName = "Diagnostics")]
public class Diagnostics
{
    [XmlElement(ElementName = "Diagnostic")]
    public List<Diagnostic> Diagnostic { get; set; } = [ ];
}

[XmlRoot(ElementName = "Project")]
public class Project
{
    [XmlElement(ElementName = "Diagnostics")]
    public required Diagnostics Diagnostics { get; set; }

    [XmlAttribute(AttributeName = "Name")]
    public required string Name { get; set; }

    [XmlAttribute(AttributeName = "FilePath")]
    public required string FilePath { get; set; }
}

[XmlRoot(ElementName = "Projects")]
public class Projects
{
    [XmlElement(ElementName = "Project")]
    public List<Project> Project { get; set; } = [ ];
}

[XmlRoot(ElementName = "CodeAnalysis")]
public class CodeAnalysis
{
    [XmlElement(ElementName = "Summary")]
    public required Summary Summary { get; set; }

    [XmlElement(ElementName = "Projects")]
    public required Projects Projects { get; set; }
}

[XmlRoot(ElementName = "Roslynator")]
public class Roslynator
{
    [XmlElement(ElementName = "CodeAnalysis")]
    public required CodeAnalysis CodeAnalysis { get; set; }
}
