import * as ReactHookForm from "react-hook-form"
import { ATTR_TYPE, SchemaDefinitionGlobalState, TYPE_COMMAND_MODEL } from "../../../types";
import * as UI from "../../../UI"
import { Allotment, LayoutPriority } from "allotment";
import { DecsendantsGrid } from "./DecsendantsGrid";

/**
 * ルート集約1個分の編集ペイン
 */
export function AggregatePane(props: {
  selectedRootAggregateIndex: number
  formMethods: ReactHookForm.UseFormReturn<SchemaDefinitionGlobalState>
  className?: string
}) {
  const {
    selectedRootAggregateIndex,
    formMethods: { register, control, watch },
    className,
  } = props

  const rootAggregate = ReactHookForm.useWatch({ name: `xmlElementTrees.${selectedRootAggregateIndex}.xmlElements.0`, control })

  return (
    <UI.MentionCellDataSourceContext.Provider value={watch()}>
      <div className={`flex flex-col gap-1 ${className ?? ''}`}>

        {/* ヘッダ */}
        <div className="flex flex-wrap items-center gap-1 px-1">

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

          {/* TODO: 削除ボタン */}
        </div>

        <Allotment
          vertical
          proportionalLayout={false} // 一部のペインのみ伸縮するようにする
        >
          {/* ルート集約の属性 */}
          <Allotment.Pane preferredSize={120} snap minSize={80}>
            <div className="w-full h-full p-1 flex flex-col gap-1">

              {/* ルート集約のコメント */}
              <ReactHookForm.Controller
                control={control}
                name={`xmlElementTrees.${selectedRootAggregateIndex}.xmlElements.0.comment`}
                render={({ field }) => (
                  <div className="w-full border border-gray-700 px-1">
                    <UI.SchemaDefinitionMentionTextarea
                      {...field}
                      className="w-full"
                      placeholder="コメントを入力..."
                    />
                  </div>
                )}
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
              trigger={() => Promise.resolve()} // TODO
              getValidationResult={() => { return { _own: [] } }} // TODO
              className="w-full h-full pt-1"
            />
          </Allotment.Pane>
        </Allotment>

      </div>
    </UI.MentionCellDataSourceContext.Provider>
  )
}
