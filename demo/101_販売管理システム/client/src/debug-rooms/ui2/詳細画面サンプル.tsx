import React from "react"
import * as RHF from "react-hook-form"
import useEvent from "react-use-event-hook"
import { EditableGrid, type EditableGridLeafColumn } from "@halllky/editable-grid"
import { PageBase } from "../../app/PageBase"
import { PageTitle } from "../../ui/PageTitle"
import { Button } from "../../ui/Button"
import { CheckBox } from "../../ui/CheckBox"
import { FieldGroup, SearchDialogHost, useDisplayDataForm, type GridRow } from "../../ui2"
import { ValuesPreview } from "./ValuesPreview"
import { 従業員検索ダイアログ } from "./従業員検索ダイアログ"
import * as サンプル伝票 from "./サンプル伝票"

/**
 * ui2 の動作確認画面のうち、詳細画面を想定したもの。
 * 画面表示用データのフォームを、モックの集約モジュールを使って組み立てる。
 * nijo.xml のデータ構造定義には依存しないので、どのプロジェクトでもそのまま動作確認に使える。
 */
export default function DisplayDataFormSamplePage() {
  return (
    // 検索ダイアログの描画先。実際のアプリケーションではルートに置くが、この画面だけで完結させるためここに置く
    <SearchDialogHost>
      <PageBase
        browserTitle="UI2 詳細画面サンプル"
        header={<PageTitle>UI2 詳細画面サンプル</PageTitle>}
        contents={<DisplayDataFormSample />}
      />
    </SearchDialogHost>
  )
}

