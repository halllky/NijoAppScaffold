using Nijo.CodeGenerating;
using Nijo.Parts.Common;
using Nijo.SchemaParsing;
using System;
using System.Xml.Linq;
using Nijo.DataStructures.Aggregates;

namespace Nijo.DataStructures;

/// <summary>
/// データ構造のルート集約に指定すると、そのルート集約から生成されるソースコードが増える属性。
/// 属性の指定可否と値の検証に加え、その属性が指定されたルート集約に対する検証とソースコードの生成を担う。
/// </summary>
internal abstract class FeatureOption : NodeOption {

    /// <summary>
    /// この属性が指定されたルート集約1個に対する検証を行います。
    /// </summary>
    public virtual void ValidateRootAggregate(XElement rootAggregateElement, SchemaParseContext context, SchemaValidationErrors errors) { }

    /// <summary>
    /// この属性が指定されたルート集約1個と対応するソースコードを生成します。
    /// </summary>
    public abstract void Render(CodeRenderingContext ctx, RootAggregate rootAggregate, SourceFileByAggregate aggregateFile);

    /// <summary>
    /// この属性が指定されたルート集約が1個以上存在する場合に1回だけ呼ばれ、ルート集約1個と対応しないソースコードを生成します。
    /// </summary>
    public virtual void RenderCommon(CodeRenderingContext ctx) { }
}
