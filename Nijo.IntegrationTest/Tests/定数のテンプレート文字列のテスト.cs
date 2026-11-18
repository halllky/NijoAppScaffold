using System.Linq;
using NUnit.Framework;

namespace Nijo.IntegrationTest.Tests;

public class 定数のテンプレート文字列のテスト {

    [Test]
    public async Task グループの種類の既定値と定数ごとの上書きを指定できること() {
        using var project = await NijoTestUtil.CreateNewProjectAsync("""
            <NijoAppScaffold>
              <Constants>
                <画面のURL TemplateKind="URL">
                  <売上詳細 ConstantType="template" ConstantValue="/uriage/{売上SEQ:int}?tab={タブ}" />
                  <JSONの例 ConstantType="template" ConstantValue="{{&quot;name&quot;: &quot;{名前}&quot;}}" TemplateKind="JSON" />
                  <入れ子 ConstantType="child">
                    <商品詳細 ConstantType="template" ConstantValue="/shohin/{商品SEQ:int}" />
                  </入れ子>
                </画面のURL>
              </Constants>
            </NijoAppScaffold>
            """);

        var errors = await project.EnumerateValidationErrorsAsync();
        Assert.That(errors.SelectMany(GetMessages), Is.Empty);
    }

    [TestCase("/uriage/{0}", "名前で書いてください", TestName = "番号で書いた引数はエラー")]
    [TestCase("{\"name\": \"{名前}\"}", "名前で書いてください", TestName = "重ねていない波括弧はエラー")]
    [TestCase("/uriage/{売上SEQ:date}", "型「date」は使えません", TestName = "未知の型はエラー")]
    [TestCase("/uriage/{売上SEQ:int}/{売上SEQ}", "異なる型が指定されています", TestName = "同じ引数に異なる型はエラー")]
    public async Task テンプレートの書き方の誤りがエラーになること(string template, string expectedMessage) {
        using var project = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <Constants>
                <画面のURL TemplateKind="URL">
                  <売上詳細 ConstantType="template" ConstantValue="{{System.Security.SecurityElement.Escape(template)}}" />
                </画面のURL>
              </Constants>
            </NijoAppScaffold>
            """);

        var errors = await project.EnumerateValidationErrorsAsync();
        Assert.That(errors.SelectMany(GetMessages), Has.Some.Contains(expectedMessage));
    }

    [Test]
    public async Task テンプレートの種類が無いとエラーになること() {
        using var project = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <Constants>
                <画面のURL>
                  <売上詳細 ConstantType="template" ConstantValue="/uriage/{売上SEQ:int}" />
                </画面のURL>
              </Constants>
            </NijoAppScaffold>
            """);

        var errors = await project.EnumerateValidationErrorsAsync();
        Assert.That(errors.SelectMany(GetMessages), Does.Contain("テンプレート文字列の定数には、自身かその親のグループに TemplateKind 属性を指定してください。"));
    }

    [Test]
    public async Task テンプレート文字列でない定数にテンプレートの種類を指定するとエラーになること() {
        using var project = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <Constants>
                <画面のURL>
                  <従業員 ConstantType="string" ConstantValue="/jugyoin" TemplateKind="URL" />
                </画面のURL>
              </Constants>
            </NijoAppScaffold>
            """);

        var errors = await project.EnumerateValidationErrorsAsync();
        Assert.That(errors.SelectMany(GetMessages), Does.Contain("TemplateKind 属性は、グループかテンプレート文字列の定数にだけ指定できます。"));
    }

    private static IEnumerable<string> GetMessages(SchemaParsing.SchemaParseContext.ValidationError error) {
        return error.OwnErrors.Concat(error.AttributeErrors.SelectMany(attr => attr.Value));
    }
}
