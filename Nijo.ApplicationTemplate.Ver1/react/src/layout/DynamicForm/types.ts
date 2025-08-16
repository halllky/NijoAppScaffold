import React from "react"
import * as ReactHookForm from "react-hook-form"
import { ColumnDefFactories, EditableGridColumnDef } from "../EditableGrid"

/** DynamicFormのprops */
export type DynamicFormProps = {
  /** データ構造の定義 */
  root: MemberOwner
  /** 値メンバーの種類定義 */
  membersTypes: ValueMemberDefinitionMap
  /** フォームのデフォルト値 */
  defaultValues?: DynamicFormValues
  /** ラベル列の幅。未指定の場合は既定の幅が使用される。 */
  labelWidthPx?: number
  /** ルート要素に適用される。スタイルの微調整に用いる。 */
  className?: string
}

/** DynamicFormのref */
export type DynamicFormRef = {
  /** react-hook-form の useForm の戻り値。最新の値の取得などに使用する。 */
  useFormReturn: ReactHookForm.UseFormReturn<DynamicFormValues>
}

// ----------------------------------------------

/** フォームの値 */
export type DynamicFormValues = Record<string, unknown>

/** ルートオブジェクト、ネストされたオブジェクト、配列、のいずれか */
export type MemberOwner = {
  /** メンバー */
  members: Member[]
}

// ----------------------------------------------
// メンバー

/** DynamicFormのメンバー */
export type Member =
  | SectionMember
  | ArrayMember
  | ValueMember
  | NoneMember

/** ネストされたセクション */
export type SectionMember = MemberOwner & {
  /** このメンバーがネストされたセクションであることを示す。 */
  isSection: true
  /**
   * 主にhtmlのname属性の構築に用いられるメンバー名。
   * 内部的にはルートオブジェクトからメンバーまでのパスがピリオドで連結されていく。
   */
  physicalName?: string
  /** 画面上に表示する名称。未指定の場合はphysicalNameが使用される。 */
  displayName?: string
  /** フォームのレンダリングコンポーネント。未指定の場合は既定のレンダリングコンポーネントが使用される。 */
  render?: FormRenderer
  /** フォームのラベル部分に追加のカスタマイズUIを表示したい場合に使用する。 `render` が指定されている場合は無視される。 */
  renderFormLabel?: FormRenderer

  isArray?: never
  type?: never
}

/** 配列 */
export type ArrayMember = MemberOwner & {
  /** このメンバーが配列であることを示す。 */
  isArray: true
  /**
   * 主にhtmlのname属性の構築に用いられるメンバー名。
   * 内部的にはルートオブジェクトからメンバーまでのパスがピリオドで連結されていく。
   */
  physicalName: string
  /** 画面上に表示する名称。未指定の場合はphysicalNameが使用される。 */
  displayName?: string
  /** 新しいアイテムを作成するための関数。 */
  onCreateNewItem: () => DynamicFormValues
  /** フォームのレンダリングコンポーネント。未指定の場合は既定のレンダリングコンポーネントが使用される。 */
  render?: ArrayFormRenderer
  /** フォームのラベル部分に追加のカスタマイズUIを表示したい場合に使用する。 `render` が指定されている場合は無視される。 */
  renderFormLabel?: ArrayFormRenderer

  isSection?: never
  type?: never
}

/** 値メンバー */
export type ValueMember = {
  /**
   * このメンバーの型。
   * DynamicFormを呼び出す側で定義した型のうちから選ぶ必要がある。
  */
  type: string
  /**
   * 主にhtmlのname属性の構築に用いられるメンバー名。
   * 内部的にはルートオブジェクトからメンバーまでのパスがピリオドで連結されていく。
   */
  physicalName: string
  /** 画面上に表示する名称。未指定の場合はphysicalNameが使用される。 */
  displayName?: string
  /** フォームのラベル部分に追加のカスタマイズUIを表示したい場合に使用する。 */
  renderFormLabel?: ValueMemberFormRenderer
  /** フォームのレンダリングコンポーネント。未指定の場合は既定のレンダリングコンポーネントが使用される。 */
  renderFormValue?: ValueMemberFormRenderer
  /** グリッドの列定義。未指定の場合は既定の列定義が使用される。 */
  getGridColumnDef?: GetGridColumnDefFunction
  /** このメンバーを横幅いっぱいにするかどうか。 */
  fullWidth?: boolean

  isArray?: never
  isSection?: never
}

