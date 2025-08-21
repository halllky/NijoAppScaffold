import React from "react"

/** 既定のレンダリングで用いられるラベル */
export const Label = ({
  children,
  className,
  ...rest
}: React.HTMLAttributes<HTMLSpanElement>) => {

  return (
    <span className={`text-sm text-gray-500 select-none ${className ?? ''}`} {...rest}>
      {children}

      {/* 高さ調整用のゼロ幅スペース */}
      <span className="text-base">
        {"\u200b"}
      </span>
    </span>
  )
}