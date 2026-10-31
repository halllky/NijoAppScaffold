using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyApp.Debugging;

namespace MyApp.UnitTest;

/// <summary>
/// コマンドの読み込み、検証、保存の一連の動きのテスト
/// </summary>
partial class DB接続あり_更新あり {

    #region 読み込み
    [Test]
    public async Task 単数の読み込みで見つからなければnullになる() {
        var db = TestUtilImpl.Instance.CreateScope(nameof(単数の読み込みで見つからなければnullになる)).App.DbContext;

        var loaded = await db.LoadForEdit機器分類マスタAsync(new 機器分類マスタKeyWithVersion(0) { 機器分類ID = "NOTHING" });

        Assert.That(loaded, Is.Null);
    }

    [Test]
    public void バージョンがnullのキーは作れない() {
        Assert.Throws<ArgumentNullException>(() => _ = new 機器分類マスタKeyWithVersion(null) { 機器分類ID = "X" });
    }

    [Test]
    public async Task 複数の読み込みは見つからないものを結果に含まず分割して読み込む() {
        var db = TestUtilImpl.Instance.CreateScope(nameof(複数の読み込みは見つからないものを結果に含まず分割して読み込む)).App.DbContext;

        // 1回のSQLあたりのキーの数の上限である100件を超える件数を用意する
        const int COUNT = 230;
        await using (var tran = await db.Database.BeginTransactionAsync()) {
            (await db.TrySaveAsync(Enumerable.Range(0, COUNT).Select(i => Create機器分類(db, $"C{i}").AssertValid()).ToArray())).AssertSucceeded();
            await tran.CommitAsync();
        }

        var keys = Enumerable.Range(0, COUNT)
            .Select(i => new 機器分類マスタKeyWithVersion(0) { 機器分類ID = $"C{i}" })
            .Append(new 機器分類マスタKeyWithVersion(0) { 機器分類ID = "NOTHING" });
        var loaded = await db.LoadForEdit機器分類マスタAsync(keys);

        Assert.Multiple(() => {
            Assert.That(loaded, Has.Count.EqualTo(COUNT));
            Assert.That(loaded.ContainsKey(new() { 機器分類ID = "C229" }), Is.True);
            Assert.That(loaded.ContainsKey(new() { 機器分類ID = "NOTHING" }), Is.False);
        });
    }

