import { Allotment, LayoutPriority } from "allotment";
import React from "react";
import * as ReactHookForm from "react-hook-form"
import { usePersonalSettings } from "../../PersonalSettings";
import { ATTR_TYPE, ModelPageForm, ApplicationState } from "../../types";
import { Button } from "../../UI";
import { Diagram } from "./Diagram";
import { AggregatePane } from "./AggregatePane";
import { NewRootAddDialog } from "./NewRootAddDialog";
import { UUID } from "uuidjs";
import { PlusIcon } from "@heroicons/react/24/solid";

/**
 * データ構造定義タブ。
 *
 * スキーマ定義の有向グラフを表示する。
 * 特定のルート集約が選択されている場合はその集約の編集ペインを表示する。
 */
export default function ({ visible, formMethods }: {
  visible: boolean
  formMethods: ReactHookForm.UseFormReturn<ApplicationState>
}) {

  const watchedXmlElementTrees = ReactHookForm.useWatch({ name: "xmlElementTrees", control: formMethods.control })

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
    const index = watchedXmlElementTrees?.findIndex(tree => tree.xmlElements?.[0]?.uniqueId === aggregateId)
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

  // 新規ルート集約作成ダイアログ
  const { append, remove } = ReactHookForm.useFieldArray({ name: "xmlElementTrees", control: formMethods.control })
  const [isNewRootDialogOpen, setIsNewRootDialogOpen] = React.useState(false)
  const handleRegisterNewRoot = (name: string, modelType: string) => {
    const newRoot: ModelPageForm = {
      xmlElements: [{
        uniqueId: UUID.generate(),
        indent: 0,
        localName: name,
        attributes: { [ATTR_TYPE]: modelType },
      }],
    }

    append(newRoot)
    setIsNewRootDialogOpen(false)

    // 新規作成したルート集約を選択状態にする
    // レンダリングを待つためにsetTimeoutを入れる
    setTimeout(() => {
      selectRootAggregate(newRoot.xmlElements[0].uniqueId)
      setSelectedRootAggregateIndex(watchedXmlElementTrees.length) // append後の長さはlength+1なのでindexはlengthになる
      setAggPaneVisible(true)
    }, 0)
  }

  // ルート集約削除
  const handleDeleteRootAggregate = () => {
    if (selectedRootAggregateIndex !== undefined) {
      remove(selectedRootAggregateIndex)
    }
    setSelectedRootAggregateIndex(undefined)
    setAggPaneVisible(false)
  }

  return (
    <Allotment
      key={aggPaneOrientation} // 配置方向変更時にAllotmentを再生成してレイアウトをリセット
      vertical={aggPaneOrientation === 'vertical'}
      proportionalLayout={false} // 特定のペインだけ伸縮させる
      separator={false}
      className={visible ? "" : "hidden"}
    >
      {/* ダイアグラム */}
      <Allotment.Pane priority={LayoutPriority.High}>
        <Diagram
          formMethods={formMethods}
          onSelectedRootAggregateChanged={selectRootAggregate}
          className="h-full w-full"
        >
          <Button onClick={() => setIsNewRootDialogOpen(true)} icon={PlusIcon} fill>
            新規作成
          </Button>
          <NewRootAddDialog
            open={isNewRootDialogOpen}
            onClose={() => setIsNewRootDialogOpen(false)}
            onRegister={handleRegisterNewRoot}
          />
        </Diagram>
      </Allotment.Pane>

      {/* ルート集約編集ペイン */}
      <Allotment.Pane preferredSize="50%" visible={aggPaneVisible}>
        {selectedRootAggregateIndex !== undefined && (
          <AggregatePane
            key={selectedRootAggregateIndex}
            selectedRootAggregateIndex={selectedRootAggregateIndex}
            formMethods={formMethods}
            className={`h-full w-full border-gray-400 ${aggPaneOrientation === 'vertical' ? 'border-t' : 'border-l'}`}
            onRequestDelete={handleDeleteRootAggregate}
            orientation={aggPaneOrientation}
            onSwitchOrientation={() => setAggPaneOrientation(aggPaneOrientation === 'horizontal' ? 'vertical' : 'horizontal')}
          />
        )}
      </Allotment.Pane>
    </Allotment>
  )
}
