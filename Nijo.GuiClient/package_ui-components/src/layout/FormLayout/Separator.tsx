import React from "react"
import { RecentParentContext } from "./internal-context"

/** 区切りライン */
export const Separator = () => {

  const parent = React.useContext(RecentParentContext)

  return parent !== 'responsive-container' ? (
    // 縦方向並べのときの水平線
    <div style={{
      gridColumn: '1 / -1',
      height: '1.5rem',
      flexShrink: 0,
      display: 'flex',
      alignItems: 'center',
    }}>
      <hr style={{
        flexGrow: 1,
        borderTop: '1px solid #ccc',
      }} />
    </div>
  ) : (
    // 横方向並べのときの垂直線
    <div style={{
      alignSelf: 'stretch',
      flexShrink: 0,
      borderLeft: '1px solid #ccc',
      marginLeft: '4px',
      marginRight: '4px',
    }} />
  )
}