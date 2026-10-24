import { RootAggregateAppearance } from "../backend"

/** ルート集約の区分ごとの配色。ダイアグラムのエッジの色にも使う */
export const ROOT_AGGREGATE_COLORS: Record<RootAggregateAppearance, {
  /** エッジの線の色 */
  stroke: string
  /** 区分名の文字色のクラス */
  nameText: string
  /** 区分の説明文の文字色のクラス */
  descriptionText: string
  /** ルート集約の箱のクラス */
  rootBox: string
  /** ルート集約のヘッダのクラス */
  rootHeader: string
  /** 子集約の箱のクラス */
  childBox: string
  /** 子集約のヘッダのクラス */
  childHeader: string
}> = {
  stored: {
    stroke: "#ea580c", // orange-600
    nameText: "text-orange-600",
    descriptionText: "text-orange-500",
    rootBox: "border-orange-600 bg-orange-50",
    rootHeader: "bg-orange-600 text-white",
    childBox: "border-orange-300 bg-orange-50",
    childHeader: "bg-orange-100 text-orange-900",
  },
  queryable: {
    stroke: "#059669", // emerald-600
    nameText: "text-emerald-600",
    descriptionText: "text-emerald-500",
    rootBox: "border-emerald-600 bg-emerald-50",
    rootHeader: "bg-emerald-600 text-white",
    childBox: "border-emerald-300 bg-emerald-50",
    childHeader: "bg-emerald-100 text-emerald-900",
  },
  command: {
    stroke: "#0284c7", // sky-600
    nameText: "text-sky-600",
    descriptionText: "text-sky-500",
    rootBox: "border-sky-600 bg-sky-50",
    rootHeader: "bg-sky-600 text-white",
    childBox: "border-sky-300 bg-sky-50",
    childHeader: "bg-sky-100 text-sky-900",
  },
  structure: {
    stroke: "#4b5563", // gray-600
    nameText: "text-gray-600",
    descriptionText: "text-gray-500",
    rootBox: "border-gray-600 bg-gray-50",
    rootHeader: "bg-gray-600 text-white",
    childBox: "border-gray-300 bg-gray-50",
    childHeader: "bg-gray-100 text-gray-900",
  },
}
