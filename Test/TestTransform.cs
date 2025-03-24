using System.Text.Json;
using System.Text.Json.Serialization;
using CodeQualityToGitlab;
using FluentAssertions;

namespace Test;

public class TestTransform
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = new LowerCaseNamingPolicy(),
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public void TestTransformAllWorks()
    {
        var target = new FileInfo(Path.GetTempFileName());

        Transform.TransformAll("**/**.sarif.json", "**/*roslynator.xml", target, null, true);

        var options = JsonSerializerOptions;

        using var r = new StreamReader(target.FullName);
        var json = r.ReadToEnd();
        var result = JsonSerializer.Deserialize<List<CodeQuality>>(json, options);

        result.Should().HaveCount(8);
    }

    [Fact]
    public void TestTransformCreatesExpectedPath()
    {
        var target = new FileInfo(Path.GetTempFileName());

        Transform.TransformAll("codeanalysis.sarif4.json", "", target, @"/builds/39753701/backend", true);

        var options = JsonSerializerOptions;

        using var r = new StreamReader(target.FullName);
        var json = r.ReadToEnd();
        var result = JsonSerializer.Deserialize<List<CodeQuality>>(json, options);

        result.Should().HaveCount(8);
    }
}
