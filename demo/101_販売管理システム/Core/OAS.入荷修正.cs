using Microsoft.EntityFrameworkCore;
using MyApp.Core.Authorization;

namespace MyApp;

partial class OverridedApplicationService {
    public override async Task Execute入荷修正(入荷詳細DisplayData param, IPresentationContext<入荷詳細Messages> context) {
        if (LoginUser == null || !LoginUser.CanUse入荷登録) {
            context.Messages.AddError("入荷担当のみ実行可能です。");
            return;
        }

        var id = param.Values.入荷ID;
        if (string.IsNullOrEmpty(id)) {
            context.Messages.AddError("入荷IDが指定されていません。");
            return;
        }

        // トランザクションの範囲は入荷ヘッダ更新から入荷明細登録まで
        using var tran = context.Options.IgnoreConfirm
            ? await DbContext.Database.BeginTransactionAsync()
            : null;

        // ヘッダ更新
        var updateHeaderResult = await Update入荷Async(param.Values.入荷ID, param.Values.Version, header => {
            header.入荷日時 = param.Values.入荷日時;
            header.担当者 = new() { 従業員番号 = param.Values.担当者.従業員番号 };
            header.備考 = param.Values.備考;
        }, context);

        if (updateHeaderResult.Result == DataModelSaveResultType.Error) return;

        // 既存明細の読み込み
        var existingDetails = await DbContext.入荷明細DbSet
            .Include(x => x.商品)
            .Where(x => x.入荷!.入荷ID == id)
            .ToListAsync();

        var processedIds = new HashSet<string?>();

        // 1. 更新と新規登録
        for (var i = 0; i < param.入荷商品一覧.Count; i++) {
            var inputItem = param.入荷商品一覧[i];
            var message = context.Messages.入荷商品一覧[i];
            var inputId = inputItem.Values.入荷明細ID;

            if (inputItem.ExistsInDatabase) {
                // 更新
                var existingItem = existingDetails.FirstOrDefault(x => x.入荷明細ID == inputId);
                if (existingItem == null) {
                    message.AddError($"指定された入荷明細が存在しません。(ID: {inputId})");
                    continue;
                }
                processedIds.Add(inputId);

                // 商品変更チェック
                if (existingItem.商品?.商品SEQ != inputItem.Values.商品.商品SEQ) {
                    if (existingItem.残数量 != existingItem.入荷数量) {
                        message.商品.AddError("既に出荷引当が行われているため、商品は変更できません。");
                    }
                }

                // 数量変更チェック
                var usedQuantity = existingItem.入荷数量 - existingItem.残数量;
                var newQuantity = inputItem.Values.数量;
                if (newQuantity < usedQuantity) {
                    message.数量.AddError($"入荷数量を既に使用されている数量({usedQuantity})未満にすることはできません。");
                }

                if (message.HasError()) continue;

                // 更新実行
                await Update入荷明細Async(inputId, inputItem.Values.Version, detail => {
                    detail.商品 = new() { 商品SEQ = inputItem.Values.商品.商品SEQ };
                    detail.仕入単価_税抜 = inputItem.Values.仕入単価_税抜;
                    detail.消費税区分 = inputItem.Values.消費税区分;
                    detail.入荷数量 = newQuantity;
                    detail.残数量 = newQuantity - usedQuantity;
                    detail.備考 = inputItem.Values.備考;
                }, context, message);

            } else {
                // 新規登録
                await Create入荷明細Async(new() {
                    入荷明細ID = Guid.NewGuid().ToString(),
                    入荷 = new() { 入荷ID = id },
                    在庫調整 = null,
                    商品 = new() { 商品SEQ = inputItem.Values.商品.商品SEQ },
                    仕入単価_税抜 = inputItem.Values.仕入単価_税抜,
                    消費税区分 = inputItem.Values.消費税区分,
                    入荷数量 = inputItem.Values.数量,
                    残数量 = inputItem.Values.数量,
                    備考 = inputItem.Values.備考,
                }, context, message);
            }
        }

        // 2. 削除
        foreach (var existingItem in existingDetails) {
            if (!processedIds.Contains(existingItem.入荷明細ID)) {
                // 使用済みチェック
                if (existingItem.残数量 != existingItem.入荷数量) {
                    context.Messages.AddError($"既に出荷引当が行われているため、削除できません。(商品: {existingItem.商品?.商品名})");
                    continue;
                }

                await Delete入荷明細Async(new() {
                    入荷明細ID = existingItem.入荷明細ID,
                    Version = existingItem.Version,
                }, context);
            }
        }

        if (tran != null && !context.Messages.HasError()) {
            await tran.CommitAsync();
        }
    }
}
