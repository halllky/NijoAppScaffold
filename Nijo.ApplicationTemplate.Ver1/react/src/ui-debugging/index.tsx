import * as React from "react"
import * as ReactRouter from "react-router-dom"
import DynamicFormDebugging from "./DynamicFormDebugging"
import _001_セクションと配列の基本的組み合わせ from "../layout/DynamicForm/__tests__/001_セクションと配列の基本的組み合わせ"
import _002_値メンバーの各種型 from "../layout/DynamicForm/__tests__/002_値メンバーの各種型"
import _003_カスタムレンダリング from "../layout/DynamicForm/__tests__/003_カスタムレンダリング"
import _004_カスタムレンダリング2 from "../layout/DynamicForm/__tests__/004_カスタムレンダリング2"
import _005_複雑なネスト構造 from "../layout/DynamicForm/__tests__/005_複雑なネスト構造"
import _006_空データ構造 from "../layout/DynamicForm/__tests__/006_空データ構造"
import _007_グリッド機能 from "../layout/DynamicForm/__tests__/007_グリッド機能"
import ResponsiveForm実験 from "./ResponsiveForm実験"
import ResponsiveFormTest_01_基本形 from "./ResponsiveFormTest_01_基本形"
import FormLayoutComponentsTest from "./FormLayoutComponentsTest"
import FormLayoutComponentsTest_Extended from "./FormLayoutComponentsTest_Extended"

export default function () {

  const pathname = ReactRouter.useLocation().pathname
  const isHereIndex = React.useMemo(() => {
    return pathname === '/'
  }, [pathname])

  const currentPage = React.useMemo(() => {
    return getDebuggingPages()
      .flatMap(group => group.links)
      .find(link => link.path === pathname)
  }, [pathname])

  const menuItems = React.useMemo(() => {
    return getDebuggingPages()
  }, [])

  return (
    <div className="w-full h-full flex flex-col gap-2 p-4">

      <h1 className="text-xl font-bold flex items-center gap-2">
        <ReactRouter.Link to="/">
          UIデバッグ画面
        </ReactRouter.Link>
        {currentPage && (<>
          <span className="text-gray-500">&gt;</span>
          <span className="">{currentPage.label}</span>
        </>)}
      </h1>

      <hr className="border-t border-gray-300" />

      {isHereIndex && menuItems.map((group, index) => (
        <div key={index} className="flex flex-col gap-2">

          <h2 className="font-bold">{group.groupName}</h2>

          <div className="flex flex-col gap-2 pl-4">
            {group.links.map((link, index) => (
              <ReactRouter.Link key={index} to={link.path ?? ''} className="text-sky-600 underline">
                {link.label}
              </ReactRouter.Link>
            ))}
          </div>
        </div>
      ))}

      <ReactRouter.Outlet />
    </div>
  )
}

// -------------------------------------

export const getDebuggingPages = (): { groupName: string, links: (ReactRouter.RouteObject & { label: string })[] }[] => [
  {
    groupName: 'ResponsiveFormのデバッグ',
    links: [
      {
        path: '/responsive-form/000',
        label: 'どういうdivがレンダリングされればいいかの検討の残骸',
        element: <ResponsiveForm実験 />,
      },
      {
        path: '/responsive-form/001',
        label: 'ResponsiveFormの基本形',
        element: <ResponsiveFormTest_01_基本形 />,
      },
      {
        path: '/responsive-form/002',
        label: 'FormLayoutコンポーネントのパターン網羅テスト',
        element: <FormLayoutComponentsTest />,
      },
      {
        path: '/responsive-form/003',
        label: 'FormLayoutコンポーネントのパターン網羅テスト（拡張）',
        element: <FormLayoutComponentsTest_Extended />,
      }
    ],
  },
  {
    groupName: 'DynamicFormのデバッグ',
    links: [
      {
        path: '/dynamic-form/001',
        label: '001_セクションと配列の基本的組み合わせ',
        element: <DynamicFormDebugging getSchema={_001_セクションと配列の基本的組み合わせ} />,
      },
      {
        path: '/dynamic-form/002',
        label: '002_値メンバーの各種型',
        element: <DynamicFormDebugging getSchema={_002_値メンバーの各種型} />,
      },
      {
        path: '/dynamic-form/003',
        label: '003_カスタムレンダリング',
        element: <DynamicFormDebugging getSchema={_003_カスタムレンダリング} />,
      },
      {
        path: '/dynamic-form/004',
        label: '004_カスタムレンダリング2',
        element: <DynamicFormDebugging getSchema={_004_カスタムレンダリング2} />,
      },
      {
        path: '/dynamic-form/005',
        label: '005_複雑なネスト構造',
        element: <DynamicFormDebugging getSchema={_005_複雑なネスト構造} />,
      },
      {
        path: '/dynamic-form/006',
        label: '006_空データ構造',
        element: <DynamicFormDebugging getSchema={_006_空データ構造} />,
      },
      {
        path: '/dynamic-form/007',
        label: '007_グリッド機能',
        element: <DynamicFormDebugging getSchema={_007_グリッド機能} />,
      },
    ],
  }
]
