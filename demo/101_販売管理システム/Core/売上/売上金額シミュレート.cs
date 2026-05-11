
namespace MyApp;

partial class OverridedApplicationService {

    /// <summary>
    /// 自動計算される売上金額をシミュレートする。
    /// 結果は引数のオブジェクトのプロパティに直接設定される。
    /// </summary>
    public static void Simulate(売上詳細DisplayData param) {

        foreach (var item in param.売上詳細の売上明細) {

            // 丸め処理は明細単位
            var taxRate = item.商品?.消費税区分 switch {
                消費税区分.一般税率 => 1.10m,
                消費税区分.軽減税率 => 1.08m,
                _ => 1.00m,
            };
            var multiplied = (item.売上数量 ?? 0m) * (item.商品?.売値単価_税抜 ?? 0m) * taxRate;
            var rounded = Math.Round(multiplied, 0, MidpointRounding.AwayFromZero);

            // 取消（赤伝）の場合は負の金額
            item.売上総額_税込_自動計算 = item.区分 == 売上明細区分.売上
                ? (int)rounded
                : -(int)rounded;
        }

        param.合計金額 = param.売上詳細の売上明細.Sum(d => d.売上総額_税込_手修正
                                                    ?? d.売上総額_税込_自動計算
                                                    ?? 0);
    }

    public override Task Execute売上金額シミュレートAsync(売上詳細DisplayData param, IPresentationContextWithReturnValue<売上詳細DisplayData, 売上詳細Messages> context) {
        Simulate(param);
        context.ReturnValue = param;
        return Task.CompletedTask;
    }
}
