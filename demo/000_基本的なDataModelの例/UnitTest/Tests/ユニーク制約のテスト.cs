using MyApp;
using MyApp.Debugging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.UnitTest;

/// <summary>
/// パターン
///
/// * ユニーク制約がついているメンバーの属性のパターン
///   * ValueMember
///   * RefToMember
/// * ユニーク制約がついているメンバーの種類のパターン
///   * 主キー以外の列にユニーク制約がついているとき
///     * 同じ列に複数のユニーク制約がついている
///     * ユニーク制約の組み合わせが複数存在する
///   * 主キーが複合キー1, 2 から成るとき
///     * 1だけにユニーク制約がついている
///     * 1, 3(非主キー) の組み合わせにユニーク制約がついている
///   * 重複（以下3種類のユニーク制約が同じ集約に対して定義されている）
///     * 列1, 2 から成るもの
///     * 列1, 3 から成るもの
///     * 列1, 2, 3 から成るもの
/// </summary>
public class ユニーク制約のテスト {

    [Test]
    public async Task 非主キーのユニーク制約が効いているか() {
        var scope = TestUtilImpl.Instance.CreateScope("ユニーク制約のテスト");

        // TODO
    }

    public async Task ユニーク制約を含むナビゲーションプロパティのクエリが正しく動作するか() {
        var scope = TestUtilImpl.Instance.CreateScope("ユニーク制約を含むナビゲーションプロパティのクエリが正しく動作するか");

        // TODO
    }
}
