using Microsoft.Extensions.DependencyInjection;
using MyApp.Core.外部システム;
using MyApp.Core.外部システム.商品管理システム;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp;

/// <summary>
/// アプリケーションサービス。
/// 主要なロジックはこのクラスに記述される。
/// プレゼンテーション層（UI、スケジューラー、ユニットテスト）から呼び出される。
/// </summary>
partial class OverridedApplicationService {
    #region 外部システム

    /// <summary>
    /// 外部リソース更新用のトランザクションを開始する。
    /// </summary>
    public async Task<ExternalResourceTransaction> BeginTransactionAsync() {
        if (CurrentTransaction != null) {
            throw new InvalidOperationException("すでにトランザクションが開始されています。ネストしたトランザクションはサポートされていません。");
        }
        if (DbContext.Database.CurrentTransaction != null) {
            throw new InvalidOperationException("すでにDBトランザクションが開始されています。外部リソース更新トランザクションはネストしたトランザクションをサポートしていません。");
        }

        var dbTransaction = await DbContext.Database.BeginTransactionAsync();
        _cachedCurrentTransaction = new ExternalResourceTransaction(dbTransaction, Log);
        return _cachedCurrentTransaction;
    }
    public ExternalResourceTransaction? CurrentTransaction {
        get {
            // コミットまたはロールバック済みの場合はキャッシュをクリア
            if (_cachedCurrentTransaction != null && _cachedCurrentTransaction.IsCommittedOrRolledBack) {
                _cachedCurrentTransaction = null;
            }
            return _cachedCurrentTransaction;
        }
    }
    private ExternalResourceTransaction? _cachedCurrentTransaction;

    /// <summary>
    /// 商品管理システムインターフェース。
    /// </summary>
    public I商品管理システム 商品管理システム => _cached商品管理システム ??= ServiceProvider.GetRequiredService<I商品管理システム>();
    private I商品管理システム? _cached商品管理システム;
    #endregion 外部システム
}
