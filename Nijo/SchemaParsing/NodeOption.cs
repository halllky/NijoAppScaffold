using Nijo.CodeGenerating;
using Nijo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Nijo.SchemaParsing;

/// <summary>
/// ノードに指定できるオプション属性
/// </summary>
public class NodeOption {
    /// <summary>
    /// XML要素で指定されるときのこのオプションのキー。
    /// XMLの属性名として使用可能な文字のみ使える。
    /// </summary>
    public required string AttributeName { get; init; }
    /// <summary>
    /// 人間にとって分かりやすい名前をつけてください。
    /// </summary>
    public required string DisplayName { get; init; }
    /// <summary>
    /// このオプション属性の説明文
    /// </summary>
    public required string HelpText { get; init; }
    /// <summary>
    /// この属性の値の型
    /// </summary>
    public required E_NodeOptionType Type { get; init; }
    /// <summary>
    /// <see cref="E_NodeOptionType"/> が列挙体の場合のみに使用される、選択肢の候補
    /// </summary>
    public string[]? TypeEnumValues { get; init; }
    /// <summary>
    /// この属性が特定のモデル・ノード種別の組み合わせで利用可能かどうかを判定する。
    /// GUI上での属性入力欄の活性制御に使用される。
    /// </summary>
    public required Func<IModel, E_NodeType, bool> IsAvailable { get; init; }
    /// <summary>
    /// この属性に対する入力検証のうち、IsAvailableでは判定できない複雑な検証。
    /// 保存時にサーバー側で実行される。
    /// </summary>
    public required Action<NodeOptionValidateContext> ValidateOthers { get; init; }
}

/// <summary>
/// ノードオプションの値の型
/// </summary>
public enum E_NodeOptionType {
    Boolean,
    String,
    Integer,
    EnumSelect,
}

/// <summary>
/// <see cref="NodeOption.Validate"/> の引数
/// </summary>
public class NodeOptionValidateContext {
    /// <summary>XMLで指定されているこの属性の値</summary>
    public required string Value { get; init; }
    /// <summary>検証対象のXML要素</summary>
    public required XElement XElement { get; init; }
    /// <summary>ノード種別</summary>
    public required E_NodeType NodeType { get; init; }
    /// <summary>エラーがあったらここに追加</summary>
    public required Action<string> AddError { get; init; }
    /// <summary>コンテキスト情報</summary>
    public required SchemaParseContext SchemaParseContext { get; init; }
}

// ----------------------------------------

/// <summary>
/// 標準のオプション属性
/// </summary>
internal static class BasicNodeOptions {

    internal static NodeOption DisplayName = new() {
        AttributeName = "DisplayName",
        DisplayName = "表示用名称",
        Type = E_NodeOptionType.String,
        HelpText = $$"""
            ソースコード上にあらわれる物理名とは別に表示用名称を設けたい場合に指定してください。
            表示用名称に改行を含めることはできません。
            """,
        IsAvailable = (model, nodeType) => {
            return true;
        },
        ValidateOthers = ctx => {
            // 改行不可
            if (ctx.Value.Contains('\n')) ctx.AddError("改行を含めることはできません。");
        },
    };

    internal static NodeOption DbName = new() {
        AttributeName = "DbName",
        DisplayName = "データベース上名称",
        Type = E_NodeOptionType.String,
        HelpText = $$"""
            データベースのテーブル名またはカラム名を明示的に指定したい場合に設定してください。
            集約に定義した場合はテーブル名、値に定義した場合はカラム名になります。
            未指定の場合、物理名がそのままテーブル名やカラム名になります。
            ここで指定する値に改行を含めることはできません。
            複数の集約で同じテーブル名を指定することはできません。
            """,
        IsAvailable = (model, nodeType) => {
            return model is DataModel || model is QueryModel;
        },
        ValidateOthers = ctx => {
            // QueryModelの場合はビューにマッピングされる場合のみ指定可能
            if (ctx.SchemaParseContext.TryGetModel(ctx.XElement, out var model) && model is QueryModel) {
                var root = ctx.XElement.GetRootAggregateElement();
                var mapToView = root.Attribute(MapToView!.AttributeName);

                if (mapToView == null) ctx.AddError("ビューにマッピングされない場合は指定できません。");
            }

            // 改行不可
            if (ctx.Value.Contains('\n')) ctx.AddError("改行を含めることはできません。");
        },
    };

