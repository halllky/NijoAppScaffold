import { createBrowserRouter } from "react-router-dom"
import { RootLayout } from "./layout/RootLayout"
import P000 from "./pages/P000_トップページ"
import P100 from "./pages/P100_売上"
import P200 from "./pages/P200_入荷"
import P300 from "./pages/P300_商品"
import P400 from "./pages/P400_従業員"
import P101 from "./pages/P101_売上詳細"
import P201 from "./pages/P201_入荷詳細"
import P301 from "./pages/P301_商品詳細"
import UIComponentCatalog from "./debug-rooms/UIコンポーネントカタログ"

export const router = createBrowserRouter([
  {
    element: <RootLayout />,
    children: [

      // 業務画面
      P000,
      P100,
      P200,
      P300,
      P400,
      ...P101,
      ...P201,
      P301,

      // デバッグ用画面（開発環境でのみ表示）
      ...(!import.meta.env.DEV ? [] : [
        UIComponentCatalog,
      ]),
    ],
  },
])
