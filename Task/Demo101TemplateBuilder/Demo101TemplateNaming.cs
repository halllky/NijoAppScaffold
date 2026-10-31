namespace Demo101TemplateBuilder;

/// <summary>
/// デモ101の固有の名前を、空テンプレートと同じ既定の名前（MyApp）に戻す処理。
/// 新規プロジェクトを作った利用者が、どちらのテンプレートでも同じ手順でアプリ名を変更できるようにするためのもの。
/// </summary>
public static class Demo101TemplateNaming
{
    private const string DEMO_NAME = "Demo101";
    private const string DEFAULT_NAME = "MyApp";
    private const string DEMO_APPLICATION_NAME = "デモ101：販売管理システム";
    private const string DEMO_DISPLAY_NAME = "販売管理システム";

    /// <summary>
    /// <paramref name="workDir"/> に展開されたデモ101のソース一式の名前を既定の名前に戻す。
    /// </summary>
    public static void ResetToDefaultName(string workDir)
    {
        RenameSolutionAndProjects(workDir);
        Replace(Path.Combine(workDir, "nijo.xml"), $"ApplicationName=\"{DEMO_APPLICATION_NAME}\"", $"ApplicationName=\"{DEFAULT_NAME}\"");
        Replace(Path.Combine(workDir, "client", "package.json"), "\"name\": \"101-sales-management-system\"", "\"name\": \"my-app\"");
        Replace(Path.Combine(workDir, "client", "index.html"), $"<title>{DEMO_DISPLAY_NAME}</title>", $"<title>{DEFAULT_NAME}</title>");
        Replace(Path.Combine(workDir, "client", "src", "appName.ts"), $"\"{DEMO_DISPLAY_NAME}\"", $"\"{DEFAULT_NAME}\"");
    }

    /// <summary>.sln と .csproj のファイル名および、それらの中でのお互いへの参照を変更する</summary>
    private static void RenameSolutionAndProjects(string workDir)
    {
        var files = Directory.GetFiles(workDir, $"{DEMO_NAME}*.sln")
            .Concat(Directory.GetFiles(workDir, $"{DEMO_NAME}*.csproj", SearchOption.AllDirectories))
            .ToArray();
        if (files.Length == 0)
        {
            throw new InvalidOperationException($"{DEMO_NAME} のソリューションが見つかりません（デモ101の構造が変わった可能性があります）: {workDir}");
        }

        foreach (var file in files)
        {
            File.WriteAllText(file, File.ReadAllText(file).Replace(DEMO_NAME, DEFAULT_NAME));
            File.Move(file, Path.Combine(Path.GetDirectoryName(file)!, Path.GetFileName(file).Replace(DEMO_NAME, DEFAULT_NAME)));
        }
    }

    private static void Replace(string file, string oldValue, string newValue)
    {
        var text = File.ReadAllText(file);
        if (!text.Contains(oldValue))
        {
            throw new InvalidOperationException($"置換対象が見つかりません（デモ101の構造が変わった可能性があります）: {file}: {oldValue}");
        }
        File.WriteAllText(file, text.Replace(oldValue, newValue));
    }
}
