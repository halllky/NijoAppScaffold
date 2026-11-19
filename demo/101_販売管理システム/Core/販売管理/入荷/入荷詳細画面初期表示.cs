using Microsoft.EntityFrameworkCore;

namespace MyApp;

partial class OverridedApplicationService {

    protected override async Task<入荷詳細DisplayData?> OnExecute入荷詳細画面初期表示Async(入荷詳細画面初期表示ParameterDisplayData param, 入荷詳細画面初期表示ParameterMessages messages, OperationContext context) {
        var id = param.入荷ID;
        if (string.IsNullOrEmpty(id)) {
            messages.AddError(MSG_DEMO101.NotSpecifiedError("入荷ID"));
            return null;
        }

        var header = await DbContext.入荷DbSet
            .Include(x => x.担当者)
            .Include(x => x.入荷の添付書類)
            .ThenInclude(x => x.ファイル)
            .Include(x => x.RefFrom入荷明細_入荷)
            .ThenInclude(x => x.商品)
            .FirstOrDefaultAsync(x => x.入荷ID == id);

        if (header == null) {
            messages.AddError(MSG_DEMO101.ArrivalNotFoundError(id));
            return null;
        }

        return new() {
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
            // 添付した順に並べる。同じ保存で添付したものは、ファイル名の順にする
            入荷詳細の添付書類 = header.入荷の添付書類
                .OrderBy(x => x.CreatedAt)
                .ThenBy(x => x.ファイル?.名前)
                .Select(x => new 入荷詳細の添付書類DisplayData {
                    添付書類ID = x.添付書類ID,
                    ファイル = new() {
                        識別子 = x.ファイル?.識別子,
                        名前 = x.ファイル?.名前,
                        サイズ = x.ファイル?.サイズ,
                        種類 = x.ファイル?.種類,
                    },
                    備考 = x.備考,
                    ExistsInDatabase = true,
                }).ToList(),
        };
    }
}
