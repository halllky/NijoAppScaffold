using System.Linq;
using NUnit.Framework;

namespace Nijo.IntegrationTest.Tests;

public class テンプレート文字列のテスト {

    [Test]
    public async Task グループの種類の既定値と上書きを指定できること() {
        using var project = await NijoTestUtil.CreateNewProjectAsync("""
            <NijoAppScaffold>
              <Templates>
                <画面のURL TemplateKind="URL">
                  <売上詳細 Template="/uriage/{売上SEQ:int}?tab={タブ}" />
                  <JSONの例 Template="{{&quot;name&quot;: &quot;{名前}&quot;}}" TemplateKind="JSON" />
                  <入れ子 TemplateKind="文言">
                    <商品詳細 Template="商品 {商品SEQ:int} の詳細" />
                  </入れ子>
                </画面のURL>
              </Templates>
            </NijoAppScaffold>
            """);

        var errors = await project.EnumerateValidationErrorsAsync();
        Assert.That(errors.SelectMany(GetMessages), Is.Empty);
    }

    [TestCase("/uriage/{0}", "名前で書いてください", TestName = "番号で書いた引数はエラー")]
    [TestCase("{\"name\": \"{名前}\"}", "名前で書いてください", TestName = "重ねていない波括弧はエラー")]
    [TestCase("/uriage/{売上SEQ:date}", "型「date」は使えません", TestName = "未知の型はエラー")]
    [TestCase("/uriage/{売上SEQ:int}/{売上SEQ}", "異なる型が指定されています", TestName = "同じ引数に異なる型はエラー")]
    [TestCase("/uriage/{templateKind}", "引数の名前に「templateKind」は使えません", TestName = "予約された引数名はエラー")]
    public async Task テンプレートの書き方の誤りがエラーになること(string template, string expectedMessage) {
        using var project = await NijoTestUtil.CreateNewProjectAsync($$"""
            <NijoAppScaffold>
              <Templates>
                <画面のURL TemplateKind="URL">
                  <売上詳細 Template="{{System.Security.SecurityElement.Escape(template)}}" />
                </画面のURL>
              </Templates>
            </NijoAppScaffold>
            """);

        var errors = await project.EnumerateValidationErrorsAsync();
        Assert.That(errors.SelectMany(GetMessages), Has.Some.Contains(expectedMessage));
    }

    [Test]
    public async Task テンプレートの種類が無いとエラーになること() {
        using var project = await NijoTestUtil.CreateNewProjectAsync("""
            <NijoAppScaffold>
              <Templates>
                <画面のURL>
                  <売上詳細 Template="/uriage/{売上SEQ:int}" />
                </画面のURL>
              </Templates>
            </NijoAppScaffold>
            """);

        var errors = await project.EnumerateValidationErrorsAsync();
        Assert.That(errors.SelectMany(GetMessages), Has.Some.Contains("TemplateKind 属性を指定してください"));
    }

    [Test]
    public async Task グループにテンプレート文字列を指定するとエラーになること() {
        using var project = await NijoTestUtil.CreateNewProjectAsync("""
            <NijoAppScaffold>
              <Templates>
                <画面のURL TemplateKind="URL" Template="/uriage">
                  <売上詳細 Template="/uriage/{売上SEQ:int}" />
                </画面のURL>
              </Templates>
            </NijoAppScaffold>
            """);

        var errors = await project.EnumerateValidationErrorsAsync();
        Assert.That(errors.SelectMany(GetMessages), Has.Some.Contains("グループには指定できません"));
    }

    private static IEnumerable<string> GetMessages(SchemaParsing.SchemaParseContext.ValidationError error) {
        return error.OwnErrors.Concat(error.AttributeErrors.SelectMany(attr => attr.Value));
    }
}
