import React from "react"
import * as ReactHookForm from "react-hook-form"
import * as Icon from "@heroicons/react/24/solid"
import { UUID } from "uuidjs"
import { ApplicationState, ATTR_DISPLAY_NAME, ATTR_TYPE, TYPE_VALUE_OBJECT_MODEL2, XmlElementAttributeName, XmlElementItem } from "../../types"
import * as UI from '../../UI'

type FormType = ApplicationState
const ATTR_LATIN_NAME = 'LatinName' as XmlElementAttributeName

export default function ValueObjectGrid(props: {
  formMethods: ReactHookForm.UseFormReturn<FormType>
}) {
  const { control, setValue, getValues } = props.formMethods
  const xmlElementTrees = ReactHookForm.useWatch({
    control,
    name: 'xmlElementTrees',
  }) ?? []

  const valueObjectIndexes = React.useMemo(() => {
    return xmlElementTrees
      .map((tree, index) => ({ tree, index }))
      .filter(({ tree }) => tree.xmlElements?.[0]?.attributes?.[ATTR_TYPE] === TYPE_VALUE_OBJECT_MODEL2)
      .map(({ index }) => index)
  }, [xmlElementTrees])

  const handleAddValueObject = () => {
    const newValueObject: XmlElementItem = {
      uniqueId: UUID.generate(),
      indent: 0,
      localName: '',
      value: undefined,
      attributes: { [ATTR_TYPE]: TYPE_VALUE_OBJECT_MODEL2 },
      comment: undefined,
    }
    const current = getValues('xmlElementTrees') ?? []
    setValue('xmlElementTrees', [...current, { xmlElements: [newValueObject] }])
  }

  return (
    <div className="flex flex-col gap-3 py-2">
      {valueObjectIndexes.map(index => {
        const uniqueId = xmlElementTrees[index].xmlElements[0]?.uniqueId
        return (
          <div key={uniqueId ?? index} id={`value-object-def-${uniqueId}`}>
            <SingleValueObjectEditor index={index} formMethods={props.formMethods} />
          </div>
        )
      })}

      <div>
        <UI.Button icon={Icon.PlusIcon} onClick={handleAddValueObject}>
          新しい Value Object 2 を追加
        </UI.Button>
      </div>
    </div>
  )
}

function SingleValueObjectEditor({ index, formMethods }: {
  index: number
  formMethods: ReactHookForm.UseFormReturn<FormType>
}) {
  const { register, getValues, setValue } = formMethods

  const rootNamePath = `xmlElementTrees.${index}.xmlElements.0.localName` as const
  const rootDisplayNamePath = `xmlElementTrees.${index}.xmlElements.0.attributes.${ATTR_DISPLAY_NAME}` as const
  const rootLatinNamePath = `xmlElementTrees.${index}.xmlElements.0.attributes.${ATTR_LATIN_NAME}` as const
  const rootCommentPath = `xmlElementTrees.${index}.xmlElements.0.comment` as const

  const handleDelete = () => {
    if (!window.confirm('この Value Object 2 を削除しますか？')) return
    const current = [...(getValues('xmlElementTrees') ?? [])]
    current.splice(index, 1)
    setValue('xmlElementTrees', current)
  }

  return (
    <div className="flex flex-col gap-2 border border-gray-300 bg-gray-50 p-3 rounded">
      <div className="flex items-center gap-2">
        <UI.WordTextBox
          {...register(rootNamePath)}
          className="basis-80 border px-1 font-bold"
          placeholder="値オブジェクト名"
        />
        <UI.WordTextBox
          {...register(rootDisplayNamePath)}
          className="basis-72 border px-1"
          placeholder="表示名"
        />
        <UI.WordTextBox
          {...register(rootLatinNamePath)}
          className="basis-72 border px-1"
          placeholder="LatinName"
        />
        <UI.Button mini hideText icon={Icon.TrashIcon} onClick={handleDelete}>
          Value Object 2 の削除
        </UI.Button>
      </div>

      <textarea
        {...register(rootCommentPath)}
        className="min-h-20 border border-gray-300 bg-white px-2 py-1 text-sm"
        placeholder="コメント"
      />
    </div>
  )
}
