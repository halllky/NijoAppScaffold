import { UUID } from "uuidjs"
import type { DbSchema } from "./DbSchema"

/**
 * DBビューアの画面の設定。
 * サーバーに JSON のまま保存され、ソースコードと一緒にバージョン管理される。
 * サーバーは中身を解釈しないため、構造はこの型だけが定義する。
 */
export type DbViewerSettings = {
  /**
   * DB定義の全テーブルを表示するサブジェクトエリア。常に1個存在する。
   * 一般のサブジェクトエリアと異なり、削除・改名・表示するテーブルの選択はできない。
   */
  allTablesArea: SubjectArea
  /** 利用者が作成したサブジェクトエリア */
  subjectAreas: SubjectArea[]
}

/**
 * サブジェクトエリア。
 * 全テーブルのうち、ある観点で関心のあるテーブルだけを選んで表示するER図1枚分。
 */
export type SubjectArea = {
  id: string
  name: string
  /** テーブルのノードの見出しに表示する属性。配列の順に表示される */
  tableAttributes: TableAttribute[]
  /** テーブルのノードのカラム一覧に表示する属性。配列の順に列として表示される */
  columnAttributes: ColumnAttribute[]
  /** このサブジェクトエリアに表示するよう選択されたテーブル */
  tables: SubjectAreaTable[]
  /** このサブジェクトエリア上に開かれているデータプレビュー */
  dataPreviews: DataPreview[]
  /** 最後に表示していた表示範囲。未保存の場合は全体が収まるよう表示される */
  viewport?: { x: number, y: number, zoom: number }
}

/** サブジェクトエリアに表示するよう選択されたテーブル1個 */
export type SubjectAreaTable = {
  /** テーブルの物理名 */
  tableName: string
  position: XY
  /** 未保存の場合はカラム数に応じた既定の大きさ */
  size?: Size
  /** 折り畳まれている場合はカラム一覧を表示しない */
  collapsed: boolean
  /**
   * このテーブルに直接つながる未選択のテーブルのノードの位置。
   * キーはつながるテーブルの物理名、値はこのテーブルのノードからの相対位置。
   * 未保存の隣接テーブルは自動で配置される。
   */
  neighborPositions?: { [tableName: string]: XY }
}

/** テーブルの中身を表示するフローティングウィンドウ1個 */
export type DataPreview = {
  id: string
  /** テーブルの物理名 */
  tableName: string
  /** WHERE句の条件部分（WHERE キーワードは含まない）。空なら絞り込まない */
  where: string
  /** ORDER BY句の並び順部分（ORDER BY キーワードは含まない）。空なら並び順を指定しない */
  orderBy: string
  /** 1ページあたりの件数 */
  pageSize: number
  position: XY
  /** 未保存の場合は既定の大きさ */
  size?: Size
  /** 折り畳まれている場合は WHERE, ORDER BY の入力欄を表示しない */
  collapsed: boolean
}

export type XY = { x: number, y: number }
export type Size = { width: number, height: number }

/** テーブルのノードの見出しに表示できる属性 */
export type TableAttribute = "logicalName" | "physicalName" | "comment"
/** テーブルのノードのカラム一覧に表示できる属性 */
export type ColumnAttribute = "isPrimaryKey" | "logicalName" | "physicalName" | "type" | "isNotNull" | "isUnique" | "comment"

/** テーブルの属性の表示名。設定画面での並び順もこの順 */
export const TABLE_ATTRIBUTE_LABELS: { [key in TableAttribute]: string } = {
  logicalName: "論理名",
  physicalName: "物理名",
  comment: "コメント",
}
/** カラムの属性の表示名。設定画面での並び順もこの順 */
export const COLUMN_ATTRIBUTE_LABELS: { [key in ColumnAttribute]: string } = {
  isPrimaryKey: "PK",
  logicalName: "論理名",
  physicalName: "物理名",
  type: "型",
  isNotNull: "NOT NULL",
  isUnique: "ユニーク",
  comment: "コメント",
}

/** データプレビューの1ページあたりの件数の既定値 */
export const DEFAULT_PAGE_SIZE = 50

/**
 * 保存されていた設定を読み込む。
 * 手で編集されたり古い形式のまま残っていたりして欠けている項目は既定値で補う。
 * 保存されていない場合は空の設定を返す。
 */
export function parseSettings(saved: unknown): DbViewerSettings {
  const raw = (saved ?? {}) as Partial<DbViewerSettings>
  return {
    // 全テーブルのサブジェクトエリアの ID と名前は固定。JSON が手で書き換えられていても元に戻す
    allTablesArea: { ...parseArea(raw.allTablesArea ?? {}), id: ALL_TABLES_AREA_ID, name: ALL_TABLES_AREA_NAME },
    subjectAreas: (raw.subjectAreas ?? []).map(parseArea),
  }
}

/** 全テーブルのサブジェクトエリアの ID */
export const ALL_TABLES_AREA_ID = "all-tables"
/** 全テーブルのサブジェクトエリアの名前 */
const ALL_TABLES_AREA_NAME = "全てのテーブル"

/** 全テーブルのサブジェクトエリアかどうか */
export function isAllTablesArea(area: SubjectArea): boolean {
  return area.id === ALL_TABLES_AREA_ID
}

