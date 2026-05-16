using Nijo.CodeGenerating;
using Nijo.Parts.Common;
using Nijo.Parts.CSharp;
using System;
using static Nijo.Models.WriteModel2Modules.DataClassForSaveBase;

namespace Nijo.Models.ReadModel2Modules {
    internal static class ISaveCommandConvertible {
        internal const string INTERFACE_NAME = "ISaveCommandConvertible";
        internal const string GET_SAVE_TYPE = "GetSaveType";

        internal static SourceFile Render() => new() {
            FileName = "ISaveCommandConvertible.cs",
            Contents = $$"""
                namespace {{CodeRenderingContext.CurrentContext.Config.RootNamespace}};

                /// <summary>
                /// 追加・更新・削除のいずれかのコマンドに変換可能なオブジェクト
                /// </summary>
                public interface {{INTERFACE_NAME}} {
                    /// <summary>
                    /// このデータがDBに保存済みかどうか。
                    /// つまり新規作成のときはfalse, 閲覧・更新・削除のときはtrue
                    /// </summary>
                    bool {{EditablePresentationObject.EXISTS_IN_DB_CS}} { get; }
                    /// <summary>
                    /// 画面上で何らかの変更が加えられてから、保存処理の実行でその変更が確定するまでの間、trueになる。
                    /// </summary>
                    bool {{EditablePresentationObject.WILL_BE_CHANGED_CS}} { get; }
                    /// <summary>
                    /// 画面上で削除が指示されてから、保存処理の実行でその削除が確定するまでの間、trueになる。
                    /// </summary>
                    bool {{EditablePresentationObject.WILL_BE_DELETED_CS}} { get; }
                }

                public static class ISaveCommandConvertibleExtensions {
                    /// <summary>
                    /// このオブジェクトの状態から、保存時に追加・更新・削除のうちどの処理が実行されるべきかを表す区分を返します。
                    /// </summary>
                    public static {{ADD_MOD_DEL_ENUM_CS}} {{GET_SAVE_TYPE}}<T>(this T obj) where T : {{INTERFACE_NAME}} {
                        if (obj.{{EditablePresentationObject.WILL_BE_DELETED_CS}}) return {{ADD_MOD_DEL_ENUM_CS}}.DEL;
                        if (!obj.{{EditablePresentationObject.EXISTS_IN_DB_CS}}) return {{ADD_MOD_DEL_ENUM_CS}}.ADD;
                        if (obj.{{EditablePresentationObject.WILL_BE_CHANGED_CS}}) return {{ADD_MOD_DEL_ENUM_CS}}.MOD;
                        return {{ADD_MOD_DEL_ENUM_CS}}.NONE;
                    }
                }
                """,
        };
    }
}
