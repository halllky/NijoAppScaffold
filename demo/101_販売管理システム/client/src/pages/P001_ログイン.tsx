import * as ReactRouter from "react-router-dom"
import { PageTitle } from "../ui"
import { useLoginUser } from "../login/loginUser"
import { getReturnTo, LOGIN_URL } from "../login/loginUrl"
import { LoginForm } from "../login/LoginForm"

/**
 * ルーティング定義。
 *
 * default export していないのは、ログインしていなくても開けるよう、
 * 業務画面のようなワイルドカードでの収集ではなく明示的登録にするため。
 */
export const loginRoute = {
  path: LOGIN_URL,
  element: <P001_ログイン />,
} satisfies ReactRouter.RouteObject

/**
 * P001 ログイン。
 * ログインしたら、ログイン画面に来る前に開こうとしていた画面（URL のクエリパラメータ returnTo）に移る。
 * ログイン状態を画面全体に反映するため、画面の移動はページの読み込み直しで行う。
 */
function P001_ログイン() {
  const loginUser = useLoginUser()
  const [searchParams] = ReactRouter.useSearchParams()
  const returnTo = getReturnTo(searchParams)

  // ログイン済みならログイン画面は不要
  if (loginUser) return <ReactRouter.Navigate to={returnTo} replace />

  return (
    <div className="min-h-full flex items-center justify-center bg-gray-100">
      <div className="bg-white p-8 rounded shadow-md w-full max-w-md">
        <div className="mb-6">
          <PageTitle>ログイン</PageTitle>
        </div>
        {/* 従業員番号の初期値はデモ用の管理者アカウント */}
        <LoginForm defaultUserId="admin" onLoggedIn={() => window.location.assign(returnTo)} />
      </div>
    </div>
  )
}
