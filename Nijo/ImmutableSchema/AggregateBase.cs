using Nijo.CodeGenerating;
using Nijo.SchemaParsing;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Nijo.ImmutableSchema {
    /// <summary>
    /// モデルの集約。
    /// 集約ルート, Child, Children, VariationItem のいずれか。
    /// </summary>
    public abstract class AggregateBase : ISchemaPathNode {

        internal AggregateBase(XElement xElement, SchemaParseContext ctx, ISchemaPathNode? previous) {
            XElement = xElement;
            _ctx = ctx;
            PreviousNode = previous;
        }
        public XElement XElement { get; }
        private protected readonly SchemaParseContext _ctx;

        XElement ISchemaPathNode.XElement => XElement;
        public ISchemaPathNode? PreviousNode { get; }

        /// <summary>
        /// 物理名
        /// </summary>
        public string PhysicalName => _ctx.GetPhysicalName(XElement);
        /// <summary>
        /// 表示用名称
        /// </summary>
        public string DisplayName => _ctx.GetDisplayName(XElement);
        /// <summary>
        /// データベーステーブル名
        /// </summary>
        public string DbName => _ctx.GetDbName(XElement);
        /// <summary>
        /// ラテン語名
        /// </summary>
        public string LatinName => _ctx.GetLatinName(XElement);

        /// <summary>
        /// この集約が参照先エントリーとして参照された場合の名前。
        /// スキーマ定義xmlのType属性の表記と一致しているとメタデータを使った処理が書きやすくて嬉しいので合わせている。
        /// </summary>
        public string RefEntryName => $"ref-to:{EnumerateThisAndAncestors().Select(a => a.PhysicalName).Join("/")}";

        /// <summary>
        /// コメント
        /// </summary>
        [Obsolete("SchemaParseContext.GetCommentSingleLine または SchemaParseContext.GetCommentMultiLine を直接使ってください")]
        public string GetComment(E_CsTs csts) => _ctx.GetCommentSingleLine(XElement, csts);

        /// <summary>
        /// この集約が持つメンバーを列挙します。
        /// <list type="bullet">
        /// <item>親: 列挙しません。</item>
        /// <item>子(Child, Children): 列挙します。</item>
        /// <item>参照先(RefTo): 列挙します。</item>
        /// </list>
        /// </summary>
        public IEnumerable<IAggregateMember> GetMembers() {
            foreach (var el in XElement.ElementsWithoutMemo()) {
                // パスの巻き戻しの場合（この集約の1つ前がこの集約の子、かつその子を列挙しようとしている場合）
                // 新たにインスタンスを作るのでなく1つ前のインスタンスをそのまま使う
                if (el == PreviousNode?.XElement) {
                    yield return (IAggregateMember)PreviousNode;
                    continue;
                }

                var nodeType = _ctx.GetNodeType(el);
                yield return nodeType switch {
                    E_NodeType.ChildAggregate => new ChildAggregate(el, _ctx, this),
                    E_NodeType.ChildrenAggregate => new ChildrenAggregate(el, _ctx, this),
                    E_NodeType.Ref => new RefToMember(el, _ctx, this),
                    E_NodeType.ValueMember => new ValueMember(el, _ctx, this),
                    E_NodeType.StaticEnumValue => new Models.StaticEnumModelModules.StaticEnumValueDef(el, _ctx, this),
                    _ => throw new InvalidOperationException($"メンバーでない種類: {nodeType}（{el}）"),
                };
            }
        }
        /// <summary>
        /// この集約に直接属するキー項目を返します。
        /// つまりキーに <see cref="RefToMember"/> が含まれるならばそのRef自身を列挙します。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IAggregateMember> GetOwnKeys() {
            return GetMembers().Where(m => m is ValueMember vm && vm.IsKey
                                        || m is RefToMember rm && rm.IsKey);
        }
        /// <summary>
        /// キー項目のうち <see cref="ValueMember"/> を列挙します。
        /// つまりキーに <see cref="RefToMember"/> が含まれるならばそのRefの値メンバーを、
        /// キーに親が含まれるならば親の値メンバーを列挙します。
        /// </summary>
        public IEnumerable<ValueMember> GetKeyVMs() {
            // 親および祖先のキー項目を列挙
            var parent = GetParent();
            if (parent != null) {
                foreach (var parentKeyVm in parent.GetKeyVMs()) {
                    yield return parentKeyVm;
                }
            }
            // 自身のメンバーからキー項目を列挙
            foreach (var member in GetMembers()) {
                if (member is ValueMember vm && vm.IsKey) {
                    yield return vm;

                } else if (member is RefToMember refTo && refTo.IsKey) {
                    foreach (var refToVm in refTo.RefTo.GetKeyVMs()) {
                        yield return refToVm;
                    }
                }
            }
        }


        #region 親子
        /// <summary>
        /// この集約の親を返します。
        /// </summary>
        public AggregateBase? GetParent() {
            // この集約がルート集約の場合
            if (XElement.GetParentWithoutMemo() == XElement.Document?.Root) return null;

            // 1つ前の集約が親の場合
            if (PreviousNode is AggregateBase agg && agg.XElement == XElement.GetParentWithoutMemo()) {
                return agg;
            }

            // 子から親に辿る場合
            return _ctx.ToAggregateBase(XElement.GetParentWithoutMemo() ?? throw new InvalidOperationException(), this);
        }

        /// <summary>
        /// 祖先集約を列挙します。ルート集約が先
        /// </summary>
        public IEnumerable<AggregateBase> EnumerateAncestors() {
            var ancestors = new List<AggregateBase>();
            var current = GetParent();

            while (current != null) {
                ancestors.Add(current);
                current = current.GetParent();
            }

            // ルート集約が先になるよう逆順にして返す
            return ancestors.AsEnumerable().Reverse();
        }
        /// <summary>
        /// この集約と祖先集約を列挙します。ルート集約が先
        /// </summary>
        public IEnumerable<AggregateBase> EnumerateThisAndAncestors() {
            foreach (var ancestor in EnumerateAncestors()) {
                yield return ancestor;
            }

            yield return this;
        }
        /// <summary>
        /// ルート集約を返します。
        /// </summary>
        internal RootAggregate GetRoot() {
            var ancestor = this;
            while (true) {
                var parent = ancestor.GetParent();
                if (parent == null) return (RootAggregate)ancestor;
                ancestor = parent;
            }
        }

        /// <summary>
        /// ルート集約からこのメンバーまでのパスを列挙する。
        /// 経路情報はクリアされ、ルート集約がエントリーになる。
        /// </summary>
        public IEnumerable<AggregateBase> GetPathFromRoot() {
            var ancesotors = XElement
                .AncestorsAndSelf()
                .Reverse()
                // * ドキュメントルートも祖先に含まれてしまうので除外
                // * メモ型はカウントしない
                .Where(el => el != XElement.Document?.Root
                          && el.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value != SchemaParseContext.NODE_TYPE_MEMO);

            var prev = (AggregateBase?)null;

            foreach (var el in ancesotors) {
                var aggregate = _ctx.ToAggregateBase(el, prev);
                yield return aggregate;
                prev = aggregate;
            }
        }

        /// <summary>
        /// 子孫集約を列挙します。
        /// </summary>
        public IEnumerable<AggregateBase> EnumerateDescendants() {
            foreach (var child in GetMembers().OfType<AggregateBase>()) {
                yield return child;
                foreach (var descendant in child.EnumerateDescendants()) {
                    yield return descendant;
                }
            }
        }
        /// <summary>
        /// この集約と子孫集約を列挙します。
        /// </summary>
        public IEnumerable<AggregateBase> EnumerateThisAndDescendants() {
            yield return this;

            foreach (var descendant in EnumerateDescendants()) {
                yield return descendant;
            }
        }

        /// <summary>
        /// この集約が引数の集約の親か否かを返します。
        /// </summary>
        public bool IsParentOf(AggregateBase aggregate) {
            return aggregate.XElement.GetParentWithoutMemo() == XElement;
        }
        /// <summary>
        /// この集約が引数の集約の子か否かを返します。
        /// （<see cref="ChildAggregate"/> または <see cref="ChildrenAggregate"/> のいずれでもtrue）
        /// </summary>
        public bool IsChildOf(AggregateBase aggregate) {
            return XElement.GetParentWithoutMemo() == aggregate.XElement;
        }
        /// <summary>
        /// この集約が引数の集約の祖先か否かを返します。
        /// </summary>
        public bool IsAncestorOf(AggregateBase aggregate) {
            return aggregate.XElement.Ancestors().Contains(XElement);
        }
        /// <summary>
        /// この集約が引数の集約の子孫か否かを返します。
        /// </summary>
        public bool IsDescendantOf(AggregateBase aggregate) {
            return aggregate.XElement.Descendants().Contains(XElement);
        }
        #endregion 親子


        #region 外部参照
        /// <summary>
        /// この集約がメソッドの引数の集約の唯一のキーか否かを返します。
        /// なお、引数の集約がChildrenの場合、Childrenは親の主キーを継承するため、必ずfalseになります。
        /// </summary>
        /// <param name="refFrom">参照元</param>
        public bool IsSingleKeyOf(AggregateBase refFrom) {
            if (refFrom is ChildrenAggregate) return false;

            var keys = refFrom.GetOwnKeys().ToArray();
            if (keys.Length != 1) return false;
            if (keys[0] is not RefToMember rm) return false;
            if (rm.RefTo != this) return false;
            return true;
        }
        /// <summary>
        /// この集約を直接外部参照しているメンバーを列挙します。
        /// </summary>
        public IEnumerable<RefToMember> GetRefFroms() {
            return _ctx
                .FindRefFrom(XElement)
                .Select(el => el == PreviousNode?.XElement
                    ? (RefToMember)PreviousNode // パスの巻き戻しの場合
                    : new RefToMember(el, _ctx, this));
        }
        #endregion 外部参照


        /// <summary>
        /// <see cref="ISchemaPathNode"/> としての経路情報をクリアした新しいインスタンスを返す
        /// </summary>
        public abstract AggregateBase AsEntry();


        #region 等価比較
        public override int GetHashCode() {
            return XElement.GetHashCode();
        }
        public override bool Equals(object? obj) {
            return obj is AggregateBase agg
                && agg.XElement == XElement;
        }
        public static bool operator ==(AggregateBase? left, AggregateBase? right) => ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.Equals(right);
        public static bool operator !=(AggregateBase? left, AggregateBase? right) => !(left == right);
        #endregion 等価比較

        public override string ToString() {
            // デバッグ用
            return $"{GetType().Name}({this.GetPathFromEntry().Select(x => x.XElement.Name.LocalName).Join(">")})";
        }
    }

    /// <summary>
    /// 集約ルート
    /// </summary>
    public class RootAggregate : AggregateBase {
        internal RootAggregate(XElement xElement, SchemaParseContext ctx, ISchemaPathNode? previous)
            : base(xElement, ctx, previous) { }

        public bool IsView => XElement.Attribute(BasicNodeOptions.MapToView.AttributeName) != null;
        public IModel Model => _ctx.TryGetModel(XElement, out var model) ? model : throw new InvalidOperationException("ありえない");

        public override AggregateBase AsEntry() {
            return new RootAggregate(XElement, _ctx, null);
        }

        #region DataModelと全く同じ型のQueryModel, CommandModel を生成するかどうか
        public bool GenerateDefaultQueryModel => XElement.Attribute(BasicNodeOptions.GenerateDefaultQueryModel.AttributeName) != null;
        public bool GenerateBatchUpdateCommand => XElement.Attribute(BasicNodeOptions.GenerateBatchUpdateCommand.AttributeName) != null;
        #endregion DataModelと全く同じ型のQueryModel, CommandModel を生成するかどうか

        #region このCommandModelの引数・戻り値の集約を返す
        /// <summary>
        /// このCommandModelの引数の構造体を返します。
        /// このルート集約がCommandModelでない場合は例外。
        /// </summary>
        public ICreatablePresentationLayerStructure? GetParameterStructure() {
            if (Model is not Models.CommandModel) throw new InvalidOperationException($"{PhysicalName}はCommandModelでない");
            var parameter = XElement.Attribute(BasicNodeOptions.Parameter.AttributeName);
            return parameter == null ? null : GetTargetStructure(parameter, true);
        }
        /// <summary>
        /// このCommandModelの戻り値の構造体を返します。
        /// このルート集約がCommandModelでない場合は例外。
        /// </summary>
        public ICreatablePresentationLayerStructure? GetReturnValueStructure() {
            if (Model is not Models.CommandModel) throw new InvalidOperationException($"{PhysicalName}はCommandModelでない");
            var returnValue = XElement.Attribute(BasicNodeOptions.ReturnValue.AttributeName);
            return returnValue == null ? null : GetTargetStructure(returnValue, false);
        }

        /// <summary>
        /// このルート集約をBasicNodeOptions.Parameterに指定しているコマンドモデルを列挙します。
        /// </summary>
        public IEnumerable<RootAggregate> EnumerateCommandModelsRefferingAsParameter() {
            foreach (var rootElement in _ctx.Document.Root?.ElementsWithoutMemo() ?? []) {
                // コマンドモデルのみを対象とする
                if (!_ctx.TryGetModel(rootElement, out var model) || model is not Models.CommandModel) {
                    continue;
                }

                var parameterAttr = rootElement.Attribute(BasicNodeOptions.Parameter.AttributeName);
                if (parameterAttr == null) {
                    continue;
                }

                // Parameter属性の値を解析
                var splitted = parameterAttr.Value.Split(':');
                var parameterTargetName = splitted[0];

                // このルート集約がParameter属性で指定されている場合
                if (parameterTargetName == PhysicalName) {
                    yield return (RootAggregate)_ctx.ToAggregateBase(rootElement, null);
                }
            }
        }

        private ICreatablePresentationLayerStructure GetTargetStructure(XAttribute attribute, bool isParameter) {
            var splitted = attribute.Value.Split(':');
            var targetPhysicalName = splitted[0];
            var targetXElement = _ctx.Document.Root?.Element(targetPhysicalName)
                ?? throw new InvalidOperationException($"対象の集約が見つかりません: {targetPhysicalName}");

            var targetRootAggregate = (RootAggregate)_ctx.ToAggregateBase(targetXElement, null);
            if (splitted.Length == 1) {
                return isParameter
                    ? new Models.StructureModelModules.StructureDisplayData(targetRootAggregate)
                    : new Models.StructureModelModules.PlainStructure(targetRootAggregate);

            } else if (BasicNodeOptions.StructureRefToAvailable.TryGetValue(splitted[1], out var factory)) {
                return factory(targetRootAggregate);

            } else {
                throw new InvalidOperationException($"不正な参照先の種類: {splitted[1]}");
            }
        }
        #endregion このCommandModelの引数・戻り値の集約を返す
    }

    /// <summary>
    /// 親集約と1対1で対応する子集約。
    /// </summary>
    public class ChildAggregate : AggregateBase, IRelationalMember {
        internal ChildAggregate(XElement xElement, SchemaParseContext ctx, ISchemaPathNode? previous)
            : base(xElement, ctx, previous) { }

        public decimal Order => XElement.ElementsBeforeSelf().Count();
        public AggregateBase Owner {
            get {
                var parent = XElement.GetParentWithoutMemo();
                return parent == PreviousNode?.XElement
                    ? (AggregateBase?)PreviousNode ?? throw new InvalidOperationException() // パスの巻き戻しの場合
                    : _ctx.ToAggregateBase(parent ?? throw new InvalidOperationException(), this);
            }
        }

        /// <summary>【廃止予定】画面上で追加削除されるタイミングが親と異なるかどうか</summary>
        public bool HasLifeCycle => true || XElement.Attribute(BasicNodeOptions.HasLifeCycle.AttributeName) != null;

        AggregateBase IRelationalMember.MemberAggregate => this;

        public override AggregateBase AsEntry() {
            return new ChildAggregate(XElement, _ctx, null);
        }
    }

    /// <summary>
    /// 親集約と1対多で対応する子集約。
    /// </summary>
    public class ChildrenAggregate : AggregateBase, IRelationalMember {
        internal ChildrenAggregate(XElement xElement, SchemaParseContext ctx, ISchemaPathNode? previous)
            : base(xElement, ctx, previous) { }

        public decimal Order => XElement.ElementsBeforeSelf().Count();
        public AggregateBase Owner {
            get {
                var parent = XElement.GetParentWithoutMemo();
                return parent == PreviousNode?.XElement
                    ? (AggregateBase?)PreviousNode ?? throw new InvalidOperationException() // パスの巻き戻しの場合
                    : _ctx.ToAggregateBase(parent ?? throw new InvalidOperationException(), this);
            }
        }
        AggregateBase IRelationalMember.MemberAggregate => this;

        public override AggregateBase AsEntry() {
            return new ChildrenAggregate(XElement, _ctx, null);
        }

        /// <summary>
        /// Childrenのメンバーに対するループ処理をレンダリングするとき、
        /// そのループ変数として使うために "x", "x0", "x1", ... という名前を返す。
        /// 引数に "i" を指定した場合は "i", "j", "k", ... という名前を返す。
        /// 変数は、宣言方法に気を付ければ、同じ深さのChildrenが複数あっても衝突しない名前になる。
        /// </summary>
        public string GetLoopVarName(string alpha = "x") {
            // 深さ。ルート集約直下のChildrenのとき0になる
            var depth = XElement
                .Ancestors()
                // メモ型は親としてカウントしない
                .Where(el => el.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value != SchemaParseContext.NODE_TYPE_MEMO)
                .Count() - 2;

            if (depth == 0) {
                return alpha;

            } else if (alpha == "i") {
                // i, j, k, ... を返す。
                // zまで到達した場合は aa, ab, ac, ... とする
                var index = depth + 8; // 'i'からのオフセット
                var sb = new StringBuilder();
                while (index >= 0) {
                    sb.Insert(0, (char)('a' + (index % 26)));
                    index = (index / 26) - 1;
                }
                return sb.ToString();

            } else {
                return alpha + depth;
            }
        }
    }
}
