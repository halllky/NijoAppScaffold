import * as React from "react"
import { VForm2 } from "../VForm2"
import { DynamicFormContext } from "./DynamicFormContext"
import { MemberOwner, ValueMember, ValueMemberDefinition, ValueMemberFormRendererProps } from "./types"

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
  const { useFormReturn, props: { membersTypes } } = React.useContext(DynamicFormContext)
  const typeDef: ValueMemberDefinition | undefined = React.useMemo(() => {
    return membersTypes[member.type]
  }, [member.type])

  // レンダリング処理の引数
  const rendererProps: ValueMemberFormRendererProps = {
    member,
    owner,
    name: ancestorsPath ? `${ancestorsPath}.${member.physicalName}` : member.physicalName,
    useFormReturn: useFormReturn,
    typeDef,
  }

  return (
    <VForm2.Item label={(
      <>
        <VForm2.LabelText>
          {member.displayName ?? member.physicalName}
        </VForm2.LabelText>

        {/* ラベルの脇に追加のコンポーネントがある場合はレンダリング */}
        {member.renderFormLabel?.(rendererProps)}
      </>
    )}>
      {/* 定義情報が無い場合はエラー */}
      {typeDef === undefined && (
        <span className="text-rose-600">
          エラー！ {member.type} 型の定義が見つかりません
        </span>
      )}

      {/* このメンバー個別のレンダリングが指定されている場合は優先 */}
      {typeDef && member.renderForm && (
        member.renderForm(rendererProps)
      )}

      {/* 型定義に従ったレンダリング */}
      {typeDef && !member.renderForm && (
        typeDef.renderForm(rendererProps)
      )}
    </VForm2.Item>
  )
}