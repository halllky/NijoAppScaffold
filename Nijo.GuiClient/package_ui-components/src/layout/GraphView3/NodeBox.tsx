import React from "react"
import { Handle, Position, type NodeProps } from "@xyflow/react"
import { DEFAULT_NODE_STYLE, PARENT_HEADER_HEIGHT, PARENT_PADDING, type FlowNode } from "./types-internal"

/**
 * ノード1個の描画。
 * 子ノードは React Flow によってこのノードに重ねて描画されるため、
 * 子ノードを持つ場合はラベルを上端に寄せ、内側を空けておく。
 */
export const NodeBox = ({ data, selected }: NodeProps<FlowNode>) => {
  const { label, className, style, hasChildren } = data

  const borderColor = selected
    ? style?.borderColorSelected ?? DEFAULT_NODE_STYLE.borderColorSelected
    : style?.borderColor ?? DEFAULT_NODE_STYLE.borderColor

  const boxStyle: React.CSSProperties = {
    boxSizing: "border-box",
    // 子ノードを持つノードは React Flow から与えられた大きさいっぱいに広がる
    width: hasChildren ? "100%" : style?.width,
    height: hasChildren ? "100%" : style?.height,
    color: style?.color ?? DEFAULT_NODE_STYLE.color,
    backgroundColor: style?.backgroundColor
      ?? (hasChildren ? DEFAULT_NODE_STYLE.backgroundColorAsParent : DEFAULT_NODE_STYLE.backgroundColor),
    borderColor,
    // 選択中であることが分かるよう、選択中は枠線を少しだけ太くする
    borderWidth: (style?.borderWidth ?? DEFAULT_NODE_STYLE.borderWidth) + (selected ? 1 : 0),
    borderStyle: style?.borderStyle ?? DEFAULT_NODE_STYLE.borderStyle,
    borderRadius: style?.borderRadius ?? DEFAULT_NODE_STYLE.borderRadius,
    fontSize: style?.fontSize ?? DEFAULT_NODE_STYLE.fontSize,
    display: "flex",
    flexDirection: "column",
    alignItems: hasChildren ? "flex-start" : "center",
    justifyContent: hasChildren ? "flex-start" : "center",
    padding: hasChildren ? `2px ${PARENT_PADDING}px` : "6px 12px",
    minWidth: hasChildren ? undefined : 72,
    lineHeight: 1.3,
    whiteSpace: "pre-wrap",
    textAlign: hasChildren ? "left" : "center",
    overflow: "hidden",
  }

  return (
    <div className={className} style={boxStyle}>

      {/*
        エッジの接続点。
        エッジはノードの外周のどこに接続するかを自前で算出するため、接続点そのものは見えないようにしている。
        ただし React Flow は接続点が1つも無いノードに繋がるエッジを描画しないので、置くこと自体は必要。
      */}
      <Handle type="target" position={Position.Top} style={INVISIBLE_HANDLE_STYLE} isConnectable={false} />
      <Handle type="source" position={Position.Top} style={INVISIBLE_HANDLE_STYLE} isConnectable={false} />

      {/* ラベル。子ノードを持つ場合は子ノードと重ならないよう上端の帯の中に表示する。 */}
      <div style={hasChildren ? { height: PARENT_HEADER_HEIGHT, flexShrink: 0 } : undefined}>
        {label}
      </div>
    </div>
  )
}

// -------------------------------------

/** 見えない接続点のスタイル */
const INVISIBLE_HANDLE_STYLE: React.CSSProperties = {
  width: 1,
  height: 1,
  minWidth: 1,
  minHeight: 1,
  opacity: 0,
  border: "none",
  background: "transparent",
  pointerEvents: "none",
}
