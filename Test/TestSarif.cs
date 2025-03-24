using System.Text.Json;
using System.Text.Json.Serialization;
using CodeQualityToGitlab;
using CodeQualityToGitlab.SarifConverters;
using FluentAssertions;

namespace Test;

public class TestSarif
{
    [Fact]
    public void TestSarifWorks()
    {
        var source = new FileInfo("codeanalysis.sarif.json");
        var target = new FileInfo(Path.GetTempFileName());

        SarifConverter.ConvertToCodeQuality(
            source,
            target,
            $"C:{Path.DirectorySeparatorChar}dev" + Path.DirectorySeparatorChar
        );

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = new LowerCaseNamingPolicy(),
            Converters = { new JsonStringEnumConverter() }
        };

        using var r = new StreamReader(target.FullName);
        var json = r.ReadToEnd();
        var result = JsonSerializer.Deserialize<List<CodeQuality>>(json, options);

        result.Should().HaveCount(1);
        var codeQuality = result!.First();
        codeQuality
            .Description
            .Should()
            .Be(
                "CS8618: Non-nullable property 'Name' must contain a non-null value when exiting constructor. Consider declaring the property as nullable."
            );
        codeQuality.Severity.Should().Be(Severity.major);
        codeQuality.Location.Path.Should().Be("example\\Reader.cs");
        codeQuality.Location.Lines.Begin.Should().Be(12);
    }

    [Fact]
    public void TestSarifWorks2()
    {
        var source = new FileInfo("codeanalysis.sarif2.json");
        var target = new FileInfo(Path.GetTempFileName());

        SarifConverter.ConvertToCodeQuality(source, target);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = new LowerCaseNamingPolicy(),
            Converters = { new JsonStringEnumConverter() }
        };

        using var r = new StreamReader(target.FullName);
        var json = r.ReadToEnd();
        var result = JsonSerializer.Deserialize<List<CodeQuality>>(json, options);

        result.Should().HaveCount(7);
        var codeQuality = result!.First();
        codeQuality
            .Location
            .Path
            .Should()
            .Be("/builds/christian.sauer/net_examle/ClassLibrary1/Class1.cs");
        codeQuality.Location.Lines.Begin.Should().Be(26);
    }

    [Fact]
    public void TestSarifWorks3()
    {
        var source = new FileInfo("codeanalysis.sarif3.json");
        var target = new FileInfo(Path.GetTempFileName());

        SarifConverter.ConvertToCodeQuality(source, target);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = new LowerCaseNamingPolicy(),
            Converters = { new JsonStringEnumConverter() }
        };

        using var r = new StreamReader(target.FullName);
        var json = r.ReadToEnd();
        var result = JsonSerializer.Deserialize<List<CodeQuality>>(json, options);

        result.Should().HaveCount(0);
    }

    [Fact]
    public void TestCrashWorks()
    {
        var source = new FileInfo("crash.sarif.json");
        var target = new FileInfo(Path.GetTempFileName());

        SarifConverter.ConvertToCodeQuality(source, target);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = new LowerCaseNamingPolicy(),
            Converters = { new JsonStringEnumConverter() }
        };

        using var r = new StreamReader(target.FullName);
        var json = r.ReadToEnd();
        var result = JsonSerializer.Deserialize<List<CodeQuality>>(json, options);

        result.Should().HaveCount(1);
    }

    [Fact]
    public void TestSarif21Works()
    {
        var source = new FileInfo("codeanalysis.sarif21.json");
        var target = new FileInfo(Path.GetTempFileName());

        SarifConverter.ConvertToCodeQuality(source, target);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = new LowerCaseNamingPolicy(),
            Converters = { new JsonStringEnumConverter() }
        };

        using var r = new StreamReader(target.FullName);
        var json = r.ReadToEnd();
        var result = JsonSerializer.Deserialize<List<CodeQuality>>(json, options);

        result.Should().HaveCount(1);
        result!
            .First()
            .Should()
            .BeEquivalentTo(
                new CodeQuality
                {
                    Description = "no-unused-vars: Microsoft.CodeAnalysis.Sarif.Message",
                    Fingerprint = "75510FEE03EDAC9F5241783C86ACBFEB",
                    Severity = Severity.blocker,
                    Location = new()
                    {
                        Path =
                            @"C:\dev\sarif\sarif-tutorials\samples\Introduction\simple-example.js",
                        Lines = new() { Begin = 1 }
                    }
                }
            );
    }
}
