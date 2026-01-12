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
        if (member.IsKey) {
            return base.GetRandomWord(context, member);

        } else {
            var value = $"{member.DisplayName}その{context.Random.Next(1, 100)}";
            if (member.MaxLength != null && value.Length > (int.TryParse(member.MaxLength, out var ml) ? ml : 12)) {
                value = value.Substring(0, int.TryParse(member.MaxLength, out var ml2) ? ml2 : 12);
            }
            return value;
        }
    }
}

#endif
