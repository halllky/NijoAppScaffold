using NUnit.Framework;

namespace Nijo.IntegrationTest.Tests;

public class IsStoredとIsQueryable属性のテスト {

    [Test]
    public async Task ルート集約にType属性を指定するとエラーになること() {
        using var project = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <Data1 Type="data-model">
                  <Id Type="int" IsKey="True" />
                </Data1>
              </DataStructures>
              <Commands>
                <Command1 Type="command-model" />
              </Commands>
            </NijoAppScaffold>
            """);
        var errors = await project.EnumerateValidationErrorsAsync();
        var erroredElements = errors
            .Where(e => (e.AttributeErrors.GetValueOrDefault("Type") ?? []).Contains("ルート集約には Type 属性を指定できません。"))
            .Select(e => e.XElement.Name.LocalName);
        Assert.That(erroredElements, Is.EquivalentTo(new[] { "Data1", "Command1" }));
    }

    [Test]
    public async Task MapToViewはIsStoredと同時に指定できないこと() {
        using var project = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <Data1 IsStored="True" IsQueryable="True" MapToView="True">
                  <Id Type="int" IsKey="True" />
                </Data1>
              </DataStructures>
            </NijoAppScaffold>
            """);
        var errors = await project.EnumerateValidationErrorsAsync();
        var messages = errors.SelectMany(e => e.AttributeErrors.GetValueOrDefault("MapToView") ?? []);
        Assert.That(messages, Does.Contain("この属性はこの要素には指定できません。"));
    }

    [Test]
    public async Task カスタム属性のAvailableForに指定できない値を指定するとエラーになること() {
        using var project = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <Data1 IsStored="True">
                  <Id Type="int" IsKey="True" />
                </Data1>
              </DataStructures>
              <CustomAttributes>
                <Custom-1 AvailableFor="data-model" PhysicalName="IsCurrency" Type="Boolean" />
              </CustomAttributes>
            </NijoAppScaffold>
            """);
        var errors = await project.EnumerateValidationErrorsAsync();
        var messages = errors.SelectMany(e => e.OwnErrors);
        Assert.That(messages, Has.Some.StartsWith("カスタム属性 'IsCurrency' の AvailableFor に、不正な値 'data-model' が指定されています。"));
    }

    [Test]
    public async Task カスタム属性はIsStoredとIsQueryableの組み合わせで使用可否が決まること() {
        // queryable を対象とするカスタム属性は、IsStored と IsQueryable が両方指定されたデータ構造でも使用できる
        using var project1 = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <Data1 IsStored="True" IsQueryable="True" Custom-1="True">
                  <Id Type="int" IsKey="True" />
                </Data1>
                <Query1 IsQueryable="True" Custom-1="True">
                  <Id Type="int" />
                </Query1>
              </DataStructures>
              <CustomAttributes>
                <Custom-1 AvailableFor="queryable" PhysicalName="IsCurrency" Type="Boolean" />
              </CustomAttributes>
            </NijoAppScaffold>
            """);
        Assert.That(await project1.EnumerateValidationErrorsAsync(), Is.Empty);

        // IsStored のみのデータ構造や構造体では使用できない
        using var project2 = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <Data1 IsStored="True" Custom-1="True">
                  <Id Type="int" IsKey="True" />
                </Data1>
                <Struct1 Custom-1="True">
                  <Id Type="int" />
                </Struct1>
              </DataStructures>
              <CustomAttributes>
                <Custom-1 AvailableFor="queryable" PhysicalName="IsCurrency" Type="Boolean" />
              </CustomAttributes>
            </NijoAppScaffold>
            """);
        var errors = await project2.EnumerateValidationErrorsAsync();
        var messages = errors
            .Select(e => (e.XElement.Name.LocalName, Messages: e.AttributeErrors.GetValueOrDefault("Custom-1") ?? []))
            .Where(x => x.Messages.Contains("カスタム属性 'IsCurrency' はこの要素では使用できません。"))
            .Select(x => x.LocalName);
        Assert.That(messages, Is.EquivalentTo(new[] { "Data1", "Struct1" }));
    }
}
