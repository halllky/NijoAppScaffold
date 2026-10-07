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
/// デモ101の nijo.xml やフォルダ構造に変更があった場合、このクラスの更新が必要になる可能性がある。
/// そのため <c>Nijo.IntegrationTest</c> にこのクラスを実行したうえで dotnet build / npm run tsc が
/// 通ることを確認するテストを用意している。
/// </para>
/// </summary>
public static class Demo101TemplatePruner
{

    /// <summary>DataStructures直下の要素でUniqueIdがこれに合致するものを削除する</summary>
    private static readonly HashSet<string> REMOVE_DATA_STRUCTURE_IDS = [
        "a1b2c3d4-e5f6-4a1b-8c2d-3e4f5a6b7c8d", // 商品
        "e5f6a1b2-c3d4-4e5f-2a6b-7c8d9e0f1a2b", // 入荷
        "f6a1b2c3-d4e5-4f2a-9b3c-4d5e6f7a8b9c", // 売上
        "bf2453f1-2a89-4209-ab79-00d526fd15a9", // 入荷詳細
        "a5e5b615-6cc1-4a50-8cc7-fe910dca543b", // 入荷明細
        "d0929a1e-1490-42c3-a728-d740c85ff7b7", // 売上一覧
        "2352e942-65b1-47fe-ac3b-0c49bd4e295f", // 売上詳細
        "0d252a96-0dd0-45eb-af33-e761419117c7", // 売上詳細画面初期表示Parameter
        "ca790c30-db44-4439-9452-2a1310a901bb", // 入荷一覧
        "cf33b04a-b6a2-4559-b98c-563ddcc76394", // 商品一覧
        "2555dc15-3ed7-48cb-9e78-0e7b09de86a3", // 在庫調整Parameter
        "4a1fa98c-1503-4d77-8161-e0e2f5f2fd1d", // 在庫調整
        "9604df00-d30a-4033-affd-6d2d625558fa", // 商品在庫増減履歴
        "c766195e-c279-4a50-84bb-fcbf46ac9798", // 入荷詳細画面初期表示Parameter
        "7c3fe519-2692-4465-b151-9884cc73cdb7", // 売上新規登録ReturnValue
        "da3c0330-ac57-43ce-8cba-212da1053aab", // 入荷登録ReturnValue
    ];

    /// <summary>Commands直下の要素でUniqueIdがこれに合致するものを削除する</summary>
    private static readonly HashSet<string> REMOVE_COMMAND_IDS = [
        "c2a6e537-f471-4512-ae2b-d4a38017604e", // 入荷登録
        "6cb9f1e7-afb0-4197-be66-d9779315026c", // 商品データ取込
        "e4e427ae-7a2d-468b-864e-6536dbb0a47a", // 売上新規登録
        "590f0ad0-92c9-4281-82f7-6f8c0d04de1d", // 売上詳細画面初期表示
        "271203a5-7b54-449b-a443-92019f7f1035", // 売上修正
        "6eae3b9e-64ab-49a6-bb26-1738a31561c7", // 入荷修正
        "23aacfdc-c4d5-499f-891e-999542dc5258", // 在庫調整登録
        "d8cff664-d1a8-43c8-ac5e-cc1b2a8fb400", // 入荷詳細画面初期表示
        "00556384-1f6d-4596-89d6-2cd1a9094ea5", // 売上金額シミュレート
    ];

    /// <summary>StaticEnums直下の要素でUniqueIdがこれに合致するものを削除する</summary>
    private static readonly HashSet<string> REMOVE_ENUM_IDS = [
        "e4a132f9-580e-477f-abf3-9359c92a643a", // 消費税区分
        "724ac342-ebd2-4a0b-bb75-5bd174cd5f60", // 売上明細区分
    ];

    /// <summary>
    /// CustomAttributes直下の要素（タグ名 "Custom-&lt;UniqueId&gt;"）でこれに合致するものを削除する。
    /// "金額項目"(IsCurrency)・"数量"(IsQuantity) はどの項目にも使用されなくなるが、
    /// 汎用的な書式指定として今後利用者が自分の項目に付与できるように定義自体は残す
    /// （削除すると生成後のメタデータの型からプロパティが消え、それを参照している client/src/ui がコンパイルエラーになる）。
    /// </summary>
    private static readonly HashSet<string> REMOVE_CUSTOM_ATTRIBUTE_IDS = [
        "0f2d701d-f481-42d4-8dfb-5296c1b40246", // 0以上のみ
    ];

