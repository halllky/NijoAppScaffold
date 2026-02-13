import React from "react"

/**
 * タブヘッダのコンポーネント
 */
export function TabHeader(props: {
  isSelected: boolean
  isAppTitle?: boolean
  onClick?: () => void
  children?: React.ReactNode
}) {

  let className = "z-0 self-stretch px-2 border-t border-x rounded-t-sm cursor-pointer select-none"

  if (props.isSelected) {
    className += " mb-[-5px] border-gray-400 bg-white font-bold"
  } else if (props.isAppTitle) {
    className += " mb-[-4px] border-transparent"
  } else {
    className += " mb-[-4px] border-gray-300 bg-gray-100 hover:bg-gray-200 text-gray-600"
  }

  if (props.isAppTitle) {
    className += " text-base font-bold flex items-center gap-1"
  } else {
    className += " text-xs pt-2"
  }

  return (
    <div className={className} onClick={props.onClick}>
      {props.children}
    </div>
  )
}
