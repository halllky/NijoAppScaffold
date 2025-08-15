import React from "react"

/** 既定のレンダリングで用いられるラベル */
export const DynamicFormLabel = ({
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

/** グルーピングの境界線 */
export const DynamicFormSpacer = () => {
  return (
    <div className="col-span-full h-4" />
  )
}