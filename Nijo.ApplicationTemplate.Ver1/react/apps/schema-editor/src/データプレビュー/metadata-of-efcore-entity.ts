
/** DataModelのメタデータ */
export namespace MetadataOfEFCoreEntity {

  /** 集約 */
  export type Aggregate = {
    type: "root" | "child" | "children"
    /**
     * この集約の物理名のパス。この集約がChild, Children の場合はルート集約からのスラッシュ区切り。
     */
    path: string
    /**
     * 直近の親集約のパス。直近の親がルート集約でない場合はスラッシュ区切り。
     * この集約がルート集約の場合はnull。
     */
    parentAggregatePath: string | null
    physicalName: string
    displayName: string
    tableName: string
    description: string
    members: (AggregateMember | Aggregate)[]

    // 以下はAggregateMemberにしか無いメンバー
    columnName?: never
    typeName?: never
    enumType?: never
    isPrimaryKey?: never
    isNullable?: never
    refToRelationName?: never
    refToAggregatePath?: never
    refToColumnName?: never
  }

  /** 集約のメンバー */
  export type AggregateMember = {
    type: "own-column" | "parent-key" | "ref-key" | "ref-parent-key"
    physicalName: string
    displayName: string
    columnName: string
    description: string
    /**
     * 値メンバーの型名。XMLスキーマ定義上の型名。
     */
    typeName: string
    /**
     * 列挙体種類名。
     * このメンバーが列挙体でない場合はnull。
     */
    enumType: string | null
    isPrimaryKey: boolean
    isNullable: boolean
    /**
     * 外部参照先とこの集約の関係性の名前。
     * テーブルAからBへ複数の参照経路がある場合にそれらの識別に用いる。
     * このメンバーがref-keyでない場合はnull。
     */
    refToRelationName: string | null
    /**
     * 外部参照先テーブルのルート集約からのパス（スラッシュ区切り）。
     * このメンバーがref-keyでない場合はnull。
     */
    refToAggregatePath: string | null
    /**
     * このメンバーと対応する、外部参照先テーブルのメンバーのDB上のカラム名。
     * このメンバーがref-keyでない場合はnull。
     */
    refToColumnName: string | null

    // 以下はAggregateにしか無いメンバー
    path?: never
    parentAggregatePath?: never
    tableName?: never
    members?: never
  }
}
