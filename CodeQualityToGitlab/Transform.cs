using CodeQualityToGitlab.SarifConverters;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Serilog;

namespace CodeQualityToGitlab;

public static class Transform
{
    private static IEnumerable<CodeQuality> TransformAllRaw(
        string sarifGlob,
        string roslynatorGlob,
        string? pathRoot
    )
    {
        var allIssues = new List<CodeQuality>();
        Process(sarifGlob, pathRoot, allIssues, SarifConverter.ConvertToCodeQualityRaw);
        Process(roslynatorGlob, pathRoot, allIssues, RoslynatorConverter.ConvertToCodeQualityRaw);

        return allIssues;
    }

    private static void Process(
        string roslynatorGlob,
        string? pathRoot,
        List<CodeQuality> allIssues,
        Func<FileInfo, string?, List<CodeQuality>> processFunc
    )
    {
        Matcher matcher = new();
        matcher.AddIncludePatterns([roslynatorGlob]);

        const string searchDirectory = ".";

        var currentDir = new DirectoryInfo(searchDirectory);
        var result = matcher.Execute(new DirectoryInfoWrapper(currentDir));

        if (!result.HasMatches)
        {
            Log.Warning(
                "No matching files found for pattern: {Pattern} in {CurrentDir}",
                roslynatorGlob,
                currentDir.FullName
            );
        }

        foreach (var match in result.Files)
        {
            var toTransform = match.Path;
            Log.Information("Processing: {File}", toTransform);
            var cqrs = processFunc(new(toTransform), pathRoot);
            allIssues.AddRange(cqrs);
        }
    }

    public static void TransformAll(
        string sarifGlob,
        string roslynatorGlob,
        FileInfo target,
        string? pathRoot,
        bool bumpToMajor
    )
    {
        var cqrs = TransformAllRaw(sarifGlob, roslynatorGlob, pathRoot);

        cqrs = cqrs.DistinctBy(x => x.Fingerprint).ToList();

        if (bumpToMajor)
        {
            foreach (var cqr in cqrs.Where(cqr => cqr.Severity is Severity.minor or Severity.info))
            {
                cqr.Severity = Severity.major;
            }
        }

        Common.WriteToDisk(target, cqrs);
    }
}
