import * as React from "react"
import { DynamicFormContext } from "./DynamicFormContext"
import { MemberOwner, ValueMember, ValueMemberFormRendererProps } from "./types"
import FormLayout from "../FormLayout"

/**
 * 値メンバーのレンダリング。
 * フォームのラベルと値の組を表示する。
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
    <FormLayout.Field
      label={typeof member.label === 'string' ? member.label : undefined}
      labelEnd={typeof member.label === 'function' ? member.label(rendererProps) : undefined}
      fullWidth={member.fullWidth}
    >
      {member.contents?.(rendererProps)}
    </FormLayout.Field>
  )
}
