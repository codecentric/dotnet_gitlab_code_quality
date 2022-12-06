using System.CommandLine;
using System.Text.Json;

namespace CodeQualityToGitlab;

// three commands:
// 1. Roslynator -> cqi
// 2. ms-style -> cqi
// 3 merge N -> cqi

// ReSharper disable once UnusedType.Global
class Program
{
    
    static async Task Main(string[] args)
    {
        var rootCommand = new RootCommand("Tool to convert Dotnet-Formats to Gitlab code quality");
        var roslynatorToCodeQuality = new Command("rosylnator", "Convert Rosylnator file to Code Quality issue");
        var sarifToCodeQuality = new Command("sarif", "Convert Sarif files to Code Quality issue");
        var mergeCodeQuality = new Command("merge-codequality", "Merge multiple code quality files into one");
        
        rootCommand.Add(roslynatorToCodeQuality);
        rootCommand.Add(sarifToCodeQuality);
        rootCommand.Add(mergeCodeQuality);

        rootCommand.SetHandler(() =>
        {
            Console.WriteLine("Hello world!");
        });

        await rootCommand.InvokeAsync(args);
        
        //Environment.CurrentDirectory + Path.DirectorySeparatorChar
    }
    
    /// <param name="source">Source Roslynator file</param>
    /// <param name="target">Code Quality target file</param>
    // ReSharper disable once UnusedMember.Local
    static void Main2(FileInfo source, FileInfo target)
    {
        Console.WriteLine($"source: {source}");
        Console.WriteLine($"target: {target}");

        if (!source.Exists)
        {
            throw new ArgumentOutOfRangeException($"{source} does not exist");
        }
    }

   
}

