import { Allotment, LayoutPriority } from "allotment";
import React from "react";
import * as ReactHookForm from "react-hook-form"
import { PlusIcon, TrashIcon } from "@heroicons/react/24/solid";
import { usePersonalSettings } from "../../PersonalSettings";
import { EditingProject, EditingRootAggregate, MODEL_COMMAND } from "../../backend";
import { Button } from "../../UI";
import { Diagram, DiagramRef, DiagramStructureProvider, useNotifyDiagramStructureChanged } from "./Diagram";
import { DiagramSearchBox } from "./diagram-searching";
import AggregatePane from "./AggregatePane";
import { NewRootAddDialog } from "./NewRootAddDialog";
import { UUID } from "uuidjs";
import { RootAggregateLocation, findRootAggregateLocation } from "../rootAggregateLocation";

export type DataStructureTabRef = {
  selectRootAggregate: (rootOrDescendantXmlElementUniqueId: string | undefined | null) => void
}

/**
 * データ構造定義タブ。
 *
 * スキーマ定義の有向グラフを表示する。
 * 特定のルート集約が選択されている場合はその集約の編集ペインを表示する。
 */
function DataStructureTab(props: {
  visible: boolean
  formMethods: ReactHookForm.UseFormReturn<EditingProject>
  dataStructureRef: React.RefObject<DataStructureTabRef | null>
  diagramRef: React.RefObject<DiagramRef | null>
}) {
  // ダイアグラムの構成は、構成に影響する編集があったときだけ組み立て直す。
  // データ構造タブ全体（ダイアグラムと編集ペインの双方）が通知する側になるため、両方の外側で保持する
  return (
    <DiagramStructureProvider formMethods={props.formMethods}>
      <DataStructureTabBody {...props} />
    </DiagramStructureProvider>
  )
}

export default React.memo(DataStructureTab)

// -------------------------------------

