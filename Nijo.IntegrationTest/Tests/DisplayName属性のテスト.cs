using System.Linq;
using System.Xml.Linq;
using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Models.StaticEnumModelModules;
using Nijo.SchemaParsing;
using NUnit.Framework;

namespace Nijo.IntegrationTest.Tests;

public class DisplayName属性のテスト {

    [Test]
    public async Task DisplayNameIsEmpty指定時は表示用名称が空文字になること() {
        using var project = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <Data1 Type="data-model" DisplayNameIsEmpty="True">
                  <Id Type="int" IsKey="True" DisplayNameIsEmpty="True" />
                  <Name Type="word" />
                </Data1>
              </DataStructures>
            </NijoAppScaffold>
            """);

        var errors = await project.EnumerateValidationErrorsAsync();
        Assert.That(errors, Is.Empty);

        var schema = await BuildSchemaAsync(project.Project.SchemaXmlPath);
        var rootAggregate = schema.GetRootAggregates().Single(root => root.PhysicalName == "Data1");
        var idMember = rootAggregate.GetMembers().OfType<ValueMember>().Single(member => member.PhysicalName == "Id");
        var nameMember = rootAggregate.GetMembers().OfType<ValueMember>().Single(member => member.PhysicalName == "Name");

        Assert.Multiple(() => {
            Assert.That(rootAggregate.DisplayName, Is.EqualTo(string.Empty));
            Assert.That(idMember.DisplayName, Is.EqualTo(string.Empty));
            Assert.That(nameMember.DisplayName, Is.EqualTo("Name"));
        });
    }

    [Test]
    public async Task DisplayNameとDisplayNameIsEmptyの併用はエラーになること() {
        using var project = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <DataStructures>
                <Data1 Type="data-model" DisplayName="表示名" DisplayNameIsEmpty="True">
                  <Id Type="int" IsKey="True" />
                </Data1>
              </DataStructures>
            </NijoAppScaffold>
            """);

        var errors = await project.EnumerateValidationErrorsAsync();
        var messages = errors.SelectMany(error => error.AttributeErrors.Values.SelectMany(value => value)).ToArray();

        Assert.That(messages, Does.Contain("DisplayName 属性と同時に指定することはできません。"));
    }

    [Test]
    public async Task StaticEnumの値でDisplayNameIsEmptyを使用できること() {
        using var project = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <StaticEnums>
                <Status Type="enum">
                  <Open key="1" DisplayNameIsEmpty="True" />
                  <Closed key="2" DisplayName="完了" />
                </Status>
              </StaticEnums>
            </NijoAppScaffold>
            """);

        var errors = await project.EnumerateValidationErrorsAsync();
        Assert.That(errors, Is.Empty);

        var schema = await BuildSchemaAsync(project.Project.SchemaXmlPath);
        var staticEnum = schema.GetRootAggregates().Single(root => root.PhysicalName == "Status");
        var open = staticEnum.GetMembers().OfType<StaticEnumValueDef>().Single(member => member.PhysicalName == "Open");
        var closed = staticEnum.GetMembers().OfType<StaticEnumValueDef>().Single(member => member.PhysicalName == "Closed");

        Assert.Multiple(() => {
            Assert.That(open.DisplayName, Is.EqualTo(string.Empty));
            Assert.That(closed.DisplayName, Is.EqualTo("完了"));
        });

        Assert.That(await project.GenerateCodeAsync(), Is.True);
        Assert.That(await project.CheckCompileAsync(), Is.True);
    }

    private static async Task<ApplicationSchema> BuildSchemaAsync(string schemaXmlPath) {
        await using var stream = File.OpenRead(schemaXmlPath);
        var document = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);
        var rule = SchemaParseRule.Default();
        var parseContext = new SchemaParseContext(document, rule, GeneratedProjectOptions.Parse(document, true));

        var success = parseContext.TryBuildSchema(document, out var schema, out var errors);
        Assert.That(success, Is.True, string.Join(Environment.NewLine, errors.SelectMany(error => error.OwnErrors)));

        return schema;
    }
}
