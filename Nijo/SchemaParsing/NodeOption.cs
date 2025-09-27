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
    /// 真偽値 or 文字列
    /// </summary>
    public required E_NodeOptionType Type { get; init; }
    /// <summary>
    /// この属性に対する入力検証
    /// </summary>
    public required Action<NodeOptionValidateContext> Validate { get; init; }
    /// <summary>
    /// あるモデルのメンバーがこの属性を指定することができるかどうか。
    /// nullの場合は指定可能と判定されます。
    /// </summary>
    public required Func<IModel, bool> IsAvailableModelMembers { get; init; }
}

/// <summary>
/// 真偽値 or 文字列
/// </summary>
public enum E_NodeOptionType {
    Boolean,
    String,
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
        Validate = ctx => {
            // 改行不可
            if (ctx.Value.Contains('\n')) ctx.AddError("改行を含めることはできません。");
        },
        IsAvailableModelMembers = model => {
            return true;
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
        Validate = ctx => {
            // 改行不可
            if (ctx.Value.Contains('\n')) ctx.AddError("改行を含めることはできません。");
        },
        IsAvailableModelMembers = model => {
            if (model is DataModel) return true;
            return false;
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
        Validate = ctx => {
            // 改行不可
            if (ctx.Value.Contains('\n')) ctx.AddError("改行を含めることはできません。");
        },
        IsAvailableModelMembers = model => {
            return true;
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
        Validate = ctx => {
            var nodeType = ctx.SchemaParseContext.GetNodeType(ctx.XElement);

            // モデルの種類を判定
            if (ctx.SchemaParseContext.TryGetModel(ctx.XElement, out var model)) {
                // コマンドモデルの場合はキー属性を定義できない
                if (model is CommandModel) {
                    ctx.AddError("コマンドモデルでは主キー属性を定義できません。");
                    return;
                }

                // データモデルの子集約には主キー属性を付与できない
                if (model is DataModel && nodeType == E_NodeType.ChildAggregate) {
                    ctx.AddError("データモデルの子集約には主キー属性を付与できません。");
                    return;
                }

                // クエリモデルの子集約・子配列には主キー属性を付与できない
                if (model is QueryModel && (nodeType == E_NodeType.ChildAggregate || nodeType == E_NodeType.ChildrenAggregate)) {
                    ctx.AddError("クエリモデルの子集約・子配列には主キー属性を付与できません。");
                    return;
                }
            }
        },
        IsAvailableModelMembers = model => {
            if (model is DataModel) return true;
            if (model is QueryModel) return true;
            return false;
        },
    };

    internal static NodeOption IsRequired = new() {
        AttributeName = "IsRequired",
        DisplayName = "必須",
        Type = E_NodeOptionType.Boolean,
        HelpText = $$"""
            必須項目であることを表します。
            新規登録処理や更新処理での必須入力チェック処理が自動生成されます。
            """,
        Validate = ctx => {
            // 特に制約なし
        },
        IsAvailableModelMembers = model => {
            if (model is DataModel) return true;
            if (model is QueryModel) return true;
            if (model is CommandModel) return true;
            return false;
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
        Validate = ctx => {
            // データモデルのルート集約のみ許可
            if (ctx.NodeType != E_NodeType.RootAggregate) {
                ctx.AddError("このオプションはルート集約にのみ指定できます。");
                return;
            }

            if (ctx.SchemaParseContext.TryGetModel(ctx.XElement, out var model)) {
                if (model is not DataModel) {
                    ctx.AddError("このオプションはデータモデルにのみ指定できます。");
                }
            }
        },
        IsAvailableModelMembers = model => {
            return model is DataModel;
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
        Validate = ctx => {
            // このオプションを使用するためにはGenerateDefaultQueryModelの指定が必須
            if (ctx.XElement.Attribute(GenerateDefaultQueryModel.AttributeName) == null) {
                ctx.AddError($"このオプションを使用するためには{GenerateDefaultQueryModel.AttributeName}属性の指定が必須です。");
            }

            // データモデルのルート集約のみ許可
            if (ctx.NodeType != E_NodeType.RootAggregate) {
                ctx.AddError("このオプションはルート集約にのみ指定できます。");
                return;
            }

            if (ctx.SchemaParseContext.TryGetModel(ctx.XElement, out var model)) {
                if (!(model is DataModel)) {
                    ctx.AddError("このオプションはデータモデルにのみ指定できます。");
                }
            }
        },
        IsAvailableModelMembers = model => {
            return false;
        },
    };
    #endregion DataModel用


    #region QueryModel用
    internal static NodeOption IsReadOnly = new() {
        AttributeName = "IsReadOnly",
        DisplayName = "読み取り専用集約",
        Type = E_NodeOptionType.Boolean,
        HelpText = $$"""
            このQueryModelが読み取り専用かどうか
            """,
        Validate = ctx => {
            // クエリモデルのみ許可
            if (ctx.SchemaParseContext.TryGetModel(ctx.XElement, out var model)) {
                if (!(model is QueryModel)) {
                    ctx.AddError("このオプションはクエリモデルにのみ指定できます。");
                }
            }
        },
        IsAvailableModelMembers = model => {
            if (model is QueryModel) return true;
            return false;
        },
    };
    internal static NodeOption HasLifeCycle = new() {
        AttributeName = "HasLifeCycle",
        DisplayName = "【廃止予定】独立ライフサイクル",
        Type = E_NodeOptionType.Boolean,
        HelpText = $$"""
            【廃止予定】画面上で追加削除されるタイミングが親と異なるかどうか。
            """,
        Validate = ctx => {
            // クエリモデルのみ許可
            if (ctx.SchemaParseContext.TryGetModel(ctx.XElement, out var model)) {
                if (!(model is QueryModel)) {
                    ctx.AddError("このオプションはクエリモデルにのみ指定できます。");
                }
            }
        },
        IsAvailableModelMembers = model => {
            if (model is QueryModel) return true;
            return false;
        },
    };
    #endregion QueryModel用


    #region CommandModel用
    internal static NodeOption Parameter = new() {
        AttributeName = "Parameter",
        DisplayName = "コマンドモデルの引数の型",
        Type = E_NodeOptionType.String,
        HelpText = $$"""
            コマンドモデルの引数の型を指定します。
            以下のいずれかを指定できます：
            - 構造体モデルのルート集約名
            - クエリモデルのルート集約名 + ":{{REF_TO_OBJECT_DISPLAY_DATA}}" または ":{{REF_TO_OBJECT_SEARCH_CONDITION}}"
            指定しなかった場合は引数なしのコマンドとみなされます。
            """,
        Validate = ctx => {
            // コマンドモデルのルート集約のみ許可
            if (ctx.NodeType != E_NodeType.RootAggregate) {
                ctx.AddError("このオプションはルート集約にのみ指定できます。");
                return;
            }

            if (ctx.SchemaParseContext.TryGetModel(ctx.XElement, out var model) && model is not CommandModel) {
                ctx.AddError("このオプションはコマンドモデルにのみ指定できます。");
                return;
            }

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
                targetType != REF_TO_OBJECT_DISPLAY_DATA &&
                targetType != REF_TO_OBJECT_SEARCH_CONDITION) {
                ctx.AddError($"参照先の種類は {REF_TO_OBJECT_DISPLAY_DATA} または {REF_TO_OBJECT_SEARCH_CONDITION} のみ指定できます。");
                return;
            }

            // 参照先の存在確認
            var targetElement = ctx.SchemaParseContext.Document.Root?.Element(targetPhysicalName);
            if (targetElement == null) {
                ctx.AddError($"参照先の集約が見つかりません。物理名: {targetPhysicalName}");
                return;
            }
        },
        IsAvailableModelMembers = model => {
            return model is CommandModel;
        },
    };

    internal static NodeOption ReturnValue = new() {
        AttributeName = "ReturnValue",
        DisplayName = "コマンドモデルの戻り値の型",
        Type = E_NodeOptionType.String,
        HelpText = $$"""
            コマンドモデルの戻り値の型を指定します。
            以下のいずれかを指定できます：
            - 構造体モデルのルート集約名
            - クエリモデルのルート集約名 + ":{{REF_TO_OBJECT_DISPLAY_DATA}}" または ":{{REF_TO_OBJECT_SEARCH_CONDITION}}"
            指定しなかった場合は戻り値なしのコマンドとみなされます。
            """,
        Validate = ctx => {
            // コマンドモデルのルート集約のみ許可
            if (ctx.NodeType != E_NodeType.RootAggregate) {
                ctx.AddError("このオプションはルート集約にのみ指定できます。");
                return;
            }

            if (ctx.SchemaParseContext.TryGetModel(ctx.XElement, out var model) && model is not CommandModel) {
                ctx.AddError("このオプションはコマンドモデルにのみ指定できます。");
                return;
            }

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
                targetType != REF_TO_OBJECT_DISPLAY_DATA &&
                targetType != REF_TO_OBJECT_SEARCH_CONDITION) {
                ctx.AddError($"参照先の種類は {REF_TO_OBJECT_DISPLAY_DATA} または {REF_TO_OBJECT_SEARCH_CONDITION} のみ指定できます。");
                return;
            }

            // 参照先の存在確認
            var targetElement = ctx.SchemaParseContext.Document.Root?.Element(targetPhysicalName);
            if (targetElement == null) {
                ctx.AddError($"参照先の集約が見つかりません。物理名: {targetPhysicalName}");
                return;
            }
        },
        IsAvailableModelMembers = model => {
            return model is CommandModel;
        },
    };
    #endregion CommandModel用


    #region StructureModel用
    internal const string REF_TO_OBJECT_DISPLAY_DATA = "DisplayData";
    internal const string REF_TO_OBJECT_SEARCH_CONDITION = "SearchCondition";

    internal static NodeOption RefToObject = new() {
        AttributeName = "RefToObject",
        DisplayName = "参照先オブジェクト",
        Type = E_NodeOptionType.String,
        HelpText = $$"""
            CommandModelまたはStructureModelはQueryModelの検索条件か画面表示用データのいずれかしか参照できない。
            その2種のうちどちらを参照するかの指定。
            "{{REF_TO_OBJECT_DISPLAY_DATA}}"か"{{REF_TO_OBJECT_SEARCH_CONDITION}}"のみ指定可能。
            """,
        Validate = ctx => {
            if (ctx.Value != REF_TO_OBJECT_DISPLAY_DATA && ctx.Value != REF_TO_OBJECT_SEARCH_CONDITION) {
                ctx.AddError($"{REF_TO_OBJECT_DISPLAY_DATA}か{REF_TO_OBJECT_SEARCH_CONDITION}のみ指定可能です。");
            }

            // 外部参照の場合のみ許可
            if (ctx.NodeType != E_NodeType.Ref) {
                ctx.AddError("このオプションは外部参照（ref-to）にのみ指定できます。");
                return;
            }

            if (ctx.SchemaParseContext.TryGetModel(ctx.XElement, out var model)) {
                if (model is not CommandModel && model is not StructureModel) {
                    ctx.AddError("このオプションはコマンドモデルまたはStructureModelの外部参照にのみ指定できます。");
                }
            }
        },
        IsAvailableModelMembers = model => {
            if (model is CommandModel) return true;
            if (model is StructureModel) return true;
            return false;
        },
    };
    #endregion StructureModel用


    #region StaticEnumModel用
    internal static NodeOption StaticEnumValue = new() {
        AttributeName = Models.StaticEnumModelModules.StaticEnumValueDef.ATTR_KEY,
        DisplayName = "静的列挙型値",
        Type = E_NodeOptionType.String,
        HelpText = $$"""
            静的列挙型の区分値を指定します。
            C#のenumの値となるため、整数で指定してください。
            """,
        Validate = ctx => {
            // 整数値のみ許可
            if (!int.TryParse(ctx.Value, out _)) {
                ctx.AddError("整数値で指定してください。");
            }
        },
        IsAvailableModelMembers = model => {
            return model is StaticEnumModel;
        },
    };
    #endregion StaticEnumModel用


    #region ValueMember用
    internal static NodeOption MaxLength = new() {
        AttributeName = "MaxLength",
        DisplayName = "最大長",
        Type = E_NodeOptionType.String,
        HelpText = $$"""
            文字列項目の最大長。整数で指定してください。
            """,
        Validate = ctx => {
            // 整数値のみ許可
            if (!int.TryParse(ctx.Value, out _)) {
                ctx.AddError("整数値で指定してください。");
            }
        },
        IsAvailableModelMembers = model => {
            if (model is DataModel) return true;
            if (model is QueryModel) return true;
            if (model is CommandModel) return true;
            return false;
        },
    };
    internal static NodeOption CharacterType = new() {
        AttributeName = "CharacterType",
        DisplayName = "文字種",
        Type = E_NodeOptionType.String,
        HelpText = $$"""
            文字種。半角、半角英数、など
            """,
        Validate = ctx => {

        },
        IsAvailableModelMembers = model => {
            if (model is DataModel) return true;
            if (model is QueryModel) return true;
            if (model is CommandModel) return true;
            return false;
        },
    };
    internal static NodeOption TotalDigit = new() {
        AttributeName = "TotalDigit",
        DisplayName = "総合桁数",
        Type = E_NodeOptionType.String,
        HelpText = $$"""
            数値系属性の整数部桁数 + 小数部桁数
            """,
        Validate = ctx => {
            // 整数値のみ許可
            if (!int.TryParse(ctx.Value, out _)) {
                ctx.AddError("整数値で指定してください。");
            }
        },
        IsAvailableModelMembers = model => {
            if (model is DataModel) return true;
            if (model is QueryModel) return true;
            if (model is CommandModel) return true;
            return false;
        },
    };
    internal static NodeOption DecimalPlace = new() {
        AttributeName = "DecimalPlace",
        DisplayName = "小数部桁数",
        Type = E_NodeOptionType.String,
        HelpText = $$"""
            数値系属性の小数部桁数
            """,
        Validate = ctx => {
            // 整数値のみ許可
            if (!int.TryParse(ctx.Value, out _)) {
                ctx.AddError("整数値で指定してください。");
            }
        },
        IsAvailableModelMembers = model => {
            if (model is DataModel) return true;
            if (model is QueryModel) return true;
            if (model is CommandModel) return true;
            return false;
        },
    };
    internal static NodeOption SequenceName = new() {
        AttributeName = "SequenceName",
        DisplayName = "シーケンス名",
        Type = E_NodeOptionType.String,
        HelpText = $$"""
            シーケンス物理名
            """,
        Validate = ctx => {

        },
        IsAvailableModelMembers = model => {
            return model is DataModel;
        },
    };

    internal static NodeOption UserHelpText = new() {
        AttributeName = "UserHelpText",
        DisplayName = "ユーザー向け説明文",
        Type = E_NodeOptionType.String,
        HelpText = $$"""
            アプリケーションのエンドユーザーに向けた説明文を指定します。
            画面上でユーザーに表示し、項目の入力方法や意味を説明するために使用してください。
            改行を含めることができます。
            """,
        Validate = ctx => {
            // 特に制約なし（改行も許可）
        },
        IsAvailableModelMembers = model => {
            return true;
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
        Validate = ctx => {
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
            var valueAttr = ctx.XElement.Attribute(ConstantValue.AttributeName);
            if (valueAttr == null) {
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
        IsAvailableModelMembers = model => {
            return model is ConstantModel;
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
        Validate = ctx => {
            // 値の妥当性チェックはConstantTypeで実施
        },
        IsAvailableModelMembers = model => {
            return model is ConstantModel;
        },
    };

    internal static NodeOption TemplateParams = new() {
        AttributeName = "TemplateParams",
        DisplayName = "【廃止予定】テンプレートパラメータ",
        Type = E_NodeOptionType.String,
        HelpText = $$"""
            【廃止予定】テンプレート文字列の引数名をカンマ区切りで指定します。
            現在は{0}, {1}, ... から自動的に引数が判定されるため、この属性は不要です。
            """,
        Validate = ctx => {
            // 廃止予定の警告
            ctx.AddError("TemplateParams属性は廃止予定です。テンプレート文字列の引数は{0}, {1}, ...から自動的に判定されます。");
        },
        IsAvailableModelMembers = model => {
            return false; // 使用不可にする
        },
    };
    #endregion ConstantModel用
}
