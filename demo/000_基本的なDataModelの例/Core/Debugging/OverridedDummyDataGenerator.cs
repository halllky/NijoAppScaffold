#if DEBUG
namespace MyApp;

/// <summary>
/// ダミーデータ生成クラスのオーバーライド。
/// 標準のダミーデータ生成処理をカスタマイズしたい場合はここで適宜オーバーライドする。
/// 生成ロジックは nijo.xml の Type="" の種類単位。
/// </summary>
public partial class OverridedDummyDataGenerator : DummyDataGenerator {
    private const string CONTRACT_TYPE = "010";
    private const string ATTENDANCE_TYPE = "020";

    protected override 旧システム部署情報CreateCommand CreateRandom旧システム部署情報(DummyDataGenerateContext context, int itemIndex) {
        return new 旧システム部署情報CreateCommand {
            旧システムコード = $"LEGACY-{itemIndex + 1:000}",
            名称 = GetRandomWord(context, MetadataForPage.旧システム部署情報.Members.名称),
        };
    }

    protected override IEnumerable<汎用マスタCreateCommand> CreatePatternsOf汎用マスタ(DummyDataGenerateContext context) {
        for (var i = 0; i < 10; i++) {
            yield return new 汎用マスタCreateCommand {
                汎用種別 = CONTRACT_TYPE,
                区分値 = $"CONTRACT-{i + 1:000}",
                表示名称 = $"契約区分{i + 1}",
            };
        }

        for (var i = 0; i < 10; i++) {
            yield return new 汎用マスタCreateCommand {
                汎用種別 = ATTENDANCE_TYPE,
                区分値 = $"ATTEND-{i + 1:000}",
                表示名称 = $"勤怠管理区分{i + 1}",
            };
        }
    }


    // 「旧システム部署情報」は「部署」からユニーク制約が張られているので、重複防止のために多めに作っておく
    protected override IEnumerable<旧システム部署情報CreateCommand> CreatePatternsOf旧システム部署情報(DummyDataGenerateContext context) {
        for (var i = 0; i < 20 * 4; i++) {
            yield return CreateRandom旧システム部署情報(context, i);
        }
    }
    protected override IEnumerable<部署CreateCommand> CreatePatternsOf部署(DummyDataGenerateContext context) {
        var attendanceKeys = context.Generated汎用マスタ
            .Where(x => x.汎用種別 == ATTENDANCE_TYPE)
            .Select(汎用マスタKey.FromRootDbEntity)
            .ToArray();
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
                        勤怠管理区分 = attendanceKeys.Length == 0
                            ? null
                            : attendanceKeys[context.Random.Next(0, attendanceKeys.Length)],
                    }).ToList(),
                }).ToList(),
            };
        }
    }

    protected override 社員CreateCommand CreateRandom社員(DummyDataGenerateContext context, int itemIndex) {
        var contractKeys = context.Generated汎用マスタ
            .Where(x => x.汎用種別 == CONTRACT_TYPE)
            .Select(汎用マスタKey.FromRootDbEntity)
            .ToArray();

        return new 社員CreateCommand {
            社員ID = GetRandomInteger(context, MetadataForPage.社員.Members.社員ID),
            氏名 = GetRandomWord(context, MetadataForPage.社員.Members.氏名),
            所属部署 = context.Generated部署.Count == 0
                ? null
                : context.Generated部署
                    .Select(x => 部署Key.FromRootDbEntity(x))
                    .ElementAt(context.Random.Next(0, context.Generated部署.Count)),
            契約種別 = contractKeys.Length == 0
                ? null
                : contractKeys[context.Random.Next(0, contractKeys.Length)],
        };
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