    [Test]
    public async Task 外部参照と値オブジェクトを含む複合キーで読み込める() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(外部参照と値オブジェクトを含む複合キーで読み込める));
        var db = scope.App.DbContext;
        await new OverridedDummyDataGenerator().GenerateAsync(scope.App);

        var employee = await db.医療従事者マスタDbSet.OrderBy(x => x.医療従事者ID).FirstAsync();
        const string DEPARTMENT_ID = "D1";
        var date = new DateOnly(2030, 1, 2);

        await using (var tran = await db.Database.BeginTransactionAsync()) {
            var department = db.StartCreating診療科マスタ();
            department.診療科ID = DEPARTMENT_ID;
            department.診療科名 = "内科";
            (await db.TrySaveAsync(department.AssertValid())).AssertSucceeded();

            var schedule = db.StartCreating勤務スケジュール();
            schedule.医療従事者 = new() { 医療従事者ID = (医療従事者ID型?)employee.医療従事者ID };
            schedule.診療科 = new() { 診療科ID = DEPARTMENT_ID };
            schedule.日付 = date;
            (await db.TrySaveAsync(schedule.AssertValid())).AssertSucceeded();
            await tran.CommitAsync();
        }

        var key = new 勤務スケジュールKeyWithVersion(0) {
            医療従事者 = new() { 医療従事者ID = (医療従事者ID型?)employee.医療従事者ID },
            診療科 = new() { 診療科ID = DEPARTMENT_ID },
            日付 = date,
        };
        var loaded = await db.LoadForEdit勤務スケジュールAsync([key]);

        Assert.Multiple(() => {
            Assert.That(loaded.ContainsKey(key), Is.True, "別のインスタンスでも同じ値のキーなら引き当てられること");
            Assert.That(loaded[key].日付, Is.EqualTo(date));
        });
    }

    [Test]
    public async Task 任意の条件で子孫ごと読み込める() {
        var db = TestUtilImpl.Instance.CreateScope(nameof(任意の条件で子孫ごと読み込める)).App.DbContext;
        await using (var tran = await db.Database.BeginTransactionAsync()) {
            (await db.TrySaveAsync(Create保守計画(db, "PLAN1").AssertValid())).AssertSucceeded();
            (await db.TrySaveAsync(Create保守計画(db, "PLAN2").AssertValid())).AssertSucceeded();
            await tran.CommitAsync();
        }

        var loaded = await db.LoadForEdit保守計画Async(query => query.Where(x => x.保守計画ID == "PLAN2"));

        Assert.Multiple(() => {
            Assert.That(loaded.Single().保守計画ID, Is.EqualTo("PLAN2"));
            Assert.That(loaded.Single().作業工程, Has.Count.EqualTo(2));
            Assert.That(loaded.Single().作業工程[0].作業手順.Single().確認項目, Has.Count.EqualTo(1));
        });
    }
    #endregion 読み込み


    #region 検証
    [Test]
    public async Task 読み込み時点でバージョンが食い違っていれば検証エラーになる() {
        var db = TestUtilImpl.Instance.CreateScope(nameof(読み込み時点でバージョンが食い違っていれば検証エラーになる)).App.DbContext;
        await using (var tran = await db.Database.BeginTransactionAsync()) {
            (await db.TrySaveAsync(Create機器分類(db, "C1").AssertValid())).AssertSucceeded();
            await tran.CommitAsync();
        }

        var loaded = await db.LoadForEdit機器分類マスタAsync(new 機器分類マスタKeyWithVersion(99) { 機器分類ID = "C1" });

        Assert.Multiple(() => {
            Assert.That(loaded!.TryValidate(out _, out var updateErrors), Is.False);
            Assert.That(updateErrors.HasError(), Is.True);
            Assert.That(loaded.ToDeleteCommand().TryValidate(out _, out var deleteErrors), Is.False);
            Assert.That(deleteErrors.HasError(), Is.True);
        });
    }

    [Test]
    public void 検証エラーは項目ごとの位置に付く() {
        var db = TestUtilImpl.Instance.CreateScope(nameof(検証エラーは項目ごとの位置に付く)).App.DbContext;

        // 必須の参照のキーが空
        var reservation = db.StartCreating予約();
        reservation.予約ID = "R1";
        reservation.予約日時 = DateTime.Now;
        reservation.患者 = null;
        reservation.TryValidate(out _, out var reservationErrors);
        Assert.That(reservationErrors.患者.HasError(), Is.True);

        // Children の2行目の項目
        var plan = Create保守計画(db, "PLAN1");
        plan.作業工程[1].工程名 = new string('あ', 101);
        plan.TryValidate(out _, out var planErrors);
        Assert.Multiple(() => {
            Assert.That(planErrors.作業工程[0].工程名.HasError(), Is.False);
            Assert.That(planErrors.作業工程[1].工程名.HasError(), Is.True);
        });

        // 値オブジェクト型のキー
        var employee = db.StartCreating医療従事者マスタ();
        employee.医療従事者ID = null;
        employee.氏名 = "氏名";
        employee.TryValidate(out _, out var employeeErrors);
        Assert.That(employeeErrors.医療従事者ID.HasError(), Is.True);

        // 検証はコマンドの状態を変更しない
        Assert.That(plan.作業工程[1].工程名, Has.Length.EqualTo(101));
    }

    [Test]
    public void カスタム属性の検証はDbContextに1回書けば全集約に適用される() {
        var db = TestUtilImpl.Instance.CreateScope(nameof(カスタム属性の検証はDbContextに1回書けば全集約に適用される)).App.DbContext;

        var warehouse = db.StartCreating保管庫マスタ();
        warehouse.保管庫ID = "W1";
        warehouse.保管庫名 = "倉庫";
        warehouse.保管庫マスタの住所 = new() { 郵便番号 = "ABC", 都道府県 = null, 市区町村 = null, 番地建物名 = null };

        var department = db.StartCreating診療科マスタ();
        department.診療科ID = "D1";
        department.診療科名 = "内科";
        department.診療科マスタの住所 = new() { 郵便番号 = "XYZ", 都道府県 = null, 市区町村 = null, 番地建物名 = null };

        warehouse.TryValidate(out _, out var warehouseErrors);
        department.TryValidate(out _, out var departmentErrors);
        Assert.Multiple(() => {
            Assert.That(warehouseErrors.保管庫マスタの住所.郵便番号.HasError(), Is.True);
            Assert.That(departmentErrors.診療科マスタの住所.郵便番号.HasError(), Is.True);
        });
    }

    [Test]
    public void 検証エラーがあると検証済みコマンドは得られず無ければ得られる() {
        var db = TestUtilImpl.Instance.CreateScope(nameof(検証エラーがあると検証済みコマンドは得られず無ければ得られる)).App.DbContext;

        var command = Create機器分類(db, "C1");
        command.機器分類名 = new string('あ', 51);
        var isValid = command.TryValidate(out var validated, out var errors);
        Assert.Multiple(() => {
            Assert.That(isValid, Is.False);
            Assert.That(validated, Is.Null);
            Assert.That(errors.機器分類名.HasError(), Is.True);
        });

        command.機器分類名 = "修正後";
        var isValid2 = command.TryValidate(out var validated2, out var errors2);
        Assert.Multiple(() => {
            Assert.That(isValid2, Is.True);
            Assert.That(validated2, Is.Not.Null);
            Assert.That(errors2.HasError(), Is.False);
        });
    }

    [Test]
    public async Task 検証後にコマンドを書き換えても検証時点の値で保存される() {
        var db = TestUtilImpl.Instance.CreateScope(nameof(検証後にコマンドを書き換えても検証時点の値で保存される)).App.DbContext;
        await using var tran = await db.Database.BeginTransactionAsync();

        var command = Create保守計画(db, "PLAN1");
        var validated = command.AssertValid();
        command.計画名 = "検証後の値";
        command.作業工程[0].工程名 = "検証後の値";
        command.作業工程.RemoveAt(1);
        (await db.TrySaveAsync(validated)).AssertSucceeded();

        var root = await db.保守計画DbSet.SingleAsync();
        var processes = await db.作業工程DbSet.OrderBy(x => x.工程番号).ToListAsync();
        Assert.Multiple(() => {
            Assert.That(root.計画名, Is.EqualTo("定期保守"));
            Assert.That(processes.Select(x => x.工程名), Is.EqualTo(new[] { "点検", "清掃" }));
        });
    }
    #endregion 検証


    #region 保存結果
    [Test]
    public async Task 一括保存の結果は引数の順番と対応し削除は含まれない() {
        var db = TestUtilImpl.Instance.CreateScope(nameof(一括保存の結果は引数の順番と対応し削除は含まれない)).App.DbContext;
        await using var tran = await db.Database.BeginTransactionAsync();
        (await db.TrySaveAsync([Create機器分類(db, "UPD").AssertValid(), Create機器分類(db, "DEL").AssertValid()])).AssertSucceeded();

        var loaded = await db.LoadForEdit機器分類マスタAsync([
            new 機器分類マスタKeyWithVersion(0) { 機器分類ID = "UPD" },
            new 機器分類マスタKeyWithVersion(0) { 機器分類ID = "DEL" },
        ]);
        var update = loaded[new() { 機器分類ID = "UPD" }];
        update.機器分類名 = "更新後";

        var saved = (await db.TrySaveAsync<機器分類マスタUpdateCommand>(new ValidatedCommand<機器分類マスタUpdateCommand>[] {
            Create機器分類(db, "NEW").AssertValid(),
            loaded[new() { 機器分類ID = "DEL" }].ToDeleteCommand().AssertValid(),
            update.AssertValid(),
        })).AssertSucceeded();

        Assert.Multiple(async () => {
            Assert.That(saved.Select(x => x.機器分類ID), Is.EqualTo(new[] { "NEW", "UPD" }));
            Assert.That(saved[1], Is.Not.SameAs(update), "検証前のコマンドとは別のインスタンスが返ること");
            Assert.That(saved[1].機器分類名, Is.EqualTo("更新後"));
            Assert.That(await db.機器分類マスタDbSet.AnyAsync(x => x.機器分類ID == "DEL"), Is.False);
        });
    }

    [Test]
    public async Task 保存後のコマンドは別の更新にそのまま使える() {
        var db = TestUtilImpl.Instance.CreateScope(nameof(保存後のコマンドは別の更新にそのまま使える)).App.DbContext;
        await using var tran = await db.Database.BeginTransactionAsync();

        var saved = (await db.TrySaveAsync(Create保守計画(db, "PLAN1").AssertValid())).AssertSucceeded();
        saved.計画名 = "1回目の更新";
        var saved2 = (await db.TrySaveAsync(saved.AssertValid())).AssertSucceeded();
        saved2.計画名 = "2回目の更新";
        (await db.TrySaveAsync(saved2.AssertValid())).AssertSucceeded();

        var entity = await db.保守計画DbSet.SingleAsync();
        Assert.Multiple(() => {
            Assert.That(entity.計画名, Is.EqualTo("2回目の更新"));
            Assert.That(entity.Version, Is.EqualTo(2));
        });
    }
    #endregion 保存結果


    #region 失敗の分類
    [Test]
    public async Task 読み込み後に他者が更新すると楽観排他エラーになる() {
        var db = TestUtilImpl.Instance.CreateScope(nameof(読み込み後に他者が更新すると楽観排他エラーになる)).App.DbContext;
        await using var tran = await db.Database.BeginTransactionAsync();
        (await db.TrySaveAsync(Create機器分類(db, "C1").AssertValid())).AssertSucceeded();

        // キーで読み込んだ場合も任意の条件で読み込んだ場合も排他が効く
        var byKey = await db.LoadForEdit機器分類マスタAsync(new 機器分類マスタKeyWithVersion(0) { 機器分類ID = "C1" });
        var byQuery = (await db.LoadForEdit機器分類マスタAsync(query => query.Where(x => x.機器分類ID == "C1"))).Single();
        await db.機器分類マスタDbSet.ExecuteUpdateAsync(x => x.SetProperty(e => e.Version, 5));

        byKey!.機器分類名 = "更新";
        var byKeyResult = await db.TrySaveAsync(byKey.AssertValid());
        byQuery.機器分類名 = "更新";
        var byQueryResult = await db.TrySaveAsync(byQuery.AssertValid());

        Assert.Multiple(() => {
            Assert.That(byKeyResult.FailureKind, Is.EqualTo(SaveFailureKind.Concurrency));
            Assert.That(byQueryResult.FailureKind, Is.EqualTo(SaveFailureKind.Concurrency));
            Assert.That(byKeyResult.Errors.HasError(), Is.True);
        });
    }

    [Test]
    public async Task 既存データとのキー重複は一意制約違反になる() {
        var db = TestUtilImpl.Instance.CreateScope(nameof(既存データとのキー重複は一意制約違反になる)).App.DbContext;
        await using var tran = await db.Database.BeginTransactionAsync();
        (await db.TrySaveAsync(Create機器分類(db, "C1").AssertValid())).AssertSucceeded();

        var result = await db.TrySaveAsync(Create機器分類(db, "C1").AssertValid());

        Assert.That(result.FailureKind, Is.EqualTo(SaveFailureKind.UniqueConstraint));
    }

    [Test]
    public async Task 同時に保存するコマンド同士のキー重複は一意制約違反になる() {
        var db = TestUtilImpl.Instance.CreateScope(nameof(同時に保存するコマンド同士のキー重複は一意制約違反になる)).App.DbContext;
        await using var tran = await db.Database.BeginTransactionAsync();

        var result = await db.TrySaveAsync([Create機器分類(db, "C1").AssertValid(), Create機器分類(db, "C1").AssertValid()]);

        Assert.Multiple(() => {
            Assert.That(result.FailureKind, Is.EqualTo(SaveFailureKind.UniqueConstraint));
            Assert.That(db.ChangeTracker.Entries(), Is.Empty);
        });
    }

    [Test]
    public async Task 運用担当者への通知が不要な失敗ではErrorログが出ない() {
        var logs = new ErrorLogCollector();
        var scope = TestUtilImpl.Instance.CreateScope(
            nameof(運用担当者への通知が不要な失敗ではErrorログが出ない),
            services => services.AddLogging(logging => logging.AddProvider(logs)));
        var db = scope.App.DbContext;
        await using var tran = await db.Database.BeginTransactionAsync();
        (await db.TrySaveAsync(Create機器分類(db, "C1").AssertValid())).AssertSucceeded();

        // 一意制約違反
        var unique = await db.TrySaveAsync(Create機器分類(db, "C1").AssertValid());
        Assert.That(unique.FailureKind, Is.EqualTo(SaveFailureKind.UniqueConstraint));

        // 楽観排他エラー
        var loaded = await db.LoadForEdit機器分類マスタAsync(new 機器分類マスタKeyWithVersion(0) { 機器分類ID = "C1" });
        await db.機器分類マスタDbSet.ExecuteUpdateAsync(x => x.SetProperty(e => e.Version, 5));
        var concurrency = await db.TrySaveAsync(loaded!.AssertValid());
        Assert.That(concurrency.FailureKind, Is.EqualTo(SaveFailureKind.Concurrency));

        Assert.That(logs.Messages, Is.Empty);
    }

    [Test]
    public async Task 運用担当者への通知が必要な失敗ではErrorログが出る() {
        var logs = new ErrorLogCollector();
        var scope = TestUtilImpl.Instance.CreateScope(
            nameof(運用担当者への通知が必要な失敗ではErrorログが出る),
            services => services.AddLogging(logging => logging.AddProvider(logs)));
        var db = scope.App.DbContext;
        await using var tran = await db.Database.BeginTransactionAsync();

        // 外部キー制約違反
        var schedule = db.StartCreating勤務スケジュール();
        schedule.医療従事者 = new() { 医療従事者ID = (医療従事者ID型?)"NOTHING" };
        schedule.診療科 = new() { 診療科ID = "NOTHING" };
        schedule.日付 = new DateOnly(2030, 1, 2);
        var result = await db.TrySaveAsync(schedule.AssertValid());

        Assert.Multiple(() => {
            Assert.That(result.FailureKind, Is.EqualTo(SaveFailureKind.DatabaseError));
            Assert.That(logs.Messages, Is.Not.Empty);
        });
    }

    /// <summary>Error レベル以上のログを収集する</summary>
    private sealed class ErrorLogCollector : ILoggerProvider, ILogger {
        private readonly System.Collections.Concurrent.ConcurrentQueue<string> _messages = new();
        public IReadOnlyCollection<string> Messages => _messages;

        public ILogger CreateLogger(string categoryName) => this;
        public void Dispose() { }
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Error;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
            if (IsEnabled(logLevel)) _messages.Enqueue(formatter(state, exception));
        }
    }

    [Test]
    public async Task 保存処理の前に別の変更があるとプログラム誤りになる() {
        var db = TestUtilImpl.Instance.CreateScope(nameof(保存処理の前に別の変更があるとプログラム誤りになる)).App.DbContext;
        await using var tran = await db.Database.BeginTransactionAsync();
        db.機器分類マスタDbSet.Add(new 機器分類マスタDbEntity { 機器分類ID = "OTHER", Version = 0 });

        var result = await db.TrySaveAsync(Create機器分類(db, "C1").AssertValid());

        Assert.That(result.FailureKind, Is.EqualTo(SaveFailureKind.ProgrammingError));
    }

    [Test]
    public async Task 成功しても失敗しても変更追跡が残らない() {
        var db = TestUtilImpl.Instance.CreateScope(nameof(成功しても失敗しても変更追跡が残らない)).App.DbContext;
        await using var tran = await db.Database.BeginTransactionAsync();

        (await db.TrySaveAsync(Create保守計画(db, "PLAN1").AssertValid())).AssertSucceeded();
        Assert.That(db.ChangeTracker.Entries(), Is.Empty);

        var failed = await db.TrySaveAsync(Create保守計画(db, "PLAN1").AssertValid());
        Assert.Multiple(() => {
            Assert.That(failed.Success, Is.False);
            Assert.That(failed.Errors.HasError(), Is.True);
            Assert.That(() => failed.Updated, Throws.InvalidOperationException);
            Assert.That(db.ChangeTracker.Entries(), Is.Empty);
        });
    }

    [Test]
    public async Task 一度保存処理にかけたコマンドは再度使用できない() {
        var db = TestUtilImpl.Instance.CreateScope(nameof(一度保存処理にかけたコマンドは再度使用できない)).App.DbContext;
        await using var tran = await db.Database.BeginTransactionAsync();
        var command = Create機器分類(db, "C1");
        var validated = command.AssertValid();
        (await db.TrySaveAsync(validated)).AssertSucceeded();

        Assert.ThrowsAsync<InvalidOperationException>(async () => await db.TrySaveAsync(validated));
        Assert.ThrowsAsync<InvalidOperationException>(async () => await db.TrySaveAsync(command.AssertValid()), "検証し直して別の検証済みコマンドを作っても再度使用できないこと");
    }

    [Test]
    public void トランザクションを開始せずに保存すると例外になる() {
        var db = TestUtilImpl.Instance.CreateScope(nameof(トランザクションを開始せずに保存すると例外になる)).App.DbContext;

        Assert.ThrowsAsync<InvalidOperationException>(async () => await db.TrySaveAsync(Create機器分類(db, "C1").AssertValid()));
    }
    #endregion 失敗の分類


    #region トランザクション
    [Test]
    public async Task 同一トランザクション内で失敗したあとも後続の保存をコミットできる() {
        var db = TestUtilImpl.Instance.CreateScope(nameof(同一トランザクション内で失敗したあとも後続の保存をコミットできる)).App.DbContext;

        await using (var tran = await db.Database.BeginTransactionAsync()) {
            (await db.TrySaveAsync(Create機器分類(db, "C1").AssertValid())).AssertSucceeded();

            // SQL発行時点で失敗させる
            var failed = await db.TrySaveAsync(Create機器分類(db, "C1").AssertValid());
            Assert.That(failed.Success, Is.False);

            (await db.TrySaveAsync(Create機器分類(db, "C2").AssertValid())).AssertSucceeded();
            await tran.CommitAsync();
        }

        var ids = await db.機器分類マスタDbSet.Select(x => x.機器分類ID).OrderBy(x => x).ToListAsync();
        Assert.That(ids, Is.EqualTo(new[] { "C1", "C2" }));
    }

    [Test]
    public async Task コミットしなければロールバックされる() {
        var db = TestUtilImpl.Instance.CreateScope(nameof(コミットしなければロールバックされる)).App.DbContext;

        await using (var tran = await db.Database.BeginTransactionAsync()) {
            (await db.TrySaveAsync(Create機器分類(db, "C1").AssertValid())).AssertSucceeded();
        }

        Assert.That(await db.機器分類マスタDbSet.AnyAsync(), Is.False);
    }
    #endregion トランザクション


    #region 子孫と監査情報
    [Test]
    public async Task 子孫の追加更新削除で監査情報が正しく設定される() {
        var audit = new FixedAuditInfo { CurrentTime = new DateTime(2030, 1, 1), CurrentUser = "作成者" };
        var scope = TestUtilImpl.Instance.CreateScope(
            nameof(子孫の追加更新削除で監査情報が正しく設定される),
            services => services.AddScoped<IAuditInfo>(_ => audit));
        var db = scope.App.DbContext;
        await using var tran = await db.Database.BeginTransactionAsync();

        var created = (await db.TrySaveAsync(Create保守計画(db, "PLAN1").AssertValid())).AssertSucceeded();

        audit.CurrentTime = new DateTime(2030, 2, 2);
        audit.CurrentUser = "更新者";

        created.作業工程[0].工程名 = "更新された工程";
        created.作業工程.RemoveAt(1);
        created.作業工程.Add(new() { 工程番号 = 3, 工程名 = "追加された工程", 作業手順 = [], 作業体制 = null });
        var updated = (await db.TrySaveAsync(created.AssertValid())).AssertSucceeded();

        var root = await db.保守計画DbSet.SingleAsync();
        var existing = await db.作業工程DbSet.SingleAsync(x => x.工程番号 == 1);
        var added = await db.作業工程DbSet.SingleAsync(x => x.工程番号 == 3);
        Assert.Multiple(async () => {
            Assert.That(updated.作業工程.Select(x => x.工程番号), Is.EquivalentTo(new[] { 1, 3 }), "削除された子孫は保存後のデータに含まれないこと");
            Assert.That(await db.作業工程DbSet.AnyAsync(x => x.工程番号 == 2), Is.False);

            Assert.That((root.Version, root.CreatedAt, root.UpdatedAt, root.CreateUser, root.UpdateUser),
                Is.EqualTo((1, new DateTime(2030, 1, 1), new DateTime(2030, 2, 2), "作成者", "更新者")));
            Assert.That((existing.CreatedAt, existing.UpdatedAt, existing.CreateUser, existing.UpdateUser),
                Is.EqualTo((new DateTime(2030, 1, 1), new DateTime(2030, 2, 2), "作成者", "更新者")));
            Assert.That((added.CreatedAt, added.UpdatedAt, added.CreateUser, added.UpdateUser),
                Is.EqualTo((new DateTime(2030, 2, 2), new DateTime(2030, 2, 2), "更新者", "更新者")));
        });
    }

    [Test]
    public async Task 存在しないChildは読み込んで保存しても登録されない() {
        var db = TestUtilImpl.Instance.CreateScope(nameof(存在しないChildは読み込んで保存しても登録されない)).App.DbContext;
        await using var tran = await db.Database.BeginTransactionAsync();

        var plan = Create保守計画(db, "PLAN1");
        (await db.TrySaveAsync(plan.AssertValid())).AssertSucceeded();
        Assert.That(await db.作業体制DbSet.CountAsync(), Is.EqualTo(1));

        var loaded = await db.LoadForEdit保守計画Async(new 保守計画KeyWithVersion(0) { 保守計画ID = "PLAN1" });
        Assert.That(loaded!.作業工程.Single(x => x.工程番号 == 2).作業体制, Is.Null);

        loaded.計画名 = "更新";
        (await db.TrySaveAsync(loaded.AssertValid())).AssertSucceeded();

        Assert.That(await db.作業体制DbSet.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task 論理削除は読み込んだデータを子孫ごと退避テーブルに移送する() {
        var audit = new FixedAuditInfo { CurrentTime = new DateTime(2030, 3, 3), CurrentUser = "削除者" };
        var scope = TestUtilImpl.Instance.CreateScope(
            nameof(論理削除は読み込んだデータを子孫ごと退避テーブルに移送する),
            services => services.AddScoped<IAuditInfo>(_ => audit));
        var db = scope.App.DbContext;
        await using var tran = await db.Database.BeginTransactionAsync();
        (await db.TrySaveAsync(Create保守計画(db, "PLAN1").AssertValid())).AssertSucceeded();

        var loaded = await db.LoadForEdit保守計画Async(new 保守計画KeyWithVersion(0) { 保守計画ID = "PLAN1" });
        (await db.TrySaveAsync(loaded!.ToDeleteCommand().AssertValid())).AssertSucceeded();

        var deleted = await db.保守計画DeletedDbSet.SingleAsync();
        Assert.Multiple(async () => {
            Assert.That(await db.保守計画DbSet.AnyAsync(), Is.False);
            Assert.That(await db.作業工程DbSet.AnyAsync(), Is.False);
            Assert.That(await db.作業工程DeletedDbSet.CountAsync(), Is.EqualTo(2));
            Assert.That(deleted.DeletedAt, Is.EqualTo(new DateTime(2030, 3, 3)));
            Assert.That(deleted.DeletedUser, Is.EqualTo("削除者"));
            Assert.That(db.ChangeTracker.Entries(), Is.Empty);
        });
    }

    private sealed class FixedAuditInfo : IAuditInfo {
        public DateTime CurrentTime { get; set; }
        public string CurrentUser { get; set; } = string.Empty;
    }
    #endregion 子孫と監査情報


    private static 機器分類マスタCreateCommand Create機器分類(OverridedDbContext db, string id) {
        var command = db.StartCreating機器分類マスタ();
        command.機器分類ID = id;
        command.機器分類名 = $"機器分類{id}";
        return command;
    }

    /// <summary>工程1は作業体制あり、工程2は作業体制なし</summary>
    private static 保守計画CreateCommand Create保守計画(OverridedDbContext db, string id) {
        var command = db.StartCreating保守計画();
        command.保守計画ID = id;
        command.計画名 = "定期保守";
        command.作業工程 = [
            new() {
                工程番号 = 1,
                工程名 = "点検",
                作業手順 = [
                    new() {
                        手順番号 = 1,
                        手順内容 = "外観を確認する",
                        確認項目 = [new() { 確認項目番号 = 1, 確認内容 = "破損がないこと" }],
                    },
                ],
                作業体制 = new() {
                    責任者名 = "責任者A",
                    作業者 = [new() { 作業者番号 = 1, 作業者名 = "作業者A" }],
                },
            },
            new() {
                工程番号 = 2,
                工程名 = "清掃",
                作業手順 = [],
                作業体制 = null,
            },
        ];
        return command;
    }
}
