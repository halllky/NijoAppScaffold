import React from "react";
import * as ReactHookForm from "react-hook-form"
import * as Icon from "@heroicons/react/24/outline"
import { ATTR_TYPE, ApplicationState, TYPE_COMMAND_MODEL } from "../../../types";
import * as UI from "../../../UI"
import { Allotment, LayoutPriority } from "allotment";
import { DecsendantsGrid } from "./DecsendantsGrid";
import RootAggreagateAttrs from "./RootAggreagateAttrs";

/**
 * ルート集約1個分の編集ペイン
 */
export function AggregatePane(props: {
  selectedRootAggregateIndex: number
  formMethods: ReactHookForm.UseFormReturn<ApplicationState>
  className?: string
  onRequestDelete?: () => void
  orientation?: 'horizontal' | 'vertical'
  onSwitchOrientation?: () => void
}) {
  const {
    selectedRootAggregateIndex,
    formMethods: { register, control, watch },
    className,
    onRequestDelete,
    orientation,
    onSwitchOrientation,
  } = props

  const handleDelete = () => {
    if (!confirm(`「${rootAggregate.localName}」を削除しますか？`)) return
    onRequestDelete?.()
  }

  const rootAggregate = ReactHookForm.useWatch({ name: `xmlElementTrees.${selectedRootAggregateIndex}.xmlElements.0`, control })

  return (
    <UI.MentionCellDataSourceContext.Provider value={watch()}>
      <div className={`flex flex-col gap-1 ${className ?? ''}`}>

        {/* ヘッダ */}
        <div className="flex flex-wrap items-center gap-1 px-1 pt-1">

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
            {...register(`xmlElementTrees.${selectedRootAggregateIndex}.xmlElements.0.localName`)}
            className="flex-1 font-bold"
          />

          {/* モデル */}
          <ReactHookForm.Controller
            control={control}
            name={`xmlElementTrees.${selectedRootAggregateIndex}.xmlElements.0.attributes.${ATTR_TYPE}`}
            render={({ field }) => (
              <UI.ModelTypeSelector {...field} className="min-w-48" />
            )}
          />

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
              <RootAggreagateAttrs
                selectedRootAggregateIndex={selectedRootAggregateIndex}
                formMethods={props.formMethods}
                className="w-full"
              />
            </div>
          </Allotment.Pane>

          {/* 子孫集約 */}
          <Allotment.Pane
            priority={LayoutPriority.High}
            minSize={80}
            visible={rootAggregate.attributes[ATTR_TYPE] !== TYPE_COMMAND_MODEL}
          >
            <DecsendantsGrid
              selectedRootAggregateIndex={selectedRootAggregateIndex}
              formMethods={props.formMethods}
              trigger={React.useCallback(() => Promise.resolve(), [])} // TODO
              getValidationResult={React.useCallback(() => { return { _own: [] } }, [])} // TODO
              className="w-full h-full py-1"
            />
          </Allotment.Pane>
        </Allotment>

      </div>
    </UI.MentionCellDataSourceContext.Provider>
  )
}
