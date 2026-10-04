import React from "react"
import { CheckBox } from "../../ui/CheckBox"
import * as Grid from "../../ui/grid"
import { FieldColumn, FieldGroup, SearchDialogHost, SearchPageBase, useSearchConditionForm, type SearchResult, type SortComboItem } from "../../ui2"
import { 検索ダイアログ一覧 } from "./検索ダイアログ一覧"
import * as サンプル伝票 from "./サンプル伝票"

/**
 * ui2 の動作確認画面のうち、一覧検索画面を想定したもの。
 * 検索条件のフォームを、モックの集約モジュールを使って組み立てる。
 * 検索処理はサーバーを模擬したものを使う。
 * nijo.xml のデータ構造定義には依存しないので、どのプロジェクトでもそのまま動作確認に使える。
 */
export default function SearchConditionFormSamplePage() {
  return (
    // 検索ダイアログの描画先と対応表。実際のアプリケーションではルートに置くが、この画面だけで完結させるためここに置く
    <SearchDialogHost dialogs={検索ダイアログ一覧}>
      <SearchConditionFormSample />
    </SearchDialogHost>
  )
}

/** 検索条件のフォームと一覧検索画面の動作確認 */
function SearchConditionFormSample() {

  // 動作確認用の切り替え。オンにすると、検索処理がエラーを返す
  const [simulatesServerError, setSimulatesServerError] = React.useState(false)

  // 検索条件のフォーム
  const form = useSearchConditionForm(サンプル伝票)
  const { Input, FieldLabel } = form

  return (
    <SearchPageBase
      form={form}
      pageTitle="UI2 一覧検索サンプル"
      searchConditionDefaultSize={180}
      sortOptions={SORT_OPTIONS}
      createInitialCondition={createInitialCondition}
      search={(condition, signal) => searchOnMockServer(condition, signal, simulatesServerError)}
      header={(
        // 動作確認用の操作
        <CheckBox checked={simulatesServerError} onChange={e => setSimulatesServerError(e.target.checked)}>
          検索でサーバーエラーを模擬
        </CheckBox>
      )}
      searchCondition={(
        // 検索条件欄。数値・日付は範囲指定、真偽値は「該当する/しない」、列挙体は選択肢ごとのチェックボックスになる
        <FieldGroup labelWidth="5rem">
          {/* 伝票の内容 */}
          <FieldColumn>
            <FieldLabel name="伝票番号">
              <Input.TextBox name="伝票番号" className="w-32" />
            </FieldLabel>
            <FieldLabel name="件名">
              <Input.TextBox name="件名" />
            </FieldLabel>
            {/* 文章の項目も、検索条件では1行の入力欄になる */}
            <FieldLabel name="備考">
              <Input.TextArea name="備考" />
            </FieldLabel>
          </FieldColumn>

          {/* 日付と金額 */}
          <FieldColumn>
            <FieldLabel name="伝票日付">
              <Input.DateInput name="伝票日付" />
            </FieldLabel>
            <FieldLabel name="合計金額">
              <Input.NumericTextBox name="合計金額" />
            </FieldLabel>
          </FieldColumn>

          {/* 状態と担当 */}
          <FieldColumn>
            <FieldLabel name="確定済み">
              <Input.CheckBox name="確定済み" />
            </FieldLabel>
            {/* 列挙体。チェックした値のいずれかに一致するものに絞り込む */}
            <FieldLabel name="優先度">
              <Input.EnumSelection name="優先度" />
            </FieldLabel>
            {/* 外部参照。コードと名称のどちらも手入力でき、検索ダイアログで選ぶこともできる */}
            <FieldLabel name="担当者">
              <Input.RefTo name="担当者" params={{ 退職者を含む: true }} />
            </FieldLabel>
          </FieldColumn>
        </FieldGroup>
      )}
      defineSearchResultColumns={[() => [
        Grid.textColumn<サンプル伝票.DisplayData>('伝票番号', row => row.伝票番号, { columnId: '伝票番号', defaultWidth: 112 }),
        Grid.textColumn<サンプル伝票.DisplayData>('件名', row => row.件名, { columnId: '件名', defaultWidth: 200 }),
        Grid.textColumn<サンプル伝票.DisplayData>('伝票日付', row => row.伝票日付, { columnId: '伝票日付', defaultWidth: 112 }),
        Grid.numericColumn<サンプル伝票.DisplayData>('合計金額', row => row.合計金額, { columnId: '合計金額', defaultWidth: 120, suffix: '円' }),
        Grid.textColumn<サンプル伝票.DisplayData>('確定済み', row => row.確定済み ? '確定' : '', { columnId: '確定済み', defaultWidth: 80 }),
        Grid.textColumn<サンプル伝票.DisplayData>('優先度', row => row.優先度, { columnId: '優先度', defaultWidth: 80 }),
        Grid.textColumn<サンプル伝票.DisplayData>('担当者', row => row.担当者.従業員番号, { columnId: '担当者.従業員番号', defaultWidth: 96 }),
        Grid.textColumn<サンプル伝票.DisplayData>('', row => row.担当者.氏名, { columnId: '担当者.氏名', defaultWidth: 120 }),
        Grid.textColumn<サンプル伝票.DisplayData>('備考', row => row.備考, { columnId: '備考', defaultWidth: 240 }),
      ], []]}
    />
  )
}

