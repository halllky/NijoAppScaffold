#if DEBUG
namespace MyApp;

/// <summary>
/// ダミーデータ生成クラスのオーバーライド。
/// 標準のダミーデータ生成処理をカスタマイズしたい場合はここで適宜オーバーライドする。
/// 生成ロジックは nijo.xml の Type="" の種類単位。
/// </summary>
public partial class OverridedDummyDataGenerator : DummyDataGenerator {

    // 「旧システム部署情報」は「部署」からユニーク制約が張られているので、重複防止のために多めに作っておく
    protected override IEnumerable<旧システム部署情報CreateCommand> CreatePatternsOf旧システム部署情報(DummyDataGenerateContext context) {
        for (var i = 0; i < 20 * 4; i++) {
            yield return CreateRandom旧システム部署情報(context, i);
        }
    }
    protected override IEnumerable<部署CreateCommand> CreatePatternsOf部署(DummyDataGenerateContext context) {
        var oldBushoCount = 0;
        for (var i = 0; i < 20; i++) {
            yield return new 部署CreateCommand {
                部署ID = GetRandomInteger(context, MetadataForPage.部署.Members.部署ID),
                部署名 = GetRandomWord(context, MetadataForPage.部署.Members.部署名),
                事業所 = context.Generated事業所.Count == 0
                ? null
                : context.Generated事業所
                    .Select(x => 事業所Key.FromSearchResult(x))
                    .ElementAt(context.Random.Next(0, context.Generated事業所.Count)),
                課 = Enumerable.Range(0, 4).Select(i => new 課CreateCommand {
                    コード = GetRandomWord(context, MetadataForPage.部署.Members.課.Members.コード),
                    旧システムコード = context.Generated旧システム部署情報.Count == 0
                        ? null
                        : context.Generated旧システム部署情報
                            .Select(x => 旧システム部署情報Key.FromRootDbEntity(x))
                            .ElementAt(oldBushoCount++),
                    課名称 = GetRandomWord(context, MetadataForPage.部署.Members.課.Members.課名称),
                    係 = Enumerable.Range(0, 4).Select(i => new 係CreateCommand {
                        連番 = GetRandomInteger(context, MetadataForPage.部署.Members.課.Members.係.Members.連番),
                        係名称 = GetRandomWord(context, MetadataForPage.部署.Members.課.Members.係.Members.係名称),
                        勤怠管理区分 = context.Generated汎用マスタ.Count == 0
                            ? null
                            : context.Generated汎用マスタ
                                .Select(x => 汎用マスタKey.FromRootDbEntity(x))
                                .ElementAt(context.Random.Next(0, context.Generated汎用マスタ.Count)),
                    }).ToList(),
                }).ToList(),
            };
        }
    }

    // 単語型のダミーデータ生成ロジック
    protected override string? GetRandomWord(DummyDataGenerateContext context, MetadataForPage.ValueMetadata member) {
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
