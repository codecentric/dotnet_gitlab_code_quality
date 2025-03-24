namespace CodeQualityToGitlab.SarifConverters;

public static class SarifConverter
{
    public static List<CodeQuality> ConvertToCodeQualityRaw(FileInfo source, string? pathRoot)
    {
        var logContents = File.ReadAllText(source.FullName);

        return logContents.Contains(" \"$schema\": \"http://json.schemastore.org/sarif-2")
            ? new Converter2(source, pathRoot).Convert()
            : new Converter1(source, pathRoot).Convert();
    }

    public static void ConvertToCodeQuality(
        FileInfo source,
        FileInfo target,
        string? pathRoot = null
    )
    {
        var cqrs = ConvertToCodeQualityRaw(source, pathRoot);
        Common.WriteToDisk(target, cqrs);
    }
}
