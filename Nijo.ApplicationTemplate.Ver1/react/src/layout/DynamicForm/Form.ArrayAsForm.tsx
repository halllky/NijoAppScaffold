import React from "react"
import * as ReactHookForm from "react-hook-form"
import * as Icon from "@heroicons/react/24/outline"
import { DynamicFormContext } from "./DynamicFormContext"
import { ArrayFormRendererProps, ArrayMember, MemberOwner } from "./types"
import { MembersGroupByBreakPoint } from "./Form.Members"
import { IconButton } from "../../input"
import useEvent from "react-use-event-hook"
import ResponsiveForm from "../ResponsiveForm"

/**
 * 配列を縦並びのフォームで表示する。
 */
export const FormArrayAsForm = ({ member: array, owner, ancestorsPath }: {
  member: ArrayMember
  owner: MemberOwner
  ancestorsPath: string
}) => {

  // 定義情報など
  const { useFormReturn } = React.useContext(DynamicFormContext)

  // useFieldArray
  const arrayMemberPath = `${ancestorsPath}.${array.physicalName}`
  const useFieldArrayReturn = ReactHookForm.useFieldArray({
    control: useFormReturn.control,
    name: arrayMemberPath,
  })
  const { fields, append, remove } = useFieldArrayReturn

  // レンダリング処理の引数
  const rendererProps: ArrayFormRendererProps = {
    name: arrayMemberPath,
    useFormReturn,
    useFieldArrayReturn,
    owner,
  }

  // 追加
  const handleAppend = useEvent(() => {
    const newItem = array.onCreateNewItem()
    append(newItem)
  })

  // レンダリング処理が明示されている場合はそれが優先
  if (array.render) {
    return (
      <div className="col-span-full">
        {array.render(rendererProps)}
      </div>

    )
  }

  // 既定のレンダリング
  return (
    <ResponsiveForm.Section
      fullWidth
      label={array.displayName ?? array.physicalName}
      labelEnd={array.renderFormLabel?.(rendererProps)}
    >
      {/* 要素一覧 */}
      {fields.map((field, index) => (
        <ResponsiveForm.Item
          key={field.id}
          fullWidth
          label={`${index + 1}`}
          labelEnd={(
            <IconButton icon={Icon.TrashIcon} outline mini onClick={() => remove(index)}>
              削除
            </IconButton>
          )}
        >
          <MembersGroupByBreakPoint
            owner={array}
            ancestorsPath={`${arrayMemberPath}.${index}`}
          />
        </ResponsiveForm.Item>
      ))}

      {/* 追加ボタン */}
      <ResponsiveForm.Item fullWidth>
        <IconButton icon={Icon.PlusCircleIcon} outline mini onClick={handleAppend}>
          追加
        </IconButton>
      </ResponsiveForm.Item>
    </ResponsiveForm.Section>
  )
}
