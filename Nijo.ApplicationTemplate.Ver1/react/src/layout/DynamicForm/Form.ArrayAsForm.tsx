import React from "react"
import * as ReactHookForm from "react-hook-form"
import { DynamicFormContext } from "./DynamicFormContext"
import { ArrayMember, FormRendererProps, MemberOwner } from "./types"
import { VForm2 } from "../VForm2"
import { MembersGroupByBreakPoint } from "./Form.Members"
import { IconButton } from "../../input"
import useEvent from "react-use-event-hook"

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
  const { fields, append, remove } = ReactHookForm.useFieldArray({
    control: useFormReturn.control,
    name: arrayMemberPath,
  })

  // レンダリング処理の引数
  const rendererProps: FormRendererProps = {
    name: arrayMemberPath,
    useFormReturn,
    owner,
  }

  // 追加
  const handleAppend = useEvent(() => {
    const newItem = array.onCreateNewItem()
    append(newItem)
  })

  return (
    <VForm2.Indent label={(
      <>
        <VForm2.LabelText>
          {array.displayName ?? array.physicalName}
        </VForm2.LabelText>

        {/* ラベルの脇に追加のコンポーネントがある場合はレンダリング */}
        {array.renderFormLabel?.(rendererProps)}
      </>
    )}>

      {/* レンダリング処理が明示されている場合はそれが優先 */}
      {array.render ? (
        array.render(rendererProps)
      ) : (
        // 既定のレンダリング
        <>
          {fields.map((field, index) => (
            <VForm2.Indent key={field.id} label={(
              <>
                <VForm2.LabelText>
                  {`${index + 1}`}
                </VForm2.LabelText>
                <IconButton outline onClick={() => remove(index)}>
                  削除
                </IconButton>
              </>
            )}>
              <MembersGroupByBreakPoint
                owner={array}
                ancestorsPath={`${arrayMemberPath}.${index}`}
              />
            </VForm2.Indent>
          ))}
          <IconButton outline onClick={handleAppend}>
            追加
          </IconButton>
        </>
      )}
    </VForm2.Indent>
  )
}