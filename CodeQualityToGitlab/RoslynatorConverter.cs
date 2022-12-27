using System.Text.Json;
using System.Xml.Serialization;

namespace CodeQualityToGitlab;

public static class RoslynatorConverter
{
    public static void ConvertToCodeQuality(FileInfo source, FileInfo target, string pathRoot)
    {
        var serializer =
            new XmlSerializer(typeof(Roslynator));

        var result = new List<CodeQuality>();
        using Stream reader = new FileStream(source.FullName, FileMode.Open);
        var roslynator = (Roslynator)(serializer.Deserialize(reader) ?? throw new ArgumentException("no data"));
        foreach (var project in roslynator.CodeAnalysis.Projects.Project)
        {
            Console.WriteLine($"Working on {project.Name}");

            foreach (var diagnostic in project.Diagnostics.Diagnostic)
            {
                var lineNumber = GetLineNumber(diagnostic);
                var cqr = new CodeQuality
                {
                    Description = $"{diagnostic.Id}: {diagnostic.Message}",
                    Severity = GetSeverity(diagnostic.Severity),
                    Location = new LocationCq
                    {
                        Path = GetPath(diagnostic, project, pathRoot), 
                        Lines = new Lines { Begin = lineNumber }
                    },
                    Fingerprint = Common.GetHash($"{project.Name}{diagnostic.Id}{lineNumber}")
                };

                result.Add(cqr);
            }
        }
        Common.WriteToDisk(target, result);
    }

    private static string GetPath(Diagnostic diagnostic, Project project, string pathRoot)
    {
        var path = diagnostic.FilePath ?? project.FilePath;
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
            _ => throw new ArgumentOutOfRangeException(diagnosticSeverity, $"unknown: {diagnosticSeverity}")
        };
    }
}

public class LowerCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name) =>
        name.ToLower();
}
