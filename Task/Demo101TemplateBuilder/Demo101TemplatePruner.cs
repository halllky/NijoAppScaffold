using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Demo101TemplateBuilder;

/// <summary>
/// <para>
/// /demo/101_販売管理システム （デモ101）から、新規プロジェクトテンプレート（demo101-template）を作るために、
/// 販売管理業務固有の部分（商品・入荷・売上・在庫調整など）を削除する処理。
/// </para>
/// <para>
/// ここで行うのは nijo.xml の定義の削除と、それに依存するフォルダ・ファイルの削除だけである。
/// ソースコードの書き換えは行わない。デモ101側は、業務固有の部分をフォルダ・ファイル単位で削除しても
/// 残りのソースがそのままコンパイルできる構造になっている（業務画面の自己登録、partial による分割など）。
/// </para>
/// <para>
/// nijo.xml の定義と画面は、テンプレートに残すものを列挙し、それ以外はすべて削除する。
/// デモ101に業務を足しても消し忘れが起きず、基盤を足して列挙し忘れた場合はコンパイルエラーで気付ける。
/// <c>Nijo.IntegrationTest</c> にこのクラスを実行したうえでビルドとテストが通ることを確認するテストを用意している。
/// </para>
/// </summary>
public static class Demo101TemplatePruner
{

    /// <summary>テンプレートに残す、DataStructures直下の要素のUniqueId</summary>
    private static readonly HashSet<string> KEEP_DATA_STRUCTURE_IDS = [
        "659bf33a-c624-4e67-96a9-460809d70995", // 従業員
        "7e844312-66b4-4693-b8eb-c4d3a4dd2565", // 従業員Ref
        "7508f4ed-a9ba-4c3d-b258-eb328affebdc", // 従業員マスタ
        "28bf1f9e-8b77-4a78-938d-e29578c44da3", // 従業員一括更新Parameter
        "ca0a28e1-6792-4d8b-ad9e-4a373ba190fc", // パスワード再発行Parameter
        "da5e2ba7-864b-4d23-852b-1a4c6f525f98", // パスワード変更Parameter
        "a775c249-a449-4700-a33a-262ca6c820f3", // ログインParameter
        "f594d8ac-f7ff-44ca-878f-fcbfd1e79ee1", // ログインユーザー情報
        "72d0c66e-b523-4dd1-b8a0-8384368fe38b", // セッション
    ];

    /// <summary>テンプレートに残す、Commands直下の要素のUniqueId</summary>
    private static readonly HashSet<string> KEEP_COMMAND_IDS = [
        "af387361-2401-4308-9e04-8e5973f39426", // ログイン
        "8877c58b-260a-4de3-a007-ce73d1d222b3", // ログアウト
        "87def3ce-18a3-4ad1-9ab1-bf0ad3c80763", // ログイン状態確認
        "c6feafdf-b973-4196-afa1-111eb9638087", // パスワード変更
        "abbe87d5-69df-45a0-9166-1b05f08de613", // パスワード再発行
        "5388c52e-2932-4226-bf5b-734ae8a50112", // 従業員一括更新
        "eadf411b-2c47-483f-b0a8-fc52c9486adc", // クリーンナップ
    ];

    private const string PERMISSION_ENUM_ID = "d3d0cb8d-23a2-4b86-a85e-b8965284c824"; // 権限区分

    /// <summary>テンプレートに残す、StaticEnums直下の要素のUniqueId</summary>
    private static readonly HashSet<string> KEEP_ENUM_IDS = [PERMISSION_ENUM_ID];

    /// <summary>テンプレートに残す、権限区分の値のUniqueId。業務ごとの権限区分は案件で足す</summary>
    private static readonly HashSet<string> KEEP_PERMISSION_VALUE_IDS = [
        "95e18962-7298-442c-97a1-20d00de5c5bd", // システム管理者
    ];

    /// <summary>
    /// テンプレートに残す、CustomAttributes直下の要素（タグ名 "Custom-&lt;UniqueId&gt;"）のUniqueId。
    /// "金額項目"(IsCurrency)・"数量"(IsQuantity) はどの項目にも使用されなくなるが、
    /// 汎用的な書式指定として今後利用者が自分の項目に付与できるように定義自体は残す
    /// （削除すると生成後のメタデータの型からプロパティが消え、それを参照している client/src/ui がコンパイルエラーになる）。
    /// </summary>
    private static readonly HashSet<string> KEEP_CUSTOM_ATTRIBUTE_IDS = [
        "c582bb2e-804e-4136-93f0-85139aaa675f", // 最大長
        "e543a1e3-a539-459b-86ce-a17686043d59", // 金額項目
        "beceba92-faf8-49eb-a987-ff0d53889d64", // 数量
        "4d868e55-06d6-44eb-aa97-901f39dd1782", // マスキング
    ];

    /// <summary>テンプレートに不要なフォルダ・ファイル</summary>
    private static readonly string[] DELETE_PATHS = [
        // 販売管理業務に固有のコードはこれらのフォルダの中に置くことになっている
        "Core/販売管理",
        "UnitTest/Tests/販売管理",
        // 販売管理業務の実装の見本を案内する文書。案内先のファイルがテンプレートでは削除されるため
        "標準仕様書_参考実装.md",
        // 業務テーブルを含むDBスキーマに依存するもの
        "Core/Migrations",
        "Core/MigrationsScript",
        "nijo.viewState.json",
        "nijo.dbViewer.json",
    ];

    /// <summary>画面のフォルダ。<see cref="KEEP_PAGES"/> 以外はすべて業務画面として削除する。</summary>
    private const string PAGES_DIR = "client/src/pages";

