import React from "react";
import * as ReactHookForm from "react-hook-form"
import * as Icon from "@heroicons/react/24/outline"
import { ATTR_IS_GENERIC_LOOKUP_TABLE, ATTR_IS_QUERYABLE, ATTR_IS_STORED, EditingProject } from "../../../backend";
import * as UI from "../../../UI"
import { Allotment, LayoutPriority } from "allotment";
import DecsendantsGrid from "./DecsendantsGrid";
import RootAggregateAttrs from "./RootAggregateAttrs";
import GenericLookupTableCategoriesPane from "./GenericLookupTableCategoriesPane";
import { RootAggregateLocation } from "../../rootAggregateLocation";
import { useNotifyDiagramStructureChanged } from "../Diagram";

/**
 * ルート集約1個分の編集ペイン
 */
function AggregatePane(props: {
  rootLocation: RootAggregateLocation
  formMethods: ReactHookForm.UseFormReturn<EditingProject>
  className?: string
  onRequestDelete?: () => void
  orientation?: 'horizontal' | 'vertical'
  onSwitchOrientation?: () => void
}) {
  const {
    rootLocation,
    formMethods: { register, getValues, setValue, control },
    className,
    onRequestDelete,
    orientation,
    onSwitchOrientation,
  } = props

  const rootPath = `${rootLocation.list}.${rootLocation.index}` as const

  // ダイアグラムの構成に影響する編集の通知
  const notifyStructureChanged = useNotifyDiagramStructureChanged()

  const handleDelete = () => {
    const physicalName = getValues(`${rootPath}.physicalName`)
    if (!confirm(`「${physicalName}」を削除しますか？`)) return
    onRequestDelete?.()
  }

  const isGenericLookupTable = ReactHookForm.useWatch({ name: `${rootPath}.attributes.${ATTR_IS_GENERIC_LOOKUP_TABLE}`, control }) === true

  // データ構造の IsStored, IsQueryable 属性の変更
  const [isStored, isQueryable] = ReactHookForm.useWatch({
    name: [`${rootPath}.attributes.${ATTR_IS_STORED}`, `${rootPath}.attributes.${ATTR_IS_QUERYABLE}`],
    control,
  })
  const handleDataStructureFlagsChange = (flags: UI.DataStructureFlags) => {
    setValue(`${rootPath}.attributes.${ATTR_IS_STORED}`, flags.isStored, { shouldDirty: true })
    setValue(`${rootPath}.attributes.${ATTR_IS_QUERYABLE}`, flags.isQueryable, { shouldDirty: true })
    notifyStructureChanged()
  }

  return (
    <div className={`flex flex-col gap-1 ${className ?? ''}`}>

      {/* ヘッダ */}
      <div className="flex flex-wrap items-center gap-1 p-1">

        {/* 分割方向切り替え */}
        {onSwitchOrientation && (
          <UI.Button
            icon={orientation === 'vertical' ? Icon.ArrowsRightLeftIcon : Icon.ArrowsUpDownIcon}
            mini
            hideText
            onClick={onSwitchOrientation}
          >
            分割方向切り替え
          </UI.Button>
        )}

        {/* ルート集約名 */}
        {/* TODO: GUI上ではDisplayNameを編集し、LocalNameへの変換はサーバー側で行うようにする */}
        <UI.WordTextBox
          {...register(`${rootPath}.physicalName`, { onChange: notifyStructureChanged })}
          className="flex-1 font-bold"
        />

        {/* データ構造の IsStored, IsQueryable 属性 */}
        {rootLocation.list === 'dataStructures' && (
          <UI.DataStructureFlagsCheckBoxes
            value={{ isStored: isStored === true, isQueryable: isQueryable === true }}
            onChange={handleDataStructureFlagsChange}
            className="px-1"
          />
        )}

        {/* 削除ボタン */}
        <UI.Button icon={Icon.TrashIcon} mini hideText onClick={handleDelete}>
          削除
        </UI.Button>
      </div>

      <Allotment
        vertical
        proportionalLayout={false} // 一部のペインのみ伸縮するようにする
        className="px-1"
      >
        {/* ルート集約の属性 */}
        <Allotment.Pane preferredSize={120} snap minSize={80}>
          <div className="w-full h-full p-1 bg-gray-200 border-t border-x border-gray-300 overflow-auto">
            <RootAggregateAttrs
              rootLocation={rootLocation}
              formMethods={props.formMethods}
              className="w-full"
            />
          </div>
        </Allotment.Pane>

        {/* 子孫集約 */}
        <Allotment.Pane
          priority={LayoutPriority.High}
          minSize={80}
          visible={rootLocation.list !== 'commands'}
        >
          <DecsendantsGrid
            rootLocation={rootLocation}
            formMethods={props.formMethods}
            className="w-full h-full py-1"
          />
        </Allotment.Pane>

        {/* 汎用参照テーブルのカテゴリ定義 */}
        {isGenericLookupTable && (
          <Allotment.Pane
            preferredSize={200}
            minSize={80}
          >
            <div className="w-full h-full p-1 bg-gray-100 border-t border-x border-gray-300 overflow-auto">
              <GenericLookupTableCategoriesPane
                rootLocation={rootLocation}
                formMethods={props.formMethods}
                className="w-full"
              />
            </div>
          </Allotment.Pane>
        )}
      </Allotment>

    </div>
  )
}

export default React.memo(AggregatePane)

