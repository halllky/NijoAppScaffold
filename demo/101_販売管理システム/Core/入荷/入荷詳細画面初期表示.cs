using Microsoft.EntityFrameworkCore;

namespace MyApp;

partial class OverridedApplicationService {

    public override async Task Execute入荷詳細画面初期表示Async(入荷詳細画面初期表示ParameterDisplayData param, IPresentationContextWithReturnValue<入荷詳細DisplayData, 入荷詳細画面初期表示ParameterMessages> context) {
        var id = param.入荷ID;
        if (string.IsNullOrEmpty(id)) {
            context.Messages.AddError("入荷IDが指定されていません。");
            return;
        }

        var header = await DbContext.入荷DbSet
            .Include(x => x.担当者)
            .Include(x => x.RefFrom入荷明細_入荷)
            .ThenInclude(x => x.商品)
            .FirstOrDefaultAsync(x => x.入荷ID == id);

        if (header == null) {
            context.Messages.AddError($"入荷データが見つかりません。(ID: {id})");
            return;
        }

        context.ReturnValue = new() {
            入荷ID = header.入荷ID,
            入荷日時 = header.入荷日時,
            備考 = header.備考,
            担当者 = new() {
                従業員番号 = header.担当者?.従業員番号,
                氏名 = header.担当者?.氏名,
            },
            Version = header.Version,
            入荷商品一覧 = header.RefFrom入荷明細_入荷.Select(detail => new 入荷商品一覧DisplayData {
                入荷明細ID = detail.入荷明細ID,
                商品 = new() {
                    商品SEQ = detail.商品?.商品SEQ,
                    商品名 = detail.商品?.商品名,
                    売値単価_税抜 = detail.商品?.売値単価_税抜,
                    外部システム側ID = detail.商品?.外部システム側ID,
                    消費税区分 = detail.商品?.消費税区分,
                },
                数量 = detail.入荷数量,
                仕入単価_税抜 = detail.仕入単価_税抜,
                消費税区分 = detail.消費税区分,
                備考 = detail.備考,
                Version = detail.Version,
                ExistsInDatabase = true,
            }).ToList(),
        };
    }
}
