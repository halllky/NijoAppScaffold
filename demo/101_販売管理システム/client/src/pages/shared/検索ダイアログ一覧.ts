import { 従業員検索ダイアログ } from "./従業員検索ダイアログ"
import { 商品検索ダイアログ } from "./商品検索ダイアログ"

/**
 * 外部参照先と検索ダイアログの対応表。
 * 画面のルートに置いた SearchDialogHost に渡す。
 */
export const 検索ダイアログ一覧 = {
  'ref-to:従業員Ref': 従業員検索ダイアログ,
  'ref-to:商品': 商品検索ダイアログ,
}

// 外部参照の入力欄が、項目の name から検索ダイアログとそのパラメータの型を決められるよう、対応表の型を登録する
type 検索ダイアログ一覧 = typeof 検索ダイアログ一覧
declare module "../../ui/search-dialog/SearchDialogRegistry" {
  interface SearchDialogRegistry extends 検索ダイアログ一覧 { }
}
