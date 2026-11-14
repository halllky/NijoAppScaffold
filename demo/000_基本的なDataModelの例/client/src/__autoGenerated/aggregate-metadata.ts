
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : AggregateMetadata.cs
// ※ このファイルを生成したメソッド: RenderTypeScriptType

import * as EnumDefs from "./enum-defs"

/** 集約のメタデータ */
export namespace AggregateMetadata {
  /** 1つのルート集約のメタデータ */
  export type Entry = {
    /** ルート集約自身のメタデータ */
    root: Root
    /** 画面表示用データのメタデータ */
    displayData?: Table
    /** 検索条件の絞り込み条件のメタデータ。キーは絞り込み条件のプロパティ名を除いたパス */
    searchCondition?: Table
  }

  /** ルート集約のメタデータ */
  export type Root = NodeOptions & {
    kind: 'root'
    /** 永続化されるデータ構造かどうか */
    isStored: boolean
    /** 検索可能なデータ構造かどうか */
    isQueryable: boolean
    /** XMLスキーマ上でこの要素を示す一意なID */
    uniqueId: string
    /** ルート集約直下のメンバーの物理名 */
    members: readonly string[]
  }

  /** 構造体の全メンバーのメタデータ。キーはプロパティ名をドットでつないだパス（配列の添字は含まない） */
  export type Table = { [path: string]: Member }

  /** 構造体のメンバーのメタデータ */
  export type Member = Value | Ref | Child | Children | Parent

  /** 値のメタデータ */
  export type Value = MemberBase & {
    kind: 'value'
    /** 値の型名。このメンバーが enum または value-ojbect の場合は undefined になる */
    type?: 'word' | 'description' | 'int' | 'decimal' | 'sequence' | 'datetime' | 'date' | 'yearmonth' | 'year' | 'bool' | 'bytearray'
    /** 列挙体の型名。このメンバーが列挙体の場合のみ */
    enumType?: keyof EnumDefs.EnumTypeMap
    /** 値オブジェクトの型名。このメンバーが値オブジェクトの場合のみ */
    valueObjectType?: never
  }

  /** 外部参照のメタデータ */
  export type Ref = MemberBase & {
    kind: 'ref'
    /** 参照先。参照先が子孫集約の場合はルート集約からのパスのスラッシュ区切り */
    refTo: `ref-to:${string}`
    /** 直下のメンバーのパス */
    members: readonly string[]
  }

  /** 子集約のメタデータ */
  export type Child = MemberBase & {
    kind: 'child'
    /** 直下のメンバーのパス */
    members: readonly string[]
  }

  /** 子配列のメタデータ */
  export type Children = MemberBase & {
    kind: 'children'
    /** 直下のメンバーのパス */
    members: readonly string[]
  }

  /** 子集約を外部参照したときに、参照先から見た親集約を表すメンバーのメタデータ */
  export type Parent = MemberBase & {
    kind: 'parent'
    /** 直下のメンバーのパス */
    members: readonly string[]
  }

  /** 構造体のメンバーのメタデータの共通部分 */
  type MemberBase = NodeOptions & {
    /** 親のメンバーのパス。構造体の直下のメンバーの場合は null */
    parent: string | null
    /**
     * この項目が外部参照の境界を一度でも越えた先にあるかどうか。
     * true の場合、ノードオプションやカスタム属性は参照先の集約で定義されたものである。
     */
    isOutsideTree: boolean
    /** XMLスキーマ上でこの要素を示す一意なID */
    uniqueId: string
  }

  /** XMLスキーマ上で定義されたノードオプションとカスタム属性 */
  type NodeOptions = {
    /** XMLコメント */
    comment?: string
    /** 表示用名称 */
    displayName: string
    /** 表示用名称を空文字にする */
    displayNameIsEmpty?: boolean
    /** データベース上名称 */
    dbName?: string
    /** ラテン語名 */
    latinName?: string
    /** キー */
    isKey?: boolean
    /** NOT NULL */
    isNotNull?: boolean
    /** 論理削除を行う */
    useSoftDelete?: boolean
    /** ユニーク制約 */
    uniqueConstraints?: string
    /** 汎用テーブル */
    isGenericLookupTable?: boolean
    /** ハードコード */
    isHardCodedPrimaryKey?: boolean
    /** カテゴリ */
    category?: string
    /** ビューへマッピング */
    mapToView?: boolean
    /** 検索条件のみに生成 */
    onlySearchCondition?: boolean
    /** 参照先オブジェクト */
    refToObject?: string
    /** 最大長 */
    maxLength?: number
    /** 総合桁数 */
    totalDigit?: number
    /** 小数部桁数 */
    decimalPlace?: number
    /** シーケンス名 */
    sequenceName?: string
    /** 検索挙動 */
    stringSearchBehavior?: string
  }
}
