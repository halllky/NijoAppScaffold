import * as React from "react"
import { DynamicFormContext } from "./DynamicFormContext"
import { MemberOwner, ValueMember, ValueMemberDefinition, ValueMemberFormRendererProps } from "./types"
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
  const { useFormReturn, props: { membersTypes } } = React.useContext(DynamicFormContext)
  const typeDef: ValueMemberDefinition | undefined = React.useMemo(() => {
    return membersTypes[member.type]
  }, [member.type])

  // レンダリング処理の引数
  const rendererProps: ValueMemberFormRendererProps = {
    owner,
    name: ancestorsPath ? `${ancestorsPath}.${member.physicalName}` : member.physicalName,
    useFormReturn: useFormReturn,
    typeDef,
  }

  return (
    <>
      {/* ラベル */}
      <div className={`pr-1 py-px ${member.fullWidth ? 'col-span-full' : 'text-right'}`}>
        <DynamicFormLabel>
          {member.displayName ?? member.physicalName}
        </DynamicFormLabel>

        {/* ラベルの脇に追加のコンポーネントがある場合はレンダリング */}
        {member.renderFormLabel?.(rendererProps)}
      </div>

      {/* 値 */}
      <div className={`py-px ${member.fullWidth ? 'col-span-full' : ''}`}>
        {/* 定義情報が無い場合はエラー */}
        {typeDef === undefined && (
          <span className="text-rose-600">
            エラー！ {member.type} 型の定義が見つかりません
          </span>
        )}

        {/* このメンバー個別のレンダリングが指定されている場合は優先 */}
        {typeDef && member.renderFormValue && (
          member.renderFormValue(rendererProps)
        )}

        {/* 型定義に従ったレンダリング */}
        {typeDef && !member.renderFormValue && (
          typeDef.renderForm(rendererProps)
        )}
      </div>
    </>
  )
}
