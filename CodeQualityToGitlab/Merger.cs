using System.Text.Json;
using System.Text.Json.Serialization;

namespace CodeQualityToGitlab;

public static class Merger
{
    public static void Merge(FileInfo[] sources, FileInfo target)
    {
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
            catch (Exception)
            {
                Console.WriteLine($"Error while deserializing file {source.FullName}");
                throw;
            }
        }
        
        Common.WriteToDisk(target, result);
    }
}