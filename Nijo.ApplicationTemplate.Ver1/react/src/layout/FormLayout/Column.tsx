import React from "react";
import { IsInColumnContext, FormLayoutContext } from "./ResponsiveFormContext";

/**
 * CSS Grid を使い要素を縦方向に並べる。
 * `Section` の直下に配置する。
 * `Column` の下には `Item` または `ItemGroup` を配置する。
 */
export const Column = (props: {
  children?: React.ReactNode
  className?: string
}) => {

  const { labelWidth, valueWidth } = React.useContext(FormLayoutContext)

  return (
    <IsInColumnContext.Provider value={true}>
      <div className={`grid gap-1 ${props.className ?? ''}`} style={{
        gridTemplateColumns: `${labelWidth}px minmax(${valueWidth}px, 1fr)`,
      }}>
        {props.children}
      </div>
    </IsInColumnContext.Provider>
  )
};
