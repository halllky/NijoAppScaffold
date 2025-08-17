import * as React from "react"
import { DynamicFormContext } from "./DynamicFormContext"
import { MemberOwner, ValueMember, ValueMemberFormRendererProps } from "./types"
import { DynamicFormLabel } from "./layout"

/**
 * 値メンバーのレンダリング。
 * VForm2のラベルと値の組を表示する。
 */
export const FormValueMember = ({ member, owner, ancestorsPath }: {
  member: ValueMember
  owner: MemberOwner
  /** ルートオブジェクトからこのメンバー **のオーナー** までのパス */
  ancestorsPath: string
}) => {
  // 定義情報など
  const { useFormReturn, isWideLayout } = React.useContext(DynamicFormContext)

  // レンダリング処理の引数
  const rendererProps: ValueMemberFormRendererProps = {
    owner,
    name: (ancestorsPath ? `${ancestorsPath}.${member.physicalName}` : member.physicalName) ?? '',
    useFormReturn: useFormReturn,
  }

  // スタイルクラス
  let valueDivClassName = 'py-px'
  let labelDivClassName = 'pr-1 py-px'

  if (member.fullWidth) {
    // 横幅いっぱいの場合は常にcol-span-full
    valueDivClassName += ' col-span-full'
    labelDivClassName += ' col-span-full'
  } else if (member.noLabel) {
    // ラベルなしの場合は2列占有
    valueDivClassName += ' col-span-2'
  } else {
    // 通常のフィールドは2列レイアウトでも4列レイアウトでもラベル1列・値1列
    labelDivClassName += ' text-right'
  }

  return (
    <>
      {/* ラベル */}
      {!member.noLabel && (
        <div className={labelDivClassName}>
          <DynamicFormLabel>
            {member.displayName ?? member.physicalName}
          </DynamicFormLabel>

          {/* ラベルの脇に追加のコンポーネントがある場合はレンダリング */}
          {member.renderFormLabel?.(rendererProps)}
        </div>
      )}

      {/* 値 */}
      <div className={valueDivClassName}>
        {member.renderFormValue?.(rendererProps)}
      </div>
    </>
  )
}
