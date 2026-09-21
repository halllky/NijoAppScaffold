import * as React from "react"
import * as ReactRouter from "react-router-dom"
import FormLayoutPatternsDebugging from "./FormLayoutPatternsDebugging"
import GraphView2Debugging from "./GraphView2Debugging"
import GraphView3Debugging from "./GraphView3Debugging"
import ReactContextDebugging from "./ReactContextDebugging"

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
    groupName: '刷新後',
    links: [
      {
        path: '/graph-view-2/001',
        label: 'GraphView2 基本機能',
        element: <GraphView2Debugging />,
      },
      {
        path: '/graph-view-3/001',
        label: 'GraphView3 (React Flow) 基本機能',
        element: <GraphView3Debugging />,
      },
      {
        path: '/react-context/001',
        label: 'React Context レンダリング調査',
        element: <ReactContextDebugging />,
      }
    ]
  },
  {
    groupName: 'FormLayoutのデバッグ',
    links: [
      {
        path: '/form-layout/001',
        label: 'FormLayout レイアウト構造パターン集',
        element: <FormLayoutPatternsDebugging />,
      }
    ],
  },
]
