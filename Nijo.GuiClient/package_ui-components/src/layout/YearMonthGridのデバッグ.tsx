import * as YearMonthGrid from "./YearMonthGrid";

type TestBodyItem =
  | { type: 'range', appearance: 'コメント', text: string, since: YearMonthGrid.YearMonth, until: YearMonthGrid.YearMonth }
  | { type: 'point', appearance: 'マイルストーン', name: string, yearMonth: YearMonthGrid.YearMonth }

type TestData = {
  bodyItems: TestBodyItem[]
}

const testRows: TestData[] = [
  { bodyItems: [{ type: 'range', appearance: 'コメント', text: "コメントです。", since: '2024/05', until: '2024/08' }] },
  { bodyItems: [{ type: 'point', appearance: 'マイルストーン', name: "内部テスト完了", yearMonth: '2024/06' }] },
  { bodyItems: [{ type: 'point', appearance: 'マイルストーン', name: "本番リリース", yearMonth: '2024/07' }] },
];

export default function () {


  return (
    <YearMonthGrid.Grid
      since="2024/01"
      until="2024/12"
      rows={testRows}
      renderBodyItem={renderBodyItem}
    />
  )
}

const renderBodyItem = (props: YearMonthGrid.BodyItemRendererProps<TestData, TestBodyItem>) => {
  const { row, rowIndex, item } = props;

  switch (item.type) {
    case 'range':
      return (
        <div>
          <strong>{item.appearance}</strong>: {item.text} ({item.since} - {item.until})
        </div>
      );
    case 'point':
      return (
        <div>
          <strong>{item.appearance}</strong>: {item.name} ({item.yearMonth})
        </div>
      );
    default:
      return null;
  }
}
