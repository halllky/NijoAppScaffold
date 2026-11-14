
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : NijoAttr.cs
// ※ このファイルを生成したメソッド: RenderDeclaration

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
/// 利用可能文字
/// この Attribute は自動生成後のアプリケーションの共通処理内部でリフレクションを使用して動的な処理を行う際に使われることを想定しています。
/// 自動生成されたソースコード自体がこの Attribute を使用することはありません。
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class AvailableCharacterAttribute : System.Attribute {
    public enum E_Value {
        半角数字および半角ハイフンのみ,
    }

    public E_Value Value { get; }

    /// <inheritdoc cref="AvailableCharacterAttribute" />
    public AvailableCharacterAttribute(E_Value value) {
        Value = value;
    }
}
