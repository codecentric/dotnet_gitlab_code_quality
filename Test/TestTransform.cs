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
    public void TestTHandlesDotsInPathsForSarif1()
    {
        var target = new FileInfo(Path.GetTempFileName());

        Transform.TransformAll("codeanalysis.sarif4.json", "", target, "/builds/folder/backend/", true);

        var options = JsonSerializerOptions;

        using var r = new StreamReader(target.FullName);
        var json = r.ReadToEnd();
        var result = JsonSerializer.Deserialize<List<CodeQuality>>(json, options);

        result.Should().NotBeNull();
        result!.First().Location.Path.Should().Contain("SR.CLI");

        result.Should().HaveCount(4);
    }
}
