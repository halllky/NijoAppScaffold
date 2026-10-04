// 画面から使ってよいもの。
// 入力コンポーネント・ラベル・メッセージ表示の実体は、フックの戻り値を通して使う。

export * from "./useDisplayDataForm"
export * from "./useSearchConditionForm"
export { SearchPageBase, type SearchPageBaseProps, type SortComboItem } from "./SearchPageBase/SearchPageBase"
export type { SearchResult } from "./usePagedSearch"
export { FieldGroup, FieldColumn, type FieldGroupProps, type FieldColumnProps } from "./FieldGroup"
export {
  defineSearchDialog,
  type SearchDialogDefinition,
  type SearchDialogRefFieldDefinition,
  type SearchDialogConditionProps,
  type SearchDialog,
  type OpenSearchDialog,
  type FindByCodeResult,
} from "./search-dialog/defineSearchDialog"
export type { SearchDialogRegistry, SearchDialogParamsProp, RefToParamsProp } from "./search-dialog/SearchDialogRegistry"
export { SearchDialogHost, type SearchDialogHostProps } from "./search-dialog/SearchDialogHost"
export type { GridRow, UseEditableGridOptions, UseEditableGridReturn, BoundUseEditableGrid } from "./useEditableGrid"
export type { BoundInputs, GridColumnHelper } from "./input/bindInputs"
export type { InputRules } from "./input/InputProps"
export type { FieldLabelProps } from "./FieldLabel"
export type { RootErrorsProps } from "./RootErrors"
export type { Messages } from "./MessageList"
export type { TextBoxProps, TextColumnOptions } from "./input/TextBox"
export type { TextAreaProps, TextAreaColumnOptions } from "./input/TextArea"
export type { NumericTextBoxProps, NumericColumnOptions } from "./input/NumericTextBox"
export type { DateInputProps, DateColumnOptions } from "./input/DateInput"
export type { CheckBoxProps, CheckBoxColumnOptions } from "./input/CheckBox"
export type { EnumSelectionProps, EnumColumnOptions } from "./input/EnumSelection"
export type { RefToProps, RefToColumnOptions } from "./input/RefTo"
export type { ButtonColumnOptions } from "./input/ButtonColumn"
export type { PresentationContextDetail } from "./ServerMessages"

// メタデータから機械的に組み立てる画面部品
export { AutoSearchConditionFields, type AutoSearchConditionFieldsProps } from "./auto/AutoSearchConditionFields"
export { AutoDisplayDataFields, type AutoDisplayDataFieldsProps } from "./auto/AutoDisplayDataFields"
export { autoResultColumns } from "./auto/autoResultColumns"
export { defineAutoSearchDialog, type AutoSearchDialogDefinition } from "./auto/defineAutoSearchDialog"

// 画面の枠とログイン
export { PageBase, type PageBaseProps } from "./PageBase"
export { ErrorPage } from "./ErrorPage"
export { useLoginLogout, LoginUserProvider } from "./useLoginLogout"

// フォームと結びつかない部品
export { Breadcrumb, type BreadcrumbItem } from "./parts/Breadcrumb"
export { Button } from "./parts/Button"
export { CheckBox } from "./parts/CheckBox"
export { Modal } from "./parts/Modal"
export { MultiSelect } from "./parts/MultiSelect"
export { NowLoading } from "./parts/NowLoading"
export { PageTitle } from "./parts/PageTitle"
export { Pager } from "./parts/Pager"
export { useOutsideClick } from "./parts/useOutsideClick"
export * as ReadOnlyColumn from "./ReadOnlyColumn"