    internal static NodeOption LatinName = new() {
        AttributeName = "LatinName",
        DisplayName = "ラテン語名",
        Type = E_NodeOptionType.String,
        HelpText = $$"""
            URLなど、ラテン語名しか用いることができない部分の名称を明示的に指定したい場合に設定してください。
            既定では集約を表す一意な文字列から生成されたハッシュ値が用いられます。
            ここで指定する値に改行を含めることはできません。
            """,
        IsAvailable = (model, nodeType) => {
            return true;
        },
        ValidateOthers = ctx => {
            // 改行不可
            if (ctx.Value.Contains('\n')) ctx.AddError("改行を含めることはできません。");
        },
    };

    internal static NodeOption IsKey = new() {
        AttributeName = "IsKey",
        DisplayName = "キー",
        Type = E_NodeOptionType.Boolean,
        HelpText = $$"""
            この項目がその集約のキーであることを表します。
            DataModelのルート集約またはChildrenの場合、指定必須。
            Commandの要素には指定不可。
            QueryModelのキーはルート集約にのみ指定可能。

            なお自動生成される処理でQueryModelのキーに依存する処理は無い。
            DisplayDataの主キーアサイン関数にのみ影響する。
            （カスタマイズ処理でURLとDisplayDataの間のデータのやり取りに使用する想定）
            """,
        IsAvailable = (model, nodeType) => {
            return (model is DataModel
                 || model is QueryModel)
                 && (nodeType == E_NodeType.ValueMember
                  || nodeType == E_NodeType.Ref);
        },
        ValidateOthers = ctx => {
            if (!ctx.SchemaParseContext.TryGetModel(ctx.XElement, out var model)) return;

            var owner = ctx.XElement.Parent;
            if (owner == null) return;

            var ownerType = ctx.SchemaParseContext.GetNodeType(owner);

            // データモデルの子集約には付与不可
            if (model is DataModel && ownerType == E_NodeType.ChildAggregate) {
                ctx.AddError("データモデルの子集約にはキーを指定できません。");
            }

            // クエリモデルの子集約には付与不可
            if (model is QueryModel && ownerType == E_NodeType.ChildAggregate) {
                ctx.AddError("クエリモデルの子集約にはキーを指定できません。");
            }

            // クエリモデルの子配列は、ビューにマッピングされない場合は付与不可
            if (model is QueryModel && ownerType == E_NodeType.ChildrenAggregate) {
                var root = ctx.XElement.GetRootAggregateElement();
                var mapToView = root.Attribute(MapToView!.AttributeName);

                if (mapToView == null) {
                    ctx.AddError("クエリモデルの子配列で、ビューにマッピングされない場合はキーを指定できません。");
                }
            }

            // クエリモデルのルート集約は、ビューにマッピングされない場合は付与不可
            if (model is QueryModel && ownerType == E_NodeType.RootAggregate) {
                var root = ctx.XElement.GetRootAggregateElement();
                var mapToView = root.Attribute(MapToView!.AttributeName);

                if (mapToView == null) {
                    ctx.AddError("クエリモデルのルート集約で、ビューにマッピングされない場合はキーを指定できません。");
                }
            }
        },
    };

    internal static NodeOption IsNotNull = new() {
        AttributeName = "IsNotNull",
        DisplayName = "NOT NULL",
        Type = E_NodeOptionType.Boolean,
        HelpText = $$"""
            RDBMS 上で NOT NULL 制約がつくことを表します。
            また、新規登録処理や更新処理でアプリケーション側で必須入力チェック処理が行われるようになります。
            """,
        IsAvailable = (model, nodeType) => {
            return model is DataModel
                && (nodeType == E_NodeType.ValueMember
                 || nodeType == E_NodeType.Ref);
        },
        ValidateOthers = ctx => {
            // 特に制約なし
        },
    };


