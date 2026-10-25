using NUnit.Framework;

namespace Nijo.IntegrationTest.Tests;

public class IsKey属性のテスト {

    [Test]
    public async Task DataModel_ルート集約でキーが必須であること() {
        // 正常系：キーあり
        using var project1 = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <Data1 IsStored="True">
                  <Id Type="int" IsKey="True" />
                  <Name Type="word" />
                </Data1>
              </DataStructures>
            </NijoAppScaffold>
            """);

        var errors1 = await project1.EnumerateValidationErrorsAsync();
        Assert.That(errors1, Is.Empty);

        // 異常系：キーなし
        using var project2 = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <Data2 IsStored="True">
                  <Id Type="int" />
                  <Name Type="word" />
                </Data2>
              </DataStructures>
            </NijoAppScaffold>
            """);
        var errors2 = await project2.EnumerateValidationErrorsAsync();
        Assert.That(errors2.SelectMany(e => e.OwnErrors), Does.Contain("キーが指定されていません。"));
    }

    [Test]
    public async Task DataModel_Childrenでキーが必須であること() {
        // 正常系：Childrenにキーあり
        using var project1 = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <Data1 IsStored="True">
                  <Id Type="int" IsKey="True" />
                  <Children Type="children">
                    <ChildId Type="int" IsKey="True" />
                  </Children>
                </Data1>
              </DataStructures>
            </NijoAppScaffold>
            """);
        var errors1 = await project1.EnumerateValidationErrorsAsync();
        Assert.That(errors1, Is.Empty);

        // 異常系：Childrenにキーなし
        using var project2 = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <Data2 IsStored="True">
                  <Id Type="int" IsKey="True" />
                  <Children Type="children">
                    <ChildId Type="int" />
                  </Children>
                </Data2>
              </DataStructures>
            </NijoAppScaffold>
            """);
        var errors2 = await project2.EnumerateValidationErrorsAsync();

        var messages = errors2.SelectMany(e => e.OwnErrors).ToArray();

        Assert.That(messages, Is.EquivalentTo(new[] { "キーが指定されていません。" }));
    }

    [Test]
    public async Task DataModel_Childはキー指定不可であること() {
        // 異常系：Child内のメンバーにIsKeyを指定
        using var project = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <Data1 IsStored="True">
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

        Assert.That(messages, Is.EquivalentTo(new[] { "IsStored属性が指定されたデータ構造の子集約にはキーを指定できません。" }));
    }

    [Test]
    public async Task QueryModel_Viewにマッピングされる場合はルート集約とChildrenにキーが必須() {
        // 正常系：ルートとChildrenにキーあり
        using var project1 = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <Query1 IsQueryable="True" MapToView="True">
                  <Id Type="int" IsKey="True" />
                  <Children Type="children">
                     <ChildId Type="int" IsKey="True" />
                  </Children>
                </Query1>
              </DataStructures>
            </NijoAppScaffold>
            """);
        Assert.That(await project1.EnumerateValidationErrorsAsync(), Is.Empty);

        // 異常系：ルートにキーなし（Childrenあり）
        using var project2 = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <Query2 IsQueryable="True" MapToView="True">
                  <Id Type="int" />
                  <Children Type="children">
                     <ChildId Type="int" IsKey="True" />
                  </Children>
                </Query2>
              </DataStructures>
            </NijoAppScaffold>
            """);
        var errors = await project2.EnumerateValidationErrorsAsync();
        Assert.That(errors.SelectMany(e => e.OwnErrors), Does.Contain("Child/Childrenがあるビューにマッピングされるデータ構造にはキーが必要です。"));

        // 異常系：Childrenにキーなし
        using var project3 = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <Query3 IsQueryable="True" MapToView="True">
                  <Id Type="int" IsKey="True" />
                  <Children Type="children">
                     <ChildId Type="int" />
                  </Children>
                </Query3>
              </DataStructures>
            </NijoAppScaffold>
            """);
        errors = await project3.EnumerateValidationErrorsAsync();
        Assert.That(errors.SelectMany(e => e.OwnErrors), Does.Contain("Child/Childrenがあるビューにマッピングされるデータ構造にはキーが必要です。"));
    }

    [Test]
    public async Task QueryModel_Viewにマッピングされる場合であってもChildにはキー指定不可() {
        using var project = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <Query1 IsQueryable="True" MapToView="True">
                  <Id Type="int" IsKey="True" />
                  <ChildEntry Type="child">
                     <ChildId Type="int" IsKey="True" />
                  </ChildEntry>
                </Query1>
              </DataStructures>
            </NijoAppScaffold>
            """);
        var errors = await project.EnumerateValidationErrorsAsync();
        var allErrors = errors.SelectMany(e => e.AttributeErrors.Values.SelectMany(v => v));
        Assert.That(allErrors, Does.Contain("IsQueryable属性のみが指定されたデータ構造の子集約にはキーを指定できません。"));
    }

    [Test]
    public async Task QueryModel_Viewにマッピングされない場合はキー指定不可() {
        // ルートにキー指定
        using var project1 = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <Query1 IsQueryable="True">
                  <Id Type="int" IsKey="True" />
                </Query1>
              </DataStructures>
            </NijoAppScaffold>
            """);
        var errors1 = await project1.EnumerateValidationErrorsAsync();
        var allErrors1 = errors1.SelectMany(e => {
            return e.OwnErrors.Concat(e.AttributeErrors.Values.SelectMany(v => v));
        });

        Assert.That(allErrors1, Does.Contain("IsQueryable属性のみが指定されたデータ構造のルート集約には、ビューにマッピングされない場合はキーを指定できません。"));

        // Childrenにキー指定
        using var project2 = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <Query2 IsQueryable="True">
                  <Children Type="children">
                     <ChildId Type="int" IsKey="True" />
                  </Children>
                </Query2>
              </DataStructures>
            </NijoAppScaffold>
            """);
        var errors2 = await project2.EnumerateValidationErrorsAsync();
        var allErrors2 = errors2.SelectMany(e => e.AttributeErrors.Values.SelectMany(v => v));
        Assert.That(allErrors2, Does.Contain("IsQueryable属性のみが指定されたデータ構造の子配列には、ビューにマッピングされない場合はキーを指定できません。"));
    }

    [Test]
    public async Task その他のモデルにはキー指定不可であること() {
        // StructureModel でテスト
        using var project = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <Struct1>
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
