import React from "react"
import * as ReactHookForm from "react-hook-form"
import * as Icon from "@heroicons/react/24/outline"
import { DynamicFormContext } from "./DynamicFormContext"
import { ArrayFormRendererProps, ArrayMember, MemberOwner } from "./types"
import { MembersGroupByBreakPoint } from "./Form.Members"
import { IconButton } from "../../input"
import useEvent from "react-use-event-hook"
import FormLayout from "../FormLayout"

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
    if (!array.onCreateNewItem) return;
    const newItem = array.onCreateNewItem()
    append(newItem)
  })

  return (
    <>
      {array.arrayLabel && (
        <FormLayout.Field
          fullWidth
          label={typeof array.arrayLabel === 'string' ? array.arrayLabel : undefined}
          labelEnd={typeof array.arrayLabel === 'function' ? array.arrayLabel(rendererProps) : undefined}
        />
      )}

      {/* 要素一覧 */}
      {fields.map((field, index) => (
        <React.Fragment key={field.id}>
          <FormLayout.Section
            border
            label={typeof array.itemLabel === 'string'
              ? `${array.itemLabel} ${index + 1}`
              : undefined}
            labelEnd={(
              <>
                {typeof array.itemLabel === 'function' ? array.itemLabel({
                  ...rendererProps,
                  itemName: `${arrayMemberPath}.${index}`,
                  itemIndex: index,
                }) : undefined}
                <IconButton icon={Icon.TrashIcon} outline mini onClick={() => remove(index)}>
                  削除
                </IconButton>
              </>
            )}
          >

            {array.contents ? (
              array.contents(rendererProps)
            ) : (
              <MembersGroupByBreakPoint
                owner={array}
                ancestorsPath={`${arrayMemberPath}.${index}`}
              />
            )}
          </FormLayout.Section>

          <FormLayout.Spacer />
        </React.Fragment>
      ))}

      {/* 追加ボタン */}
      {array.onCreateNewItem && (
        <FormLayout.Field fullWidth>
          <IconButton icon={Icon.PlusCircleIcon} outline mini onClick={handleAppend}>
            {typeof array.arrayLabel === 'string' ? `${array.arrayLabel} を追加` : '追加'}
          </IconButton>
        </FormLayout.Field>
      )}
    </>
  )
}
