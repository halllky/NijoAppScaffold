// 画面から使ってよいもの。
// 入力コンポーネント・ラベル・メッセージ表示の実体は、フックの戻り値を通して使う。

export * from "./useDisplayDataForm"
export * from "./useSearchConditionForm"
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
export type { GridRow, UseEditableGridOptions, BoundUseEditableGrid } from "./useEditableGrid"
export type { BoundInputs, GridColumnHelper } from "./input/bindInputs"
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
