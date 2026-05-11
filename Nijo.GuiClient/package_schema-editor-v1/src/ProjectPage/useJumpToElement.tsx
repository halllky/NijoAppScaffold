import React from "react"

/**
 * 特定のXML要素を即座に画面上に表示する関数を提供する関数
 */
export type JumpToElementFunction = (rootOrDescendantXmlElementUniqueId: string | null | undefined) => void

/**
 * 特定のXML要素を即座に画面上に表示する関数を提供するReact Context。
 */
export const JumpToElementContext = React.createContext<JumpToElementFunction | null>(null)