    /// <summary>業務固有のフォルダ・ファイル。テンプレートには不要なので削除する。</summary>
    private static readonly string[] DELETE_PATHS = [
        "Core/入荷",
        "Core/商品",
        "Core/在庫調整",
        "Core/売上",
        "Core/外部システム/商品管理システム",
        // nijo.xml から削除したカスタム属性・sequence 型の項目に対応する override
        "Core/カスタム属性/0以上のみ.cs",
        "Core/OverridedDbContext.Sequence.cs",
        "Core/OverridedDummyDataGenerator.販売管理.cs",
        // 業務テーブルを含むDBスキーマに依存するもの。
        // マイグレーションは、テンプレートを使い始めるときに利用者が改めて作成する。
        "Core/Migrations",
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
    ];

    /// <summary>
    /// マイグレーションSQLのフォルダ。マイグレーション1回分ごとのSQL（ファイル名が数字で始まるもの）を削除する。
    /// マイグレーションの都度作り直されるSQL（ファイル名が数字以外で始まるもの）は、DBスキーマに依存しないので残す。
    /// </summary>
    private const string MIGRATIONS_SCRIPT_DIR = "Core/MigrationsScript";

    /// <summary>
    /// <paramref name="workDir"/> に展開されたデモ101のソース一式から、
    /// 販売管理業務固有の部分を削除する。
    /// </summary>
    public static void Prune(string workDir)
    {
        PruneNijoXml(Path.Combine(workDir, "nijo.xml"));
        DeleteUnneededPaths(workDir);
        DeleteBusinessPages(workDir);
        DeleteMigrationScripts(workDir);
    }

    // ============================================================
    // 1. nijo.xml から販売管理業務固有の定義を削除する
    // ============================================================

    private static void PruneNijoXml(string xmlPath)
    {
        var doc = XDocument.Load(xmlPath);
        var root = doc.Root ?? throw new InvalidOperationException($"ルート要素が見つかりません: {xmlPath}");

        RemoveChildrenByUniqueId(root.Element("DataStructures"), REMOVE_DATA_STRUCTURE_IDS);
        RemoveChildrenByUniqueId(root.Element("Commands"), REMOVE_COMMAND_IDS);
        RemoveChildrenByUniqueId(root.Element("StaticEnums"), REMOVE_ENUM_IDS);

        var customAttributes = root.Element("CustomAttributes");
        if (customAttributes != null)
        {
            foreach (var el in customAttributes.Elements().ToArray())
            {
                const string PREFIX = "Custom-";
                if (!el.Name.LocalName.StartsWith(PREFIX)) continue;
                var id = el.Name.LocalName[PREFIX.Length..];
                if (REMOVE_CUSTOM_ATTRIBUTE_IDS.Contains(id))
                {
                    RemoveWithPrecedingComment(el);
                }
            }
        }

        using var writer = XmlWriter.Create(xmlPath, new XmlWriterSettings
        {
            Indent = true,
            Encoding = new UTF8Encoding(false),
        });
        doc.Save(writer);
    }

    private static void RemoveChildrenByUniqueId(XElement? section, HashSet<string> ids)
    {
        if (section == null) return;
        foreach (var el in section.Elements().ToArray())
        {
            var uniqueId = (string?)el.Attribute("UniqueId");
            if (uniqueId != null && ids.Contains(uniqueId))
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
    // 2. 業務固有のフォルダ・ファイルを削除する
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
    // 3. 業務画面を削除する
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

        foreach (var dir in Directory.GetDirectories(pagesDir))
        {
            Directory.Delete(dir, recursive: true);
        }
        foreach (var file in Directory.GetFiles(pagesDir))
        {
            if (KEEP_PAGES.Contains(Path.GetFileName(file))) continue;
            File.Delete(file);
        }
    }

    // ============================================================
    // 4. マイグレーションSQLを削除する
    // ============================================================

    private static void DeleteMigrationScripts(string workDir)
    {
        var scriptDir = Path.Combine(workDir, MIGRATIONS_SCRIPT_DIR);
        if (!Directory.Exists(scriptDir))
        {
            throw new InvalidOperationException($"マイグレーションSQLのフォルダが見つかりません（デモ101の構造が変わった可能性があります）: {MIGRATIONS_SCRIPT_DIR}");
        }

        foreach (var file in Directory.GetFiles(scriptDir, "*.sql"))
        {
            if (char.IsAsciiDigit(Path.GetFileName(file)[0]))
            {
                File.Delete(file);
            }
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
