using Microsoft.EntityFrameworkCore;
using MyApp.Core.Authorization;

namespace MyApp;

partial class OverridedApplicationService {
    public override async Task Execute入荷修正Async(入荷詳細DisplayData param, IPresentationContext<入荷詳細Messages> context) {
        if (LoginUser == null || !LoginUser.CanUse入荷登録) {
            context.Messages.AddError("入荷担当のみ実行可能です。");
            return;
        }

        var id = param.入荷ID;
        if (string.IsNullOrEmpty(id)) {
            context.Messages.AddError("入荷IDが指定されていません。");
            return;
        }

        // ヘッダ
        var header = await DbContext.LoadForEdit入荷Async(new 入荷KeyWithVersion(param.Version) {
            入荷ID = id,
        });
        if (header == null) {
            context.Messages.AddError(MSG.DataNotFoundError());
            return;
        }
        header.入荷日時 = param.入荷日時;
        header.担当者 = new() { 従業員番号 = param.担当者.従業員番号 };
        header.備考 = param.備考;
        header.GetValidationErrors().CopyTo(context.Messages);

        // 既存明細の読み込み
        var existingDetails = await DbContext.入荷明細DbSet
            .Include(x => x.商品)
            .Where(x => x.入荷!.入荷ID == id)
            .ToListAsync();

        var loaded = await DbContext.LoadForEdit入荷明細Async(existingDetails.Select(existingItem => new 入荷明細KeyWithVersion(
            // 画面で表示していた明細は画面表示時のバージョンで排他をかける
            param.入荷商品一覧.FirstOrDefault(x => x.ExistsInDatabase && x.入荷明細ID == existingItem.入荷明細ID)?.Version ?? existingItem.Version) {
            入荷明細ID = existingItem.入荷明細ID,
        }));

        var saveCommands = new List<入荷明細SaveCommand>();
        var changedProducts = new List<int?>();
        var processedIds = new HashSet<string?>();

        // 更新と新規登録
        for (var i = 0; i < param.入荷商品一覧.Count; i++) {
            var inputItem = param.入荷商品一覧[i];
            var message = context.Messages.入荷商品一覧[i];
            var inputId = inputItem.入荷明細ID;

            if (inputItem.ExistsInDatabase) {
                // 更新
                var existingItem = existingDetails.FirstOrDefault(x => x.入荷明細ID == inputId);
                if (existingItem == null || !loaded.TryGetValue(new() { 入荷明細ID = inputId }, out var detail)) {
                    message.AddError($"指定された入荷明細が存在しません。(ID: {inputId})");
                    continue;
                }
                processedIds.Add(inputId);

                // 商品変更チェック
                if (existingItem.商品?.商品SEQ != inputItem.商品.商品SEQ) {
                    if (existingItem.残数量 != existingItem.入荷数量) {
                        message.商品.AddError("既に出荷引当が行われているため、商品は変更できません。");
                    }
                }

                // 数量変更チェック
                var usedQuantity = existingItem.入荷数量 - existingItem.残数量;
                var newQuantity = inputItem.数量;
                if (newQuantity < usedQuantity) {
                    message.数量.AddError($"入荷数量を既に使用されている数量({usedQuantity})未満にすることはできません。");
                }

                if (message.HasError()) continue;

                detail.商品 = new() { 商品SEQ = inputItem.商品.商品SEQ };
                detail.仕入単価_税抜 = inputItem.仕入単価_税抜;
                detail.消費税区分 = inputItem.消費税区分;
                detail.入荷数量 = newQuantity;
                detail.残数量 = newQuantity - usedQuantity;
                detail.備考 = inputItem.備考;

                var errors = detail.GetValidationErrors();
                errors.CopyTo(message);
                errors.入荷数量.CopyTo(message.数量);
                saveCommands.Add(detail);

                // 在庫数量が変化した場合は商品管理システムへの通知が必要
                if (detail.残数量 != existingItem.残数量) {
                    changedProducts.Add(detail.商品?.商品SEQ);
                }

            } else {
                // 新規登録
                var detail = DbContext.StartCreating入荷明細();
                detail.入荷明細ID = Guid.NewGuid().ToString();
                detail.入荷 = new() { 入荷ID = id };
                detail.在庫調整 = null;
                detail.商品 = new() { 商品SEQ = inputItem.商品.商品SEQ };
                detail.仕入単価_税抜 = inputItem.仕入単価_税抜;
                detail.消費税区分 = inputItem.消費税区分;
                detail.入荷数量 = inputItem.数量;
                detail.残数量 = inputItem.数量;
                detail.備考 = inputItem.備考;

                var errors = detail.GetValidationErrors();
                errors.CopyTo(message);
                errors.入荷数量.CopyTo(message.数量);
                saveCommands.Add(detail);

                changedProducts.Add(detail.商品?.商品SEQ);
            }
        }

        // 削除
        foreach (var existingItem in existingDetails) {
            if (!processedIds.Contains(existingItem.入荷明細ID)) {
                // 使用済みチェック
                if (existingItem.残数量 != existingItem.入荷数量) {
                    context.Messages.AddError($"既に出荷引当が行われているため、削除できません。(商品: {existingItem.商品?.商品名})");
                    continue;
                }
                if (!loaded.TryGetValue(new() { 入荷明細ID = existingItem.入荷明細ID }, out var detail)) {
                    context.Messages.AddError(MSG.DataNotFoundError());
                    continue;
                }

                var deleteCommand = detail.ToDeleteCommand();
                deleteCommand.GetValidationErrors().CopyTo(context.Messages);
                saveCommands.Add(deleteCommand);
            }
        }

        if (context.Messages.HasError()) return;

        // エラーチェックのみの場合は確認メッセージを返す。
        if (context.ValidationOnly) {
            context.AddConfirm("入荷データを修正します。よろしいですか？");
            return;
        }

        // トランザクションの範囲は入荷ヘッダ更新から入荷明細登録まで
        await using var tran = await BeginTransactionAsync();

        var headerResult = await DbContext.TrySaveAsync(header);
        if (!headerResult.Success) {
            headerResult.Errors.CopyTo(context.Messages);
            return;
        }

        var detailResult = await DbContext.TrySaveAsync(saveCommands);
        if (!detailResult.Success) {
            detailResult.Errors.CopyTo(context.Messages);
            return;
        }
        await Notify在庫数量Async(changedProducts);

        await tran.CommitAsync();
        context.Messages.AddInfo("入荷登録が完了しました。");
    }
}