/** 画面表示用データのフォームの動作確認 */
function DisplayDataFormSample() {

  // 動作確認用の切り替え
  const [isReadOnly, setIsReadOnly] = React.useState(false)
  const [hidesCompletedRows, setHidesCompletedRows] = React.useState(false)
  const [validValues, setValidValues] = React.useState<サンプル伝票.DisplayData>()
  const [selectedEmployees, setSelectedEmployees] = React.useState<unknown>()

  // フォーム
  const {
    formMethods,
    Input,
    FieldLabel,
    RootErrors,
    useEditableGrid,
    setServerMessages,
    clearServerMessages,
  } = useDisplayDataForm(サンプル伝票, {
    defaultValues: loadSampleDisplayData,
    isReadOnly,
  })
  const { control, getValues, handleSubmit } = formMethods

  // 明細の行の追加・削除。グリッドはこれらの手段を持たないので useFieldArray を併用する
  const { append: appendDetailRow, remove: removeDetailRow } = RHF.useFieldArray({ control, name: '明細' })
  const handleAddDetailRow = () => {
    appendDetailRow(サンプル伝票.createNewDisplayData_明細())
  }
  // グリッドが渡す行の位置は絞り込み後の画面上の位置なので、配列インデックスは instanceId から求める
  const handleRemoveDetailRow = useEvent((instanceId: string) => {
    const index = getValues('明細').findIndex(row => row.instanceId === instanceId)
    if (index !== -1) removeDetailRow(index)
  })

  // 明細のグリッド
  const detailGridProps = useEditableGrid('明細', col => [
    col.text('品名', { defaultWidth: 160 }),
    col.numeric('数量', { defaultWidth: 80 }),
    col.numeric('単価', { header: '単価（税抜）', defaultWidth: 120 }),
    col.date('納期', { defaultWidth: 120 }),
    col.checkBox('完了', { defaultWidth: 56 }),
    col.refTo('検品者', { dialog: 従業員検索ダイアログ, params: { 退職者を含む: false }, defaultWidth: 200 }),
    col.textArea('備考', { defaultWidth: 240, wrap: true }),
    deleteButtonColumn(handleRemoveDetailRow),
  ], [], {
    selectDisplayRows: hidesCompletedRows
      ? rows => rows.filter(row => !row.完了)
      : undefined,
  })

  // 検索ダイアログを外部参照の入力欄を介さずに直接開く。複数選択の動作確認用
  const { selectMany: selectEmployees } = 従業員検索ダイアログ.useOpen()
  const handleSelectEmployees = async () => {
    const selected = await selectEmployees({ 退職者を含む: true })
    setSelectedEmployees(selected ?? '（選ばずに閉じた）')
  }

  // 検証を通過したら送信される値を表示する。実際の画面ではここで保存処理を呼ぶ
  const handleValidate = handleSubmit(values => {
    setValidValues(values)
  })
  // サーバーで検証エラーが発生した場合を模擬する
  const handleSimulateServerError = () => {
    setServerMessages(createSampleServerMessages(getValues()))
  }

  // 現在のフォームの値。動作確認用に画面下部に表示する
  const currentValues = RHF.useWatch({ control })

  return (
    <div className="py-4 pb-20 flex flex-col gap-4">

      {/* 動作確認用の操作 */}
      <div className="flex flex-wrap items-center gap-4 p-2 bg-gray-100 rounded">
        <CheckBox checked={isReadOnly} onChange={e => setIsReadOnly(e.target.checked)}>
          読み取り専用
        </CheckBox>
        <CheckBox checked={hidesCompletedRows} onChange={e => setHidesCompletedRows(e.target.checked)}>
          完了した明細を隠す
        </CheckBox>
        <Button outline mini onClick={handleSimulateServerError}>
          サーバーエラーを模擬
        </Button>
        <Button outline mini onClick={clearServerMessages}>
          サーバーのメッセージを消す
        </Button>
        <Button outline mini onClick={handleSelectEmployees}>
          検索ダイアログで複数選択
        </Button>
      </div>

      <form onSubmit={handleValidate} className="flex flex-col gap-4">

        {/* どの項目にも表示されないメッセージ */}
        <RootErrors />

        {/* ヘッダ部 */}
        <FieldGroup title="基本情報">
          <FieldLabel name="伝票番号" isRequired>
            <Input.TextBox name="伝票番号" className="w-32" />
          </FieldLabel>
          <FieldLabel name="件名" isRequired>
            <Input.TextBox name="件名" />
          </FieldLabel>
          <FieldLabel name="伝票日付" isRequired>
            <Input.DateInput name="伝票日付" />
          </FieldLabel>
          <FieldLabel name="計上年月">
            <Input.DateInput name="計上年月" />
          </FieldLabel>
          <FieldLabel name="担当者" isRequired>
            <Input.RefTo name="担当者" dialog={従業員検索ダイアログ} params={{ 退職者を含む: false }} />
          </FieldLabel>
          <FieldLabel name="登録日時">
            <Input.DateInput name="登録日時" isReadOnly />
          </FieldLabel>
          <FieldLabel name="合計金額" afterLabel={<span className="text-xs text-gray-500 self-center">自動計算</span>}>
            <Input.NumericTextBox name="合計金額" isReadOnly />
          </FieldLabel>
          <FieldLabel name="税率">
            <Input.NumericTextBox name="税率" className="w-24" />
          </FieldLabel>
          <FieldLabel name="確定済み">
            <Input.CheckBox name="確定済み">確定する</Input.CheckBox>
          </FieldLabel>
          <FieldLabel name="備考" vertical wide>
            <Input.TextArea name="備考" className="min-h-16 max-h-48" />
          </FieldLabel>
        </FieldGroup>

        {/* 明細部。明細全体に対するメッセージはラベルの下に、各セルに対するメッセージはセルに表示される */}
        <FieldLabel
          name="明細"
          vertical
          afterLabel={!isReadOnly && (
            <Button mini outline onClick={handleAddDetailRow}>行追加</Button>
          )}
        >
          <EditableGrid
            {...detailGridProps}
            isReadOnly={isReadOnly}
            className="h-64 resize-y border border-gray-700"
          />
        </FieldLabel>

        {/* フッタ */}
        <div className="flex justify-end">
          <Button submit fill>検証</Button>
        </div>
      </form>

      {/* 動作確認用の値の表示 */}
      <div className="grid grid-cols-3 gap-4">
        <ValuesPreview title="現在の値" values={currentValues} />
        <ValuesPreview title="検証を通過した値" values={validValues} />
        <ValuesPreview title="検索ダイアログで複数選択した値" values={selectedEmployees} />
      </div>
    </div>
  )
}

