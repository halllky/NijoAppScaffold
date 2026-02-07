import { Allotment, LayoutPriority } from "allotment";
import React from "react";
import * as ReactHookForm from "react-hook-form"
import { usePersonalSettings } from "../../Settings";
import { SchemaDefinitionGlobalState } from "../../types";
import { Diagram } from "./Diagram";
import { AggregatePane } from "./AggregatePane";

/**
 * データ構造定義タブ。
 *
 * スキーマ定義の有向グラフを表示する。
 * 特定のルート集約が選択されている場合はその集約の編集ペインを表示する。
 */
export default function ({ formMethods }: {
  formMethods: ReactHookForm.UseFormReturn<SchemaDefinitionGlobalState>
}) {

  const xmlElementTrees = ReactHookForm.useWatch({ name: "xmlElementTrees", control: formMethods.control })

  // 個人設定
  const { personalSettings, save: savePersonalSettings } = usePersonalSettings()

  // 選択中のルート集約
  const [selectedRootAggregateIndex, setSelectedRootAggregateIndex] = React.useState<number | undefined>(undefined);
  const selectRootAggregate = (aggregateId: string | null) => {
    if (aggregateId === null) {
      setSelectedRootAggregateIndex(undefined)
      setAggPaneVisible(false)
      return;
    }
    const index = xmlElementTrees?.findIndex(tree => tree.xmlElements?.[0]?.uniqueId === aggregateId)
    if (index === undefined || index === -1) return;
    setSelectedRootAggregateIndex(index)
    setAggPaneVisible(true)
  }

  // 選択中のルート集約を画面右側に表示する
  const [aggPaneVisible, setAggPaneVisible] = React.useState(false)
  const aggPaneOrientation = personalSettings.aggPaneOrientation ?? 'horizontal'
  const setAggPaneOrientation = React.useCallback((orientation: 'horizontal' | 'vertical') => {
    savePersonalSettings('aggPaneOrientation', orientation)
  }, [savePersonalSettings])

  return (
    <Allotment
      key={aggPaneOrientation} // 配置方向変更時にAllotmentを再生成してレイアウトをリセット
      vertical={aggPaneOrientation === 'vertical'}
      proportionalLayout={false} // 特定のペインだけ伸縮させる
      separator={false}
      className="pt-1"
    >
      {/* ダイアグラム */}
      <Allotment.Pane priority={LayoutPriority.High}>
        <Diagram
          formMethods={formMethods}
          onSelectedRootAggregateChanged={selectRootAggregate}
          className="h-full w-full border-t border-gray-400"
        />
      </Allotment.Pane>

      {/* ルート集約編集ペイン */}
      <Allotment.Pane preferredSize="50%" visible={aggPaneVisible}>
        {selectedRootAggregateIndex !== undefined && (
          <AggregatePane
            key={selectedRootAggregateIndex}
            selectedRootAggregateIndex={selectedRootAggregateIndex}
            formMethods={formMethods}
            className={`h-full w-full border-gray-400 ${aggPaneOrientation === 'vertical' ? 'border-t' : 'border-l'}`}
          />
        )}
      </Allotment.Pane>
    </Allotment>
  )
}
