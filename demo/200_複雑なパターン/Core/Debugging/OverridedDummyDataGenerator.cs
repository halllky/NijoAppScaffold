#if DEBUG
namespace MyApp;

/// <summary>
/// ダミーデータ生成クラスのオーバーライド。
/// 標準のダミーデータ生成処理をカスタマイズしたい場合はここで適宜オーバーライドする。
/// 生成ロジックは nijo.xml の Type="" の種類単位。
/// </summary>
public partial class OverridedDummyDataGenerator : DummyDataGenerator {

    // 単語型のダミーデータ生成ロジック
    protected override string? GetRandomWord(DummyDataGenerateContext context, MetadataForPage.ValueMetadata member) {
        if (member.CharacterType == nameof(E_CharacterType.半角数字および半角ハイフンのみ)) {
            var maxLength = member.MaxLength ?? 12;
            return $"{context.Random.Next(100000, 999999)}-{context.Random.Next(1000, 9999)}".Substring(0, Math.Min(maxLength, 12));
        }

        if (member.IsKey) {
            return base.GetRandomWord(context, member);

        } else {
            var value = $"{member.DisplayName}その{context.Random.Next(1, 100)}";
            if (member.MaxLength != null && value.Length > (member.MaxLength ?? 12)) {
                value = value.Substring(0, member.MaxLength ?? 12);
            }
            return value;
        }
    }
}

#endif
