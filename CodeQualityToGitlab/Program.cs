using System.CommandLine;

namespace CodeQualityToGitlab;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        var sourceArgument = new Argument<FileInfo>(
            name: "source",
            description: "The file to convert"
            );

        var targetArgument = new Argument<FileInfo>(
            name: "target",
            description: "The target file"
        );

        var bumpToMajorOption = new Option<bool>(name: "--all_major",
            "if true all info and minor issues are promoted to major for Gitlab");
        
        var rootPathArgument = new Argument<string?>(
            name: "root",
            description: "The name root of the repository. Gitlab requires Code Quality issues to contain paths relative to the repository, " +
                         "but the tools report them as absolute file paths. " +
                         "Everything given in with this option will be removed. E.g. root is 'c:/dev' and the file name is something like 'c:/dev/myrepo/file.cs' it will transformed to 'myrepo/file.cs'. Can often be omitted. ",
            getDefaultValue: () => null
        );
        
        var rootCommand = new RootCommand("Tool to convert Dotnet-Formats to Gitlab code quality");
        var roslynatorToCodeQuality = new Command("roslynator", "Convert Roslynator file to Code Quality issue")
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
        
        roslynatorToCodeQuality.SetHandler(RoslynatorConverter.ConvertToCodeQuality, sourceArgument, targetArgument, rootPathArgument);
        sarifToCodeQuality.SetHandler(SarifConverter.ConvertToCodeQuality, sourceArgument, targetArgument, rootPathArgument);
        mergeCodeQuality.SetHandler(Merger.Merge, sourcesArgument, targetArgument, bumpToMajorOption);
        
        rootCommand.Add(roslynatorToCodeQuality);
        rootCommand.Add(sarifToCodeQuality);
        rootCommand.Add(mergeCodeQuality);

        return await rootCommand.InvokeAsync(args);
    }
}