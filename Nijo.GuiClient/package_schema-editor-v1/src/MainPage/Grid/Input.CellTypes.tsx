import React from "react"
import * as ReactHookForm from "react-hook-form"
import * as ReactTable from "@tanstack/react-table"
import * as Layout from "@nijo/ui-components/layout"
import { SchemaDefinitionGlobalState, XmlElementAttribute } from "../../types"
import { GetValidationResultFunction } from "../useValidation"

/** メンバーグリッドの行の型 */
export type GridRowType = ReactHookForm.FieldArrayWithId<SchemaDefinitionGlobalState, `xmlElementTrees.${number}.xmlElements`>

// --------------------------------------------

/** LocalName のセルのレイアウト */
export const createLocalNameCell = (
  cellType: Layout.ColumnDefFactories<GridRowType>,
  getValidationResult: GetValidationResultFunction
) => {
  return cellType.text('localName', '', {
    defaultWidth: 220,
    isFixed: true,
    renderCell: (context: ReactTable.CellContext<GridRowType, unknown>) => {
      // エラー情報を取得
      const validation = getValidationResult(context.row.original.uniqueId)
      const hasOwnError = validation?._own?.length > 0
      const bgColor = hasOwnError ? 'bg-amber-300/50' : ''

      return (
        <div className={`px-1 flex-1 inline-flex text-left truncate ${bgColor}`}>

          {/* インデント */}
          {Array.from({ length: context.row.original.indent - 1 }).map((_, i) => (
            <React.Fragment key={i}>
              {/* インデントのテキスト */}
              <div className="basis-[20px] min-w-[20px] relative leading-none">
                {/* {i >= 1 && (
                  // インデントを表す縦線
                  <div className="absolute top-[-1px] bottom-[-1px] left-0 border-l border-gray-300 border-dotted leading-none"></div>
                )} */}
              </div>
            </React.Fragment>
          ))}

          <span className="flex-1 truncate">
            {context.cell.getValue() as string}
          </span>
        </div>
      )
    }
  })
}

/** 属性のセル */
export const createAttributeCell = (
  attrDef: XmlElementAttribute,
  cellType: Layout.ColumnDefFactories<GridRowType>,
  getValidationResult: GetValidationResultFunction
) => {
  return cellType.other(attrDef.displayName, {
    defaultWidth: 120,
    // 編集開始時処理
    onStartEditing: e => {
      e.setEditorInitialValue(e.row.attributes[attrDef.attributeName] ?? '')
    },
    // 編集終了時処理
    onEndEditing: e => {
      const clone = window.structuredClone(e.row)
      if (e.value.trim() === '') {
        delete clone.attributes[attrDef.attributeName]
      } else {
        clone.attributes[attrDef.attributeName] = e.value
      }
      e.setEditedRow(clone)
    },
    // セルのレンダリング
    renderCell: context => {
      const value = context.row.original.attributes[attrDef.attributeName]
      // エラー情報を取得
      const validationResult = getValidationResult(context.row.original.uniqueId)
      const hasError = validationResult?.[attrDef.attributeName]?.length > 0

      return (
        <PlainCell className={`px-1 truncate ${hasError ? 'bg-amber-300/50' : ''}`}>
          {value}
        </PlainCell>
      )
    },
  })
}

// -----------------------------

const PlainCell = ({ children, className }: {
  children?: React.ReactNode
  className?: string
}) => {
  return (
    <div className={`flex-1 inline-flex text-left truncate ${className ?? ''}`}>
      <span className="flex-1 truncate">
        {children}
      </span>
    </div>
  )
}