/** 並び順の選択肢 */
const SORT_OPTIONS: SortComboItem<サンプル伝票.SortableMember>[] = [
  { key: 'サンプル伝票.伝票番号', label: '伝票番号' },
  { key: 'サンプル伝票.伝票日付', label: '伝票日付' },
  { key: 'サンプル伝票.合計金額', label: '合計金額' },
]

/** 画面を開いたときとクリアしたときの検索条件。新しい伝票から順に表示する */
function createInitialCondition(): サンプル伝票.SearchCondition {
  return {
    ...サンプル伝票.createNewSearchCondition(),
    sort: ['サンプル伝票.伝票日付（降順）'],
  }
}

//#region サーバー側の検索処理の模擬

/** サーバー側の検索処理を模擬する。少し待ってから、絞り込み・並べ替え・ページングをした結果を返す */
async function searchOnMockServer(
  condition: サンプル伝票.SearchCondition,
  signal: AbortSignal,
  simulatesServerError: boolean,
): Promise<SearchResult<サンプル伝票.DisplayData>> {
  await new Promise(resolve => setTimeout(resolve, 300))
  if (signal.aborted) return { type: 'canceled' }

  // 画面上に表示領域がある項目と、ルートのそれぞれに対するメッセージを返す
  if (simulatesServerError) return {
    type: 'error',
    detail: {
      error: ['検索条件が広すぎます。条件を追加してください。'],
      children: {
        filter: { children: { 伝票日付: { error: ['1年を超える期間は指定できません。'] } } },
      },
    },
  }

  const hits = MOCK_SLIPS
    .filter(slip => matchesFilter(slip, condition.filter))
    .sort((a, b) => compareBySort(a, b, condition.sort))

  const skip = Number(condition.skip || 0)
  const take = Number(condition.take || hits.length)
  return {
    type: 'ok',
    returnValue: {
      currentPageItems: hits.slice(skip, skip + take),
      totalCount: hits.length,
    },
    detail: {},
  }
}

