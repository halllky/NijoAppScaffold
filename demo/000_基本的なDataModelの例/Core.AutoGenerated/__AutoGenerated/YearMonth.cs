
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : YearMonthMember.cs
// ※ このファイルを生成したメソッド: RenderStaticSources

using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyApp;

/// <summary>
/// 年月を表す値オブジェクト
/// </summary>
public readonly struct YearMonth : IComparable<YearMonth>, IEquatable<YearMonth> {
    /// <summary>
    /// 数値表現（YYYYMM形式の6桁整数）
    /// </summary>
    private readonly int _value;

    /// <summary>
    /// 数値表現（YYYYMM形式の6桁整数）
    /// </summary>
    public int Value => _value;

    /// <summary>
    /// 年
    /// </summary>
    public int Year => _value / 100;

    /// <summary>
    /// 月（1〜12）
    /// </summary>
    public int Month => _value % 100;

    /// <summary>
    /// 指定した年月から値オブジェクトを生成します
    /// </summary>
    /// <param name="year">年（4桁）</param>
    /// <param name="month">月（1〜12）</param>
    public YearMonth(int year, int month) {
        if (year < 1 || year > 9999) throw new ArgumentOutOfRangeException(nameof(year), "年は1〜9999の範囲で指定してください");
        if (month < 1 || month > 12) throw new ArgumentOutOfRangeException(nameof(month), "月は1〜12の範囲で指定してください");
        _value = year * 100 + month;
    }

    /// <summary>
    /// YYYYMMの数値表現から値オブジェクトを生成します
    /// </summary>
    /// <param name="value">YYYYMMの6桁整数</param>
    public YearMonth(int value) {
        int year = value / 100;
        int month = value % 100;

        if (year < 1 || year > 9999) throw new ArgumentOutOfRangeException(nameof(value), "年は1〜9999の範囲である必要があります");
        if (month < 1 || month > 12) throw new ArgumentOutOfRangeException(nameof(value), "月は1〜12の範囲である必要があります");

        _value = value;
    }

    /// <summary>
    /// DateTimeから値オブジェクトを生成します
    /// </summary>
    public static YearMonth FromDateTime(DateTime dateTime) {
        return new YearMonth(dateTime.Year, dateTime.Month);
    }

    /// <summary>
    /// DateOnlyから値オブジェクトを生成します
    /// </summary>
    public static YearMonth FromDateOnly(DateOnly dateOnly) {
        return new YearMonth(dateOnly.Year, dateOnly.Month);
    }

    /// <summary>
    /// 年月をDateTime型に変換します（日付は1日として扱います）
    /// </summary>
    public DateTime ToDateTime() {
        return new DateTime(Year, Month, 1);
    }

    /// <summary>
    /// 現在の年月を取得します
    /// </summary>
    public static YearMonth Now => FromDateTime(DateTime.Now);

    /// <summary>
    /// UTC現在の年月を取得します
    /// </summary>
    public static YearMonth UtcNow => FromDateTime(DateTime.UtcNow);

    /// <summary>
    /// 年月をYYYY/MM形式の文字列に変換します
    /// </summary>
    public override string ToString() {
        return $"{Year:0000}/{Month:00}";
    }

    /// <summary>
    /// 2つの年月を比較します
    /// </summary>
    public int CompareTo(YearMonth other) {
        return _value.CompareTo(other._value);
    }

    /// <summary>
    /// 2つの年月が等しいかどうかを判定します
    /// </summary>
    public bool Equals(YearMonth other) {
        return _value == other._value;
    }

    /// <summary>
    /// オブジェクトが等しいかどうかを判定します
    /// </summary>
    public override bool Equals(object? obj) {
        return obj is YearMonth yearMonth && Equals(yearMonth);
    }

    /// <summary>
    /// ハッシュコードを取得します
    /// </summary>
    public override int GetHashCode() {
        return _value.GetHashCode();
    }

    /// <summary>
    /// 等価演算子
    /// </summary>
    public static bool operator ==(YearMonth left, YearMonth right) {
        return left.Equals(right);
    }

    /// <summary>
    /// 非等価演算子
    /// </summary>
    public static bool operator !=(YearMonth left, YearMonth right) {
        return !left.Equals(right);
    }

    /// <summary>
    /// 比較演算子（より大きい）
    /// </summary>
    public static bool operator >(YearMonth left, YearMonth right) {
        return left.CompareTo(right) > 0;
    }

    /// <summary>
    /// 比較演算子（より小さい）
    /// </summary>
    public static bool operator <(YearMonth left, YearMonth right) {
        return left.CompareTo(right) < 0;
    }

    /// <summary>
    /// 比較演算子（以上）
    /// </summary>
    public static bool operator >=(YearMonth left, YearMonth right) {
        return left.CompareTo(right) >= 0;
    }

    /// <summary>
    /// 比較演算子（以下）
    /// </summary>
    public static bool operator <=(YearMonth left, YearMonth right) {
        return left.CompareTo(right) <= 0;
    }

    /// <summary>
    /// 明示的な型変換演算子（int -> YearMonth）
    /// </summary>
    public static explicit operator YearMonth(int value) {
        return new YearMonth(value);
    }

    /// <summary>
    /// 明示的な型変換演算子（YearMonth -> int）
    /// </summary>
    public static explicit operator int(YearMonth yearMonth) {
        return yearMonth._value;
    }
}
