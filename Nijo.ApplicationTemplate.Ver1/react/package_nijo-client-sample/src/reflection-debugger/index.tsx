import { Allotment, LayoutPriority } from 'allotment';
export { QueryModelLoadView } from './QueryModelLoadView';
import SchemaGraph from './SchemaGraph';
import React from 'react';
import RootAggregateView, { RootAggregateViewProps } from './RootAggregateView';
import { buildMetadataAndHelper, MetadataContext } from './MetadataContext';
import { UnhandledMessage, UnhandledMessageContextProvider } from '../util';

/**
 * スキーマ定義をもとにしたデバッグビュー
 */
export default function ({ className }: {
  className?: string
}) {
  const metadataContextValue = React.useMemo(() => {
    return buildMetadataAndHelper()
  }, [])

  // 選択されているルート集約のID
  const [selectedRootAggregateId, setSelectedRootAggregateId] = React.useState<string | null>(null)

  // 右側ペインに表示するビュー
  const detailPaneProps = React.useMemo((): RootAggregateViewProps => ({
    selectedRootAggregateId,
  }), [selectedRootAggregateId])

  return (
    <MetadataContext.Provider value={metadataContextValue}>
      <UnhandledMessageContextProvider>
        <div className={`flex flex-col gap-px ${className ?? ''}`}>

          {/* エラーメッセージ */}
          <UnhandledMessage />

          <Allotment
            proportionalLayout={false}
            className="flex-1"
          >
            <Allotment.Pane
              minSize={40}
              priority={LayoutPriority.High}
            >
              <SchemaGraph
                onSelectedRootAggregateChange={setSelectedRootAggregateId}
                className="h-full w-full"
              />
            </Allotment.Pane>

            <Allotment.Pane minSize={40} className="flex flex-col gap-1 p-1">
              <RootAggregateView className="flex-1" {...detailPaneProps} />
            </Allotment.Pane>
          </Allotment>
        </div>
      </UnhandledMessageContextProvider>
    </MetadataContext.Provider>
  )
}
