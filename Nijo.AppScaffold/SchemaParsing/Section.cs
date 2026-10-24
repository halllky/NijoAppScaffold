using Microsoft.Extensions.Logging;
using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Nijo.SchemaParsing;

/// <summary>
/// nijo.xml のセクション1個分の定義。
/// そのセクションに定義されるルート集約の、ルート集約単位の検証と、ソースコードの生成を担う。
/// 個々の属性の検証は <see cref="NodeOption"/> が担う。
/// </summary>
internal abstract class Section {
    /// <summary>このクラスが定義するセクション</summary>
    public abstract E_Section Kind { get; }

    /// <summary>
    /// このセクションに定義されたルート集約1個に対する検証を行います。
    /// </summary>
    public virtual void Validate(XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) { }

    /// <summary>
    /// このセクションに定義されたルート集約からソースコードを生成します。
    /// このセクションにルート集約が1個も定義されていない場合も呼ばれます。
    /// </summary>
    public abstract void Render(CodeRenderingContext ctx);

    /// <summary>
    /// ルート集約ごとのソースコード生成処理を並列で実行します。
    /// </summary>
    protected static void RenderEach<T>(CodeRenderingContext ctx, IEnumerable<T> rootAggregates, Action<T> render) where T : ISectionRoot {
        var parallelOptions = new ParallelOptions {
            MaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, 4),
        };
        Parallel.ForEach(rootAggregates, parallelOptions, rootAggregate => {
            ctx.Logger.LogInformation("レンダリング開始: {name}", rootAggregate.DisplayName);
            try {
                render(rootAggregate);
            } catch (Exception ex) {
                throw new InvalidOperationException($"{rootAggregate}のレンダリングで例外が発生", ex);
            }
        });
    }
}
