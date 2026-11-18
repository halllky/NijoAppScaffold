
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : NijoAttr.cs
// ※ このファイルを生成したメソッド: RenderDeclaration
// ※ このファイルを生成した nijo のバージョン: 0.0.0-dev

namespace MyApp.NijoAttr;

/// <summary>
/// 表示用名称
/// <para>
/// この Attribute は自動生成後のアプリケーションの共通処理内部でリフレクションを使用して動的な処理を行う際に使われることを想定しています。
/// 自動生成されたソースコード自体がこの Attribute を使用することはありません。
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class DisplayNameAttribute : System.Attribute {
    public string Value { get; }
    public DisplayNameAttribute(string value) {
        Value = value;
    }
}

/// <summary>
/// 表示用名称を空文字にする
/// <para>
/// この Attribute は自動生成後のアプリケーションの共通処理内部でリフレクションを使用して動的な処理を行う際に使われることを想定しています。
/// 自動生成されたソースコード自体がこの Attribute を使用することはありません。
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class DisplayNameIsEmptyAttribute : System.Attribute {
}

/// <summary>
/// データベース上名称
/// <para>
/// この Attribute は自動生成後のアプリケーションの共通処理内部でリフレクションを使用して動的な処理を行う際に使われることを想定しています。
/// 自動生成されたソースコード自体がこの Attribute を使用することはありません。
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class DbNameAttribute : System.Attribute {
    public string Value { get; }
    public DbNameAttribute(string value) {
        Value = value;
    }
}

/// <summary>
/// ラテン語名
/// <para>
/// この Attribute は自動生成後のアプリケーションの共通処理内部でリフレクションを使用して動的な処理を行う際に使われることを想定しています。
/// 自動生成されたソースコード自体がこの Attribute を使用することはありません。
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class LatinNameAttribute : System.Attribute {
    public string Value { get; }
    public LatinNameAttribute(string value) {
        Value = value;
    }
}

/// <summary>
/// キー
/// <para>
/// この Attribute は自動生成後のアプリケーションの共通処理内部でリフレクションを使用して動的な処理を行う際に使われることを想定しています。
/// 自動生成されたソースコード自体がこの Attribute を使用することはありません。
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class IsKeyAttribute : System.Attribute {
}

/// <summary>
/// NOT NULL
/// <para>
/// この Attribute は自動生成後のアプリケーションの共通処理内部でリフレクションを使用して動的な処理を行う際に使われることを想定しています。
/// 自動生成されたソースコード自体がこの Attribute を使用することはありません。
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class IsNotNullAttribute : System.Attribute {
}

/// <summary>
/// 永続化する
/// <para>
/// この Attribute は自動生成後のアプリケーションの共通処理内部でリフレクションを使用して動的な処理を行う際に使われることを想定しています。
/// 自動生成されたソースコード自体がこの Attribute を使用することはありません。
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class IsStoredAttribute : System.Attribute {
}

/// <summary>
/// 検索可能
/// <para>
/// この Attribute は自動生成後のアプリケーションの共通処理内部でリフレクションを使用して動的な処理を行う際に使われることを想定しています。
/// 自動生成されたソースコード自体がこの Attribute を使用することはありません。
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class IsQueryableAttribute : System.Attribute {
}

/// <summary>
/// 論理削除を行う
/// <para>
/// この Attribute は自動生成後のアプリケーションの共通処理内部でリフレクションを使用して動的な処理を行う際に使われることを想定しています。
/// 自動生成されたソースコード自体がこの Attribute を使用することはありません。
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class UseSoftDeleteAttribute : System.Attribute {
}

/// <summary>
/// ユニーク制約
/// <para>
/// この Attribute は自動生成後のアプリケーションの共通処理内部でリフレクションを使用して動的な処理を行う際に使われることを想定しています。
/// 自動生成されたソースコード自体がこの Attribute を使用することはありません。
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class UniqueConstraintsAttribute : System.Attribute {
    public string Value { get; }
    public UniqueConstraintsAttribute(string value) {
        Value = value;
    }
}

/// <summary>
/// 汎用テーブル
/// <para>
/// この Attribute は自動生成後のアプリケーションの共通処理内部でリフレクションを使用して動的な処理を行う際に使われることを想定しています。
/// 自動生成されたソースコード自体がこの Attribute を使用することはありません。
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class IsGenericLookupTableAttribute : System.Attribute {
}

/// <summary>
/// ハードコード
/// <para>
/// この Attribute は自動生成後のアプリケーションの共通処理内部でリフレクションを使用して動的な処理を行う際に使われることを想定しています。
/// 自動生成されたソースコード自体がこの Attribute を使用することはありません。
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class IsHardCodedPrimaryKeyAttribute : System.Attribute {
}