/** 伝票が絞り込み条件に合うかどうか。範囲指定は両端を含む */
function matchesFilter(slip: サンプル伝票.DisplayData, filter: サンプル伝票.SearchConditionFilter): boolean {
  if (filter.伝票番号 && slip.伝票番号 !== filter.伝票番号) return false
  if (filter.件名 && !slip.件名?.includes(filter.件名)) return false
  if (filter.備考 && !slip.備考?.includes(filter.備考)) return false
  if (filter.伝票日付?.from && (slip.伝票日付 ?? '') < filter.伝票日付.from) return false
  if (filter.伝票日付?.to && (slip.伝票日付 ?? '') > filter.伝票日付.to) return false
  if (filter.合計金額?.from && Number(slip.合計金額) < Number(filter.合計金額.from)) return false
  if (filter.合計金額?.to && Number(slip.合計金額) > Number(filter.合計金額.to)) return false
  if (filter.確定済み?.trueのみ && !filter.確定済み.falseのみ && !slip.確定済み) return false
  if (filter.確定済み?.falseのみ && !filter.確定済み.trueのみ && slip.確定済み) return false
  if (filter.担当者.従業員番号 && slip.担当者.従業員番号 !== filter.担当者.従業員番号) return false
  if (filter.担当者.氏名 && !slip.担当者.氏名?.includes(filter.担当者.氏名)) return false

  // 列挙体は、チェックされた値のいずれかに一致するものに絞り込む。何もチェックされていなければ絞り込まない
  const checkedPriorities = Object.entries(filter.優先度 ?? {}).filter(([, checked]) => checked).map(([value]) => value)
  if (checkedPriorities.length > 0 && !checkedPriorities.includes(slip.優先度 ?? '')) return false

  return true
}

/** 並び順の指定に従って伝票を比較する。先に指定されたものほど優先する */
function compareBySort(a: サンプル伝票.DisplayData, b: サンプル伝票.DisplayData, sort: サンプル伝票.SearchCondition['sort']): number {
  for (const key of sort) {
    const descending = key.endsWith('（降順）')
    const member = key.replace(/（昇順）$|（降順）$/, '') as サンプル伝票.SortableMember
    const compared = member === 'サンプル伝票.合計金額'
      ? Number(a.合計金額) - Number(b.合計金額)
      : (SORT_KEY_OF[member](a) ?? '').localeCompare(SORT_KEY_OF[member](b) ?? '')
    if (compared !== 0) return descending ? -compared : compared
  }
  return 0
}

/** 並び順に指定できるメンバーの値の取り出し方 */
const SORT_KEY_OF: Record<サンプル伝票.SortableMember, (slip: サンプル伝票.DisplayData) => string | null | undefined> = {
  'サンプル伝票.伝票番号': slip => slip.伝票番号,
  'サンプル伝票.伝票日付': slip => slip.伝票日付,
  'サンプル伝票.合計金額': slip => slip.合計金額,
}

/** 模擬サーバーのデータ。ページングを確認できるよう、複数ページにわたる件数にしている */
const MOCK_SLIPS: サンプル伝票.DisplayData[] = Array.from({ length: 57 }, (_, i) => ({
  ...サンプル伝票.createNewDisplayData(),
  伝票ID: `SAMPLE-${(i + 1).toString().padStart(4, '0')}`,
  伝票番号: `A${(i + 1).toString().padStart(9, '0')}`,
  件名: `${['りんご', 'みかん', 'ぶどう', 'もも'][i % 4]}の${['仕入', '返品', '補充'][i % 3]}`,
  伝票日付: `2026-${((i % 9) + 1).toString().padStart(2, '0')}-${((i * 7) % 28 + 1).toString().padStart(2, '0')}`,
  合計金額: ((i * 37) % 50 * 1000 + 500).toString(),
  確定済み: i % 3 === 0,
  優先度: ['通常', '至急', '保留', null][i % 4],
  担当者: {
    従業員番号: `E${((i % 5) + 1).toString().padStart(4, '0')}`,
    氏名: ['山田 太郎', '佐藤 花子', '鈴木 一郎', '高橋 次郎', '田中 三郎'][i % 5],
  },
  備考: i % 5 === 0 ? '要確認' : '',
  existsInDatabase: true,
  willBeChanged: false,
}))

//#endregion サーバー側の検索処理の模擬
