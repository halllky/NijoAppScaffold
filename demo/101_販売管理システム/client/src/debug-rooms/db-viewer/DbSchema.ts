/**
 * サーバーから取得する、データベースのテーブル定義の一覧。
 * 定義は EF Core のモデルから読み取られたものであり、この画面からは編集できない。
 */
export type DbSchema = {
  tables: DbSchemaTable[]
}

/** テーブルまたはビュー1個分の定義 */
export type DbSchemaTable = {
  /** 物理名。テーブルを一意に識別するキーとしても使われる */
  tableName: string
  logicalName: string
  comment: string | null
  isView: boolean
  columns: DbSchemaColumn[]
  /** このテーブルが依存側となる外部キー */
  foreignKeys: DbSchemaForeignKey[]
}

/** カラム1個分の定義 */
export type DbSchemaColumn = {
  physicalName: string
  logicalName: string
  type: string
  isPrimaryKey: boolean
  isNotNull: boolean
  isUnique: boolean
  comment: string | null
}

/** 外部キー1個分の定義。依存側のテーブルは、この外部キーを持つテーブル自身 */
export type DbSchemaForeignKey = {
  /** 主側のテーブルの物理名 */
  principalTable: string
  /** 依存側のテーブルにある、外部キーを構成するカラムの物理名 */
  columns: string[]
}

/** 2つのテーブルの間の、外部キーによる関連。自己参照は含まない */
export type TableRelation = {
  /** 依存側（外部キーを持つ側）のテーブルの物理名 */
  dependent: string
  /** 主側（参照される側）のテーブルの物理名 */
  principal: string
}

/**
 * スキーマに含まれる全テーブル間の関連を列挙する。
 * 同じテーブルの組の同じ向きの関連は、外部キーが複数あっても1個にまとめる。
 */
export function listRelations(schema: DbSchema): TableRelation[] {
  const relations = new Map<string, TableRelation>()
  for (const table of schema.tables) {
    for (const fk of table.foreignKeys) {
      if (fk.principalTable === table.tableName) continue
      const key = `${table.tableName}\u0000${fk.principalTable}`
      if (!relations.has(key)) relations.set(key, { dependent: table.tableName, principal: fk.principalTable })
    }
  }
  return Array.from(relations.values())
}

/** 指定のテーブルに外部キーで直接つながるテーブルの物理名。参照する側・される側の両方を含む */
export function listNeighbors(relations: TableRelation[], tableName: string): string[] {
  const neighbors = new Set<string>()
  for (const { dependent, principal } of relations) {
    if (dependent === tableName) neighbors.add(principal)
    if (principal === tableName) neighbors.add(dependent)
  }
  return Array.from(neighbors)
}

/**
 * テーブル名の検索キーワードに合致するかどうか。
 * 論理名・物理名のいずれかに、大文字小文字を区別せず部分一致すれば合致とみなす。
 * キーワードが空なら常に合致する。
 */
export function matchesTableKeyword(table: DbSchemaTable, keyword: string): boolean {
  const normalized = keyword.trim().toLowerCase()
  if (normalized === "") return true
  return table.tableName.toLowerCase().includes(normalized)
    || table.logicalName.toLowerCase().includes(normalized)
}
