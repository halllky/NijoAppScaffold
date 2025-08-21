import React from "react"
import * as ReactHookForm from "react-hook-form"
import * as Icon from "@heroicons/react/24/outline"
import { DynamicFormContext } from "./DynamicFormContext"
import { ArrayFormRendererProps, ArrayMember, FormRendererProps, MemberOwner } from "./types"
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
    <>
      {/* ヘッダ */}
      <div className="flex flex-wrap items-center gap-1 col-span-full">
        <ResponsiveForm.Label>
          {array.displayName ?? array.physicalName}
        </ResponsiveForm.Label>

        {/* ラベルの脇に追加のコンポーネントがある場合はレンダリング */}
        {array.renderFormLabel?.(rendererProps)}
      </div>

      {/* 要素一覧 */}
      {fields.map((field, index) => (
        <React.Fragment key={field.id}>

          {/* 境界線 */}
          <ResponsiveForm.Spacer />

          {/* 要素のヘッダ */}
          <div className="col-span-full flex flex-wrap items-center gap-1 py-px">
            <ResponsiveForm.Label>
              {`${index + 1}`}
            </ResponsiveForm.Label>
            <IconButton icon={Icon.TrashIcon} outline mini onClick={() => remove(index)}>
              削除
            </IconButton>
          </div>

          {/* 要素のメンバー */}
          <div className="grid grid-cols-[subgrid] col-span-full border border-gray-300 p-1">
            <MembersGroupByBreakPoint
              owner={array}
              ancestorsPath={`${arrayMemberPath}.${index}`}
            />
          </div>
        </React.Fragment>
      ))}

      {/* 追加ボタン */}
      <ResponsiveForm.Spacer />
      <div className="col-span-full">
        <IconButton icon={Icon.PlusCircleIcon} outline mini onClick={handleAppend}>
          追加
        </IconButton>
      </div>
    </>
  )
}