/**
 * 明細の行を削除するボタンの列。
 * 列定義ヘルパーにない列も、EditableGrid の列定義をそのまま書けば並べられることの確認を兼ねる。
 */
function deleteButtonColumn(
  onDelete: (instanceId: string) => void,
): EditableGridLeafColumn<GridRow<サンプル伝票.DisplayData, '明細'>> {
  return {
    columnId: 'delete',
    defaultWidth: 56,
    disableResizing: true,
    renderHeader: () => null,
    renderBody: ({ rowKey, isReadOnly }) => !isReadOnly && (
      <div className="flex justify-center">
        {/* ボタンのクリックでセルが選択されないよう、mousedown の伝播を止める */}
        <Button mini underline onMouseDown={e => e.stopPropagation()} onClick={() => onDelete(rowKey)}>
          削除
        </Button>
      </div>
    ),
  }
}

/** 既存データの読み込みを模擬する */
async function loadSampleDisplayData(): Promise<サンプル伝票.DisplayData> {
  const data = サンプル伝票.createNewDisplayData()
  data.伝票ID = 'SAMPLE-0001'
  data.伝票番号 = 'A000000123'
  data.件名 = '動作確認用の伝票'
  data.伝票日付 = '2026-10-01'
  data.登録日時 = '2026-10-01T09:30'
  data.計上年月 = '2026-10'
  data.合計金額 = '128000'
  data.税率 = '10.00'
  data.確定済み = false
  data.担当者 = { 従業員番号: 'E0001', 氏名: '山田 太郎' }
  data.備考 = '複数行の文章を入力できる。\n2行目。'
  data.Version = '1'
  data.existsInDatabase = true
  data.willBeChanged = false
  data.明細 = [
    { 品名: 'りんご', 数量: '10', 単価: '120.00', 納期: '2026-10-10', 完了: true, 検品者: { 従業員番号: 'E0002', 氏名: '佐藤 花子' }, 備考: '' },
    { 品名: 'みかん', 数量: '200', 単価: '45.50', 納期: '2026-10-15', 完了: false, 検品者: {}, 備考: '箱入り' },
    { 品名: 'ぶどう', 数量: '5', 単価: '980.00', 納期: '', 完了: false, 検品者: {}, 備考: '納期未定。\n入荷次第連絡する。' },
  ].map(values => ({
    ...サンプル伝票.createNewDisplayData_明細(),
    ...values,
    existsInDatabase: true,
    willBeChanged: false,
  }))
  return data
}

/**
 * サーバーから返される検証エラーを模擬する。
 * 画面上に表示領域がある項目・無い項目・明細全体・明細の行・ルートのそれぞれに対するメッセージを含める。
 */
function createSampleServerMessages(values: サンプル伝票.DisplayData) {
  return {
    error: ['保存できませんでした。入力内容を確認してください。'],
    children: {
      件名: { error: ['同じ件名の伝票が既にあります。'] },
      担当者: { children: { 従業員番号: { warn: ['退職済みの従業員です。'] } } },
      // 画面上に表示領域が無い項目なので RootErrors に表示される
      Version: { error: ['他のユーザーが先に更新しました。'] },
      明細: {
        // 明細全体に対するメッセージ。明細の FieldLabel に表示される
        warn: ['明細が10行を超えると承認が必要になります。'],
        children: Object.fromEntries(values.明細.slice(0, 2).map((_, index) => [
          index.toString(),
          { children: { 数量: { error: ['在庫が不足しています。'] } } },
        ])),
      },
    },
  }
}
