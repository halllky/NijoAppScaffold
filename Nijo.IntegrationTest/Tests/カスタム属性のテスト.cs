using NUnit.Framework;

namespace Nijo.IntegrationTest.Tests;

/// <summary>
/// カスタム属性の型（StaticEnum）、複数の値（IsMultiple）、必須（IsRequired）と、
/// クエリ・コマンドのメタデータ（OperationMetadata）のテスト
/// </summary>
public class カスタム属性のテスト {

    [Test]
    public async Task StaticEnumのカスタム属性には参照先の静的区分の値だけを指定できること() {
        using var project = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <Commands>
                <Command1 Custom-1="Admin" />
                <Command2 Custom-1="Guest" />
              </Commands>
              <StaticEnums>
                <Role>
                  <Admin key="0" />
                  <Staff key="1" />
                </Role>
              </StaticEnums>
              <CustomAttributes>
                <Custom-1 AvailableFor="command" PhysicalName="RequiredRole" Type="StaticEnum" StaticEnumType="Role" />
              </CustomAttributes>
            </NijoAppScaffold>
            """);
        var errors = await project.EnumerateValidationErrorsAsync();
        var erroredElements = errors
            .Where(e => (e.AttributeErrors.GetValueOrDefault("Custom-1") ?? []).Any(m => m.StartsWith("属性 'RequiredRole' の値 'Guest' は不正です。")))
            .Select(e => e.XElement.Name.LocalName);
        Assert.That(erroredElements, Is.EquivalentTo(new[] { "Command2" }));
    }

    [Test]
    public async Task 値を複数持てるカスタム属性はカンマで区切った値がそれぞれ検証されること() {
        using var project = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <Commands>
                <Command1 Custom-1="Admin, Staff" />
                <Command2 Custom-1="Admin,Guest" />
              </Commands>
              <StaticEnums>
                <Role>
                  <Admin key="0" />
                  <Staff key="1" />
                </Role>
              </StaticEnums>
              <CustomAttributes>
                <Custom-1 AvailableFor="command" PhysicalName="RequiredRoles" Type="StaticEnum" StaticEnumType="Role" IsMultiple="True" />
              </CustomAttributes>
            </NijoAppScaffold>
            """);
        var errors = await project.EnumerateValidationErrorsAsync();
        var messages = errors.SelectMany(e => (e.AttributeErrors.GetValueOrDefault("Custom-1") ?? []).Select(m => (e.XElement.Name.LocalName, m)));
        Assert.That(messages.Select(x => x.LocalName), Is.EquivalentTo(new[] { "Command2" }));
        Assert.That(messages.Single().m, Does.StartWith("属性 'RequiredRoles' の値 'Guest' は不正です。"));
    }

    [Test]
    public async Task 必須のカスタム属性は指定できるルート要素に値が無いとエラーになること() {
        using var project = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <Query1 IsQueryable="True" Custom-1="Public">
                  <Id Type="int" />
                </Query1>
                <Query2 IsQueryable="True">
                  <Id Type="int" />
                </Query2>
                <Data1 IsStored="True">
                  <Id Type="int" IsKey="True" />
                </Data1>
              </DataStructures>
              <Commands>
                <Command1 />
              </Commands>
              <CustomAttributes>
                <Custom-1 AvailableFor="queryable,command" PhysicalName="Visibility" Type="Enum" IsRequired="True">
                  <Option>Public</Option>
                  <Option>Private</Option>
                </Custom-1>
              </CustomAttributes>
            </NijoAppScaffold>
            """);
        var errors = await project.EnumerateValidationErrorsAsync();
        var erroredElements = errors
            .Where(e => e.OwnErrors.Contains("カスタム属性 'Visibility' は必須です。値を指定してください。"))
            .Select(e => e.XElement.Name.LocalName);

        // 指定できない Data1 と、ルート要素ではないメンバーは対象外
        Assert.That(erroredElements, Is.EquivalentTo(new[] { "Query2", "Command1" }));
    }

    [Test]
    public async Task カスタム属性の定義の組み合わせが不正な場合はエラーになること() {
        using var project = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <CustomAttributes>
                <Custom-1 AvailableFor="command" PhysicalName="NoEnumType" Type="StaticEnum" />
                <Custom-2 AvailableFor="command" PhysicalName="UnknownEnumType" Type="StaticEnum" StaticEnumType="Unknown" />
                <Custom-3 AvailableFor="command" PhysicalName="MultipleString" Type="String" IsMultiple="True" />
                <Custom-4 AvailableFor="command" PhysicalName="RequiredBoolean" Type="Boolean" IsRequired="True" />
                <Custom-5 AvailableFor="command" PhysicalName="StringWithEnumType" Type="String" StaticEnumType="Unknown" />
              </CustomAttributes>
            </NijoAppScaffold>
            """);
        var errors = await project.EnumerateValidationErrorsAsync();
        var messages = errors.SelectMany(e => e.OwnErrors).ToArray();
        Assert.Multiple(() => {
            Assert.That(messages, Has.Some.StartsWith("カスタム属性 'NoEnumType' の種類が 'StaticEnum' に設定されていますが、StaticEnumType が指定されていません。"));
            Assert.That(messages, Has.Some.EqualTo("カスタム属性 'UnknownEnumType' の StaticEnumType に指定された静的区分 'Unknown' は存在しません。"));
            Assert.That(messages, Has.Some.StartsWith("カスタム属性 'MultipleString' の IsMultiple は、"));
            Assert.That(messages, Has.Some.StartsWith("カスタム属性 'RequiredBoolean' の IsRequired は、"));
            Assert.That(messages, Has.Some.StartsWith("カスタム属性 'StringWithEnumType' の種類が 'String' に設定されていますが、StaticEnumType が指定されています。"));
        });
    }

    [Test]
    public async Task クエリとコマンドのルート要素のカスタム属性がメタデータとして生成されること() {
        using var project = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <Data1 IsStored="True" IsQueryable="True" Custom-1="Restricted" Custom-2="Admin,Staff">
                  <Id Type="int" IsKey="True" />
                  <Name Type="word" Custom-3="True" />
                </Data1>
                <Data2 IsStored="True" IsQueryable="True" Custom-1="Everyone" Custom-4="memo">
                  <Id Type="int" IsKey="True" />
                  <Ref1 Type="ref-to:Data1" />
                </Data2>
              </DataStructures>
              <Commands>
                <Command1 Custom-1="Everyone" />
              </Commands>
              <StaticEnums>
                <Role>
                  <Admin key="0" />
                  <Staff key="1" />
                </Role>
              </StaticEnums>
              <CustomAttributes>
                <Custom-1 AvailableFor="queryable,command" PhysicalName="Access" Type="Enum" IsRequired="True">
                  <Option>Everyone</Option>
                  <Option>Restricted</Option>
                </Custom-1>
                <Custom-2 AvailableFor="queryable,command" PhysicalName="Roles" Type="StaticEnum" StaticEnumType="Role" IsMultiple="True" />
                <Custom-3 AvailableFor="stored,queryable" PhysicalName="Masking" Type="Boolean" />
                <Custom-4 AvailableFor="queryable,command" PhysicalName="Note" Type="String" />
              </CustomAttributes>
            </NijoAppScaffold>
            """);
        Assert.That(await project.EnumerateValidationErrorsAsync(), Is.Empty);
        Assert.That(await project.GenerateCodeAsync(), Is.True);

        var operationCs = await File.ReadAllTextAsync(FindGeneratedFile(project, "Operation.cs"));
        var operationTs = await File.ReadAllTextAsync(FindGeneratedFile(project, "operation.ts"));
        Assert.Multiple(() => {
            // すべての処理に指定されている属性は null を許容せず、そうでない属性は許容する。
            // メンバーにだけ指定されている属性（Masking）はメタデータに含めない
            Assert.That(operationCs, Does.Contain("public required global::Nijo.NijoAttr.AccessAttribute.E_Value Access { get; init; }"));
            Assert.That(operationCs, Does.Contain("public required IReadOnlyList<global::Nijo.Role> Roles { get; init; }"));
            Assert.That(operationCs, Does.Contain("public required string? Note { get; init; }"));
            Assert.That(operationCs, Does.Not.Contain(" Masking { get; init; }"));

            // 参照検索は参照先の一覧検索の値を引き継ぐ
            Assert.That(CountOccurrences(operationCs, "Roles = [global::Nijo.Role.Admin, global::Nijo.Role.Staff],"), Is.EqualTo(2));
            Assert.That(operationCs, Does.Contain("Roles = [],"));
            Assert.That(operationCs, Does.Contain("Note = \"memo\","));
            Assert.That(operationCs, Does.Contain("Note = null,"));

            Assert.That(operationTs, Does.Contain("roles: readonly (EnumDefs.Role)[]"));
            Assert.That(operationTs, Does.Contain("note: string | undefined"));
            Assert.That(CountOccurrences(operationTs, "roles: ['Admin', 'Staff'],"), Is.EqualTo(2));
        });

        Assert.That(await project.CheckCompileAsync(), Is.True);
    }

    private static string FindGeneratedFile(NijoTestUtil.GeneratedProjectWithTestUtil project, string fileName) {
        var root = Path.GetDirectoryName(project.Project.SchemaXmlPath)!;
        return Directory.EnumerateFiles(root, fileName, SearchOption.AllDirectories).Single();
    }

    private static int CountOccurrences(string text, string value) {
        var count = 0;
        for (var index = text.IndexOf(value); index >= 0; index = text.IndexOf(value, index + value.Length)) count++;
        return count;
    }
}
