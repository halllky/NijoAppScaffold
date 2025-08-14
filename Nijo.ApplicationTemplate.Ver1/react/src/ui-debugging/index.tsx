import * as React from "react"
import * as ReactRouter from "react-router-dom"
import DynamicFormDebugging from "./DynamicFormDebugging"
import _001_セクションと配列の基本的組み合わせ from "../layout/DynamicForm/__tests__/001_セクションと配列の基本的組み合わせ"

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
    groupName: 'DynamicFormのデバッグ',
    links: [
      {
        path: '/dynamic-form/001',
        label: '001_セクションと配列の基本的組み合わせ',
        element: <DynamicFormDebugging getSchema={_001_セクションと配列の基本的組み合わせ} />,
      },
    ],
  }
]