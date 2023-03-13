namespace CodeQualityToGitlab;

using Serilog;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.VersionOne;
using Newtonsoft.Json;

public static class SarifConverter
{
    public static List<CodeQuality> ConvertToCodeQualityRaw(FileInfo source, string? pathRoot)
    {
        Log.Warning("We currently assume that the given Sarif v1.0, Sarif 2.0 is not supported");
        var logContents = File.ReadAllText(source.FullName);

        var settings = new JsonSerializerSettings
        {
            ContractResolver = SarifContractResolverVersionOne.Instance
        };

        var log = JsonConvert.DeserializeObject<SarifLogVersionOne>(logContents, settings);

        var cqrs = new List<CodeQuality>();
        foreach (var result in log.Runs.SelectMany(x => x.Results))
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
                        Location = new LocationCq
                        {
                            Path = GetPath(pathRoot, begin),
                            Lines = new Lines { Begin = begin.ResultFile.Region.StartLine }
                        },
                        Fingerprint = Common.GetHash($"{result.RuleId}|{begin.ResultFile.Uri}|{begin.ResultFile.Region.StartLine}")
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
    public static void ConvertToCodeQuality(FileInfo source, FileInfo target, string? pathRoot)
    {
        var cqrs = ConvertToCodeQualityRaw(source, pathRoot);
        Common.WriteToDisk(target, cqrs);
    }

    private static string GetPath(string? pathRoot, LocationVersionOne begin)
    {
        if (!begin.ResultFile.Uri.IsAbsoluteUri)
        {
            return begin.ResultFile.Uri.ToString();
        }

        if (string.IsNullOrWhiteSpace(pathRoot))
        {
            return begin.ResultFile.Uri.LocalPath.Replace("//", "\\");
        }
        var uri = new Uri(pathRoot);
        return uri.MakeRelativeUri(begin.ResultFile.Uri).ToString().Replace("//", "\\");
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
}