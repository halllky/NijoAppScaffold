using Nijo.SchemaParsing;
using Nijo.Util.DotnetEx;
using Nijo.CodeGenerating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Nijo.ImmutableSchema {
    /// <summary>
    /// モデルの属性のうち、外部参照。
    /// </summary>
    public class RefToMember : IRelationalMember {
        internal RefToMember(XElement xElement, SchemaParseContext ctx, ISchemaPathNode? previous) {
            XElement = xElement;
            _ctx = ctx;
            PreviousNode = previous;
        }

        public XElement XElement { get; }
        private readonly SchemaParseContext _ctx;

        XElement ISchemaPathNode.XElement => XElement;
        public ISchemaPathNode? PreviousNode { get; }

        public string PhysicalName => _ctx.GetPhysicalName(XElement);
        public string DisplayName => XElement.GetDisplayName();
        public string DbName => XElement.GetDbName();
        public decimal Order => XElement.ElementsBeforeSelf().Count();

        /// <summary>
        /// 参照元集約
        /// </summary>
        public AggregateBase Owner {
            get {
                var parent = XElement.Parent;
                return parent == PreviousNode?.XElement
                    ? (AggregateBase?)PreviousNode ?? throw new InvalidOperationException() // パスの巻き戻しの場合
                    : _ctx.ToAggregateBase(parent ?? throw new InvalidOperationException(), this);
            }
        }
        /// <summary>
        /// 参照先集約
        /// </summary>
        public AggregateBase RefTo {
            get {
                var refToElement = _ctx.FindRefTo(XElement) ?? throw new InvalidOperationException();
                return refToElement == PreviousNode?.XElement
                    ? (AggregateBase?)PreviousNode ?? throw new InvalidOperationException() // パスの巻き戻しの場合
                    : _ctx.ToAggregateBase(refToElement, this);
            }
        }
        AggregateBase IRelationalMember.MemberAggregate => RefTo;

        #region モデル毎に定義される属性
        /// <summary>キー属性か否か</summary>
        public bool IsKey => XElement.Attribute(BasicNodeOptions.IsKey.AttributeName) != null;
        /// <summary>必須か否か</summary>
        public bool IsNotNull => XElement.Attribute(BasicNodeOptions.IsNotNull.AttributeName) != null;
        /// <summary>
        /// 汎用参照テーブルへの参照において参照するカテゴリ名。
        /// 汎用参照テーブル以外への参照の場合は null。
        /// </summary>
        public string? Category => XElement.Attribute(BasicNodeOptions.GenericLookupCategory.AttributeName)?.Value;

        /// <summary>
        /// StructureModelがQueryModelを参照する際の、そのクエリモデルのどのモジュールを参照するかの指定。
        /// </summary>
        public E_RefToObject? RefToObject => XElement.Attribute(BasicNodeOptions.RefToObject.AttributeName)?.Value switch {
            BasicNodeOptions.REF_TO_OBJECT_DISPLAY_DATA => E_RefToObject.DisplayData,
            BasicNodeOptions.REF_TO_OBJECT_REF_TARGET => E_RefToObject.RefTarget,
            null => null,
            _ => throw new InvalidOperationException(),
        };

        public enum E_RefToObject {
            DisplayData,
            RefTarget,
        }
        #endregion モデル毎に定義される属性

        #region 等価比較
        public override int GetHashCode() {
            return XElement.GetHashCode();
        }
        public override bool Equals(object? obj) {
            return obj is RefToMember rm
                && rm.XElement == XElement;
        }
        public static bool operator ==(RefToMember? left, RefToMember? right) => ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.Equals(right);
        public static bool operator !=(RefToMember? left, RefToMember? right) => !(left == right);
        #endregion 等価比較

        public override string ToString() {
            // デバッグ用
            return $"{GetType().Name}({this.GetPathFromEntry().Select(x => x.XElement.Name.LocalName).Join(">")})";
        }
    }
}