    /// <summary><see cref="PAGES_DIR"/> 配下で、テンプレートに残す画面</summary>
    private static readonly string[] KEEP_PAGES = [
        "P000_トップページ.tsx",
        "P001_ログイン.tsx",
        "P002_ログアウト.tsx",
        "P400_従業員.tsx",
        "shared/従業員検索ダイアログ.tsx",
    ];

    /// <summary>
    /// <paramref name="workDir"/> に展開されたデモ101のソース一式から、
    /// 販売管理業務固有の部分を削除する。
    /// </summary>
    public static void Prune(string workDir)
    {
        PruneNijoXml(Path.Combine(workDir, "nijo.xml"));
        DeleteUnneededPaths(workDir);
        DeleteBusinessPages(workDir);
    }

    // ============================================================
    // nijo.xml から販売管理業務固有の定義を削除する
    // ============================================================

    private static void PruneNijoXml(string xmlPath)
    {
        var doc = XDocument.Load(xmlPath);
        var root = doc.Root ?? throw new InvalidOperationException($"ルート要素が見つかりません: {xmlPath}");

        var uniqueIdOf = (XElement el) => (string?)el.Attribute("UniqueId");
        KeepOnly(root.Element("DataStructures"), KEEP_DATA_STRUCTURE_IDS, uniqueIdOf);
        KeepOnly(root.Element("Commands"), KEEP_COMMAND_IDS, uniqueIdOf);
        KeepOnly(root.Element("StaticEnums"), KEEP_ENUM_IDS, uniqueIdOf);
        KeepOnly(
            root.Element("StaticEnums")?.Elements().SingleOrDefault(el => uniqueIdOf(el) == PERMISSION_ENUM_ID),
            KEEP_PERMISSION_VALUE_IDS,
            uniqueIdOf);

        const string CUSTOM_ATTRIBUTE_PREFIX = "Custom-";
        KeepOnly(root.Element("CustomAttributes"), KEEP_CUSTOM_ATTRIBUTE_IDS, el => el.Name.LocalName.StartsWith(CUSTOM_ATTRIBUTE_PREFIX)
            ? el.Name.LocalName[CUSTOM_ATTRIBUTE_PREFIX.Length..]
            : null);

        // nijo が保存するときと同じ書式にする。違うと利用者が最初に保存したときファイル全体が差分になるため
        using (var writer = XmlWriter.Create(xmlPath, new XmlWriterSettings
        {
            Indent = true,
            NewLineOnAttributes = true,
            Encoding = new UTF8Encoding(false),
            NewLineChars = "\n",
        }))
        {
            doc.Save(writer);
        }
        File.AppendAllText(xmlPath, "\n");
    }

    /// <summary>
    /// <paramref name="section"/> の子要素のうち、<paramref name="keepIds"/> に合致しないものをすべて削除する。
    /// </summary>
    private static void KeepOnly(XElement? section, HashSet<string> keepIds, Func<XElement, string?> idOf)
    {
        var children = section?.Elements().ToArray() ?? [];

        var missing = keepIds.Except(children.Select(idOf).OfType<string>()).ToArray();
        if (missing.Length > 0)
        {
            throw new InvalidOperationException($"テンプレートに残す定義が見つかりません（デモ101の構造が変わった可能性があります）: {string.Join(", ", missing)}");
        }

        foreach (var el in children)
        {
            var id = idOf(el);
            if (id == null || !keepIds.Contains(id))
            {
                RemoveWithPrecedingComment(el);
            }
        }
    }

    /// <summary>
    /// 要素を削除する。その要素の説明になっている直前のXMLコメントがあれば、それもあわせて削除する。
    /// </summary>
    private static void RemoveWithPrecedingComment(XElement el)
    {
        if (el.PreviousNode is XComment comment)
        {
            comment.Remove();
        }
        el.Remove();
    }

    // ============================================================
    // 業務固有のフォルダ・ファイルを削除する
    // ============================================================

    private static void DeleteUnneededPaths(string workDir)
    {
        foreach (var relativePath in DELETE_PATHS)
        {
            var target = Path.Combine(workDir, relativePath);
            if (Directory.Exists(target))
            {
                Directory.Delete(target, recursive: true);
            }
            else if (File.Exists(target))
            {
                File.Delete(target);
                DeleteDirectoryIfEmpty(Path.GetDirectoryName(target)!);
            }
            else
            {
                throw new InvalidOperationException($"削除対象が見つかりません（デモ101の構造が変わった可能性があります）: {relativePath}");
            }
        }
    }

    // ============================================================
    // 業務画面を削除する
    // ============================================================

    private static void DeleteBusinessPages(string workDir)
    {
        var pagesDir = Path.Combine(workDir, PAGES_DIR);
        foreach (var keep in KEEP_PAGES)
        {
            if (!File.Exists(Path.Combine(pagesDir, keep)))
            {
                throw new InvalidOperationException($"テンプレートに残す画面が見つかりません（デモ101の構造が変わった可能性があります）: {PAGES_DIR}/{keep}");
            }
        }

        foreach (var file in Directory.GetFiles(pagesDir, "*", SearchOption.AllDirectories))
        {
            // macOS のファイルシステムではファイル名の濁点が分解されていることがあるので正規化して比較する
            var relativePath = Path.GetRelativePath(pagesDir, file).Replace('\\', '/').Normalize();
            if (KEEP_PAGES.Contains(relativePath)) continue;
            File.Delete(file);
            DeleteDirectoryIfEmpty(Path.GetDirectoryName(file)!);
        }
    }

    private static void DeleteDirectoryIfEmpty(string dir)
    {
        if (!Directory.EnumerateFileSystemEntries(dir).Any())
        {
            Directory.Delete(dir);
        }
    }
}
