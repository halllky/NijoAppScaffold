import React from "react"
import * as ReactHookForm from "react-hook-form"
import { ColumnDefFactories, EditableGridColumnDef } from "../EditableGrid"
import { FormLayoutProps } from "../FormLayout"

/** DynamicFormのprops */
export type DynamicFormProps<TRoot extends ReactHookForm.FieldValues = ReactHookForm.FieldValues> = FormLayoutProps & {
  /** データ構造の定義 */
  root: MemberOwner<TRoot>
  /** フォームのデフォルト値 */
  defaultValues?: ReactHookForm.DefaultValues<TRoot>
  /** フォームを読み取り専用にするかどうか */
  isReadOnly?: boolean
}

/** DynamicFormのref */
export type DynamicFormRef<TRoot extends ReactHookForm.FieldValues = ReactHookForm.FieldValues> = {
  /** react-hook-form の useForm の戻り値。最新の値の取得などに使用する。 */
  useFormReturn: ReactHookForm.UseFormReturn<TRoot>
  /** フォーム定義をコンソール出力する（デバッグ用） */
  consoleLog: () => void
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
  type: 'section'
  /**
   * このセクションのオーナーからこのセクションまでのパス。
   * 未指定の場合、このセクションはデータ構造に影響を与えない外観だけのコンテナという位置づけになる。
   */
  physicalName?: ReactHookForm.Path<TOwner>
  /** フォームのラベル部分 */
  label?: string | CustomSectionRenderer
  /** コンテンツを丸ごとカスタマイズしたい場合は指定 */
  contents?: CustomSectionRenderer
}

/** 配列 */
export type ArrayMember<TOwner extends ReactHookForm.FieldValues = ReactHookForm.FieldValues> = MemberOwner<any> & {
  type: 'array'
  /**
   * 主にhtmlのname属性の構築に用いられるメンバー名。
   * 内部的にはルートオブジェクトからメンバーまでのパスがピリオドで連結されていく。
   */
  physicalName: ReactHookForm.Path<TOwner>
  /** 配列全体に対するラベル */
  arrayLabel?: string | CustomArrayRenderer
  /** 配列の各アイテムのラベル部分。この配列がグリッドとしてレンダリングされる場合は無視される。 */
  itemLabel?: string | CustomArrayItemRenderer
  /** コンテンツを丸ごとカスタマイズしたい場合は指定 */
  contents?: CustomArrayRenderer
  /** 新しいアイテムを作成するための関数。未指定の場合は既定のレンダリングで追加ボタンが表示されない。 */
  onCreateNewItem?: () => ReactHookForm.FieldValues
}

/** 値メンバー */
export type ValueMember<TOwner extends ReactHookForm.FieldValues = ReactHookForm.FieldValues> = {
  type?: never
  /** このフィールドのオーナーからこの値メンバーまでのパス */
  physicalName?: ReactHookForm.Path<TOwner>
  /** フォームのラベル部分 */
  label?: string | CustomValueRenderer
  /** 値部分のレンダリングコンポーネント。未指定の場合は何も表示されなくなる。 */
  contents?: CustomValueRenderer
  /** グリッドの列定義。未指定の場合はグリッドに表示されなくなる。 */
  getGridColumnDef?: GetGridColumnDefFunction
  /** このメンバーを横幅いっぱいにするかどうか。 */
  fullWidth?: boolean
}

//#endregion メンバー

// ----------------------------------------------
//#region フォームのカスタマイザー

// 各種レンダリング関数
export type CustomRenderer<T extends FormRendererProps> = (props: T) => React.ReactNode
export type CustomSectionRenderer = CustomRenderer<SectionFormRendererProps>
export type CustomArrayRenderer = CustomRenderer<ArrayFormRendererProps>
export type CustomArrayItemRenderer = CustomRenderer<ArrayFormItemRendererProps>
export type CustomValueRenderer = CustomRenderer<ValueMemberFormRendererProps>

/** メンバーのフォームのレンダリングコンポーネントの引数 */
export type FormRendererProps = {
  /** react-hook-form の useForm の戻り値。最新の値の取得などに使用する。 */
  useFormReturn: ReactHookForm.UseFormReturn<ReactHookForm.FieldValues>
  /** メンバーを保持するオブジェクトのメタデータ */
  owner: MemberOwner
  /** フォームが読み取り専用かどうか */
  isReadOnly: boolean
}

/** セクションのフォームのレンダリングコンポーネントの引数 */
export type SectionFormRendererProps = FormRendererProps & {
  /** ルートオブジェクトからこのメンバーまでのパス */
  name: string
}

/** 配列のフォームのレンダリングコンポーネントの引数 */
export type ArrayFormRendererProps = FormRendererProps & {
  /** ルートオブジェクトからこの配列までのパス */
  name: string
  /** react-hook-form の useFieldArray の戻り値。 */
  useFieldArrayReturn: ReactHookForm.UseFieldArrayReturn<ReactHookForm.FieldValues, ReactHookForm.FieldArrayPath<ReactHookForm.FieldValues>, "id">
}

export type ArrayFormItemRendererProps = ArrayFormRendererProps & {
  /** ルートオブジェクトからこの配列のアイテムまでのパス */
  itemName: string
  /** 配列のアイテムのインデックス */
  itemIndex: number
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
  /** ルートオブジェクトから配列までのパス */
  arrayPath: string
  /** 配列の行オブジェクトからこのメンバーまでのパス */
  pathFromRow: string
  /** メンバーのメタデータ */
  member: ValueMember
  /** メンバーを保持するオブジェクトのメタデータ */
  owner: MemberOwner
  /** グリッドの列定義を作成するためのヘルパー関数 */
  cellType: ColumnDefFactories<ReactHookForm.FieldValues>
  /** フォームが読み取り専用かどうか */
  isReadOnly: boolean
}

/** グリッドの列定義 */
export type GridColumnDef = EditableGridColumnDef<ReactHookForm.FieldValues>

//#endregion グリッド
