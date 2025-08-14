import React from "react"
import { VForm2 } from "../VForm2"
import { MembersGroupByBreakPoint } from "./Form.Members"
import { FormRendererProps, MemberOwner, SectionMember } from "./types"
import { DynamicFormContext } from "./DynamicFormContext"

/**
 * セクションのレンダリング。
 * セクションのラベルと値の組を表示する。
 */
export const FormSection = ({ member: section, owner, ancestorsPath }: {
  member: SectionMember
  owner: MemberOwner
  ancestorsPath: string
}) => {

  // 定義情報など
  const { useFormReturn } = React.useContext(DynamicFormContext)
  const sectionMemberPath = `${ancestorsPath}.${section.physicalName}`

  // レンダリング処理の引数
  const rendererProps: FormRendererProps = {
    name: sectionMemberPath,
    useFormReturn,
    owner,
  }

  return (
    <VForm2.Indent label={(
      <>
        <VForm2.LabelText>
          {section.displayName ?? section.physicalName}
        </VForm2.LabelText>

        {/* ラベルの脇に追加のコンポーネントがある場合はレンダリング */}
        {section.renderFormLabel?.(rendererProps)}
      </>
    )}>

      {/* レンダリング処理が明示されている場合はそれが優先 */}
      {section.render ? (
        section.render(rendererProps)
      ) : (
        // 既定のレンダリング
        <MembersGroupByBreakPoint
          ancestorsPath={sectionMemberPath}
          owner={section}
        />
      )}
    </VForm2.Indent>
  )
}