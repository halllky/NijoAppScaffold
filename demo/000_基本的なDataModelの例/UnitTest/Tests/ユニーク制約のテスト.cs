using MyApp;
using MyApp.Debugging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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
    [Category("DB接続あり（更新あり）")]
    public async Task 非主キーのユニーク制約が効いているか() {
        var scope = TestUtilImpl.Instance.CreateScope("ユニーク制約のテスト");
        var db = scope.App.DbContext;

        await using var transaction = await db.BeginTransactionAsync();

        // 準備
        var legacy = db.StartCreating旧システム部署情報();
        legacy.旧システムコード = "LEGACY-001";
        legacy.名称 = "旧システム部署A";
        (await db.TrySaveAsync(legacy)).AssertSucceeded();

        var firstDepartment = db.StartCreating部署();
        firstDepartment.部署ID = 1;
        firstDepartment.部署名 = "第一部署";
        firstDepartment.事業所 = new() { 事業所ID = "1" };
        firstDepartment.課 = [new() {
            コード = "SEC-A",
            旧システムコード = new() { 旧システムコード = "LEGACY-001" },
            課名称 = "第一課",
            係 = [],
        }];
        (await db.TrySaveAsync(firstDepartment)).AssertSucceeded();

        var duplicateDepartment = db.StartCreating部署();
        duplicateDepartment.部署ID = 2;
        duplicateDepartment.部署名 = "第二部署";
        duplicateDepartment.事業所 = new() { 事業所ID = "1" };
        duplicateDepartment.課 = [new() {
            コード = "SEC-B",
            旧システムコード = new() { 旧システムコード = "LEGACY-001" }, // ユニーク制約違反
            課名称 = "第二課",
            係 = [],
        }];
        var duplicateResult = await db.TrySaveAsync(duplicateDepartment);

        Assert.That(duplicateResult.Success, Is.False, "ユニーク制約違反でエラーになっていません。");
        Assert.That(duplicateResult.FailureKind, Is.EqualTo(SaveFailureKind.UniqueConstraint));

        var sectionCount = await db.課DbSet.CountAsync(x => x.旧システムコード_旧システムコード == "LEGACY-001");
        Assert.That(sectionCount, Is.EqualTo(1), "ユニーク制約違反時に既存データが巻き戻されていません。");

        await transaction.CommitAsync();
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task ユニーク制約を含むナビゲーションプロパティのクエリが正しく動作するか() {
        var scope = TestUtilImpl.Instance.CreateScope("ユニーク制約を含むナビゲーションプロパティのクエリが正しく動作するか");
        var db = scope.App.DbContext;

        await using var transaction = await db.BeginTransactionAsync();

        const string legacyCode = "NAV-001";
        const int departmentId = 10;
        var legacy = db.StartCreating旧システム部署情報();
        legacy.旧システムコード = legacyCode;
        legacy.名称 = "ナビ用旧システム部署";
        (await db.TrySaveAsync(legacy)).AssertSucceeded();

        var department = db.StartCreating部署();
        department.部署ID = departmentId;
        department.部署名 = "ナビゲーション検証部署";
        department.事業所 = new() { 事業所ID = "1" };
        department.課 = [new() {
            コード = "NAV-SEC",
            旧システムコード = new() { 旧システムコード = legacyCode },
            課名称 = "ナビゲーション課",
            係 = [],
        }];
        (await db.TrySaveAsync(department)).AssertSucceeded();

        await transaction.CommitAsync();

        var legacyWithNavigation = await db.旧システム部署情報DbSet
            .Include(x => x.RefFrom課_旧システムコード)
            .SingleAsync(x => x.旧システムコード == legacyCode);

        Assert.That(legacyWithNavigation.RefFrom課_旧システムコード, Is.Not.Null, "ユニーク制約で接続されたナビゲーションが取得できません。");
        Assert.That(legacyWithNavigation.RefFrom課_旧システムコード!.コード, Is.EqualTo("NAV-SEC"));
        Assert.That(legacyWithNavigation.RefFrom課_旧システムコード!.旧システムコード_旧システムコード, Is.EqualTo(legacyCode));
        Assert.That(legacyWithNavigation.RefFrom課_旧システムコード!.Parent_部署ID, Is.EqualTo(departmentId));

        var section = await db.課DbSet
            .Include(x => x.旧システムコード)
            .SingleAsync(x => x.Parent_部署ID == departmentId && x.コード == "NAV-SEC");

        Assert.That(section.旧システムコード, Is.Not.Null, "課から旧システム部署情報へのナビゲーションが解決できません。");
        Assert.That(section.旧システムコード!.旧システムコード, Is.EqualTo(legacyCode));
    }
}
