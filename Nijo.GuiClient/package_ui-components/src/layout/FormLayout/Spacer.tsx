import React from "react"
import { RecentParentContext } from "./internal-context"

/**
 * フォーム中で隙間を空けたいときに使う。
 */
export const Spacer = ({ evenIfHorizontal }: {
  /** 横並びの場合でも隙間を表示するかどうか。規定値はfalse（横方向に隙間を空けたいことが少ないため）。 */
  evenIfHorizontal?: boolean
}) => {

  const parent = React.useContext(RecentParentContext)

  // 横並びの場合は非表示
  if (parent === 'responsive-container' && !evenIfHorizontal) return null

  return (
    <div style={{
      gridColumn: '1 / -1',
      height: parent !== 'responsive-container' ? '1rem' : undefined,
      width: parent === 'responsive-container' ? '1rem' : undefined,
      flexShrink: 0,
    }} />
  )
}