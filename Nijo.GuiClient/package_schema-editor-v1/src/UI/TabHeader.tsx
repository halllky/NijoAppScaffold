import React from "react"

/**
 * タブヘッダのコンポーネント
 */
export function TabHeader(props: {
  isSelected: boolean
  onClick?: () => void
  children?: React.ReactNode
}) {

  let className = "z-0 self-stretch pt-2 px-2 text-xs border-t border-x rounded-t-sm cursor-pointer select-none"

  if (props.isSelected) {
    className += " mb-[-5px] border-gray-400 bg-white font-bold"
  } else {
    className += " mb-[-4px] border-gray-300 bg-gray-100 hover:bg-gray-200 text-gray-600"
  }

  return (
    <div className={className} onClick={props.onClick}>
      {props.children}
    </div>
  )
}
