using System.CommandLine;
using CodeQualityToGitlab.SarifConverters;
using Serilog;

namespace CodeQualityToGitlab;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

        var sourceArgument = new Argument<FileInfo>(
            name: "source",
            description: "The file to convert"
        );

        var targetArgument = new Argument<FileInfo>(name: "target", description: "The target file");

        var bumpToMajorOption = new Option<bool>(
            name: "--all_major",
            "if true all info and minor issues are promoted to major for Gitlab"
        );

        var rootPathArgument = new Argument<string?>(
            name: "root",
            description: "The name root of the repository. Gitlab requires Code Quality issues to contain paths relative to the repository, "
                + "but the tools report them as absolute file paths. "
                + "Everything given in with this option will be removed. E.g. root is 'c:/dev' and the file name is something like 'c:/dev/myrepo/file.cs' it will transformed to 'myrepo/file.cs'. Can often be omitted. ",
            getDefaultValue: () => null
        );

        var rootCommand = new RootCommand("Tool to convert Dotnet-Formats to Gitlab code quality");
        var roslynatorToCodeQuality = new Command(
            "roslynator",
            "Convert Roslynator file to Code Quality issue"
        )
        {
            sourceArgument,
            targetArgument,
            rootPathArgument
        };

        var sarifToCodeQuality = new Command("sarif", "Convert Sarif files to Code Quality issue")
        {
            sourceArgument,
            targetArgument,
            rootPathArgument
        };

        var sourcesArgument = new Argument<FileInfo[]>(
            name: "sources",
            description: "The files to merge"
        );

        var mergeCodeQuality = new Command("merge", "Merge multiple code quality files into one")
        {
            targetArgument,
            sourcesArgument,
            bumpToMajorOption
        };

        var sourceGlobArgument = new Argument<string>(
            name: "sarifGlob",
            description: "Glob pattern for the sarif files",
            getDefaultValue: () => "**/*.sarif.json"
        );

        var sourceRoslynatorArgument = new Argument<string>(
            name: "roslynatorGlob",
            description: "Glob pattern for the roslynator files",
            getDefaultValue: () => "**/roslynator.xml"
        );

        var transformCodeQuality = new Command(
            "transform",
            "Transforms files from a glob mapping and merges them to one file"
        )
        {
            sourceGlobArgument,
            sourceRoslynatorArgument,
            targetArgument,
            rootPathArgument,
            bumpToMajorOption
        };

        roslynatorToCodeQuality.SetHandler(
            RoslynatorConverter.ConvertToCodeQuality,
            sourceArgument,
            targetArgument,
            rootPathArgument
        );
        sarifToCodeQuality.SetHandler(
            SarifConverter.ConvertToCodeQuality,
            sourceArgument,
            targetArgument,
            rootPathArgument
        );
        mergeCodeQuality.SetHandler(
            Merger.Merge,
            sourcesArgument,
            targetArgument,
            bumpToMajorOption
        );
        transformCodeQuality.SetHandler(
            Transform.TransformAll,
            sourceGlobArgument,
            sourceRoslynatorArgument,
            targetArgument,
            rootPathArgument,
            bumpToMajorOption
        );
        rootCommand.Add(roslynatorToCodeQuality);
        rootCommand.Add(sarifToCodeQuality);
        rootCommand.Add(mergeCodeQuality);
        rootCommand.Add(transformCodeQuality);

        return await rootCommand.InvokeAsync(args);
    }
}
