// 画面から使ってよいもの。
// 入力コンポーネント・ラベル・メッセージ表示の実体は、フックの戻り値を通して使う。

// フォーム
export * from "./form/useDisplayDataForm"
export * from "./form/useSearchConditionForm"
export { FieldGroup, FieldColumn, type FieldGroupProps, type FieldColumnProps } from "./form/FieldGroup"
export type { FieldLabelProps } from "./form/FieldLabel"
export type { GridRow, UseEditableGridOptions, UseEditableGridReturn, BoundUseEditableGrid } from "./form/useEditableGrid"

// フォームと結びついた入力部品
export type { BoundInputs, GridColumnHelper } from "./form-input/bindInputs"
export type { InputRules } from "./form-input/InputProps"
export type { TextBoxProps, TextColumnOptions } from "./form-input/TextBox"
export type { TextAreaProps, TextAreaColumnOptions } from "./form-input/TextArea"
export type { NumericTextBoxProps, NumericColumnOptions } from "./form-input/NumericTextBox"
export type { DateInputProps, DateColumnOptions } from "./form-input/DateInput"
export type { CheckBoxProps, CheckBoxColumnOptions } from "./form-input/CheckBox"
export type { EnumSelectionProps, EnumColumnOptions } from "./form-input/EnumSelection"
export type { RefToProps, RefToColumnOptions } from "./form-input/RefTo"

// メッセージ
export type { RootErrorsProps } from "./message/RootErrors"
export type { Messages } from "./message/MessageList"
export type { PresentationContextDetail } from "./message/ServerMessages"

// 検索ダイアログ
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

// 検索
export type { SearchResult } from "./search/usePagedSearch"

// メタデータから機械的に組み立てる画面部品
export { AutoSearchConditionFields, type AutoSearchConditionFieldsProps } from "./auto/AutoSearchConditionFields"
export { AutoDisplayDataFields, type AutoDisplayDataFieldsProps } from "./auto/AutoDisplayDataFields"
export { autoResultColumns } from "./auto/autoResultColumns"
export { defineAutoSearchDialog, type AutoSearchDialogDefinition } from "./auto/defineAutoSearchDialog"

// 画面の枠
export { SearchPageBase, type SearchPageBaseProps, type SortComboItem } from "./page/SearchPageBase"
export { PageBase, type PageBaseProps } from "./page/PageBase"
export { PageTitle } from "./page/PageTitle"
export { Breadcrumb, type BreadcrumbItem } from "./page/Breadcrumb"
export { ErrorPage } from "./page/ErrorPage"

// フォームと結びつかない入力部品
export { Button } from "./input/Button"
export { CheckBox } from "./input/CheckBox"
export { MultiSelect } from "./input/MultiSelect"

// 重ね表示
export { Modal } from "./overlay/Modal"
export { NowLoading } from "./overlay/NowLoading"
export { useOutsideClick } from "./overlay/useOutsideClick"

// フォームと結びつかない一覧表示
export * as ReadOnlyColumn from "./grid/ReadOnlyColumn"
export type { ButtonColumnOptions } from "./grid/ButtonColumn"
export { Pager } from "./grid/Pager"
