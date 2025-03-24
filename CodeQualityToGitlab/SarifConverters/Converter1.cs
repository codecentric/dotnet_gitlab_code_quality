using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.VersionOne;
using Newtonsoft.Json;
using Serilog;

namespace CodeQualityToGitlab.SarifConverters;

public class Converter1(FileInfo source, string? pathRoot)
{
    public List<CodeQuality> Convert()
    {
        Log.Information("Sarif Version 1 detected");

        var logContents = File.ReadAllText(source.FullName);

        var settings = new JsonSerializerSettings
        {
            ContractResolver = SarifContractResolverVersionOne.Instance
        };

        var log = JsonConvert.DeserializeObject<SarifLogVersionOne>(logContents, settings);

        var results =
            log?.Runs
                .SelectMany(x => x.Results)
                .Where(r => r.SuppressionStates == SuppressionStatesVersionOne.None) ?? [ ];

        var cqrs = new List<CodeQuality>();
        foreach (var result in results)
        {
            var begin = result.Locations?.FirstOrDefault();

            if (begin == null)
            {
                Log.Warning("An issue has no location, skipping: {Result}", result.Message);
                continue;
            }

            try
            {
                var cqr = new CodeQuality
                {
                    Description = $"{result.RuleId}: {result.Message}",
                    Severity = GetSeverity(result.Level),
                    Location = new()
                    {
                        Path = GetPathOld(pathRoot, begin),
                        Lines = new() { Begin = begin.ResultFile.Region.StartLine }
                    },
                    Fingerprint = Common.GetHash(
                        $"{result.RuleId}|{begin.ResultFile.Uri}|{begin.ResultFile.Region.StartLine}"
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

    private static string GetPathOld(string? pathRoot, LocationVersionOne begin)
    {
        // nullability says Uri is always set, but there are tools which omit this.
        if (begin.ResultFile.Uri == null)
        {
            Log.Error(
            "There is no valid Path for the issue {@Region}, cannot create a path. Check the source sarif for missing physicalLocation.uri",
            begin.ResultFile.Region
            );
            return "noPathInSourceSarif";
        }

        if (!begin.ResultFile.Uri!.IsAbsoluteUri)
        {
            return begin.ResultFile.Uri.ToString();
        }

        if (string.IsNullOrWhiteSpace(pathRoot))
        {
            return begin.ResultFile.Uri.LocalPath.Replace("//", "\\");
        }
        var rootUri = GetUri(pathRoot);
        return rootUri.MakeRelativeUri(begin.ResultFile.Uri).ToString().Replace("//", "\\");
    }


    private static Uri GetUri(string pathRoot)
    {
        if (Path.IsPathRooted(pathRoot))
        {
            return new(new Uri("file://"),pathRoot);
        }

        return new(pathRoot);
    }

    private static Severity GetSeverity(ResultLevelVersionOne resultLevel)
    {
        return resultLevel switch
        {
            ResultLevelVersionOne.NotApplicable => Severity.minor,
            ResultLevelVersionOne.Pass => Severity.minor,
            ResultLevelVersionOne.Note => Severity.minor,
            ResultLevelVersionOne.Warning => Severity.major,
            ResultLevelVersionOne.Default => Severity.major,
            ResultLevelVersionOne.Error => Severity.blocker,
            _ => throw new ArgumentOutOfRangeException(nameof(resultLevel), resultLevel, null)
        };
    }

    private static string NormalizeSeparators(string source)
    {
        return source.Replace(@"\\", @"\").Replace("//", @"\");
    }
}
