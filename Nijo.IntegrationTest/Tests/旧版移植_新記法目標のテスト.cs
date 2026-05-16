using System.Text;
using NUnit.Framework;

namespace Nijo.IntegrationTest.Tests;

public class 旧版移植_新記法目標のテスト {

    [Test]
    public async Task 旧版と同等内容を新記法で表したスキーマが最終的に旧版と同じソースを生成すること() {
        using var legacyProject = await NijoTestUtil.CreateLegacyProjectAsync($$"""
            <LegacyApp UseWijmo="True"
                       DisableLocalRepository="True"
                       UseBatchUpdateVersion2="True"
                       DbContextName="MyDbContext"
                       CreateUserDbColumnName="REG_MAN_CD"
                       UpdateUserDbColumnName="UP_MAN_CD"
                       CreateAtDbColumnName="REG_DT"
                       UpdateAtDbColumnName="UP_DT"
                       CreatedAtDbColumnName="REG_DT"
                       UpdatedAtDbColumnName="UP_DT"
                       VersionDbColumnName="UP_VERSION"
                       VFormRefItemIsNotWide="True"
                       VFormThreshold="600"
                       MaxFileSizeMB="10"
                       MaxTotalFileSizeMB="10"
                       AttachmentFileExtensions="doc;docx;xls;xlsx;pdf">

              <申請区分種別 is="dynamic-enum-type:application-kind" DisplayName="申請区分種別" />

              <申請区分マスタ is="write-model-2 generate-default-read-model is-dynamic-enum-write-model force-generate-refto-modules"
                        LatinName="application_kind_master"
                        DisplayName="申請区分マスタ">
                <内部キー is="seq:APP_KIND_SEQ key" />
                <種別 is="word required" />
                <値CD is="word required" />
                <表示名称 is="word required name" />
              </申請区分マスタ>

              <取引状態 is="enum" DisplayName="取引状態">
                <未処理 key="0" />
                <処理済 key="1" />
                <取消 key="2" />
              </取引状態>

              <取引先コード is="value-object" DisplayName="取引先コード" LatinName="customer_code" />

              <取引先マスタ is="write-model-2 generate-default-read-model force-generate-refto-modules"
                     DbName="M_CUSTOMER"
                     LatinName="customer_master"
                     DisplayName="取引先マスタ">
                <取引先ID is="int:9 key" DbName="CUSTOMER_ID" />
                <取引先コード is="取引先コード search-behavior:前方一致 character-type:HalfWidthAlphaNum name" />
                <取引先名 is="word max-length:100 required" />
                <有効開始日 is="date" />
                <最終更新日時 is="datetime hidden" />
                <取引履歴 is="children required">
                  <履歴ID is="int:9 key" />
                  <変更月 is="year-month" />
                  <変更理由 is="sentence wide" />
                  <確認済 is="bool" />
                </取引履歴>
              </取引先マスタ>

              <取引先照会 is="read-model-2 readonly customize-batch-update-read-models force-generate-refto-modules"
                     LatinName="customer_query"
                     DisplayName="取引先照会">
                <取引先ID is="int:9 key" />
                <取引先コード is="取引先コード search-behavior:前方一致 name" />
                <取引先名 is="word max-length:100 required" />
                <有効開始日 is="date" />
                <照会履歴 is="children has-lifecycle required">
                  <履歴ID is="int:9 key" />
                  <変更月 is="year-month" />
                  <変更理由 is="sentence wide" />
                  <確認済 is="bool" />
                  <検索専用フラグ is="search-condition-only-bool" />
                  <検索専用メモ is="word search-condition-only" />
                </照会履歴>
              </取引先照会>

              <受注 is="write-model-2 generate-default-read-model"
                  DbName="T_ORDER"
                  LatinName="order_entry"
                  DisplayName="受注">
                <受注ID is="int:9 key" DbName="ORDER_ID" />
                <受注番号 is="word max-length:20 required character-type:HalfWidthAlphaNum name" />
                <取引先 is="ref-to:取引先マスタ" />
                <申請区分 is="ref-to:申請区分マスタ dynamic-enum-type-physical-name:申請区分種別" />
                <状態 is="取引状態 combo" />
                <公開状態 is="取引状態 radio hidden" />
                <金額 is="decimal:12,2 not-negative required" />
                <件数 is="int:9 not-negative" />
                <受付日 is="date" />
                <受付日時 is="datetime" />
                <識別子 is="uuid" />
                <添付資料 is="file" />
                <バイナリ is="bytearray" />
                <備考 is="sentence wide" />
                <明細 is="children required">
                  <明細ID is="int:9 key" />
                  <商品名 is="word max-length:100 required" />
                  <数量 is="int:9 not-negative required" />
                  <単価 is="decimal:9,2 not-negative" />
                  <取引状態 is="取引状態" />
                </明細>
                <配送 is="child">
                  <配送先名 is="word max-length:100" />
                  <配送先住所 is="sentence wide" />
                </配送>
              </受注>

              <受注検索 is="read-model-2 force-generate-refto-modules"
                    LatinName="order_query"
                    DisplayName="受注検索">
                <受注検索ID is="int:9 key" />
                <取引先 is="ref-to:取引先照会 grid-combo" />
                <受注番号 is="word search-behavior:部分一致" />
                <取引先コード is="取引先コード search-behavior:完全一致" />
                <受付開始 is="date search-condition-only" />
                <完了のみ is="search-condition-only-bool" />
              </受注検索>

              <受注登録 is="command" DisplayName="受注登録コマンド">
                <受付担当 is="word required" />
                <一時保存 is="bool" />
                <実行理由 is="sentence wide" />
              </受注登録>
            </LegacyApp>
            """);

        Assert.That(await legacyProject.GenerateCodeAsync(), Is.True);
        Assert.That(await legacyProject.CheckCompileAsync(), Is.True);

        using var migratedProject = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold UseWijmo="True"
                             DisableLocalRepository="True"
                             UseBatchUpdateVersion2="True"
                             DbContextName="MyDbContext"
                             CreateUserDbColumnName="REG_MAN_CD"
                             UpdateUserDbColumnName="UP_MAN_CD"
                             CreateAtDbColumnName="REG_DT"
                             UpdateAtDbColumnName="UP_DT"
                             CreatedAtDbColumnName="REG_DT"
                             UpdatedAtDbColumnName="UP_DT"
                             VersionDbColumnName="UP_VERSION"
                             VFormRefItemIsNotWide="True"
                             VFormThreshold="600"
                             MaxFileSizeMB="10"
                             MaxTotalFileSizeMB="10"
                             AttachmentFileExtensions="doc;docx;xls;xlsx;pdf"
                             CoreLibraryFolderName="core.AutoGenerated"
                             WebapiProjectFolderName="webapi"
                             ReactProjectFolderName="react"
                             SuppressAutoGeneratedComment="true">
              <DataStructures>
                <申請区分種別 Type="dynamic-enum-type:application-kind" DisplayName="申請区分種別" />

                <申請区分マスタ Type="write-model-2"
                          GenerateDefaultReadModel="True"
                          IsDynamicEnumWriteModel="True"
                          ForceGenerateRefToModules="True"
                          LatinName="application_kind_master"
                          DisplayName="申請区分マスタ">
                  <内部キー Type="seq" SequenceName="APP_KIND_SEQ" IsKey="True" />
                  <種別 Type="word" Required="True" />
                  <値CD Type="word" Required="True" />
                  <表示名称 Type="word" Required="True" Name="True" />
                </申請区分マスタ>

                <取引先マスタ Type="write-model-2"
                       GenerateDefaultReadModel="True"
                       ForceGenerateRefToModules="True"
                       DbName="M_CUSTOMER"
                       LatinName="customer_master"
                       DisplayName="取引先マスタ">
                  <取引先ID Type="int" TotalDigit="9" IsKey="True" DbName="CUSTOMER_ID" />
                  <取引先コード Type="取引先コード" SearchBehavior="前方一致" CharacterType="HalfWidthAlphaNum" Name="True" />
                  <取引先名 Type="word" MaxLength="100" Required="True" />
                  <有効開始日 Type="date" />
                  <最終更新日時 Type="datetime" Hidden="True" />
                  <取引履歴 Type="children" Required="True">
                    <履歴ID Type="int" TotalDigit="9" IsKey="True" />
                    <変更月 Type="year-month" />
                    <変更理由 Type="sentence" Wide="True" />
                    <確認済 Type="bool" />
                  </取引履歴>
                </取引先マスタ>

                <取引先照会 Type="read-model-2"
                       Readonly="True"
                       CustomizeBatchUpdateReadModels="True"
                       ForceGenerateRefToModules="True"
                       LatinName="customer_query"
                       DisplayName="取引先照会">
                  <取引先ID Type="int" TotalDigit="9" IsKey="True" />
                  <取引先コード Type="取引先コード" SearchBehavior="前方一致" Name="True" />
                  <取引先名 Type="word" MaxLength="100" Required="True" />
                  <有効開始日 Type="date" />
                  <照会履歴 Type="children" HasLifecycle="True" Required="True">
                    <履歴ID Type="int" TotalDigit="9" IsKey="True" />
                    <変更月 Type="year-month" />
                    <変更理由 Type="sentence" Wide="True" />
                    <確認済 Type="bool" />
                    <検索専用フラグ Type="search-condition-only-bool" />
                    <検索専用メモ Type="word" SearchConditionOnly="True" />
                  </照会履歴>
                </取引先照会>

                <受注 Type="write-model-2"
                    GenerateDefaultReadModel="True"
                    DbName="T_ORDER"
                    LatinName="order_entry"
                    DisplayName="受注">
                  <受注ID Type="int" TotalDigit="9" IsKey="True" DbName="ORDER_ID" />
                  <受注番号 Type="word" MaxLength="20" Required="True" CharacterType="HalfWidthAlphaNum" Name="True" />
                  <取引先 Type="ref-to:取引先マスタ" />
                  <申請区分 Type="ref-to:申請区分マスタ" DynamicEnumTypePhysicalName="申請区分種別" />
                  <状態 Type="取引状態" Combo="True" />
                  <公開状態 Type="取引状態" Radio="True" Hidden="True" />
                  <金額 Type="decimal" TotalDigit="12" DecimalPlace="2" NotNegative="True" Required="True" />
                  <件数 Type="int" TotalDigit="9" NotNegative="True" />
                  <受付日 Type="date" />
                  <受付日時 Type="datetime" />
                  <識別子 Type="uuid" />
                  <添付資料 Type="file" />
                  <バイナリ Type="bytearray" />
                  <備考 Type="sentence" Wide="True" />
                  <明細 Type="children" Required="True">
                    <明細ID Type="int" TotalDigit="9" IsKey="True" />
                    <商品名 Type="word" MaxLength="100" Required="True" />
                    <数量 Type="int" TotalDigit="9" NotNegative="True" Required="True" />
                    <単価 Type="decimal" TotalDigit="9" DecimalPlace="2" NotNegative="True" />
                    <取引状態 Type="取引状態" />
                  </明細>
                  <配送 Type="child">
                    <配送先名 Type="word" MaxLength="100" />
                    <配送先住所 Type="sentence" Wide="True" />
                  </配送>
                </受注>

                <受注検索 Type="read-model-2"
                      ForceGenerateRefToModules="True"
                      LatinName="order_query"
                      DisplayName="受注検索">
                  <受注検索ID Type="int" TotalDigit="9" IsKey="True" />
                  <取引先 Type="ref-to:取引先照会" GridCombo="True" />
                  <受注番号 Type="word" SearchBehavior="部分一致" />
                  <取引先コード Type="取引先コード" SearchBehavior="完全一致" />
                  <受付開始 Type="date" SearchConditionOnly="True" />
                  <完了のみ Type="search-condition-only-bool" />
                </受注検索>
              </DataStructures>

              <Commands>
                <受注登録 Type="command" DisplayName="受注登録コマンド">
                  <受付担当 Type="word" Required="True" />
                  <一時保存 Type="bool" />
                  <実行理由 Type="sentence" Wide="True" />
                </受注登録>
              </Commands>

              <StaticEnums>
                <取引状態 Type="enum" DisplayName="取引状態">
                  <未処理 key="0" />
                  <処理済 key="1" />
                  <取消 key="2" />
                </取引状態>
              </StaticEnums>

              <ValueObjects>
                <取引先コード Type="value-object-2" DisplayName="取引先コード" LatinName="customer_code" />
              </ValueObjects>
            </NijoAppScaffold>
            """);

        var errors = await migratedProject.EnumerateValidationErrorsAsync();
        Assert.That(errors, Is.Empty, () => string.Join(Environment.NewLine, errors.Select(FormatValidationError)));

        Assert.That(await migratedProject.GenerateCodeAsync(), Is.True);
        Assert.That(await migratedProject.CheckCompileAsync(), Is.True);

        AssertGeneratedSourcesEqual(
            legacyProject.CoreAutoGeneratedDir,
            Path.Combine(migratedProject.Project.ProjectRoot, "core.AutoGenerated", "__AutoGenerated"));
        AssertGeneratedSourcesEqual(
            legacyProject.WebApiAutoGeneratedDir,
            Path.Combine(migratedProject.Project.ProjectRoot, "webapi", "__AutoGenerated"));
        AssertGeneratedSourcesEqual(
            legacyProject.ReactAutoGeneratedDir,
            Path.Combine(migratedProject.Project.ProjectRoot, "react", "__autoGenerated"));
    }

    [Test]
    public async Task ValueObjectModel2が旧版ValueObjectModelと同じソースを生成すること() {
        using var legacyProject = await NijoTestUtil.CreateLegacyProjectAsync("""
            <LegacyApp>
              <取引先コード is="value-object" DisplayName="取引先コード" LatinName="customer_code" />
            </LegacyApp>
            """);

        Assert.That(await legacyProject.GenerateCodeAsync(), Is.True);

        using var migratedProject = await NijoTestUtil.CreateNewProjectAsync("""
            <NijoAppScaffold RootNamespace="LegacyApp" SuppressAutoGeneratedComment="true">
              <ValueObjects>
                <取引先コード Type="value-object-2" DisplayName="取引先コード" LatinName="customer_code" />
              </ValueObjects>
            </NijoAppScaffold>
            """);

        var errors = await migratedProject.EnumerateValidationErrorsAsync();
        Assert.That(errors, Is.Empty, () => string.Join(Environment.NewLine, errors.Select(FormatValidationError)));

        Assert.That(await migratedProject.GenerateCodeAsync(), Is.True);

        var expected = File.ReadAllText(
            Path.Combine(legacyProject.CoreAutoGeneratedDir, "Util", "取引先コード.cs"),
            Encoding.UTF8).ReplaceLineEndings("\n");
        var actual = File.ReadAllText(
            Path.Combine(migratedProject.Project.CoreLibraryRoot, "__AutoGenerated", "Util", "取引先コード.cs"),
            Encoding.UTF8).ReplaceLineEndings("\n");

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public async Task StaticEnumModel2が旧版StaticEnumModelと同じソースを生成すること() {
        using var legacyProject = await NijoTestUtil.CreateLegacyProjectAsync("""
            <LegacyApp>
              <取引状態 is="enum" DisplayName="取引状態">
                <未処理 key="0" />
                <処理済 key="1" DisplayName="処理(済)" />
                <取消 key="2" />
              </取引状態>
            </LegacyApp>
            """);

        Assert.That(await legacyProject.GenerateCodeAsync(), Is.True);

        using var migratedProject = await NijoTestUtil.CreateNewProjectAsync("""
            <NijoAppScaffold RootNamespace="LegacyApp"
                             SuppressAutoGeneratedComment="true"
                             CoreLibraryFolderName="core.AutoGenerated"
                             ReactProjectFolderName="react/src">
              <StaticEnums>
                <取引状態 Type="enum-2" DisplayName="取引状態">
                  <未処理 key="0" />
                  <処理済 key="1" DisplayName="処理(済)" />
                  <取消 key="2" />
                </取引状態>
              </StaticEnums>
            </NijoAppScaffold>
            """);

        var errors = await migratedProject.EnumerateValidationErrorsAsync();
        Assert.That(errors, Is.Empty, () => string.Join(Environment.NewLine, errors.Select(FormatValidationError)));

        Assert.That(await migratedProject.GenerateCodeAsync(), Is.True);

        var expectedCs = File.ReadAllText(
            Path.Combine(legacyProject.CoreAutoGeneratedDir, "Enum.cs"),
            Encoding.UTF8).ReplaceLineEndings("\n");
        var actualCs = File.ReadAllText(
            Path.Combine(migratedProject.Project.CoreLibraryRoot, "__AutoGenerated", "Enum.cs"),
            Encoding.UTF8).ReplaceLineEndings("\n");
        Assert.That(actualCs, Is.EqualTo(expectedCs), "Enum.cs");

        var expectedTs = File.ReadAllText(
            Path.Combine(legacyProject.ReactAutoGeneratedDir, "autogenerated-enum.ts"),
            Encoding.UTF8).ReplaceLineEndings("\n");
        var actualTs = File.ReadAllText(
            Path.Combine(migratedProject.Project.ReactProjectRoot, "__autoGenerated", "autogenerated-enum.ts"),
            Encoding.UTF8).ReplaceLineEndings("\n");
        Assert.That(actualTs, Is.EqualTo(expectedTs), "autogenerated-enum.ts");
    }

    [Test]
    public async Task WriteModel2が現行アーキテクチャのモデルとして解釈できること() {
        using var migratedProject = await NijoTestUtil.CreateNewProjectAsync("""
            <NijoAppScaffold RootNamespace="LegacyApp"
                             SuppressAutoGeneratedComment="true"
                             CoreLibraryFolderName="core.AutoGenerated"
                             WebapiProjectFolderName="webapi"
                             ReactProjectFolderName="react/src">
              <DataStructures>
                <顧客 Type="write-model-2"
                    DbName="M_CUSTOMER"
                    LatinName="customer"
                    DisplayName="顧客">
                  <顧客ID Type="int" TotalDigit="9" IsKey="True" DbName="CUSTOMER_ID" />
                  <顧客名 Type="word" MaxLength="100" IsNotNull="True" />
                  <備考 Type="description" />
                  <明細 Type="children">
                    <行番号 Type="int" TotalDigit="9" IsKey="True" />
                    <商品名 Type="word" MaxLength="50" IsNotNull="True" />
                  </明細>
                </顧客>
              </DataStructures>
            </NijoAppScaffold>
            """);

        var errors = await migratedProject.EnumerateValidationErrorsAsync();
        Assert.That(errors, Is.Empty, () => string.Join(Environment.NewLine, errors.Select(FormatValidationError)));

        Assert.That(await migratedProject.GenerateCodeAsync(), Is.True);
    }

    private static void AssertGeneratedSourcesEqual(string expectedDir, string actualDir) {
        var expectedFiles = Directory.Exists(expectedDir)
            ? Directory.EnumerateFiles(expectedDir, "*", SearchOption.AllDirectories)
                .ToDictionary(
                    path => Path.GetRelativePath(expectedDir, path).Replace(Path.DirectorySeparatorChar, '/'),
                    path => File.ReadAllText(path, Encoding.UTF8).ReplaceLineEndings("\n"))
            : new Dictionary<string, string>();
        var actualFiles = Directory.Exists(actualDir)
            ? Directory.EnumerateFiles(actualDir, "*", SearchOption.AllDirectories)
                .ToDictionary(
                    path => Path.GetRelativePath(actualDir, path).Replace(Path.DirectorySeparatorChar, '/'),
                    path => File.ReadAllText(path, Encoding.UTF8).ReplaceLineEndings("\n"))
            : new Dictionary<string, string>();

        Assert.That(actualFiles.Keys.OrderBy(x => x), Is.EqualTo(expectedFiles.Keys.OrderBy(x => x)));

        foreach (var key in expectedFiles.Keys.OrderBy(x => x)) {
            Assert.That(actualFiles[key], Is.EqualTo(expectedFiles[key]), key);
        }
    }

    private static string FormatValidationError(Nijo.SchemaParsing.SchemaParseContext.ValidationError error) {
        var ownErrors = error.OwnErrors.Select(message => $"- own: {message}");
        var attributeErrors = error.AttributeErrors.SelectMany(pair => pair.Value.Select(message => $"- {pair.Key}: {message}"));
        return $$"""
          {{error.XElement.AncestorsAndSelf().Reverse().Select(element => element.Name.LocalName).Aggregate((left, right) => left + "/" + right)}}
          {{string.Join(Environment.NewLine, ownErrors.Concat(attributeErrors))}}
          """;
    }
}