    #region DataModel用
    internal static NodeOption GenerateDefaultQueryModel = new() {
        AttributeName = "GenerateDefaultQueryModel",
        DisplayName = "DataModelと全く同じ型のQueryModelを生成するかどうか",
        Type = E_NodeOptionType.Boolean,
        HelpText = $$"""
            きわめて単純なマスタデータなど、データベース上のデータ構造と
            それを表示・編集する画面のデータ構造が完全一致する場合、
            この項目を指定するとDataModelと全く同じ型のQueryModelのモジュールが生成される。
            """,
        IsAvailable = (model, nodeType) => {
            // データモデルのルート集約のみ許可
            return model is DataModel && nodeType == E_NodeType.RootAggregate;
        },
        ValidateOthers = ctx => {
            // IsAvailableで基本的な判定は完了しているため、追加の検証は不要
        },
    };
    internal static NodeOption GenerateBatchUpdateCommand = new() {
        AttributeName = "GenerateBatchUpdateCommand",
        DisplayName = "DataModelと全く同じ型のQueryModelの一括更新用のWebエンドポイント・アプリケーションサービスを生成するかどうか",
        Type = E_NodeOptionType.Boolean,
        HelpText = $$"""
            標準の更新ロジックで一括更新処理を生成する場合に指定。
            DataModel、かつ、それとまったく同じ形のQueryModelが生成される場合にのみ指定可能。
            """,
        IsAvailable = (model, nodeType) => {
            return model is DataModel && nodeType == E_NodeType.RootAggregate;
        },
        ValidateOthers = ctx => {
            // このオプションを使用するためにはGenerateDefaultQueryModelの指定が必須
            if (ctx.XElement.Attribute(GenerateDefaultQueryModel.AttributeName) == null) {
                ctx.AddError($"このオプションを使用するためには{GenerateDefaultQueryModel.AttributeName}属性の指定が必須です。");
            }
        },
    };
    #endregion DataModel用


    #region QueryModel用
    internal static NodeOption MapToView = new() {
        AttributeName = "MapToView",
        DisplayName = "ビューへマッピング",
        Type = E_NodeOptionType.Boolean,
        HelpText = $$"""
            この集約をデータベースのビュー(View)にマッピングする場合に指定してください。
            指定した場合、Entity Framework CoreのToView()を使用してビューにマッピングされるEFCoreEntityが生成されます。
            ビュー定義自体はソースコード生成の対象外となり、手動で作成する必要があります。
            """,
        IsAvailable = (model, nodeType) => {
            // クエリモデルのルート集約のみ許可
            return model is QueryModel && nodeType == E_NodeType.RootAggregate;
        },
        ValidateOthers = ctx => {
            // IsAvailableで基本的な判定は完了しているため、追加の検証は不要
        },
    };
    internal static NodeOption OnlySearchCondition = new() {
        AttributeName = "OnlySearchCondition",
        DisplayName = "検索条件のみに生成",
        Type = E_NodeOptionType.Boolean,
        HelpText = $$"""
            この属性を検索条件にのみレンダリングし、検索結果にはレンダリングしません。
            検索条件のみに生成される属性は処理の挙動の指示にのみ使われる想定です。
            例: 「退職後1年以上経過している人のみ抽出」のような、検索条件の絞り込みにのみ必要な属性。
            """,
        IsAvailable = (model, nodeType) => {
            // QueryModelのValueMemberのみ許可
            return model is QueryModel && nodeType == E_NodeType.ValueMember;
        },
        ValidateOthers = ctx => {
            // IsAvailableで基本的な判定は完了しているため、追加の検証は不要
        },
    };
    internal static NodeOption StringSearchBehavior = new() {
        AttributeName = "StringSearchBehavior",
        DisplayName = "検索挙動",
        Type = E_NodeOptionType.EnumSelect,
        TypeEnumValues = new[] {
            STRING_SEARCH_BEHAVIOR_PARTIAL,
            STRING_SEARCH_BEHAVIOR_FORWARD,
            STRING_SEARCH_BEHAVIOR_BACKWARD,
            STRING_SEARCH_BEHAVIOR_EXACT,
        },
        HelpText = $$"""
            検索時の挙動を指定します。
            - {{STRING_SEARCH_BEHAVIOR_PARTIAL}}: 部分一致（デフォルト）
            - {{STRING_SEARCH_BEHAVIOR_FORWARD}}: 前方一致
            - {{STRING_SEARCH_BEHAVIOR_BACKWARD}}: 後方一致
            - {{STRING_SEARCH_BEHAVIOR_EXACT}}: 完全一致
            """,
        IsAvailable = (model, nodeType) => {
            return (model is QueryModel || model is DataModel)
                && nodeType == E_NodeType.ValueMember;
        },
        ValidateOthers = ctx => {
            // DataModel の場合は GenerateDefaultQueryModel が指定されている場合のみ許可
            if (ctx.SchemaParseContext.TryGetModel(ctx.XElement, out var model) && model is DataModel) {
                var root = ctx.XElement.GetRootAggregateElement();
                var generateDefaultQueryModel = root.Attribute(GenerateDefaultQueryModel!.AttributeName);

                if (generateDefaultQueryModel == null) {
                    ctx.AddError("検索処理が自動生成されないDataModelのため指定できません。");
                    return;
                }
            }

            // 文字列型の値メンバーにのみ指定可能
            if (!ctx.SchemaParseContext.TryResolveMemberType(ctx.XElement, out var valueMemberType)
                || valueMemberType is not ValueMemberTypes.Word
                && valueMemberType is not ValueMemberTypes.Description
                && valueMemberType is not ValueMemberTypes.ValueObjectMember) {
                ctx.AddError("この属性は文字列型の値メンバーにのみ指定可能です。");
                return;
            }

            // 指定可能な値の確認
            var validValues = new[] {
                STRING_SEARCH_BEHAVIOR_PARTIAL,
                STRING_SEARCH_BEHAVIOR_FORWARD,
                STRING_SEARCH_BEHAVIOR_BACKWARD,
                STRING_SEARCH_BEHAVIOR_EXACT,
            };
            if (!validValues.Contains(ctx.Value)) {
                ctx.AddError($"検索挙動には {string.Join(", ", validValues)} のいずれかを指定してください。");
            }
        },
    };
    internal const string STRING_SEARCH_BEHAVIOR_PARTIAL = "Partial";
    internal const string STRING_SEARCH_BEHAVIOR_FORWARD = "Forward";
    internal const string STRING_SEARCH_BEHAVIOR_BACKWARD = "Backward";
    internal const string STRING_SEARCH_BEHAVIOR_EXACT = "Exact";
    #endregion QueryModel用


