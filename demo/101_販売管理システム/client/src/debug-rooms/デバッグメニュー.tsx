import { Link } from "react-router-dom"
import * as UIコンポーネントカタログ from "./UIコンポーネントカタログ"

export default function デバッグメニュー() {
  // 開発環境でのみ表示
  if (!import.meta.env.DEV) return null

  return (
    <div className="flex flex-col gap-2 p-2 border border-gray-300">
      <h2 className="text-lg font-bold">デバッグメニュー</h2>
      <span className="text-sm text-gray-500">
        開発環境でのみ表示されるデバッグ用メニューです。
      </span>
      <hr className="border-gray-300" />
      <div className="space-y-2">
        <Link to={UIコンポーネントカタログ.URL} className="text-blue-600 underline">
          UIコンポーネントカタログへ移動
        </Link>
      </div>
    </div>
  )
}
