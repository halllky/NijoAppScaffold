import type { ModelKind } from "./aggregateTree"

/** モデルの種類ごとの配色。エッジの色にも使う */
export const MODEL_COLORS: { [key in ModelKind]: {
  /** エッジの線の色 */
  stroke: string
  /** ルート集約の箱のクラス */
  rootBox: string
  /** ルート集約のヘッダのクラス */
  rootHeader: string
  /** 子集約の箱のクラス */
  childBox: string
  /** 子集約のヘッダのクラス */
  childHeader: string
} } = {
  "write": {
    stroke: "#ec003f", // rose-600
    rootBox: "border-rose-600 bg-rose-50",
    rootHeader: "bg-rose-600 text-white",
    childBox: "border-rose-300 bg-rose-50",
    childHeader: "bg-rose-100 text-rose-900",
  },
  "write-read": {
    stroke: "#ec003f", // rose-600
    rootBox: "border-rose-600 bg-rose-50",
    rootHeader: "bg-rose-600 text-white",
    childBox: "border-rose-300 bg-rose-50",
    childHeader: "bg-rose-100 text-rose-900",
  },
  "read": {
    stroke: "#059669", // green-600
    rootBox: "border-green-600 bg-green-50",
    rootHeader: "bg-green-600 text-white",
    childBox: "border-green-300 bg-green-50",
    childHeader: "bg-green-100 text-green-900",
  },
  "command": {
    stroke: "#0084d1", // sky-600
    rootBox: "border-sky-600 bg-sky-50",
    rootHeader: "bg-sky-600 text-white",
    childBox: "border-sky-300 bg-sky-50",
    childHeader: "bg-sky-100 text-sky-900",
  },
}
