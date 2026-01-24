using NUnit.Framework;

namespace Nijo.IntegrationTest.Tests;

public class IsKey属性のテスト {

    [Test]
    public async Task データモデルのルート集約でキーが必須であること() {
        // 正常系：キーあり
        var project = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <Data1 Type="data-model">
                  <Id Type="int" IsKey="True" />
                  <Name Type="word" />
                </Data1>
              </DataStructures>
            </NijoAppScaffold>
            """);
        var errors = await project.EnumerateValidationErrorsAsync();
        Assert.That(errors, Is.Empty);

        // 異常系：キーなし
        project = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <Data2 Type="data-model">
                  <Id Type="int" />
                  <Name Type="word" />
                </Data2>
              </DataStructures>
            </NijoAppScaffold>
            """);
        errors = await project.EnumerateValidationErrorsAsync();
        Assert.That(errors.SelectMany(e => e.OwnErrors), Does.Contain("キーが指定されていません。"));
    }

    [Test]
    public async Task データモデルのChildrenでキーが必須であること() {
        // 正常系：Childrenにキーあり
        var project = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <Data1 Type="data-model">
                  <Id Type="int" IsKey="True" />
                  <Children Type="children">
                    <ChildId Type="int" IsKey="True" />
                  </Children>
                </Data1>
              </DataStructures>
            </NijoAppScaffold>
            """);
        var errors = await project.EnumerateValidationErrorsAsync();
        Assert.That(errors, Is.Empty);

        // 異常系：Childrenにキーなし
        project = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <Data2 Type="data-model">
                  <Id Type="int" IsKey="True" />
                  <Children Type="children">
                    <ChildId Type="int" />
                  </Children>
                </Data2>
              </DataStructures>
            </NijoAppScaffold>
            """);
        errors = await project.EnumerateValidationErrorsAsync();

        var messages = errors.SelectMany(e => e.OwnErrors).ToArray();

        Assert.That(messages,
            Does.Contain("キーが指定されていません。").Or.Contain("データモデルの子配列は必ず1個以上の主キーを持たなければなりません。"));
    }

    [Test]
    public async Task Childはキー指定不可であること() {
        // 異常系：Child内のメンバーにIsKeyを指定
        var project = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <Data1 Type="data-model">
                  <Id Type="int" IsKey="True" />
                  <ChildEntry Type="child">
                    <ChildId Type="int" IsKey="True" />
                  </ChildEntry>
                </Data1>
              </DataStructures>
            </NijoAppScaffold>
            """);
        var errors = await project.EnumerateValidationErrorsAsync();

        var messages = errors
            .SelectMany(e => e.OwnErrors)
            .Concat(errors.SelectMany(e => e.AttributeErrors.SelectMany(ae => ae.Value)))
            .ToArray();

        Assert.That(messages,
            Does.Contain("データモデルの子集約には主キー属性を付与することができません。").Or.Contain("データモデルの子集約にはキーを指定できません。"));
    }

    [Test]
    public async Task データモデル以外のモデルにはキー指定不可であること() {
        // StructureModel でテスト
        var project = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <Struct1 Type="structure-model">
                  <Id Type="int" IsKey="True" />
                </Struct1>
              </DataStructures>
            </NijoAppScaffold>
            """);
        var errors = await project.EnumerateValidationErrorsAsync();

        var allErrors = errors.SelectMany(e => e.AttributeErrors.Values.SelectMany(v => v)).ToList();

        Assert.That(allErrors, Does.Contain("この属性はこの要素には指定できません。"));
    }
}
