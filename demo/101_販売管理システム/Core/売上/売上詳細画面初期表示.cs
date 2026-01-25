using Microsoft.EntityFrameworkCore;
using MyApp.Core.Authorization;

namespace MyApp;

partial class OverridedApplicationService {
    public override async Task Execute売上詳細画面初期表示Async(売上詳細画面初期表示ParameterDisplayData param, IPresentationContextWithReturnValue<売上詳細DisplayData, 売上詳細画面初期表示ParameterMessages> context) {

        // 新規登録モード
        if (param.Values.新規登録モード == true) {
            context.ReturnValue = new() {
                Values = new() {
                    売上日時 = CurrentTime,
                    担当者 = new() {
                        従業員番号 = LoginUser?.従業員番号,
                        氏名 = LoginUser?.氏名,
                    },
                },
            };
            return;
        }

        // -----------------------------
        // 以降は既存データ編集モード

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

        var returnValue = new 売上詳細DisplayData() {
            Values = new() {
                売上SEQ = entity.売上SEQ,
                売上日時 = entity.売上日時,
                担当者 = new() {
                    従業員番号 = entity.担当者?.従業員番号,
                    氏名 = entity.担当者?.氏名,
                },
                合計金額 = null, // このあとのシミュレート処理で設定する
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
                    売上総額_税込_自動計算 = null, // このあとのシミュレート処理で設定する
                    売上総額_税込_手修正 = x.売上総額_税込,
                },
                ExistsInDatabase = true,
            }).ToList(),
        };

        // 金額の自動計算
        Simulate(returnValue);

        // 自動計算された金額と手修正された金額が一致する場合は、手修正された金額をクリアする
        foreach (var item in returnValue.売上詳細の売上明細) {
            if (item.Values.売上総額_税込_手修正 == item.Values.売上総額_税込_自動計算) {
                item.Values.売上総額_税込_手修正 = null;
            }
        }

        context.ReturnValue = returnValue;
    }
}
