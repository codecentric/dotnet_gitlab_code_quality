using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog;

namespace CodeQualityToGitlab;

public static class Merger
{
    public static void Merge(FileInfo[] sources, FileInfo target, bool bumpToMajor)
    {
        Log.Information("bump to major is: {BumpToMajor}", bumpToMajor);
        var result = new List<CodeQuality>();
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = new LowerCaseNamingPolicy(),
            Converters ={
                new JsonStringEnumConverter()
            }
        };

        foreach (var source in sources)
        {
            if (!source.Exists)
            {
                throw new FileNotFoundException($"The file '{source.FullName}' does not exist", source.FullName);
            }

            using var f = source.OpenRead();
            try
            {
                var data = JsonSerializer.Deserialize<List<CodeQuality>>(f, options);

                if (data == null)
                {
                    throw new ArgumentNullException($"could not deserialize content of {source.FullName}");
                }
                result.AddRange(data);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error while deserializing file {SourceFullName}", source.FullName);
                throw;
            }
        }

        result = result.DistinctBy(x => x.Fingerprint).ToList();
        
        if (bumpToMajor)
        {
            foreach (var cqr in result
                         .Where(cqr => cqr.Severity is Severity.minor or Severity.info))
            {
                cqr.Severity = Severity.major;
            }
        }
        
        Common.WriteToDisk(target, result);
    }
}