using System.Text.Json;
using System.Text.Json.Serialization;
using CodeQualityToGitlab;
using FluentAssertions;

namespace Test;

public class TestTransform
{
    [Fact]
    public void TestTransformAllWorks()
    {
        var target = new FileInfo(Path.GetTempFileName());

        Transform.TransformAll("**/**.sarif.json", "**/*roslynator.xml", target, null, true);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = new LowerCaseNamingPolicy(),
            Converters = { new JsonStringEnumConverter() }
        };

        using var r = new StreamReader(target.FullName);
        var json = r.ReadToEnd();
        var result = JsonSerializer.Deserialize<List<CodeQuality>>(json, options);

        result.Should().HaveCount(8);
    }
}
