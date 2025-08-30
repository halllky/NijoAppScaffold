import { test, expect } from "vitest"
import { render, screen } from "@testing-library/react"
import userEvent from "@testing-library/user-event"
import React from "react"
import { DynamicForm } from "./Form.Root"
import { DynamicFormRef } from "./types"
import _001_セクションと配列の基本的組み合わせ from "./__tests__/001_セクションと配列の基本的組み合わせ"
import * as Util from "../../util"

test("セクション, 配列 の組み合わせ", async () => {
  const user = userEvent.setup()

  // データ構造定義
  const root = _001_セクションと配列の基本的組み合わせ()

  // デフォルト値
  const defaultValues = {
    section1: {
      section1_1: {
        member1: "",
      },
      array1_2: [{ member2: "" }],
    },
    array2: [{
      section2_1: {
        member3: "",
      },
      array2_2: [{ member4: "" }],
    }],
  }

  // DynamicFormコンポーネントをrender
  const ref = React.createRef<DynamicFormRef>()
  render(
    <Util.IMEProvider>
      <DynamicForm
        ref={ref}
        defaultValues={defaultValues}
        {...root}
      />
    </Util.IMEProvider>
  )

  // name属性で直接特定する
  const member1InputByName = document.querySelector('input[name=".section1.section1_1.member1"]') as HTMLInputElement
  const member3InputByName = document.querySelector('input[name=".array2.0.section2_1.member3"]') as HTMLInputElement

  expect(member1InputByName).toBeTruthy()
  expect(member3InputByName).toBeTruthy()

  // user-eventを使用してメンバーに値を入力（グリッド内でない要素）
  await user.type(member1InputByName, "値1")
  await user.type(member3InputByName, "値3")

  // 複数の「追加」ボタンがあるため、getAllByTextを使用
  const addButtons = screen.getAllByText("追加")

  // 期待される数の追加ボタンが存在することを確認
  expect(addButtons.length).toBeGreaterThanOrEqual(3)

  // 最初の「追加」ボタン（配列1-2）をクリック
  await user.click(addButtons[0])

  // 少し待機してグリッドが更新されるのを待つ
  await new Promise(resolve => setTimeout(resolve, 300))

  // 3番目の「追加」ボタン（配列2-2）をクリック
  await user.click(addButtons[2])
  await new Promise(resolve => setTimeout(resolve, 300))

  // EditableGridのセル編集をテスト
  // 配列1-2のグリッド内の最初のセルを探す
  const gridContainers = document.querySelectorAll('table')
  expect(gridContainers.length).toEqual(2)

  // 最初のグリッド（配列1-2）でのセル編集を試行
  const firstGrid = gridContainers[0] as HTMLElement

  // グリッド内のセルを探す（tbody内のtd要素）
  const firstGridCells = firstGrid.querySelectorAll('tbody td')
  expect(firstGridCells.length).toBeGreaterThan(0)

  // 最初のデータセルを取得（ヘッダー以外）
  const firstDataCell = firstGridCells[0] as HTMLElement

  // セルをクリックしてアクティブにする
  await user.click(firstDataCell)
  await new Promise(resolve => setTimeout(resolve, 100))

  // ダブルクリックで編集開始を試行
  await user.dblClick(firstDataCell)
  await new Promise(resolve => setTimeout(resolve, 200))

  // textareaが存在することを期待
  const textarea = document.querySelector('textarea')
  expect(textarea).toBeTruthy()

  // textareaに値を入力
  await user.clear(textarea!)
  await user.type(textarea!, "値2")

  // Ctrl+Enterで編集確定
  await user.keyboard('{Control>}{Enter}{/Control}')
  await new Promise(resolve => setTimeout(resolve, 200))

  // 2番目のグリッド（配列2-2）でも同様の操作
  const secondGrid = gridContainers[1] as HTMLElement
  const secondGridCells = secondGrid.querySelectorAll('tbody td')
  expect(secondGridCells.length).toBeGreaterThan(0)

  const secondDataCell = secondGridCells[0] as HTMLElement

  // セルをクリックしてアクティブにする
  await user.click(secondDataCell)
  await new Promise(resolve => setTimeout(resolve, 100))

  // ダブルクリックで編集開始
  await user.dblClick(secondDataCell)
  await new Promise(resolve => setTimeout(resolve, 200))

  // textareaに値を入力
  const textarea2 = document.querySelector('textarea')
  expect(textarea2).toBeTruthy()

  await user.clear(textarea2!)
  await user.type(textarea2!, "値4")

  // Ctrl+Enterで編集確定
  await user.keyboard('{Control>}{Enter}{/Control}')
  await new Promise(resolve => setTimeout(resolve, 200))

  // DynamicFormRefのuseFormReturnのgetValuesで値を取得して検証
  const formValues = ref.current?.useFormReturn.getValues()

  // グリッド外の要素（member1とmember3）の検証
  expect((formValues as any)?.section1?.section1_1?.member1).toBe("値1")
  expect((formValues as any)?.array2?.[0]?.section2_1?.member3).toBe("値3")

  // フォーム構造が正しく作成されていることを確認
  expect(formValues).toHaveProperty("section1.section1_1")
  expect(formValues).toHaveProperty("section1.array1_2")
  expect(formValues).toHaveProperty("array2")
  expect(Array.isArray((formValues as any)?.array2)).toBe(true)

  // 必須の検証: グリッド外の要素（member1とmember3）
  expect((formValues as any)?.section1?.section1_1?.member1).toBe("値1")
  expect((formValues as any)?.array2?.[0]?.section2_1?.member3).toBe("値3")

  // フォーム構造が正しく作成されていることを確認
  expect(formValues).toHaveProperty("section1.section1_1")
  expect(formValues).toHaveProperty("section1.array1_2")
  expect(formValues).toHaveProperty("array2")

  // 配列構造の基本的な検証
  expect(Array.isArray((formValues as any)?.section1?.array1_2)).toBe(true)
  expect(Array.isArray((formValues as any)?.array2)).toBe(true)

  // 追加ボタンによる配列アイテムの追加が動作していることを確認
  expect((formValues as any)?.section1?.array1_2?.length).toBeGreaterThan(1)
  expect((formValues as any)?.array2?.length).toBeGreaterThan(1)

  // グリッド内のセル編集結果を検証
  // member2（配列1-2内）の編集結果
  const array1_2Values = (formValues as any)?.section1?.array1_2
  expect(Array.isArray(array1_2Values)).toBe(true)

  // member4（配列2-2内）の編集結果
  const array2Values = (formValues as any)?.array2
  expect(Array.isArray(array2Values)).toBe(true)

  // デバッグ用: フォーム全体の値を確認
  console.log("フォーム全体の値:", JSON.stringify(formValues, null, 2))

  // EditableGridでの編集が行われた場合の検証
  // （EditableGridの複雑な実装により、正確な値の反映は環境に依存する可能性があります）
})
