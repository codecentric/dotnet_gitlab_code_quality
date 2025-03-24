using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace CodeQualityToGitlab;

[DebuggerDisplay("{Description} {Severity} {Location.Path} {Location.Lines.Begin}")]
public class CodeQuality
{
    public required string Description { get; set; }
    public required string Fingerprint { get; set; }
    public required Severity Severity { get; set; }
    public required LocationCq Location { get; set; }
}

[DebuggerDisplay("{Path} {Lines.Begin}")]
public class LocationCq
{
    public required string Path { get; set; }
    public required Lines Lines { get; set; }
}

[DebuggerDisplay("{Begin}")]
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
