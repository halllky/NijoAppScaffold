import * as Icon from "@heroicons/react/24/solid"
import * as P100Module from "./pages/P100_売上"
import * as P200Module from "./pages/P200_入荷"
import * as P300Module from "./pages/P300_商品"
import * as P400Module from "./pages/P400_従業員"

export type RootNavigationItem = {
  to: string
  label: string
  icon?: React.ElementType
}

/**
 * ルートナビゲーションに表示する画面の一覧
 */
export const navigationItems = [
  { to: P100Module.URL, label: "売上", icon: Icon.CurrencyYenIcon },
  { to: P200Module.URL, label: "入荷", icon: Icon.TruckIcon },
  { to: P300Module.URL, label: "商品", icon: Icon.CubeIcon },
  { to: P400Module.URL, label: "従業員", icon: Icon.UserGroupIcon },
] satisfies RootNavigationItem[]
