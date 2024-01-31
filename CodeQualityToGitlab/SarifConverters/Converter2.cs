using Microsoft.CodeAnalysis.Sarif;
using Serilog;

namespace CodeQualityToGitlab.SarifConverters;

public class Converter2(FileInfo source, string? pathRoot)
{
    public List<CodeQuality> Convert()
    {
        Log.Information("Sarif Version 2 detected");

        var log = SarifLog.Load(source.FullName);

        var results = log.Runs
                         .SelectMany(x => x.Results)
                         .Where(r => r.Suppressions == null || r.Suppressions.Any());

        var cqrs = new List<CodeQuality>();
        foreach (var result in results)
        {
            var begin = result.Locations?.FirstOrDefault();

            if (begin == null)
            {
                Log.Warning("An issue has no location, skipping: {@Result}", result.Message);
                continue;
            }

            try
            {
                var startLine = begin.PhysicalLocation.Region.StartLine;
                var cqr = new CodeQuality
                {
                    Description = $"{result.RuleId}: {result.Message}",
                    Severity = GetSeverity(result.Level),
                    Location = new()
                    {
                        Path = GetPath(pathRoot, begin),
                        Lines = new()
                        { Begin = startLine }
                    },
                    Fingerprint = Common.GetHash(
                    $"{result.RuleId}|{begin.PhysicalLocation.ArtifactLocation.Uri}|{startLine}"
                    )
                };
                cqrs.Add(cqr);
            }
            catch (Exception e)
            {
                Log.Error(e, "Could not convert {@Result}, skipping", result);
            }
        }

        return cqrs;
    }

    private static string GetPath(string? pathRoot, Microsoft.CodeAnalysis.Sarif.Location begin)
    {
        // nullability says Uri is always set, but there are tools which omit this.
        var artifactLocationUri = begin.PhysicalLocation.ArtifactLocation.Uri;
        if (artifactLocationUri == null)
        {
            Log.Error(
            "There is no valid Path for the issue {@Region}, cannot create a path. Check the source sarif for missing physicalLocation.ArtifactLocation.uri",
            begin.PhysicalLocation.ArtifactLocation
            );
            return "noPathInSourceSarif";
        }

        if (!artifactLocationUri.IsAbsoluteUri)
        {
            return artifactLocationUri.ToString();
        }

        if (string.IsNullOrWhiteSpace(pathRoot))
        {
            return artifactLocationUri.LocalPath.Replace("//", "\\");
        }
        var uri = new Uri(pathRoot);
        return uri.MakeRelativeUri(artifactLocationUri).ToString().Replace("//", "\\");
    }

    private static Severity GetSeverity(FailureLevel resultLevel)
    {
        return resultLevel switch
        {
            FailureLevel.None => Severity.minor,
            FailureLevel.Note => Severity.minor,
            FailureLevel.Warning => Severity.major,
            FailureLevel.Error => Severity.blocker,
            _ => throw new ArgumentOutOfRangeException(nameof(resultLevel), resultLevel, null)
        };
    }
}