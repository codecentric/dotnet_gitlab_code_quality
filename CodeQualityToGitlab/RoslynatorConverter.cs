using System.Text.Json;
using System.Xml.Serialization;
using Serilog;

namespace CodeQualityToGitlab;

public static class RoslynatorConverter
{
    public static List<CodeQuality> ConvertToCodeQualityRaw(FileInfo source, string? pathRoot)
    {
        var serializer = new XmlSerializer(typeof(Roslynator));

        var result = new List<CodeQuality>();
        using Stream reader = new FileStream(source.FullName, FileMode.Open);
        var roslynator = (Roslynator)(
            serializer.Deserialize(reader) ?? throw new ArgumentException("no data")
        );
        foreach (var project in roslynator.CodeAnalysis.Projects.Project)
        {
            Log.Information("Working on {ProjectName}", project.Name);

            foreach (var diagnostic in project.Diagnostics.Diagnostic)
            {
                var lineNumber = GetLineNumber(diagnostic);
                var cqr = new CodeQuality
                {
                    Description = $"{diagnostic.Id}: {diagnostic.Message}",
                    Severity = GetSeverity(diagnostic.Severity),
                    Location = new()
                    {
                        Path = GetPath(diagnostic, project, pathRoot),
                        Lines = new() { Begin = lineNumber }
                    },
                    Fingerprint = Common.GetHash($"{project.Name}{diagnostic.Id}{lineNumber}")
                };

                result.Add(cqr);
            }
        }

        return result;
    }

    public static void ConvertToCodeQuality(FileInfo source, FileInfo target, string? pathRoot)
    {
        var cqrs = ConvertToCodeQualityRaw(source, pathRoot);
        Common.WriteToDisk(target, cqrs);
    }

    private static string GetPath(Diagnostic diagnostic, Project project, string? pathRoot)
    {
        var path = diagnostic.FilePath ?? project.FilePath;

        if (string.IsNullOrWhiteSpace(pathRoot))
        {
            return path;
        }

        var rv = path.Replace(pathRoot, "");
        return rv;
    }

    private static int GetLineNumber(Diagnostic diagnostic)
    {
        return diagnostic.Location != null ? Convert.ToInt32(diagnostic.Location.Line) : 1;
    }

    private static Severity GetSeverity(string diagnosticSeverity)
    {
        return diagnosticSeverity switch
        {
            "Info" => Severity.info,
            "Warning" => Severity.major,
            "Error" => Severity.critical,
            "Hidden" => Severity.minor,
            _
                => throw new ArgumentOutOfRangeException(
                    diagnosticSeverity,
                    $"unknown: {diagnosticSeverity}"
                )
        };
    }
}

public class LowerCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name) => name.ToLower();
}
