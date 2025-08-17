import * as React from "react"
import { DynamicFormContext } from "./DynamicFormContext"
import { MemberOwner, ValueMember, ValueMemberFormRendererProps } from "./types"
import { DynamicFormLabel } from "./layout"

/**
 * 値メンバーのレンダリング。
 * VForm2のラベルと値の組を表示する。
 */
export const FormValueMember = ({ member, owner, ancestorsPath, gridColumn, gridRow }: {
  member: ValueMember
  owner: MemberOwner
  /** ルートオブジェクトからこのメンバー **のオーナー** までのパス */
  ancestorsPath: string
  /** 4列レイアウト時のgrid-column指定 */
  gridColumn?: number
  /** 4列レイアウト時のgrid-row指定 */
  gridRow?: number
}) => {
  // 定義情報など
  const { useFormReturn, isWideLayout } = React.useContext(DynamicFormContext)

  // レンダリング処理の引数
  const rendererProps: ValueMemberFormRendererProps = {
    owner,
    name: (ancestorsPath ? `${ancestorsPath}.${member.physicalName}` : member.physicalName) ?? '',
    useFormReturn: useFormReturn,
  }

  // スタイルクラスとインラインスタイル
  let valueDivClassName = 'py-px'
  let labelDivClassName = 'pr-1 py-px'
  let valueDivStyle: React.CSSProperties = {}
  let labelDivStyle: React.CSSProperties = {}

  if (member.fullWidth) {
    // 横幅いっぱいの場合は常にcol-span-full
    valueDivClassName += ' col-span-full'
    labelDivClassName += ' col-span-full'
  } else if (member.noLabel) {
    // ラベルなしの場合は2列占有
    valueDivClassName += ' col-span-2'
    // 4列レイアウトでgridColumnが指定されている場合
    if (gridColumn !== undefined && gridRow !== undefined) {
      valueDivStyle.gridColumn = `${gridColumn} / span 2`
      valueDivStyle.gridRow = gridRow.toString()
    }
  } else {
    // 通常のフィールドは2列レイアウトでも4列レイアウトでもラベル1列・値1列
    labelDivClassName += ' text-right'
    // 4列レイアウトでgridColumnが指定されている場合
    if (gridColumn !== undefined && gridRow !== undefined) {
      labelDivStyle.gridColumn = gridColumn.toString()
      labelDivStyle.gridRow = gridRow.toString()
      valueDivStyle.gridColumn = (gridColumn + 1).toString()
      valueDivStyle.gridRow = gridRow.toString()
    }
  }

  return (
    <>
      {/* ラベル */}
      {!member.noLabel && (
        <div className={labelDivClassName} style={labelDivStyle}>
          <DynamicFormLabel>
            {member.displayName ?? member.physicalName}
          </DynamicFormLabel>

          {/* ラベルの脇に追加のコンポーネントがある場合はレンダリング */}
          {member.renderFormLabel?.(rendererProps)}
        </div>
      )}

      {/* 値 */}
      <div className={valueDivClassName} style={valueDivStyle}>
        {member.renderFormValue?.(rendererProps)}
      </div>
    </>
  )
}
