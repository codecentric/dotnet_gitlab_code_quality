using System.Diagnostics.CodeAnalysis;

namespace CodeQualityToGitlab;

public class CodeQuality
{
    public required string Description { get; set; }
    public required string Fingerprint { get; set; }
    public required Severity Severity { get; set; }
    public required LocationCq Location { get; set; }
}

public class LocationCq
{
    public required string Path { get; set; }
    public required Lines Lines { get; set; }
}

public class Lines
{
    public required int Begin { get; set; }
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum Severity
{
    info,
    minor,
    major,
    critical,
    blocker
}
