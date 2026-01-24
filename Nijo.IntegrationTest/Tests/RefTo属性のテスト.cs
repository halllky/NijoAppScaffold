using NUnit.Framework;

namespace Nijo.IntegrationTest.Tests;

public class RefTo属性のテスト {

    [Test]
    public async Task DataModel_から_DataModel() {
        using var project = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <Data1 Type="data-model">
                  <Id Type="int" IsKey="True" />
                </Data1>
                <Data2 Type="data-model">
                  <Id Type="int" IsKey="True" />
                  <Ref1 Type="ref-to:Data1" />
                </Data2>
              </DataStructures>
            </NijoAppScaffold>
            """);
        Assert.That(await project.EnumerateValidationErrorsAsync(), Is.Empty);
    }

    [Test]
    public async Task DataModel_から_QueryModel() {
        // 正常系: MapToView=True
        using var project1 = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <Query1 Type="query-model" MapToView="True">
                  <Id Type="int" IsKey="True" />
                </Query1>
                <Data1 Type="data-model">
                  <Id Type="int" IsKey="True" />
                  <Ref1 Type="ref-to:Query1" />
                </Data1>
              </DataStructures>
            </NijoAppScaffold>
            """);
        Assert.That(await project1.EnumerateValidationErrorsAsync(), Is.Empty);

