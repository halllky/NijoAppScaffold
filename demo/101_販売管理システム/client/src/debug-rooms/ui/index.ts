import type { RouteObject } from "react-router-dom"

/*
 * ui フォルダの部品の動作確認画面。
 * nijo.xml のデータ構造定義には依存しないので、どのプロジェクトでもそのまま動作確認に使える。
 *
 * - このフォルダ直下のモジュールが、それぞれ1つの動作確認画面。
 * - mocks フォルダは、本来はプロジェクトの自動生成されたソースや画面側のコードが用意するもの
 *   （集約のモジュール・列挙体・検索ダイアログとその対応表）を、動作確認画面のために模したもの。
 *
 * 外から使ってよいのは、このモジュールが export するルーティング定義と URL だけ。
 */

/** 詳細画面サンプルの URL */
export const 詳細画面サンプルURL = "/debug/ui/display-data"

/** 一覧検索サンプルの URL */
export const 一覧検索サンプルURL = "/debug/ui/search-condition"

/**
 * 動作確認画面のルーティング定義。
 * 画面のモジュールは画面を開いたときに読み込む。
 * 模した列挙体は読み込み時に自動生成された列挙体の選択肢へ登録されるので、動作確認画面を開くまで本処理に影響させないため。
 */
export default [
  {
    path: 詳細画面サンプルURL,
    lazy: async () => ({ Component: (await import("./詳細画面サンプル")).default }),
  },
  {
    path: 一覧検索サンプルURL,
    lazy: async () => ({ Component: (await import("./一覧検索サンプル")).default }),
  },
] satisfies RouteObject[]
