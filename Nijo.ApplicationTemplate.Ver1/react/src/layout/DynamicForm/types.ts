import React from "react"
import * as ReactHookForm from "react-hook-form"
import { ColumnDefFactories, EditableGridColumnDef } from "../EditableGrid"
import { ResponsiveFormProps } from "../FormLayout"

/** DynamicFormのprops */
export type DynamicFormProps<TRoot extends ReactHookForm.FieldValues = ReactHookForm.FieldValues> = ResponsiveFormProps & {
  /** データ構造の定義 */
  root: MemberOwner<TRoot>
  /** フォームのデフォルト値 */
  defaultValues?: ReactHookForm.DefaultValues<TRoot>
}

/** DynamicFormのref */
export type DynamicFormRef<TRoot extends ReactHookForm.FieldValues = ReactHookForm.FieldValues> = {
  /** react-hook-form の useForm の戻り値。最新の値の取得などに使用する。 */
  useFormReturn: ReactHookForm.UseFormReturn<TRoot>
}

// ----------------------------------------------

/** ルートオブジェクト、ネストされたオブジェクト、配列、のいずれか */
export type MemberOwner<TOwner extends ReactHookForm.FieldValues = ReactHookForm.FieldValues> = {
  /** メンバー */
  members: Member<TOwner>[]
}

// ----------------------------------------------
//#region メンバー

/** DynamicFormのメンバー */
export type Member<TOwner extends ReactHookForm.FieldValues = ReactHookForm.FieldValues> =
  | SectionMember<TOwner>
  | ArrayMember<TOwner>
  | ValueMember<TOwner>

/** ネストされたセクション */
export type SectionMember<TOwner extends ReactHookForm.FieldValues = ReactHookForm.FieldValues> = MemberOwner<any> & {
  /** このメンバーがネストされたセクションであることを示す。 */
  isSection: true
  /**
   * 主にhtmlのname属性の構築に用いられるメンバー名。
   * 内部的にはルートオブジェクトからメンバーまでのパスがピリオドで連結されていく。
   */
  physicalName?: ReactHookForm.Path<TOwner>
  /** 画面上に表示する名称。未指定の場合はphysicalNameが使用される。 */
  displayName?: string
  /** フォームのレンダリングコンポーネント。未指定の場合は既定のレンダリングコンポーネントが使用される。 */
  render?: SectionFormRenderer
  /** フォームのラベル部分に追加のカスタマイズUIを表示したい場合に使用する。 `render` が指定されている場合は無視される。 */
  renderFormLabel?: SectionFormRenderer

  isArray?: never
}

/** 配列 */
export type ArrayMember<TOwner extends ReactHookForm.FieldValues = ReactHookForm.FieldValues> = MemberOwner<any> & {
  /** このメンバーが配列であることを示す。 */
  isArray: true
  /**
   * 主にhtmlのname属性の構築に用いられるメンバー名。
   * 内部的にはルートオブジェクトからメンバーまでのパスがピリオドで連結されていく。
   */
  physicalName: ReactHookForm.Path<TOwner>
  /** 画面上に表示する名称。未指定の場合はphysicalNameが使用される。 */
  displayName?: string
  /** 新しいアイテムを作成するための関数。 */
  onCreateNewItem: () => ReactHookForm.FieldValues
  /** フォームのレンダリングコンポーネント。未指定の場合は既定のレンダリングコンポーネントが使用される。 */
  render?: ArrayFormRenderer
  /** フォームのラベル部分に追加のカスタマイズUIを表示したい場合に使用する。 `render` が指定されている場合は無視される。 */
  renderFormLabel?: ArrayFormRenderer

  isSection?: never
}

/** 値メンバー */
export type ValueMember<TOwner extends ReactHookForm.FieldValues = ReactHookForm.FieldValues> = {
  /**
   * 主にhtmlのname属性の構築に用いられるメンバー名。
   * 内部的にはルートオブジェクトからメンバーまでのパスがピリオドで連結されていく。
   */
  physicalName?: ReactHookForm.Path<TOwner>
  /** 画面上に表示する名称。未指定の場合はphysicalNameが使用される。 */
  displayName?: string
  /** フォームのラベル部分に追加のカスタマイズUIを表示したい場合に使用する。 */
  renderFormLabel?: ValueMemberFormRenderer
  /** 値部分のレンダリングコンポーネント。未指定の場合は何も表示されなくなる。 */
  renderFormValue?: ValueMemberFormRenderer
  /** グリッドの列定義。未指定の場合はグリッドに表示されなくなる。 */
  getGridColumnDef?: GetGridColumnDefFunction
  /** このメンバーを横幅いっぱいにするかどうか。 */
  fullWidth?: boolean
  /** ラベルを表示しない。 */
  noLabel?: boolean
  /**
   * このメンバーの後で右側の列に折り返すかどうか。
   * 未指定の場合は項目数を2で割った位置で折り返す。
   * `fullWidth` が `true` の場合は無視される。
   * 同じグループ内で複数指定された場合、2番目以降の指定は無視される。
   */
  breakAfter?: boolean

  isArray?: never
  isSection?: never
}

//#endregion メンバー

// ----------------------------------------------
//#region フォームのカスタマイザー

/** セクションメンバーのフォームのレンダリングコンポーネント */
export type SectionFormRenderer = (props: SectionFormRendererProps) => React.ReactNode
/** 配列のフォームのレンダリングコンポーネント */
export type ArrayFormRenderer = (props: ArrayFormRendererProps) => React.ReactNode
/** 値メンバーのフォームのレンダリングコンポーネント */
export type ValueMemberFormRenderer = (props: ValueMemberFormRendererProps) => React.ReactNode

/** メンバーのフォームのレンダリングコンポーネントの引数 */
export type FormRendererProps = {
  /** react-hook-form の useForm の戻り値。最新の値の取得などに使用する。 */
  useFormReturn: ReactHookForm.UseFormReturn<ReactHookForm.FieldValues>
  /** メンバーを保持するオブジェクトのメタデータ */
  owner: MemberOwner
}

export type SectionFormRendererProps = FormRendererProps & {
  /** ルートオブジェクトからこのメンバーまでのパス */
  name: string
}

/** 配列のフォームのレンダリングコンポーネントの引数 */
export type ArrayFormRendererProps = FormRendererProps & {
  /** ルートオブジェクトからこのメンバーまでのパス */
  name: string
  /** react-hook-form の useFieldArray の戻り値。 */
  useFieldArrayReturn: ReactHookForm.UseFieldArrayReturn<ReactHookForm.FieldValues, ReactHookForm.FieldArrayPath<ReactHookForm.FieldValues>, "id">
}

/** 値メンバーのフォームのレンダリングコンポーネントの引数 */
export type ValueMemberFormRendererProps = FormRendererProps & {
  /** ルートオブジェクトからこのメンバーまでのパス */
  name: string
}

//#endregion フォームのカスタマイザー
// ----------------------------------------------
//#region グリッド

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

//#endregion グリッド
