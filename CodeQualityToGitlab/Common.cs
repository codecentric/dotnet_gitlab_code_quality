using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CodeQualityToGitlab;

internal static class Common
{
    public static string GetHash(string input)
    {
        var inputBytes = Encoding.ASCII.GetBytes(input);
        var hashBytes = MD5.HashData(inputBytes);

        return Convert.ToHexString(hashBytes);
    }

    public static void WriteToDisk(FileInfo target, List<CodeQuality> result)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = new LowerCaseNamingPolicy(),
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };
        using var fileStream = File.Create(target.FullName);
        using var utf8JsonWriter = new Utf8JsonWriter(fileStream);
        JsonSerializer.Serialize(utf8JsonWriter, result, options);
        Console.WriteLine($"Result written to: {target.FullName}");
    }
}