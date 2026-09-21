import React from "react"
import * as ReactHookForm from "react-hook-form"
import { Allotment, LayoutPriority } from "allotment"
import { PlusIcon } from "@heroicons/react/24/outline"
import { Button } from "../../ui"
import {
  createNewSchemaNode,
  NODE_TYPE_STATIC_ENUM,
  type EditingProject,
} from "../../features/backend"
import { SideMenu } from "./SideMenu"
import { EditorOfDynamic, type EditorOfDynamicRef } from "./EditorOfDynamic"
import { getStaticEnumElementId, EditorOfStatic } from "./EditorOfStatic"

/**
 * 区分定義の編集画面。
 * 静的区分（列挙体）は種類ごとに、動的区分（区分マスタ）の種類はまとめて1つのグリッドで編集する。
 * 左側の目次から各区分の編集欄へ移動できる。
 * 編集対象のデータは親のフォームのコンテキストから取得する。
 */
export function DynamicAndStaticEnumPane() {

  const { control } = ReactHookForm.useFormContext<EditingProject>()

  // 静的区分の種類の追加・削除
  const { fields, append, remove } = ReactHookForm.useFieldArray({ control, name: "staticEnums" })

  const handleAddStaticEnum = () => {
    append({ root: createNewSchemaNode(0, NODE_TYPE_STATIC_ENUM), values: [] })
  }

  const handleRemoveStaticEnum = (index: number) => {
    if (!window.confirm("この区分を削除しますか？")) return
    remove(index)
  }

  // 目次から動的区分の種類の行を選択するためのグリッドの参照
  const dynamicEnumTypeEditorRef = React.useRef<EditorOfDynamicRef>(null)

  const handleClickStaticEnumLink = (uniqueId: string) => {
    document.getElementById(getStaticEnumElementId(uniqueId))?.scrollIntoView({ block: "start", behavior: "smooth" })
  }

  const handleClickDynamicEnumTypeLink = (index: number) => {
    // 動的区分の種類は1つのグリッドにまとまっているため、グリッドまでスクロールした上で該当行を選択する
    document.getElementById(DYNAMIC_ENUM_TYPES_ELEMENT_ID)?.scrollIntoView({ block: "start", behavior: "smooth" })
    dynamicEnumTypeEditorRef.current?.selectRow(index)
  }

  return (
    <Allotment proportionalLayout={false} separator={false}>

      {/* 目次 */}
      <Allotment.Pane preferredSize={240} minSize={120}>
        <SideMenu
          onClickStaticEnum={handleClickStaticEnumLink}
          onClickDynamicEnumType={handleClickDynamicEnumTypeLink}
        />
      </Allotment.Pane>

      {/* 編集欄 */}
      <Allotment.Pane priority={LayoutPriority.High}>
        <div className="w-full h-full overflow-y-auto">
          <div className="h-full max-w-4xl flex flex-col gap-8 p-2">

            {/* 静的区分（列挙体） */}
            <section className="flex flex-col gap-4">
              <h2 className="font-bold text-xl select-none">
                静的区分（列挙体）
              </h2>
              <span className="text-xs text-gray-700">
                区分値がソースコード上にハードコードされる区分。<br />
                運用中に自動的に区分が追加・削除されることがなく、
                追加・削除する際にはプログラム改修が必要になるもの。
              </span>
              {fields.map((field, index) => (
                <EditorOfStatic
                  key={field.id}
                  index={index}
                  onRemove={() => handleRemoveStaticEnum(index)}
                />
              ))}
              <Button inline Icon={PlusIcon} onClick={handleAddStaticEnum} className="self-start">
                静的区分を追加
              </Button>
            </section>

            {/* 動的区分 */}
            <section id={DYNAMIC_ENUM_TYPES_ELEMENT_ID} className="flex flex-col gap-4">
              <h2 className="font-bold text-xl select-none">
                動的区分（区分マスタ）
              </h2>
              <span className="text-xs text-gray-700">
                区分値がソースコード上にハードコードされない区分。
                運用中に自動的に区分が追加・削除される可能性があるもの。<br />
                区分値はデータベースに登録される。
              </span>
              <EditorOfDynamic ref={dynamicEnumTypeEditorRef} />
            </section>

            {/* スクロール用の空白 */}
            <div className="min-h-[50%]"></div>
          </div>
        </div>
      </Allotment.Pane>
    </Allotment>
  )
}

/** 目次からのリンク先となる、動的区分の種類の編集欄の要素ID */
const DYNAMIC_ENUM_TYPES_ELEMENT_ID = "dynamic-enum-types"