function DataStructureTabBody({ visible, formMethods, dataStructureRef, diagramRef }: {
  visible: boolean
  formMethods: ReactHookForm.UseFormReturn<EditingProject>
  dataStructureRef: React.RefObject<DataStructureTabRef | null>
  diagramRef: React.RefObject<DiagramRef | null>
}) {

  const notifyStructureChanged = useNotifyDiagramStructureChanged()

  // 個人設定
  const { personalSettings, save: savePersonalSettings } = usePersonalSettings()

  // ダイアグラムで選択中のルート集約の uniqueId（ハイライト表示のみ。編集ペインの表示とは独立）
  const [selectedRootIds, setSelectedRootIds] = React.useState<ReadonlySet<string>>(new Set())
  // 検索にヒットしたルート集約の uniqueId。検索していないときは undefined
  const [hitRootIds, setHitRootIds] = React.useState<ReadonlySet<string> | undefined>(undefined)
  // ダイアグラムのノードをドラッグ中かどうか
  const [isDiagramDragging, setIsDiagramDragging] = React.useState(false)

  // 編集ペインに表示中のルート集約
  const [rootLocation, setRootLocation] = React.useState<RootAggregateLocation | undefined>(undefined)
  const [aggPaneVisible, setAggPaneVisible] = React.useState(false)
  const aggPaneOrientation = personalSettings.aggPaneOrientation ?? 'horizontal'
  const handleSwitchAggPaneOrientation = React.useCallback(() => {
    savePersonalSettings('aggPaneOrientation', aggPaneOrientation === 'horizontal' ? 'vertical' : 'horizontal')
  }, [aggPaneOrientation, savePersonalSettings])

  /**
   * 指定のルート集約（またはその子孫）の編集ペインを開く。
   * scroll が true の場合、ダイアグラム上でそのルート集約を選択し、表示領域の中央に移動する。
   * ルート集約の新規作成直後などフォームへの反映がまだ済んでいないタイミングにも対応できるよう、
   * 少し待ってからフォームの最新値で位置を探し直す。
   */
  const openRootAggregate = React.useCallback((rootOrDescendantUniqueId: string, scroll: boolean) => {
    const location = findRootAggregateLocation(formMethods.getValues(), rootOrDescendantUniqueId)
    if (!location) return
    setRootLocation(location)
    setAggPaneVisible(true)

    if (scroll) {
      window.setTimeout(() => {
        if (!diagramRef.current) return
        const freshProject = formMethods.getValues()
        const freshLocation = findRootAggregateLocation(freshProject, rootOrDescendantUniqueId)
        if (!freshLocation) return
        const rootUniqueId = freshProject[freshLocation.list][freshLocation.index]?.uniqueId
        if (!rootUniqueId) return
        setSelectedRootIds(new Set([rootUniqueId]))
        diagramRef.current.panToRootAggregate(rootUniqueId)
      }, 300)
    }
  }, [formMethods, diagramRef])

  React.useImperativeHandle(dataStructureRef, () => ({
    selectRootAggregate: id => {
      if (!id) {
        setRootLocation(undefined)
        setAggPaneVisible(false)
        return
      }
      openRootAggregate(id, true)
    },
  }), [openRootAggregate])

  /** ダイアグラムの背景など、何もない場所を押したら編集ペインを閉じる */
  const handleClosePane = () => {
    setRootLocation(undefined)
    setAggPaneVisible(false)
  }

  // 新規ルート集約作成ダイアログ
  const dataStructuresArray = ReactHookForm.useFieldArray({ name: "dataStructures", control: formMethods.control })
  const commandsArray = ReactHookForm.useFieldArray({ name: "commands", control: formMethods.control })
  const [isNewRootDialogOpen, setIsNewRootDialogOpen] = React.useState(false)
  const handleRegisterNewRoot = (name: string, modelType: string) => {
    const list: RootAggregateLocation['list'] = modelType === MODEL_COMMAND ? 'commands' : 'dataStructures'
    const targetArray = list === 'dataStructures' ? dataStructuresArray : commandsArray
    const newUniqueId = UUID.generate()
    const newRoot: EditingRootAggregate = {
      uniqueId: newUniqueId,
      physicalName: name,
      model: modelType,
      attributes: {},
      uniqueConstraints: [],
      members: [],
    }

    targetArray.append(newRoot)
    notifyStructureChanged()
    setIsNewRootDialogOpen(false)

    // 新規作成したルート集約を選択・表示状態にする。フィールド配列への反映を待つためにsetTimeoutを入れる
    setTimeout(() => openRootAggregate(newUniqueId, true), 0)
  }

  /** 編集ペインに表示中のルート集約を、子孫ごと削除する */
  const handleDeleteOpenRootAggregate = React.useCallback(() => {
    if (rootLocation !== undefined) {
      if (rootLocation.list === 'dataStructures') dataStructuresArray.remove(rootLocation.index)
      else commandsArray.remove(rootLocation.index)
      notifyStructureChanged()
    }
    setRootLocation(undefined)
    setAggPaneVisible(false)
  }, [dataStructuresArray, commandsArray, rootLocation, notifyStructureChanged])

  /** ダイアグラムで選択中のルート集約を、子孫ごと削除する */
  const handleDeleteSelectedRootAggregate = () => {
    if (selectedRootIds.size !== 1) return
    const [selectedId] = selectedRootIds
    const location = findRootAggregateLocation(formMethods.getValues(), selectedId)
    if (!location) return
    const rootPath = `${location.list}.${location.index}` as const
    const name = formMethods.getValues(`${rootPath}.physicalName`) || "(名前未設定)"
    if (!window.confirm(`ルート集約「${name}」を削除しますか？`)) return

    setSelectedRootIds(new Set())
    if (location.list === 'dataStructures') dataStructuresArray.remove(location.index)
    else commandsArray.remove(location.index)
    notifyStructureChanged()

    // 削除したルート集約が編集ペインに表示中だった場合は閉じる
    if (rootLocation?.list === location.list && rootLocation.index === location.index) {
      setRootLocation(undefined)
      setAggPaneVisible(false)
    }
  }

  return (
    <Allotment
      key={aggPaneOrientation} // 配置方向変更時にAllotmentを再生成してレイアウトをリセット
      vertical={aggPaneOrientation === 'vertical'}
      proportionalLayout={false} // 特定のペインだけ伸縮させる
      separator={false}
      className={visible ? "" : "hidden"}
    >
      {/* ダイアグラム。
          ドラッグ中に右の編集欄を隠してもダイアグラム自体の大きさが変わらないよう、常に分割ペイン全体の幅で描画し、ペインの幅ではみ出た部分を隠している。
          React Flow はドラッグ開始時のダイアグラムの大きさで画面端の自動スクロールを判定するため、ドラッグ中に大きさが変わると、画面端でない位置で勝手にスクロールしてしまう。
          ダイアグラム上のどこかを押したら編集欄を閉じる。
          ダイアグラムの背景を押したときはライブラリがイベントの伝播を止めてしまうため、キャプチャフェーズで受け取っている */}
      <Allotment.Pane priority={LayoutPriority.High} minSize={240}>
        <div className="@container w-full h-full">
          <div className="w-[100cqw] h-full" onPointerDownCapture={handleClosePane}>
            <Diagram
              formMethods={formMethods}
              selectedIds={selectedRootIds}
              onSelectedIdsChanged={setSelectedRootIds}
              hitRootIds={hitRootIds}
              onOpenRequested={id => openRootAggregate(id, false)}
              onDraggingChanged={setIsDiagramDragging}
              diagramRef={diagramRef}
              className="h-full w-full"
            >
              {/* 操作説明 */}
              <span className="text-xs select-none">
                ドラッグでアイテムを移動できます。ダブルクリックでアイテムの詳細が表示されます。
              </span>

              <div className="flex gap-1 items-start">
                {/* ボタン */}
                <div className="flex flex-col gap-1">
                  <Button icon={PlusIcon} fill onClick={() => setIsNewRootDialogOpen(true)}>
                    新規作成
                  </Button>
                  <Button icon={TrashIcon} outline onClick={handleDeleteSelectedRootAggregate} disabled={selectedRootIds.size !== 1}>
                    選択中のルート集約を削除
                  </Button>
                </div>

                {/* 検索 */}
                <DiagramSearchBox formMethods={formMethods} onHitRootIdsChanged={setHitRootIds} />
              </div>

              <NewRootAddDialog
                open={isNewRootDialogOpen}
                onClose={() => setIsNewRootDialogOpen(false)}
                onRegister={handleRegisterNewRoot}
              />
            </Diagram>
          </div>
        </div>
      </Allotment.Pane>

      {/* ルート集約編集ペイン。
          ノードをまとめて動かすときに邪魔にならないようドラッグ中は隠す。
          隠している間もグリッドのスクロール位置や選択行が失われないよう、アンマウントせずに Activity で隠している */}
      <Allotment.Pane preferredSize="50%" visible={aggPaneVisible && !isDiagramDragging}>
        {rootLocation !== undefined && (
          <React.Activity mode={isDiagramDragging ? "hidden" : "visible"}>
            <AggregatePane
              key={`${rootLocation.list}-${rootLocation.index}`}
              rootLocation={rootLocation}
              formMethods={formMethods}
              className={`h-full w-full border-gray-400 ${aggPaneOrientation === 'vertical' ? 'border-t' : 'border-l'}`}
              onRequestDelete={handleDeleteOpenRootAggregate}
              onRootLocationChanged={setRootLocation}
              orientation={aggPaneOrientation}
              onSwitchOrientation={handleSwitchAggPaneOrientation}
            />
          </React.Activity>
        )}
      </Allotment.Pane>
    </Allotment>
  )
}