/// <summary>
/// カテゴリ
/// <para>
/// この Attribute は自動生成後のアプリケーションの共通処理内部でリフレクションを使用して動的な処理を行う際に使われることを想定しています。
/// 自動生成されたソースコード自体がこの Attribute を使用することはありません。
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class CategoryAttribute : System.Attribute {
    public string Value { get; }
    public CategoryAttribute(string value) {
        Value = value;
    }
}

/// <summary>
/// ビューへマッピング
/// <para>
/// この Attribute は自動生成後のアプリケーションの共通処理内部でリフレクションを使用して動的な処理を行う際に使われることを想定しています。
/// 自動生成されたソースコード自体がこの Attribute を使用することはありません。
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class MapToViewAttribute : System.Attribute {
}

/// <summary>
/// 検索条件のみに生成
/// <para>
/// この Attribute は自動生成後のアプリケーションの共通処理内部でリフレクションを使用して動的な処理を行う際に使われることを想定しています。
/// 自動生成されたソースコード自体がこの Attribute を使用することはありません。
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class OnlySearchConditionAttribute : System.Attribute {
}

/// <summary>
/// 参照先オブジェクト
/// <para>
/// この Attribute は自動生成後のアプリケーションの共通処理内部でリフレクションを使用して動的な処理を行う際に使われることを想定しています。
/// 自動生成されたソースコード自体がこの Attribute を使用することはありません。
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class RefToObjectAttribute : System.Attribute {
    public enum E_Value {
        DisplayData,
        RefTarget,
    }

    public E_Value Value { get; }
    public RefToObjectAttribute(E_Value value) {
        Value = value;
    }
}

/// <summary>
/// 最大長
/// <para>
/// この Attribute は自動生成後のアプリケーションの共通処理内部でリフレクションを使用して動的な処理を行う際に使われることを想定しています。
/// 自動生成されたソースコード自体がこの Attribute を使用することはありません。
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class MaxLengthAttribute : System.Attribute {
    public int Value { get; }
    public MaxLengthAttribute(int value) {
        Value = value;
    }
}

/// <summary>
/// 総合桁数
/// <para>
/// この Attribute は自動生成後のアプリケーションの共通処理内部でリフレクションを使用して動的な処理を行う際に使われることを想定しています。
/// 自動生成されたソースコード自体がこの Attribute を使用することはありません。
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class TotalDigitAttribute : System.Attribute {
    public int Value { get; }
    public TotalDigitAttribute(int value) {
        Value = value;
    }
}

/// <summary>
/// 小数部桁数
/// <para>
/// この Attribute は自動生成後のアプリケーションの共通処理内部でリフレクションを使用して動的な処理を行う際に使われることを想定しています。
/// 自動生成されたソースコード自体がこの Attribute を使用することはありません。
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class DecimalPlaceAttribute : System.Attribute {
    public int Value { get; }
    public DecimalPlaceAttribute(int value) {
        Value = value;
    }
}

/// <summary>
/// シーケンス名
/// <para>
/// この Attribute は自動生成後のアプリケーションの共通処理内部でリフレクションを使用して動的な処理を行う際に使われることを想定しています。
/// 自動生成されたソースコード自体がこの Attribute を使用することはありません。
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class SequenceNameAttribute : System.Attribute {
    public string Value { get; }
    public SequenceNameAttribute(string value) {
        Value = value;
    }
}

/// <summary>
/// 検索挙動
/// <para>
/// この Attribute は自動生成後のアプリケーションの共通処理内部でリフレクションを使用して動的な処理を行う際に使われることを想定しています。
/// 自動生成されたソースコード自体がこの Attribute を使用することはありません。
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class StringSearchBehaviorAttribute : System.Attribute {
    public enum E_Value {
        Partial,
        Forward,
        Backward,
        Exact,
        Range,
    }

    public E_Value Value { get; }
    public StringSearchBehaviorAttribute(E_Value value) {
        Value = value;
    }
}

/// <summary>
/// 最大長
/// <para>
/// DataModelのMaxLengthはDB桁数定義にのみ使用される。こちらは画面制御で使用。
/// </para>
/// この Attribute は自動生成後のアプリケーションの共通処理内部でリフレクションを使用して動的な処理を行う際に使われることを想定しています。
/// 自動生成されたソースコード自体がこの Attribute を使用することはありません。
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class CustomMaxLengthAttribute : System.Attribute {
    public decimal Value { get; }

    /// <inheritdoc cref="CustomMaxLengthAttribute" />
    public CustomMaxLengthAttribute(string value) {
        Value = decimal.Parse(value);
    }
}

