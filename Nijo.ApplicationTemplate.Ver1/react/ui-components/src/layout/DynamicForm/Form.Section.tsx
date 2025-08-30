import React from "react"
import { MembersGroupByBreakPoint } from "./Form.Members"
import { MemberOwner, SectionFormRendererProps, SectionMember } from "./types"
import { DynamicFormContext } from "./DynamicFormContext"
import FormLayout from "../FormLayout"

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
  const sectionMemberPath = section.physicalName ? `${ancestorsPath}.${section.physicalName}` : ancestorsPath

  // レンダリング処理の引数
  const rendererProps: SectionFormRendererProps = {
    name: sectionMemberPath,
    useFormReturn,
    owner,
  }

  // 既定のレンダリング
  return (
    <FormLayout.Section
      border
      label={typeof section.label === 'string' ? section.label : undefined}
      labelEnd={typeof section.label === 'function' ? section.label(rendererProps) : undefined}
    >
      {section.contents ? (
        section.contents(rendererProps)
      ) : (
        <MembersGroupByBreakPoint
          ancestorsPath={sectionMemberPath}
          owner={section}
        />
      )}
    </FormLayout.Section>
  )
}
