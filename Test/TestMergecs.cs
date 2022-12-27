using System.Text.Json;
using System.Text.Json.Serialization;
using CodeQualityToGitlab;
using FluentAssertions;
using FluentAssertions.Execution;

namespace Test;

public class TestMerger
{
    [Fact]
    public void TestMergerWorks()
    {
        var source1 = new FileInfo("gl-quality1.json" );
        var source2 = new FileInfo("gl-quality2.json" );
        var target = new FileInfo(Path.GetTempFileName());

        Merger.Merge(new[] {source1, source2}, target);
        
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = new LowerCaseNamingPolicy(),
            Converters ={
                new JsonStringEnumConverter()
            }
        };

        using var r = new StreamReader(target.FullName);
        var json = r.ReadToEnd();
        var result = JsonSerializer.Deserialize<List<CodeQuality>>(json, options);
        
        result.Should().HaveCount(2);
        var codeQuality = result!.First();
        codeQuality.Description.Should().Be("CS8618: Non-nullable property 'Name' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.");
        codeQuality.Severity.Should().Be(Severity.major);
        codeQuality.Location.Lines.Begin.Should().Be(12);
        
    }
}