import React, { useState } from "react"
import * as ReactRouter from "react-router"
import { useNavigate } from "react-router-dom"
import { PageBase } from "../layout/PageBase"
import { PageTitle } from "../layout/PageTitle"
import { FormLabel } from "../layout/FormLabel"
import { CheckBox } from "../input/CheckBox"
import { DateInput } from "../input/DateInput"
import { DescriptionTextArea } from "../input/DescriptionTextArea"
import { EnumSelection } from "../input/EnumSelection"
import { NumericTextBox } from "../input/NumericTextBox"
import { WordTextBox } from "../input/WordTextBox"
import { NowLoading } from "../layout/NowLoading"

export const URL = "/ui-component-catalog"

export function useNavigateToUIComponentCatalog() {
  const navigate = useNavigate()
  return React.useCallback(() => {
    navigate(URL)
  }, [navigate])
}

export default {
  path: URL,
  element: <UIComponentCatalog />,
} satisfies ReactRouter.RouteObject

function UIComponentCatalog() {
  // 各入力コンポーネントの状態管理用state
  const [checkBoxValue, setCheckBoxValue] = useState(false)
  const [dateValue, setDateValue] = useState("")
  const [descValue, setDescValue] = useState("")
  const [enumValue, setEnumValue] = useState<any>(null)
  const [numValue, setNumValue] = useState("")
  const [wordValue, setWordValue] = useState("")
  const [loadingVisible, setLoadingVisible] = useState(false)

  return (
    <PageBase
      browserTitle="UIコンポーネントカタログ"
      header={<PageTitle>UIコンポーネントカタログ</PageTitle>}
      contents={
        <div className="p-4 space-y-8 pb-20">
          <section>
            <h2 className="text-xl font-bold mb-4 border-b">入力フォーム</h2>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
              {/* CheckBox */}
              <div className="space-y-2">
                <h3 className="font-bold">CheckBox</h3>
                <div className="p-4 border rounded">
                  <CheckBox
                    checked={checkBoxValue}
                    onChange={e => setCheckBoxValue(e.target.checked)}
                  >
                    チェックボックス
                  </CheckBox>
                  <div className="text-sm text-gray-500 mt-2">Checked: {String(checkBoxValue)}</div>
                </div>
              </div>

              {/* DateInput */}
              <div className="space-y-2">
                <h3 className="font-bold">DateInput</h3>
                <div className="p-4 border rounded space-y-4">
                  <div>
                    <FormLabel>Date (default)</FormLabel>
                    <DateInput
                      value={dateValue}
                      onChange={e => setDateValue(e.target.value)}
                    />
                  </div>
                  <div>
                    <FormLabel>DateTime</FormLabel>
                    <DateInput
                      appearance="datetime"
                    />
                  </div>
                  <div className="text-sm text-gray-500">Value: {dateValue}</div>
                </div>
              </div>

              {/* DescriptionTextArea */}
              <div className="space-y-2">
                <h3 className="font-bold">DescriptionTextArea</h3>
                <div className="p-4 border rounded">
                  <DescriptionTextArea
                    value={descValue}
                    onChange={e => setDescValue(e.target.value)}
                    placeholder="複数行のテキストを入力..."
                    className="w-full"
                  />
                  <div className="text-sm text-gray-500 mt-2">Value: {descValue}</div>
                </div>
              </div>

              {/* EnumSelection */}
              <div className="space-y-2">
                <h3 className="font-bold">EnumSelection</h3>
                <div className="p-4 border rounded">
                  <FormLabel>売上明細区分</FormLabel>
                  <EnumSelection
                    type="売上明細区分"
                    value={enumValue}
                    onChange={e => setEnumValue(e.target.value)}
                  />
                  <div className="text-sm text-gray-500 mt-2">Value: {enumValue}</div>
                </div>
              </div>

              {/* NumericTextBox */}
              <div className="space-y-2">
                <h3 className="font-bold">NumericTextBox</h3>
                <div className="p-4 border rounded space-y-4">
                  <div>
                    <FormLabel>通常</FormLabel>
                    <NumericTextBox
                      value={numValue}
                      onChange={e => setNumValue(e.target.value)}
                    />
                  </div>
                  <div>
                    <FormLabel>3桁カンマ区切り (commaSeparated)</FormLabel>
                    <NumericTextBox
                      commaSeparated
                      defaultValue="1234567"
                    />
                  </div>
                  <div className="text-sm text-gray-500">Value: {numValue}</div>
                </div>
              </div>

              {/* WordTextBox */}
              <div className="space-y-2">
                <h3 className="font-bold">WordTextBox</h3>
                <div className="p-4 border rounded">
                  <FormLabel>単語入力 (自動trim, NFKC正規化)</FormLabel>
                  <WordTextBox
                    value={wordValue}
                    onChange={e => setWordValue(e.target.value)}
                  />
                  <div className="text-sm text-gray-500 mt-2">Value: {wordValue}</div>
                </div>
              </div>
            </div>
          </section>

          <section>
            <h2 className="text-xl font-bold mb-4 border-b">Layout Components</h2>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
              {/* FormLabel */}
              <div className="space-y-2">
                <h3 className="font-bold">FormLabel</h3>
                <div className="p-4 border rounded">
                  <FormLabel>これはフォームラベルです</FormLabel>
                  <div className="mt-1 p-2 bg-gray-100 rounded text-sm">入力フォームの項目名などに使用します</div>
                </div>
              </div>

              {/* PageTitle */}
              <div className="space-y-2">
                <h3 className="font-bold">PageTitle</h3>
                <div className="p-4 border rounded">
                  <PageTitle>ページタイトル</PageTitle>
                  <div className="mt-1 p-2 bg-gray-100 rounded text-sm">画面左上のタイトルとして使用します</div>
                </div>
              </div>

              {/* NowLoading */}
              <div className="space-y-2">
                <h3 className="font-bold">NowLoading</h3>
                <div className="p-4 border rounded relative h-40">
                  <button
                    className="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600"
                    onClick={() => {
                      setLoadingVisible(true)
                      setTimeout(() => setLoadingVisible(false), 2000)
                    }}
                  >
                    ローディングを表示 (2秒間)
                  </button>
                  <div className="mt-4">
                    ローディングコンポーネントは親要素(relative)の全面を覆います。
                  </div>
                  {loadingVisible && <NowLoading />}
                </div>
              </div>
            </div>
          </section>

          <section>
            <h2 className="text-xl font-bold mb-4 border-b">Page Layouts (Description Only)</h2>
            <ul className="list-disc pl-5 space-y-2">
              <li><strong>PageBase</strong>: 汎用的なページレイアウト。ヘッダー、フッター、コンテンツエリアを提供します。</li>
              <li><strong>SearchPageBase</strong>: 検索画面用のレイアウト。検索条件エリア、検索結果エリア、ページング機能を提供します。</li>
              <li><strong>EditPageBase</strong>: 編集画面用のレイアウト。保存処理、ダーティチェック（未保存警告）などを提供します。</li>
              <li><strong>RootLayout</strong>: アプリケーション全体のルートレイアウト。サイドメニューやグローバルナビゲーションを含みます。</li>
            </ul>
          </section>
        </div>
      }
    />
  )
}

