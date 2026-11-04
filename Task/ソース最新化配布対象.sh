# Nijo.NewProjectTemplate から各デモへ配布するファイルの一覧。Task/ソース最新化.sh が読み込む。
#
# ここに載せるのは、アプリ固有の判断を含まず、デモごとに変える理由がない手書きのファイルである。
# デモ側で直接編集しても次の最新化で上書きされるので、テンプレート側を編集すること。
# どのファイルをここに載せるかの基準は demo/README.md を参照。
#
# 末尾が / のものはフォルダ単位で同期する（テンプレートに無いファイルはデモからも消える）。

DISTRIBUTION_TARGETS=(
    # ソースの書式・改行コード
    .editorconfig
    .gitattributes

    # React
    # サーバーの呼び出し（client/src/base/ のその他のファイル）は、HTTPの取り決めがデモごとに異なるので配布しない
    client/src/base/toWillBeChangedApplied.ts

    # Core
    Core/OperationContext.cs
    Core/Debugging/EnsureCreatedAsyncEx.cs
    Core/JsonSerializerExtensions.cs
    Core/MessageContainerExtension.cs
    Core/MigrationEntryPoint.cs
    Core/OverridedDbContext.TrySaveAsync.cs

    # WebApi
    # 自動生成されたクエリ・コマンドのエンドポイント（WebApi/Base/）は、HTTPの取り決めがデモごとに異なるので配布しない
    WebApi/Properties/launchSettings.json
    WebApi/Properties/PublishProfiles/本番用ビルド.pubxml

    # UnitTest
    UnitTest/AssemblyInfo.cs
    UnitTest/SaveResultAssertion.cs
    UnitTest/TestFailureMessage.cs
    UnitTest/TestUtilImpl.cs

    # Management
    Management/Commands/GenerateGeneralLookupViewsSqlCommand.cs
    Management/Program.cs
    Management/README.md

    # Task
    Task/
)
