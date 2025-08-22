import { type LabelProps, DefaultLabel } from "./DefaultLabel"
import { Root, type ResponsiveFormProps } from "./Root"
import { Separator } from "./Separator"
import { Item, type ItemProps } from "./Item"
import { ResponsiveColumnGroup, type ColumnGroupProps } from "./ResponsiveColumnGroup"
import { ResponsiveColumn } from "./ResponsiveColumn"
import { ItemGroupInResponsiveColumn, type ItemGroupProps } from "./ItemGroupInResponsiveColumn"

export default {
  Root,
  ResponsiveColumnGroup,
  ResponsiveColumn,
  Separator,
  Item,
  ItemGroupInResponsiveColumn,
  DefaultLabel,
}

export type {
  ResponsiveFormProps,
  ColumnGroupProps,
  ItemProps,
  ItemGroupProps,
  LabelProps,
}