        // 異常系: MapToViewなし
        using var project2 = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <Query2 Type="query-model">
                  <Id Type="int" />
                </Query2>
                <Data2 Type="data-model">
                  <Id Type="int" IsKey="True" />
                  <Ref1 Type="ref-to:Query2" />
                </Data2>
              </DataStructures>
            </NijoAppScaffold>
            """);
        var errors = await project2.EnumerateValidationErrorsAsync();
        var allErrors = errors.SelectMany(e => e.OwnErrors.Concat(e.AttributeErrors.Values.SelectMany(v => v)));
        Assert.That(allErrors, Does.Contain("データモデルの集約からはデータモデルの集約またはビューにマッピングされるクエリモデルしか参照できません。"));
    }

    [Test]
    public async Task QueryModel_から_QueryModel() {
        using var project = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <Query1 Type="query-model">
                  <Id Type="int" />
                </Query1>
                <Query2 Type="query-model">
                  <Id Type="int" />
                  <Ref1 Type="ref-to:Query1" />
                </Query2>
              </DataStructures>
            </NijoAppScaffold>
            """);
        Assert.That(await project.EnumerateValidationErrorsAsync(), Is.Empty);
    }

    [Test]
    public async Task QueryModel_から_DataModel() {
        // 正常系: GenerateDefaultQueryModel=True
        using var project1 = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <Data1 Type="data-model" GenerateDefaultQueryModel="True">
                  <Id Type="int" IsKey="True" />
                </Data1>
                <Query1 Type="query-model">
                  <Id Type="int" />
                  <Ref1 Type="ref-to:Data1" />
                </Query1>
              </DataStructures>
            </NijoAppScaffold>
            """);
        Assert.That(await project1.EnumerateValidationErrorsAsync(), Is.Empty);

        // 異常系: GenerateDefaultQueryModelなし
        using var project2 = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <Data2 Type="data-model">
                  <Id Type="int" IsKey="True" />
                </Data2>
                <Query2 Type="query-model">
                  <Id Type="int" />
                  <Ref1 Type="ref-to:Data2" />
                </Query2>
              </DataStructures>
            </NijoAppScaffold>
            """);
        var errors = await project2.EnumerateValidationErrorsAsync();
        var allErrors = errors.SelectMany(e => e.OwnErrors.Concat(e.AttributeErrors.Values.SelectMany(v => v)));
        Assert.That(allErrors, Does.Contain("クエリモデルの集約からはクエリモデルの集約またはGenerateDefaultQueryModel属性が付与されたデータモデルしか参照できません。"));
    }

    [Test]
    public async Task StructureModel_から_StructureModel() {
        using var project = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <Struct1 Type="structure-model">
                  <Id Type="int" />
                </Struct1>
                <Struct2 Type="structure-model">
                  <Id Type="int" />
                  <Ref1 Type="ref-to:Struct1" />
                </Struct2>
              </DataStructures>
            </NijoAppScaffold>
            """);
        Assert.That(await project.EnumerateValidationErrorsAsync(), Is.Empty);
    }

    [Test]
    public async Task StructureModel_から_QueryModel() {
        // 正常系: RefToObject指定
        using var project1 = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <Query1 Type="query-model">
                  <Id Type="int" />
                </Query1>
                <Struct1 Type="structure-model">
                    <Ref1 Type="ref-to:Query1" RefToObject="RefTarget" />
                </Struct1>
              </DataStructures>
            </NijoAppScaffold>
            """);
        Assert.That(await project1.EnumerateValidationErrorsAsync(), Is.Empty);

        // 異常系: RefToObjectなし
        using var project2 = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <Query2 Type="query-model">
                   <Id Type="int" />
                </Query2>
                <Struct2 Type="structure-model">
                    <Ref1 Type="ref-to:Query2" />
                </Struct2>
              </DataStructures>
            </NijoAppScaffold>
            """);
        var errors = await project2.EnumerateValidationErrorsAsync();
        var allErrors = errors.SelectMany(e => e.OwnErrors.Concat(e.AttributeErrors.Values.SelectMany(v => v)));
        Assert.That(allErrors, Does.Contain("StructureModelからクエリモデルを外部参照する場合、RefToObject属性を指定する必要があります。"));
    }

    [Test]
    public async Task 複雑な参照関係_DataModel() {

        // * 参照先がルートでなく子孫集約
        // * 同じ集約を複数経路でref-toしている

        using var project = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <DataA Type="data-model" GenerateDefaultQueryModel="True">
                  <Id Type="int" IsKey="True" />
                  <ChildA Type="children">
                    <ChildId Type="int" IsKey="True" />
                    <GrandChildA Type="children">
                      <GrandChildId Type="int" IsKey="True" />
                    </GrandChildA>
                  </ChildA>
                </DataA>

                <DataB Type="data-model" GenerateDefaultQueryModel="True">
                  <Ref1 Type="ref-to:DataA/ChildA/GrandChildA" IsKey="True" />
                  <Ref2 Type="ref-to:DataA/ChildA/GrandChildA" IsKey="True" />
                </DataB>

                <DataC Type="data-model" GenerateDefaultQueryModel="True">
                  <RefB1 Type="ref-to:DataB" IsKey="True" />
                  <RefB2 Type="ref-to:DataB" />
                </DataC>
              </DataStructures>
            </NijoAppScaffold>
            """);
        Assert.That(await project.EnumerateValidationErrorsAsync(), Is.Empty);

        await project.GenerateCodeAsync();

        Assert.That(await project.CheckCompileAsync(), Is.True);
    }

    [Test]
    public async Task 複雑な参照関係_QueryModelとStructureModel() {
        using var project = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <QueryA Type="query-model" MapToView="True">
                  <Id Type="int" IsKey="True" />
                  <ChildA Type="children">
                    <ChildId Type="int" IsKey="True" />
                    <GrandChildA Type="children">
                      <GrandChildId Type="int" IsKey="True" />
                    </GrandChildA>
                  </ChildA>
                </QueryA>

                <QueryB Type="query-model" MapToView="True">
                  <Ref1 Type="ref-to:QueryA/ChildA/GrandChildA" IsKey="True" />
                  <Ref2 Type="ref-to:QueryA/ChildA/GrandChildA" IsKey="True" />
                </QueryB>

                <StructC Type="structure-model">
                  <RefB1 Type="ref-to:QueryB" RefToObject="RefTarget" />
                  <RefB2 Type="ref-to:QueryB" RefToObject="DisplayData" />
                </StructC>

                <StructD Type="structure-model">
                  <RefC1 Type="ref-to:StructC" />
                  <RefC2 Type="ref-to:StructC" />
                </StructD>
              </DataStructures>
            </NijoAppScaffold>
            """);
        Assert.That(await project.EnumerateValidationErrorsAsync(), Is.Empty);

        await project.GenerateCodeAsync();

        Assert.That(await project.CheckCompileAsync(), Is.True);
    }
}
