using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using CodeQualityToGitlab;
using FluentAssertions;

namespace Test;

public class TestRoslynator
{
    [Fact]
    public void TestRoslynatorWorks()
    {
        var source = new FileInfo("roslynator.xml" );
        var target = new FileInfo(Path.GetTempFileName());

        RoslynatorConverter.ConvertToCodeQuality(source, target, "C:\\dev" + Path.DirectorySeparatorChar);
        
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

         result.Should().HaveCount(3);
         var codeQuality = result!.First();
         codeQuality.Description.Should().Be("CA1829: Use Length/Count property instead of Count() when available");
         codeQuality.Severity.Should().Be(Severity.info);

         if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
         {
             // seems not to work on non windows atm
             codeQuality.Location.Path.Should().Be("example\\TestFirebirdImport.cs");
         }

         codeQuality.Location.Lines.Begin.Should().Be(80);
    }
}