    #region CommandModel用
    internal static NodeOption Parameter = new() {
        AttributeName = "Parameter",
        DisplayName = "コマンドモデルの引数の型",
        Type = E_NodeOptionType.String,
        HelpText = $$"""
            コマンドモデルの引数の型を指定します。
            以下のいずれかを指定できます:
            - 構造体モデルのルート集約名
            - クエリモデルのルート集約名 + {{string.Join(" または ", StructureRefToAvailable.Keys)}}
            指定しなかった場合は引数なしのコマンドとみなされます。
            """,
        IsAvailable = (model, nodeType) => {
            // コマンドモデルのルート集約のみ許可
            return model is CommandModel && nodeType == E_NodeType.RootAggregate;
        },
        ValidateOthers = ctx => {
            // 値未指定は不正
            var splitted = ctx.Value.Split(':', StringSplitOptions.RemoveEmptyEntries);
            var targetPhysicalName = splitted.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(targetPhysicalName)) {
                ctx.AddError("参照先が未指定です。");
                return;
            }

            // コロンが含まれるならその後ろは DisplayData または SearchCondition のみ
            var targetType = splitted.Skip(1).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(targetType) &&
                !StructureRefToAvailable.ContainsKey(targetType)) {
                ctx.AddError($"参照先の種類は {string.Join(" または ", StructureRefToAvailable.Keys)} のみ指定できます。");
                return;
            }

