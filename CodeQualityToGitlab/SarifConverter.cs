namespace CodeQualityToGitlab;

using Serilog;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.VersionOne;
using Newtonsoft.Json;

public static class SarifConverter
{
    public static void ConvertToCodeQuality(FileInfo source, FileInfo target, string pathRoot)
    {
        Log.Warning("We currently assume that the given Sarif v1.0, Sarif 2.0 is not supported");
        var logContents = File.ReadAllText(source.FullName);
        var uri = new Uri(pathRoot);
        var settings = new JsonSerializerSettings
        {
            ContractResolver = SarifContractResolverVersionOne.Instance
        };

        var log = JsonConvert.DeserializeObject<SarifLogVersionOne>(logContents, settings);
        
        var cqrs = new List<CodeQuality>();
        foreach (var result in log.Runs.SelectMany(x => x.Results))
        {
            var begin = result.Locations.First();
            
            var cqr = new CodeQuality
            {
                Description = $"{result.RuleId}: {result.Message}",
                Severity = GetSeverity(result.Level),
                Location = new LocationCq
                {
                    Path = uri.MakeRelativeUri(begin.ResultFile.Uri).ToString().Replace("//", "\\"),
                    Lines = new Lines { Begin = begin.ResultFile.Region.StartLine }
                },
                Fingerprint = Common.GetHash($"{result.RuleId}|{begin.ResultFile.Uri}|{begin.ResultFile.Region.StartLine}")
            };
            
            cqrs.Add(cqr);
        }
        
        Common.WriteToDisk(target, cqrs);
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