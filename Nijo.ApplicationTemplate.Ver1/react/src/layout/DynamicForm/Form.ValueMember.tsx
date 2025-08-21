import * as React from "react"
import { DynamicFormContext } from "./DynamicFormContext"
import { MemberOwner, ValueMember, ValueMemberFormRendererProps } from "./types"
import ResponsiveForm from "../ResponsiveForm"

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
    <ResponsiveForm.Item
      label={member.noLabel ? undefined : (member.displayName ?? member.physicalName)}
      labelEnd={member.noLabel ? undefined : member.renderFormLabel?.(rendererProps)}
    >
      {member.renderFormValue?.(rendererProps)}
    </ResponsiveForm.Item>
  )
}
