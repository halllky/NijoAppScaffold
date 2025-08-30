import React from "react"

export type LabelProps = {
  /** ラベルのテキスト */
  children?: React.ReactNode
  /** ラベルのクラス名 */
  className?: string
}

/** 既定のレンダリングで用いられるラベル */
export const DefaultLabel = ({ children, className }: LabelProps) => {

  return (
    <span className={className} style={{
      color: '#575757',
      userSelect: 'none',
    }}>
      {children}

      {/* 高さ調整用のゼロ幅スペース */}
      <span style={{ fontSize: '1rem' }}>
        {"\u200b"}
      </span>
    </span>
  )
}