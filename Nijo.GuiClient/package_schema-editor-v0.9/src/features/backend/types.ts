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
  type?: 'string' | 'number' | 'boolean' | null
}

/** 画面初期表示時にサーバーから受け取るデータ（C# の InitialLoadData） */
export type InitialLoadData = {
  projectRoot?: string | null
  editingXmlFilePath?: string | null
  config?: Config | null
  aggregates?: SchemaNode[] | null
  aggregateOrMemberTypes?: SchemaNodeTypeDef[] | null
  optionalAttributes?: OptionalAttributeDef[] | null
}

/** 検証・保存時にクライアントからサーバーへ送るデータ（C# の ClientRequest） */
export type ClientRequest = {
  config?: Config | null
  aggregates: SchemaNode[]
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
