import { Allotment, LayoutPriority } from 'allotment';
export { QueryModelLoadView } from './QueryModelLoadView';
import SchemaGraph from './SchemaGraph';
import React from 'react';
import RootAggregateView, { RootAggregateViewProps } from './RootAggregateView';
import { buildMetadataAndHelper, MetadataContext } from './MetadataContext';

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
      <Allotment
        proportionalLayout={false}
        className={`relative flex flex-col ${className ?? ''}`}
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

        <Allotment.Pane minSize={40} className="p-1">
          <RootAggregateView {...detailPaneProps} />
        </Allotment.Pane>
      </Allotment>
    </MetadataContext.Provider>
  )
}