/// <summary>
/// 金額項目
/// <para>
/// その数値項目が金額を表すかどうか。画面上で3桁カンマ区切りで表示されたり、サフィックスに「円」がついたりする。
/// </para>
/// この Attribute は自動生成後のアプリケーションの共通処理内部でリフレクションを使用して動的な処理を行う際に使われることを想定しています。
/// 自動生成されたソースコード自体がこの Attribute を使用することはありません。
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class IsCurrencyAttribute : System.Attribute {
    /// <inheritdoc cref="IsCurrencyAttribute" />
    public IsCurrencyAttribute() {
    }
}

/// <summary>
/// 数量
/// <para>
/// その数値項目が数量を表すか否か。画面上でサフィックスに「個」がつく。
/// </para>
/// この Attribute は自動生成後のアプリケーションの共通処理内部でリフレクションを使用して動的な処理を行う際に使われることを想定しています。
/// 自動生成されたソースコード自体がこの Attribute を使用することはありません。
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class IsQuantityAttribute : System.Attribute {
    /// <inheritdoc cref="IsQuantityAttribute" />
    public IsQuantityAttribute() {
    }
}

/// <summary>
/// 0以上のみ
/// <para>
/// その数値項目が負の数で登録されようとしたときにエラーとする。
/// </para>
/// この Attribute は自動生成後のアプリケーションの共通処理内部でリフレクションを使用して動的な処理を行う際に使われることを想定しています。
/// 自動生成されたソースコード自体がこの Attribute を使用することはありません。
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class NotNegativeAttribute : System.Attribute {
    /// <inheritdoc cref="NotNegativeAttribute" />
    public NotNegativeAttribute() {
    }
}

/// <summary>
/// マスキング
/// <para>
/// ログ出力時に値をそのまま出さずにマスキングさせる。パスワードなどに指定する。
/// </para>
/// この Attribute は自動生成後のアプリケーションの共通処理内部でリフレクションを使用して動的な処理を行う際に使われることを想定しています。
/// 自動生成されたソースコード自体がこの Attribute を使用することはありません。
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class MaskingAttribute : System.Attribute {
    /// <inheritdoc cref="MaskingAttribute" />
    public MaskingAttribute() {
    }
}

/// <summary>
/// 文字列の正規化
/// <para>
/// 画面で文字列を入力したときの正規化。指定しなければ前後の空白だけを削除する。「正規化しない」は前後の空白も削除しない（パスワードなど）。「NFKC」は前後の空白を削除し、Unicode 正規化（NFKC）も行う（全角の英数字を半角にするなど。コードなどに指定する）。
/// </para>
/// この Attribute は自動生成後のアプリケーションの共通処理内部でリフレクションを使用して動的な処理を行う際に使われることを想定しています。
/// 自動生成されたソースコード自体がこの Attribute を使用することはありません。
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class StringNormalizationAttribute : System.Attribute {
    public enum E_Value {
        正規化しない,
        NFKC,
    }

    public E_Value Value { get; }

    /// <inheritdoc cref="StringNormalizationAttribute" />
    public StringNormalizationAttribute(E_Value value) {
        Value = value;
    }
}

/// <summary>
/// 機能公開レベル
/// <para>
/// 処理（クエリとコマンド）を呼び出せる利用者。「ログイン不要」はログインせずに呼び出せる。「ログインユーザー全員」はログインしていれば誰でも呼び出せる。「権限で指定」は「機能利用可能権限」のいずれかを持つユーザーだけが呼び出せる（システム管理者はすべての権限を持つ）。「内部処理のみ」はバッチなどサーバー内部からだけ呼び出し、画面からは呼び出せない。参照検索は、参照先のクエリモデルの指定を引き継ぐ。
/// </para>
/// この Attribute は自動生成後のアプリケーションの共通処理内部でリフレクションを使用して動的な処理を行う際に使われることを想定しています。
/// 自動生成されたソースコード自体がこの Attribute を使用することはありません。
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class OperationAccessLevelAttribute : System.Attribute {
    public enum E_Value {
        ログイン不要,
        ログインユーザー全員,
        権限で指定,
        内部処理のみ,
    }

    public E_Value Value { get; }

    /// <inheritdoc cref="OperationAccessLevelAttribute" />
    public OperationAccessLevelAttribute(E_Value value) {
        Value = value;
    }
}

/// <summary>
/// 機能利用可能権限
/// <para>
/// 「機能公開レベル」が「権限で指定」の処理を呼び出すのに必要な権限。いずれか1つを持っていれば呼び出せる。
/// </para>
/// この Attribute は自動生成後のアプリケーションの共通処理内部でリフレクションを使用して動的な処理を行う際に使われることを想定しています。
/// 自動生成されたソースコード自体がこの Attribute を使用することはありません。
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class OperationAccessRoleAttribute : System.Attribute {
    public IReadOnlyList<global::MyApp.権限区分> Values { get; }

    /// <inheritdoc cref="OperationAccessRoleAttribute" />
    public OperationAccessRoleAttribute(params global::MyApp.権限区分[] values) {
        Values = values;
    }
}