/** 指定の ID のサブジェクトエリア。全テーブルのサブジェクトエリアも対象。無い場合は undefined */
export function findArea(settings: DbViewerSettings, areaId: string): SubjectArea | undefined {
  return areaId === ALL_TABLES_AREA_ID
    ? settings.allTablesArea
    : settings.subjectAreas.find(area => area.id === areaId)
}

/**
 * 指定の ID のサブジェクトエリアを変更した設定を返す。全テーブルのサブジェクトエリアも対象。
 * 該当するサブジェクトエリアが無い場合は何もしない。
 */
export function updateArea(settings: DbViewerSettings, areaId: string, updater: (area: SubjectArea) => SubjectArea): DbViewerSettings {
  if (areaId === ALL_TABLES_AREA_ID) {
    return { ...settings, allTablesArea: updater(settings.allTablesArea) }
  }
  return { ...settings, subjectAreas: settings.subjectAreas.map(area => area.id === areaId ? updater(area) : area) }
}

/**
 * 全テーブルのサブジェクトエリアに、DB定義の全テーブルが揃うよう、まだ配置が保存されていないテーブルを補ったものを返す。
 * 補ったテーブルは自動配置される。補うものが無い場合は引数のサブジェクトエリアをそのまま返す。
 */
export function withAllTables(area: SubjectArea, schema: DbSchema): SubjectArea {
  return schema.tables.reduce((acc, table) => addTableToArea(acc, table.tableName), area)
}

/** JSON から読み込んだサブジェクトエリア1個の欠けている項目を既定値で補う */
function parseArea(area: Partial<SubjectArea>): SubjectArea {
  return {
    ...area,
    id: area.id ?? UUID.generate(),
    name: area.name ?? "",
    tableAttributes: area.tableAttributes ?? DEFAULT_TABLE_ATTRIBUTES,
    columnAttributes: area.columnAttributes ?? DEFAULT_COLUMN_ATTRIBUTES,
    tables: (area.tables ?? []).map(table => ({
      ...table,
      position: table.position ?? { x: 0, y: 0 },
      collapsed: table.collapsed ?? false,
    })),
    dataPreviews: (area.dataPreviews ?? []).map(preview => ({
      ...preview,
      where: preview.where ?? "",
      orderBy: preview.orderBy ?? "",
      pageSize: preview.pageSize ?? DEFAULT_PAGE_SIZE,
      position: preview.position ?? { x: 0, y: 0 },
      collapsed: preview.collapsed ?? false,
    })),
  }
}

/** 新しい空のサブジェクトエリアを作る */
export function createSubjectArea(name: string): SubjectArea {
  return {
    id: UUID.generate(),
    name,
    tableAttributes: DEFAULT_TABLE_ATTRIBUTES,
    columnAttributes: DEFAULT_COLUMN_ATTRIBUTES,
    tables: [],
    dataPreviews: [],
  }
}

/** 新しいデータプレビューを作る。条件は空 */
export function createDataPreview(tableName: string, position: XY): DataPreview {
  return {
    id: UUID.generate(),
    tableName,
    where: "",
    orderBy: "",
    pageSize: DEFAULT_PAGE_SIZE,
    position,
    collapsed: false,
  }
}

/**
 * サブジェクトエリアにテーブルを追加する。既に含まれている場合は何もしない。
 * 追加したテーブルは、既存のテーブルと重なりにくいよう、追加した順に格子状に並ぶ位置に置かれる。
 */
export function addTableToArea(area: SubjectArea, tableName: string): SubjectArea {
  if (area.tables.some(table => table.tableName === tableName)) return area
  const index = area.tables.length
  const position = {
    x: (index % AUTO_PLACEMENT_COLUMNS) * AUTO_PLACEMENT_STEP.x,
    y: Math.floor(index / AUTO_PLACEMENT_COLUMNS) * AUTO_PLACEMENT_STEP.y,
  }
  return { ...area, tables: [...area.tables, { tableName, position, collapsed: false }] }
}

/**
 * サブジェクトエリアからテーブルを取り除く。
 * そのテーブルのデータプレビューは、テーブルとは独立して開いたものであるため残す。
 */
export function removeTableFromArea(area: SubjectArea, tableName: string): SubjectArea {
  return { ...area, tables: area.tables.filter(table => table.tableName !== tableName) }
}

/**
 * サブジェクトエリアの設定に保存されているテーブル名のうち、DB定義に存在しないもの。
 * テーブルの改名・削除の後に設定が残っている場合に発生する。重複は除く。
 */
export function findMissingTableNames(area: SubjectArea, schema: DbSchema): string[] {
  const existing = new Set(schema.tables.map(table => table.tableName))
  const referenced = [
    ...area.tables.map(table => table.tableName),
    ...area.dataPreviews.map(preview => preview.tableName),
  ]
  return Array.from(new Set(referenced.filter(tableName => !existing.has(tableName))))
}

/** テーブルを追加したときに自動で並べる格子の列数と間隔 */
const AUTO_PLACEMENT_COLUMNS = 4
const AUTO_PLACEMENT_STEP = { x: 800, y: 560 }
/** 既定で表示するテーブルの属性 */
const DEFAULT_TABLE_ATTRIBUTES: TableAttribute[] = ["logicalName", "physicalName"]
/** 既定で表示するカラムの属性 */
const DEFAULT_COLUMN_ATTRIBUTES: ColumnAttribute[] = ["isPrimaryKey", "logicalName", "physicalName", "type", "isNotNull"]