            // 参照先の存在確認
            var targetElement = ctx.SchemaParseContext.Document
                .Root
                ?.Element(SchemaParseContext.SECTION_DATA_STRUCTURES)
                ?.Element(targetPhysicalName);
            if (targetElement == null) {
                ctx.AddError($"参照先の集約が見つかりません。物理名: {targetPhysicalName}");
                return;
            }
        },
    };

    internal static NodeOption ReturnValue = new() {
        AttributeName = "ReturnValue",
        DisplayName = "コマンドモデルの戻り値の型",
        Type = E_NodeOptionType.String,
        HelpText = $$"""
            コマンドモデルの戻り値の型を指定します。
            以下のいずれかを指定できます:
            - 構造体モデルのルート集約名
            - クエリモデルのルート集約名 + {{string.Join(" または ", StructureRefToAvailable.Keys.Select(key => $":{key}"))}}
            指定しなかった場合は戻り値なしのコマンドとみなされます。
            """,
        IsAvailable = (model, nodeType) => {
            // コマンドモデルのルート集約のみ許可
            return model is CommandModel && nodeType == E_NodeType.RootAggregate;
        },
        ValidateOthers = ctx => {
            // 値未指定は不正
            var splitted = ctx.Value.Split(':', StringSplitOptions.RemoveEmptyEntries);
            var targetPhysicalName = splitted.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(targetPhysicalName)) {
                ctx.AddError("参照先が未指定です。");
                return;
            }

            // コロンが含まれるならその後ろは DisplayData または SearchCondition のみ
            var targetType = splitted.Skip(1).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(targetType) &&
                !StructureRefToAvailable.ContainsKey(targetType)) {
                ctx.AddError($"参照先の種類は {string.Join(" または ", StructureRefToAvailable.Keys)} のみ指定できます。");
                return;
            }

            // 参照先の存在確認
            var targetElement = ctx.SchemaParseContext.Document
                .Root
                ?.Element(SchemaParseContext.SECTION_DATA_STRUCTURES)
                ?.Element(targetPhysicalName);
            if (targetElement == null) {
                ctx.AddError($"参照先の集約が見つかりません。物理名: {targetPhysicalName}");
                return;
            }
        },
    };
    #endregion CommandModel用


    #region StructureModel用
    internal const string REF_TO_OBJECT_DISPLAY_DATA = "DisplayData";
    internal const string REF_TO_OBJECT_SEARCH_CONDITION = "SearchCondition";
    internal const string REF_TO_OBJECT_REF_TARGET = "RefTarget";

    internal static Dictionary<string, Func<ImmutableSchema.AggregateBase, ICreatablePresentationLayerStructure>> StructureRefToAvailable => new() {
        { REF_TO_OBJECT_DISPLAY_DATA, (aggregate) => new Models.QueryModelModules.DisplayData(aggregate) },
        { REF_TO_OBJECT_SEARCH_CONDITION, (aggregate) => new Models.QueryModelModules.SearchCondition.Entry(aggregate.GetRoot()) },
        { REF_TO_OBJECT_REF_TARGET, (aggregate) => new Models.QueryModelModules.DisplayDataRef.Entry(aggregate) },
    };

    internal static NodeOption RefToObject = new() {
        AttributeName = "RefToObject",
        DisplayName = "参照先オブジェクト",
        Type = E_NodeOptionType.EnumSelect,
        TypeEnumValues = StructureRefToAvailable.Keys.ToArray(),
        HelpText = $$"""
            CommandModelまたはStructureModelはQueryModelの検索条件、画面表示用データ、外部参照のいずれかしか参照できない。
            その3種のうちどちらを参照するかの指定。
            {{string.Join(", ", StructureRefToAvailable.Keys.Select(key => $"\"{key}\""))}}のみ指定可能。
            """,
        IsAvailable = (model, nodeType) => {
            // CommandModelまたはStructureModelの外部参照のみ許可
            return (model is CommandModel || model is StructureModel)
                && nodeType == E_NodeType.Ref;
        },
        ValidateOthers = ctx => {
            if (!StructureRefToAvailable.ContainsKey(ctx.Value)) {
                ctx.AddError($"{string.Join(" または ", StructureRefToAvailable.Keys)}のみ指定可能です。");
            }
        },
    };
    #endregion StructureModel用


    #region ValueMember用
    internal static NodeOption MaxLength = new() {
        AttributeName = "MaxLength",
        DisplayName = "最大長",
        Type = E_NodeOptionType.Integer,
        HelpText = $$"""
            RDBMS上での文字列項目の最大長。整数で指定してください。
            """,
        IsAvailable = (model, nodeType) => {
            return model is DataModel
                && nodeType == E_NodeType.ValueMember;
        },
        ValidateOthers = ctx => {
            // 整数値のみ許可
            if (!int.TryParse(ctx.Value, out _)) {
                ctx.AddError("整数値で指定してください。");
            }
        },
    };
    internal static NodeOption CharacterType = new() {
        AttributeName = "CharacterType",
        DisplayName = "文字種",
        Type = E_NodeOptionType.String,
        HelpText = $$"""
            文字種。半角英数のみ、JIS第1,2水準の文字のみ、全銀協フォーマットで使える文字のみ、など。
            最終的に廃止予定（nijo.xml上でカスタムバリデータを定義できるようにする）
            """,
        IsAvailable = (model, nodeType) => {
            return (model is DataModel
                 || model is QueryModel
                 || model is CommandModel)
                 && nodeType == E_NodeType.ValueMember;
        },
        ValidateOthers = ctx => {
            // 特に制約なし
        },
    };
    internal static NodeOption TotalDigit = new() {
        AttributeName = "TotalDigit",
        DisplayName = "総合桁数",
        Type = E_NodeOptionType.Integer,
        HelpText = $$"""
            数値系属性の整数部桁数 + 小数部桁数
            """,
        IsAvailable = (model, nodeType) => {
            return (model is DataModel
                 || model is QueryModel
                 || model is CommandModel)
                 && nodeType == E_NodeType.ValueMember;
        },
        ValidateOthers = ctx => {
            // 整数値のみ許可
            if (!int.TryParse(ctx.Value, out _)) {
                ctx.AddError("整数値で指定してください。");
            }
        },
    };
    internal static NodeOption DecimalPlace = new() {
        AttributeName = "DecimalPlace",
        DisplayName = "小数部桁数",
        Type = E_NodeOptionType.Integer,
        HelpText = $$"""
            数値系属性の小数部桁数
            """,
        IsAvailable = (model, nodeType) => {
            return (model is DataModel
                 || model is QueryModel
                 || model is CommandModel)
                 && nodeType == E_NodeType.ValueMember;
        },
        ValidateOthers = ctx => {
            // 整数値のみ許可
            if (!int.TryParse(ctx.Value, out _)) {
                ctx.AddError("整数値で指定してください。");
            }
        },
    };
    internal static NodeOption SequenceName = new() {
        AttributeName = "SequenceName",
        DisplayName = "シーケンス名",
        Type = E_NodeOptionType.String,
        HelpText = $$"""
            シーケンス物理名
            """,
        IsAvailable = (model, nodeType) => {
            return model is DataModel && nodeType == E_NodeType.ValueMember;
        },
        ValidateOthers = ctx => {
            // 特に制約なし
        },
    };
    #endregion ValueMember用


    #region ConstantModel用
    internal static NodeOption ConstantType = new() {
        AttributeName = "ConstantType",
        DisplayName = "定数の型",
        Type = E_NodeOptionType.String,
        HelpText = $$"""
            定数の型を指定します。
            string（文字列）、int（整数）、decimal（小数）、template（テンプレート文字列）、child（ネストされた定数グループ）のいずれかを指定してください。
            """,
        IsAvailable = (model, nodeType) => {
            return model is ConstantModel;
        },
        ValidateOthers = ctx => {
            var validTypes = new[] { "string", "int", "decimal", "template", "child" };
            if (!validTypes.Contains(ctx.Value)) {
                ctx.AddError($"type属性の値「{ctx.Value}」は無効です。string, int, decimal, template, child のいずれかを指定してください。");
                return;
            }

            // child型の場合はConstantValue属性は不要
            if (ctx.Value == "child") {
                return; // child型の場合は追加のバリデーション不要
            }

            // 型に応じた値の妥当性チェック
            var valueAttr = ConstantValue == null
                ? throw new Exception("ありえない")
                : ctx.XElement.Attribute(ConstantValue.AttributeName);
            if (valueAttr is null) {
                ctx.AddError("ConstantValue属性が必須です。");
                return;
            }

            switch (ctx.Value) {
                case "int":
                    if (!int.TryParse(valueAttr.Value, out _)) {
                        ctx.AddError($"int型の値「{valueAttr.Value}」は有効な整数ではありません。");
                    }
                    break;
                case "decimal":
                    if (!decimal.TryParse(valueAttr.Value, out _)) {
                        ctx.AddError($"decimal型の値「{valueAttr.Value}」は有効な小数ではありません。");
                    }
                    break;
                case "template":
                    // テンプレート型の場合、プレースホルダーは自動判定されるため追加チェックは不要
                    break;
            }
        },
    };

    internal static NodeOption ConstantValue = new() {
        AttributeName = "ConstantValue",
        DisplayName = "定数の値",
        Type = E_NodeOptionType.String,
        HelpText = $$"""
            定数の値を指定します。
            型に応じて適切な形式で指定してください。
            """,
        IsAvailable = (model, nodeType) => {
            return model is ConstantModel;
        },
        ValidateOthers = ctx => {
            // 値の妥当性チェックはConstantTypeで実施
        },
    };
    #endregion ConstantModel用
}
