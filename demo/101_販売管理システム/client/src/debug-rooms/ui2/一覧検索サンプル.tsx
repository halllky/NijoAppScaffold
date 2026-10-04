import React from "react"
import { PageBase } from "../../app/PageBase"
import { PageTitle } from "../../ui/PageTitle"
import { Button } from "../../ui/Button"
import { FieldColumn, FieldGroup, SearchDialogHost, useSearchConditionForm } from "../../ui2"
import { ValuesPreview } from "./ValuesPreview"
import { 検索ダイアログ一覧 } from "./検索ダイアログ一覧"
import * as サンプル伝票 from "./サンプル伝票"

/**
 * ui2 の動作確認画面のうち、一覧検索画面を想定したもの。
 * 検索条件のフォームを、モックの集約モジュールを使って組み立てる。
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

/** 検索条件のフォームの動作確認 */
function SearchConditionFormSample() {

  // 検索ボタンで送信される値。動作確認用に画面下部に表示する
  const [searchedCondition, setSearchedCondition] = React.useState<サンプル伝票.SearchCondition>()

  // フォーム
  const {
    formMethods: { handleSubmit, reset },
    Input,
    FieldLabel,
    RootErrors,
    setServerMessages,
    clearServerMessages,
  } = useSearchConditionForm(サンプル伝票)

  // 実際の画面ではここで検索処理を呼び、エラーが返ってきたら setServerMessages に渡す
  const handleSearch = handleSubmit(condition => {
    clearServerMessages()
    setSearchedCondition(condition)
  })
  const handleClear = () => {
    reset(サンプル伝票.createNewSearchCondition())
    clearServerMessages()
  }
  // 検索処理がエラーを返した場合を模擬する
  const handleSimulateServerError = () => {
    setServerMessages({
      error: ['検索条件が広すぎます。条件を追加してください。'],
      children: {
        filter: { children: { 伝票日付: { error: ['1年を超える期間は指定できません。'] } } },
      },
    })
  }

  return (
    <PageBase
      browserTitle="UI2 一覧検索サンプル"
      header={(
        <>
          <PageTitle>UI2 一覧検索サンプル</PageTitle>

          <div className="basis-4"></div>

          {/* 動作確認用の操作 */}
          <Button outline mini onClick={handleSimulateServerError}>
            サーバーエラーを模擬
          </Button>
        </>
      )}
      contents={(
        <div className="flex flex-col gap-4">

          {/* 検証はブラウザに任せず react-hook-form の検証ルールで行う。検索条件欄での Enter で検索できるよう、送信は form の submit で行う */}
          <form noValidate onSubmit={handleSearch} className="flex flex-col gap-4">

            {/* どの項目にも表示されないメッセージ */}
            <RootErrors />

            {/* 検索条件欄。数値・日付は範囲指定、真偽値は「該当する/しない」、列挙体は選択肢ごとのチェックボックスになる */}
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

            {/* 検索・クリア */}
            <div className="flex justify-end gap-2">
              <Button outline onClick={handleClear}>クリア</Button>
              <Button submit fill>検索</Button>
            </div>
          </form>

          {/* 動作確認用の値の表示 */}
          <ValuesPreview title="検索ボタンで送信された値" values={searchedCondition} />
        </div>
      )}
    />
  )
}
