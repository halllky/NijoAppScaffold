import { test, expect } from "vitest"
import { render, screen } from "@testing-library/react"
import userEvent from "@testing-library/user-event"
import React from "react"
import { DynamicForm } from "./Form.Root"
import { DynamicFormRef } from "./types"
import _001_セクションと配列の基本的組み合わせ from "./__tests__/001_セクションと配列の基本的組み合わせ"
import * as Util from "../../util"

/**
 * セクションと配列を組み合わせたデータ構造が DynamicForm で正しく描画され、
 * 値メンバーの入力値が往復し、配列の追加が機能することを検証する。
 *
 * 配列は子孫に配列を含むかどうかで描画経路が分岐する（Form.Members.tsx 参照）。
 *   - 子孫に配列なし: グリッド表示（<table>）。追加ボタンの文言は「追加」
 *   - 子孫に配列あり: フォーム表示。追加ボタンの文言は「<arrayLabel> を追加」
 * この性質を前提に、文言で各追加ボタンを特定する（DOM 上の出現順に依存しない）。
 *
 * 注: グリッド（仮想化された EditableGrid）のセルをダブルクリックして編集する操作は、
 * jsdom 上では要素サイズや仮想化の挙動が実ブラウザと異なり不安定なため、ここでは
 * 検証対象に含めない。本テストはグリッド外の値の往復と配列構造の生成までを保証する。
 */
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
  const getValues = () => ref.current!.useFormReturn.getValues() as any

  // --- 1. グリッド外の値メンバーが描画され、入力値が往復すること ---
  const member1Input = document.querySelector('input[name=".section1.section1_1.member1"]') as HTMLInputElement
  const member3Input = document.querySelector('input[name=".array2.0.section2_1.member3"]') as HTMLInputElement
  expect(member1Input).toBeTruthy()
  expect(member3Input).toBeTruthy()

  await user.type(member1Input, "値1")
  await user.type(member3Input, "値3")
  expect(getValues().section1.section1_1.member1).toBe("値1")
  expect(getValues().array2[0].section2_1.member3).toBe("値3")

  // --- 2. 配列の描画経路が想定どおりであること ---
  // グリッド表示: 配列1-2, 配列2[0].配列2-2 の2つ（追加ボタン文言「追加」）
  // フォーム表示: 配列2（追加ボタン文言「配列2 を追加」）
  expect(document.querySelectorAll('table')).toHaveLength(2)
  expect(screen.getAllByText("追加")).toHaveLength(2)
  const array2AddButton = screen.getByText("配列2 を追加")

  // --- 3. フォーム表示の配列（配列2）に1件追加すると、要素が増え、
  //        新しい要素の中の配列2-2グリッドぶん <table> も増えること ---
  await user.click(array2AddButton)
  expect(getValues().array2).toHaveLength(2)
  expect(document.querySelectorAll('table')).toHaveLength(3)

  // --- 4. グリッド表示の配列（配列1-2）に1件追加できること ---
  // 配列1-2はsection1配下にあり、DOM上で常に先頭の「追加」ボタンになる
  const array1_2LengthBefore = getValues().section1.array1_2.length
  await user.click(screen.getAllByText("追加")[0])
  expect(getValues().section1.array1_2.length).toBe(array1_2LengthBefore + 1)

  // --- 5. 最終的なフォーム構造の検証 ---
  const formValues = getValues()
  expect(formValues).toHaveProperty("section1.section1_1.member1", "値1")
  expect(Array.isArray(formValues.section1.array1_2)).toBe(true)
  expect(Array.isArray(formValues.array2)).toBe(true)
  expect(formValues.array2[0].section2_1.member3).toBe("値3")
  expect(Array.isArray(formValues.array2[0].array2_2)).toBe(true)
})
