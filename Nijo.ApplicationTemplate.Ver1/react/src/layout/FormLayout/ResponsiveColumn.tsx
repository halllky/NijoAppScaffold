import React from "react";
import { IsInColumnContext, FormLayoutContext } from "./internal-context";

/**
 * CSS Grid を使い要素を縦方向に並べる。
 * `Section` の直下に配置する。
 * `ResponsiveColumn` の下には `Item` または `ItemGroupInResponsiveColumn` を配置する。
 */
export const ResponsiveColumn = (props: {
  children?: React.ReactNode
  className?: string
}) => {

  const { labelWidth, valueWidth } = React.useContext(FormLayoutContext)

  return (
    <IsInColumnContext.Provider value={true}>
      <div className={props.className} style={{
        display: 'grid',
        gap: '4px',
        gridTemplateColumns: `${labelWidth}px minmax(${valueWidth}px, 1fr)`,
      }}>
        {props.children}
      </div>
    </IsInColumnContext.Provider>
  )
};
