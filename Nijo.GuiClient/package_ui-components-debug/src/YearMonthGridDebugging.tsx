import * as YearMonthGrid from "@nijo/ui-components/layout/YearMonthGrid";

type TestBodyItem = YearMonthGrid.BodyItem & {
  text: string
}
type TestData = {
  bodyItems: TestBodyItem[]
}

export default function () {
  return (
    <YearMonthGrid.Grid
      since="2024/01"
      until="2024/12"
      rows={testRows}
      renderLeftColumns={renderLeftColumns}
      renderBodyItem={renderBodyItem}
      renderRightColumns={renderRightColumns}
      className="w-3/4 resize-x border border-gray-600"
      calendarBodyWidthPx={144}
    />
  )
}

const renderLeftColumns: YearMonthGrid.FixedColumnRenderer<TestData, TestBodyItem>[] = [
  {
    header: 'タスク名',
    body: props => (
      <div>
        タスク {props.rowIndex + 1}
      </div>
    ),
  },
  {
    widthPx: 150,
    body: props => (
      <div>
        このタスクには要素が {props.row.bodyItems.length} 個含まれています。
      </div>
    )
  }
];

const renderBodyItem = (props: YearMonthGrid.BodyItemRendererProps<TestData, TestBodyItem>) => {
  const { row, rowIndex, item } = props;

  if (item.since === item.until) {
    return (
      <div className="h-full mx-1 text-sm bg-amber-100 border border-amber-400 overflow-hidden whitespace-pre-wrap text-ellipsis">
        {item.text} ({item.since})
      </div>
    );
  } else {
    return (
      <div className="h-full mx-1 text-sm bg-emerald-100 border border-emerald-400 overflow-hidden whitespace-pre-wrap text-ellipsis">
        {item.text} ({item.since} - {item.until})
      </div>
    );
  }
}

const renderRightColumns: YearMonthGrid.FixedColumnRenderer<TestData, TestBodyItem>[] = [
  {
    header: '備考',
    widthPx: 200,
    body: props => (
      <div>
        備考欄です。タスク {props.rowIndex + 1}
      </div>
    ),
  }
];

const testRows: TestData[] = [
  {
    bodyItems: [
      { heightPx: 54, text: "開発フェーズ。\n8月はサポート期間。", since: '2024/05', until: '2024/08' },
      { heightPx: 28, text: "内部テスト完了", since: '2024/06', until: '2024/06' },
      { heightPx: 28, text: "本番リリース", since: '2024/07', until: '2024/07' },
      { heightPx: 28, text: "請求予定", since: '2024/07', until: '2024/07' },
    ],
  },
  {
    bodyItems: [
      { heightPx: 28, text: "コメントです。", since: '2024/05', until: '2024/08' },
      { heightPx: 28, text: "内部テスト完了", since: '2024/06', until: '2024/06' },
      { heightPx: 28, text: "本番リリース", since: '2024/07', until: '2024/07' },
      { text: '長期タスク', since: '2023/12', until: '2024/06', heightPx: 24 },
      { text: '長期タスク', since: '2024/07', until: '2025/11', heightPx: 24 },
    ],
  },
  ...Array.from({ length: 20 }, (_, i) => ({
    bodyItems: [
      { heightPx: 28, text: `タスク完了 ${i + 1}`, since: '2024/09', until: '2024/09' } satisfies TestBodyItem,
    ],
  })),
];
