import React from "react"
import * as RHF from "react-hook-form"
import useEvent from "react-use-event-hook"
import { EditableGrid, type EditableGridLeafColumn } from "@halllky/editable-grid"
import { PageBase } from "../../app/PageBase"
import { PageTitle } from "../../ui/PageTitle"
import { Button } from "../../ui/Button"
import { CheckBox } from "../../ui/CheckBox"
import { FieldColumn, FieldGroup, SearchDialogHost, useDisplayDataForm, type GridRow } from "../../ui2"
import { ValuesPreview } from "./ValuesPreview"
import { 従業員検索ダイアログ } from "./従業員検索ダイアログ"
import { 検索ダイアログ一覧 } from "./検索ダイアログ一覧"
import * as サンプル伝票 from "./サンプル伝票"

/**
 * ui2 の動作確認画面のうち、詳細画面を想定したもの。
 * 画面表示用データのフォームを、モックの集約モジュールを使って組み立てる。
 * nijo.xml のデータ構造定義には依存しないので、どのプロジェクトでもそのまま動作確認に使える。
 */
export default function DisplayDataFormSamplePage() {
  return (
    // 検索ダイアログの描画先と対応表。実際のアプリケーションではルートに置くが、この画面だけで完結させるためここに置く
    <SearchDialogHost dialogs={検索ダイアログ一覧}>
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
  })
  const { control, getValues, setValue, handleSubmit } = formMethods

  // 明細の行の追加・削除。グリッドはこれらの手段を持たないので useFieldArray を併用する。
  // 新規追加した行は配列から取り除き、DBに存在する行は削除フラグを立てるだけにする（保存時に削除される）
  const { append: appendDetailRow, remove: removeDetailRow } = RHF.useFieldArray({ control, name: '明細' })
  const handleAddDetailRow = () => {
    appendDetailRow(サンプル伝票.createNewDisplayData_明細())
  }
  // グリッドが渡す行の位置は絞り込み後の画面上の位置なので、配列インデックスは instanceId から求める
  const handleRemoveDetailRow = useEvent((instanceId: string) => {
    const index = getValues('明細').findIndex(row => row.instanceId === instanceId)
    if (index === -1) return
    if (getValues(`明細.${index}.existsInDatabase`)) {
      setValue(`明細.${index}.willBeDeleted`, true, { shouldDirty: true })
    } else {
      removeDetailRow(index)
    }
  })

  // 明細のグリッド。削除フラグの立った行は表示しない。
  // 完了した明細は、完了を外せるよう完了の列だけ残して、セル単位で読み取り専用にする。削除ボタンも読み取り専用に従って消える
  const [detailGridProps, detailRowMessages] = useEditableGrid('明細', {
    columns: col => [
      col.text('品名', { defaultWidth: 160, isReadOnly: row => row.完了 === true }),
      col.numeric('数量', { defaultWidth: 80, isReadOnly: row => row.完了 === true }),
      col.numeric('単価', { header: '単価（税抜）', defaultWidth: 120, isReadOnly: row => row.完了 === true }),
      col.date('納期', { defaultWidth: 120, isReadOnly: row => row.完了 === true }),
      col.checkBox('完了', { defaultWidth: 56 }),
      col.refTo('検品者', { params: { 退職者を含む: false }, defaultWidth: 200, isReadOnly: row => row.完了 === true }),
      col.textArea('備考', { defaultWidth: 240, wrap: true, isReadOnly: row => row.完了 === true }),
      { ...deleteButtonColumn(handleRemoveDetailRow), isReadOnly: row => row.完了 === true },
    ],
    rows: rows => rows.filter(row => !row.willBeDeleted),
  }, [])

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

      {/* 検証はブラウザに任せず react-hook-form の検証ルールで行う。入力欄での Enter で登録が実行されないよう、Enter による送信は止める */}
      <form noValidate onSubmit={handleValidate} onKeyDown={preventSubmitByEnter} className="flex flex-col gap-4">

        {/* どの項目にも表示されないメッセージ */}
        <RootErrors />

        {/* ヘッダ部。項目の意味のまとまりごとに列を分ける。読み取り専用は入力コンポーネントごとに指定する */}
        <FieldGroup title="基本情報" labelWidth="5.5rem">
          {/* 伝票の識別と担当 */}
          <FieldColumn>
            {/* 必須マークは表示だけなので、未入力のチェックは rules で別に指定する */}
            <FieldLabel name="伝票番号" requiredMark>
              <Input.TextBox name="伝票番号" className="w-32" isReadOnly={isReadOnly} rules={{ required: true }} />
            </FieldLabel>
            <FieldLabel name="件名" requiredMark>
              <Input.TextBox name="件名" isReadOnly={isReadOnly} rules={{ required: true }} />
            </FieldLabel>
            <FieldLabel name="担当者" requiredMark>
              <Input.RefTo name="担当者" params={{ 退職者を含む: false }} isReadOnly={isReadOnly} />
            </FieldLabel>
          </FieldColumn>

          {/* 日付 */}
          <FieldColumn>
            <FieldLabel name="伝票日付" requiredMark>
              <Input.DateInput name="伝票日付" isReadOnly={isReadOnly} rules={{ required: true }} />
            </FieldLabel>
            <FieldLabel name="計上年月">
              <Input.DateInput name="計上年月" isReadOnly={isReadOnly} />
            </FieldLabel>
            <FieldLabel name="登録日時">
              <Input.DateInput name="登録日時" isReadOnly />
            </FieldLabel>
          </FieldColumn>

          {/* 金額と状態 */}
          <FieldColumn>
            <FieldLabel name="合計金額" afterLabel={<span className="text-xs text-gray-500 self-center">自動計算</span>}>
              <Input.NumericTextBox name="合計金額" isReadOnly />
            </FieldLabel>
            {/* メタデータに無い業務上の制約は rules の validate で追加する */}
            <FieldLabel name="税率">
              <Input.NumericTextBox name="税率" className="w-24" isReadOnly={isReadOnly} rules={{ validate: validateTaxRate }} />
            </FieldLabel>
            <FieldLabel name="確定済み">
              <Input.CheckBox name="確定済み" isReadOnly={isReadOnly}>確定する</Input.CheckBox>
            </FieldLabel>
          </FieldColumn>

          {/* FieldGroup の直下に置いた項目は横幅いっぱいに表示される */}
          <FieldLabel name="備考" vertical>
            <Input.TextArea name="備考" className="min-h-16 max-h-48" isReadOnly={isReadOnly} />
          </FieldLabel>
        </FieldGroup>

        {/* 明細部。明細全体に対するメッセージはラベルの下に、各行に対するメッセージはグリッドの下の一覧とセルに表示される */}
        <FieldLabel
          name="明細"
          vertical
          afterLabel={!isReadOnly && (
            <Button mini outline onClick={handleAddDetailRow}>行追加</Button>
          )}
        >
          {/* 明細の各行に対するメッセージ */}
          {detailRowMessages}
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
      <div className="flex-1 flex justify-center bg-white">
        {/* ボタンのクリックでセルが選択されないよう、mousedown の伝播を止める */}
        <Button mini underline onMouseDown={e => e.stopPropagation()} onClick={() => onDelete(rowKey)}>
          削除
        </Button>
      </div>
    ),
  }
}

/**
 * 入力欄での Enter によるフォームの送信を止める。送信はボタンのクリックだけで行う。
 * 入力欄自身の Enter の処理（入力中の値の確定など）は、この処理より先に入力欄で行われる。
 * 文章の入力欄の改行と、ボタンでの Enter（ボタンのクリック）は止めない。IME の変換の確定も止めない。
 */
function preventSubmitByEnter(e: React.KeyboardEvent<HTMLFormElement>) {
  if (e.key !== 'Enter' || e.nativeEvent.isComposing) return
  if (e.target instanceof HTMLInputElement) e.preventDefault()
}

/** 税率の業務上の制約。メタデータの桁数の範囲内でも、100% を超える値は受け付けない */
function validateTaxRate(value: unknown): string | undefined {
  if (typeof value === 'string' && value !== '' && Number(value) > 100) return '税率は100以下で入力してください。'
  return undefined
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