/** 特定のプロパティとバインドされないメンバー */
export type NoneMember = {
  physicalName?: never
  isArray?: never
  isSection?: never
  type?: never
  /** 画面上に表示する名称。未指定の場合はphysicalNameが使用される。 */
  displayName?: string
  /** フォームのレンダリングコンポーネント */
  renderForm: FormRenderer
  /** グリッドの列定義 */
  getGridColumnDef: (props: Omit<GetGridColumnDefFunctionProps, "name" | "member">) => GridColumnDef
}

// ----------------------------------------------
// 値メンバーの型定義

/**
 * 値メンバーの型定義のマップ。
 * キーは `DynamicFormValueMember` のtypeに対応する。
 */
export type ValueMemberDefinitionMap = Record<string, ValueMemberDefinition>

/** 値メンバーの型定義 */
export type ValueMemberDefinition = {
  /** フォームのレンダリングコンポーネント */
  renderForm: ValueMemberFormRenderer
  /** グリッドの列定義 */
  getGridColumnDef: GetGridColumnDefFunction
}

// ----------------------------------------------
// フォーム

/** メンバーのフォームのレンダリングコンポーネント */
export type FormRenderer = (props: FormRendererProps) => React.ReactNode
/** 配列のフォームのレンダリングコンポーネント */
export type ArrayFormRenderer = (props: ArrayFormRendererProps) => React.ReactNode
/** 値メンバーのフォームのレンダリングコンポーネント */
export type ValueMemberFormRenderer = (props: ValueMemberFormRendererProps) => React.ReactNode

/** メンバーのフォームのレンダリングコンポーネントの引数 */
export type FormRendererProps = {
  /** react-hook-form の useForm の戻り値。最新の値の取得などに使用する。 */
  useFormReturn: ReactHookForm.UseFormReturn<DynamicFormValues>
  /** ルートオブジェクトからこのメンバーまでのパス */
  name: string
  /** メンバーを保持するオブジェクトのメタデータ */
  owner: MemberOwner
}

/** 配列のフォームのレンダリングコンポーネントの引数 */
export type ArrayFormRendererProps = FormRendererProps & {
  /** react-hook-form の useFieldArray の戻り値。 */
  useFieldArrayReturn: ReactHookForm.UseFieldArrayReturn<ReactHookForm.FieldValues, ReactHookForm.FieldArrayPath<ReactHookForm.FieldValues>, "id">
}

/** 値メンバーのフォームのレンダリングコンポーネントの引数 */
export type ValueMemberFormRendererProps = FormRendererProps & {
  /** メンバーのメタデータ */
  member: ValueMember
  /** このメンバーの型定義 */
  typeDef: ValueMemberDefinition
}

// ----------------------------------------------
// グリッド

/** 値メンバーのグリッドの列定義を取得するための関数 */
export type GetGridColumnDefFunction = (props: GetGridColumnDefFunctionProps) => GridColumnDef

/** グリッドの列定義を取得するための関数の引数 */
export type GetGridColumnDefFunctionProps = {
  /** ルートオブジェクトからこのメンバーまでのパス */
  name: string
  /** メンバーのメタデータ */
  member: ValueMember
  /** メンバーを保持するオブジェクトのメタデータ */
  owner: MemberOwner
  /** グリッドの列定義を作成するためのヘルパー関数 */
  cellType: ColumnDefFactories<ReactHookForm.FieldValues>
}

/** グリッドの列定義 */
export type GridColumnDef = EditableGridColumnDef<ReactHookForm.FieldValues>
