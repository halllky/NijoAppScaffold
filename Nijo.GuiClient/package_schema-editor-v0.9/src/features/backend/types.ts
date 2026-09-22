// バックエンド（nijo-old の Runtime/NijoUi.cs）との通信処理で使われるデータの型定義。
// このファイルで定義している型のプロパティ名は NijoUi.cs の JsonPropertyName 等と合わせる必要がある。

/** データを伴って成功しうる呼び出しの結果。 */
export type LoadResult<T> =
  | { ok: true; value: T }
  | { ok: false; error?: string }

/** 保存処理の結果。バリデーションエラーで保存できなかった場合はそのエラー内容を伴う。 */
export type SaveResult =
  | { ok: true }
  | { ok: false; error?: string; validationErrors?: ValidationErrorMap }

// ---------------------------------
// データ構造

/** nijo.xml のルート要素の設定値（C# の Nijo.Core.Config）。プロパティ名はC#側のものがそのまま使われる。 */
export type Config = {
  RootNamespace: string
  GenerateUnusedRefToModules: boolean
  /** 読み取り専用（RootNamespace と同じ値） */
  EntityNamespace?: string
  /** 読み取り専用（RootNamespace と同じ値） */
  DbContextNamespace?: string
  DbContextName: string
  CreateUserDbColumnName: string | null
  UpdateUserDbColumnName: string | null
  CreatedAtDbColumnName: string | null
  UpdatedAtDbColumnName: string | null
  VersionDbColumnName: string | null
  MaxFileSizeMB: number | null
  MaxTotalFileSizeMB: number | null
  AttachmentFileExtensions: string | null
  DisableLocalRepository: boolean
  UseBatchUpdateVersion2: boolean
  ButtonColor: string | null
  UseWijmo: boolean
  VFormRefItemIsNotWide: boolean
  MultiViewDetailLinkBehavior: 'navigateToReadOnlyMode' | 'navigateToEditMode'
  VFormMaxColumnCount: number | null
  VFormMaxMemberCount: number | null
  VFormThreshold: number | null
}

/** 集約または集約メンバー。XMLのノードと1対1対応する（C# の MutableSchemaNode）。 */
export type SchemaNode = {
  depth: number
  uniqueId: string
  displayName: string
  type?: string | null
  typeDetail?: string | null
  attrValues?: OptionalAttributeValue[] | null
  comment?: string | null
  xmlFileFullPath?: string | null
}

/** オプショナル属性の値 */
export type OptionalAttributeValue = {
  key?: string | null
  value?: string | null
}

/** 集約やメンバーの種類の定義 */
export type SchemaNodeTypeDef = {
  key: string
  displayName?: string | null
  helpText?: string | null
  requiredNumberValue?: boolean | null
}

/** オプショナル属性の定義 */
export type OptionalAttributeDef = {
  key: string
  displayName?: string | null
  helpText?: string | null
  type?: 'string' | 'number' | 'boolean' | 'select' | null
  /** 種類が 'select' の場合の選択肢 */
  selectOptions?: string[] | null
}

/** ダイアグラム上のノードの位置 */
export type NodePosition = {
  x: number
  y: number
}

/**
 * ダイアグラム上のルート集約のノードの位置。キーはノードの uniqueId。
 * 位置を動かしていないノードは含まれない。
 */
export type GraphLayout = {
  [uniqueId: string]: NodePosition
}

/** 画面初期表示時にサーバーから受け取るデータ（C# の InitialLoadData） */
export type InitialLoadData = {
  projectRoot?: string | null
  editingXmlFilePath?: string | null
  config?: Config | null
  aggregates?: SchemaNode[] | null
  aggregateOrMemberTypes?: SchemaNodeTypeDef[] | null
  optionalAttributes?: OptionalAttributeDef[] | null
  graphLayout?: GraphLayout | null
}

/** 検証・保存時にクライアントからサーバーへ送るデータ（C# の ClientRequest） */
export type ClientRequest = {
  config?: Config | null
  aggregates: SchemaNode[]
  graphLayout: GraphLayout
}

/**
 * バリデーションエラー。
 * ルートのキーはノードの uniqueId、その下のキーはエラーが発生した項目、値はエラーメッセージの配列。
 * 項目のキーは "-"（行全体）, "type", "typeDetail", "comment", またはオプショナル属性のキー。
 */
export type ValidationErrorMap = {
  [uniqueId: string]: {
    [key: string]: string[]
  }
}

/** ValidationErrorMap の、行全体に対するエラーを表すキー */
export const ERR_TO_ROW = "-"

// ---------------------------------
// nijo.xml の is 属性値・オプショナル属性キーの定数
// （SchemaNode.type / OptionalAttributeValue.key に入る値。C# 側の定義と合わせる必要がある）

/** 静的区分のルート要素の種類 */
export const NODE_TYPE_STATIC_ENUM = "enum"
/** 動的区分の種類のルート要素の種類 */
export const NODE_TYPE_DYNAMIC_ENUM_TYPE = "dynamic-enum-type"

/** Write Model のルート要素の種類 */
export const NODE_TYPE_WRITE_MODEL = "write-model-2"
/** Read Model のルート要素の種類 */
export const NODE_TYPE_READ_MODEL = "read-model-2"
/** Write Model と Read Model の両方を兼ねるルート要素の種類 */
export const NODE_TYPE_WRITE_READ_MODEL = "write-model-2 generate-default-read-model"
/** Command Model のルート要素の種類 */
export const NODE_TYPE_COMMAND_MODEL = "command"
/** 値オブジェクトのルート要素の種類 */
export const NODE_TYPE_VALUE_OBJECT = "value-object"
/** 親と1対1の関係を持つ子集約の種類 */
export const NODE_TYPE_CHILD = "child"
/** 親と1対多の関係を持つ子集約の種類 */
export const NODE_TYPE_CHILDREN = "children"

/**
 * 他の集約への参照を表すメンバーの種類の接頭辞。
 * 種類の値はこの接頭辞の後ろに参照先ノードの uniqueId が続く形になる。
 */
export const NODE_TYPE_PREFIX_REF_TO = "ref-to:"
/** 静的区分を指す種類の接頭辞。後ろに静的区分のルート要素の uniqueId が続く */
export const NODE_TYPE_PREFIX_ENUM = "enum:"
/** 値オブジェクトを指す種類の接頭辞。後ろに値オブジェクトのルート要素の uniqueId が続く */
export const NODE_TYPE_PREFIX_VALUE_OBJECT = "value-object:"

/** 物理名のオプショナル属性のキー。未指定の場合は displayName から物理名が決まる */
export const ATTR_KEY_PHYSICAL_NAME = "physical-name"

/** 一覧画面の詳細リンクの挙動の選択肢（C# 側の Config.MultiViewDetailLinkBehavior の取りうる値） */
export const MULTI_VIEW_DETAIL_LINK_BEHAVIORS: { value: Config["MultiViewDetailLinkBehavior"], displayName: string }[] = [
  { value: "navigateToEditMode", displayName: "編集モードの詳細画面に遷移する" },
  { value: "navigateToReadOnlyMode", displayName: "読み取り専用モードの詳細画面に遷移する" },
]
