import React from "react"
import { MembersGroupByBreakPoint } from "./Form.Members"
import { MemberOwner, SectionFormRendererProps, SectionMember } from "./types"
import { DynamicFormContext } from "./DynamicFormContext"
import { DynamicFormLabel } from "./layout"

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
    <React.Fragment>

      {/* ヘッダ */}
      <div className="col-span-full flex flex-wrap items-center gap-1">
        <DynamicFormLabel>
          {section.displayName ?? section.physicalName}
        </DynamicFormLabel>

        {/* ラベルの脇に追加のコンポーネントがある場合はレンダリング */}
        {section.renderFormLabel?.(rendererProps)}
      </div>

      {/* メンバー */}
      <div className="col-span-full grid grid-cols-[subgrid] border border-gray-300 p-1">
        <MembersGroupByBreakPoint
          ancestorsPath={sectionMemberPath}
          owner={section}
        />
      </div>
    </React.Fragment>
  )
}