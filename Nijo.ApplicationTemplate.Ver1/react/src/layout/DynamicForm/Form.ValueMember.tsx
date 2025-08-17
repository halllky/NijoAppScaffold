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
  const { useFormReturn } = React.useContext(DynamicFormContext)

  // レンダリング処理の引数
  const rendererProps: ValueMemberFormRendererProps = {
    owner,
    name: (ancestorsPath ? `${ancestorsPath}.${member.physicalName}` : member.physicalName) ?? '',
    useFormReturn: useFormReturn,
  }

  return (
    <>
      {/* ラベル */}
      {!member.noLabel && (
        <div className={`pr-1 py-px ${member.fullWidth ? 'col-span-full' : 'text-right'}`}>
          <DynamicFormLabel>
            {member.displayName ?? member.physicalName}
          </DynamicFormLabel>

          {/* ラベルの脇に追加のコンポーネントがある場合はレンダリング */}
          {member.renderFormLabel?.(rendererProps)}
        </div>
      )}

      {/* 値 */}
      <div className={`py-px ${member.fullWidth || member.noLabel ? 'col-span-full' : ''}`}>
        {member.renderFormValue?.(rendererProps)}
      </div>
    </>
  )
}
