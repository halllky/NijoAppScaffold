import React from "react"
import { MembersGroupByBreakPoint } from "./Form.Members"
import { MemberOwner, SectionFormRendererProps, SectionMember } from "./types"
import { DynamicFormContext } from "./DynamicFormContext"
import ResponsiveForm from "../ResponsiveForm"

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

  // レンダリング処理が明示されている場合はそれが優先
  if (section.render) {
    return (
      <div className="col-span-full">
        {section.render(rendererProps)}
      </div>
    )
  }

  // 既定のレンダリング
  return (
    <ResponsiveForm.Item
      fullWidth
      label={section.displayName ?? section.physicalName}
      labelEnd={section.renderFormLabel?.(rendererProps)}
    >
      <MembersGroupByBreakPoint
        ancestorsPath={sectionMemberPath}
        owner={section}
      />
    </ResponsiveForm.Item>
  )
}
