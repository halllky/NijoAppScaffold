using Microsoft.EntityFrameworkCore;
using MyApp.Core.Authorization;

namespace MyApp;

partial class OverridedApplicationService {
    public override async Task Execute売上詳細画面初期表示(売上詳細画面初期表示ParameterDisplayData param, IPresentationContextWithReturnValue<売上詳細DisplayData, 売上詳細画面初期表示ParameterMessages> context) {
        if (param.Values.売上SEQ == null) {
            context.Messages.売上SEQ.AddError("売上SEQが指定されていません。");
            return;
        }

        var entity = await DbContext.売上DbSet
            .Include(x => x.担当者)
            .Include(x => x.売上の売上明細)
            .ThenInclude(x => x.商品)
            .FirstOrDefaultAsync(x => x.売上SEQ == param.Values.売上SEQ);

        if (entity == null) {
            context.Messages.AddError("指定された売上データが見つかりません。");
            return;
        }

        context.ReturnValue = new() {
            Values = new() {
                売上SEQ = entity.売上SEQ,
                売上日時 = entity.売上日時,
                担当者 = new() {
                    従業員番号 = entity.担当者?.従業員番号,
                    氏名 = entity.担当者?.氏名,
                },
                備考 = entity.備考,
            },
            売上詳細の売上明細 = entity.売上の売上明細.Select(x => new 売上詳細の売上明細DisplayData {
                Values = new() {
                    明細ID = x.明細ID,
                    商品 = new() {
                        商品SEQ = x.商品?.商品SEQ,
                        外部システム側ID = x.商品?.外部システム側ID,
                        商品名 = x.商品?.商品名,
                        売値単価_税抜 = x.商品?.売値単価_税抜,
                        消費税区分 = x.商品?.消費税区分,
                    },
                    区分 = x.区分,
                    売上数量 = x.売上数量,
                    売上総額_税込 = x.売上総額_税込,
                    備考 = x.備考,
                },
                ExistsInDatabase = true,
            }).ToList(),
        };
    }
}
