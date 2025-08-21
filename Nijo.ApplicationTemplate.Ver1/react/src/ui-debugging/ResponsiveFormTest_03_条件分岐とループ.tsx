import React, { useState } from "react"
import ResponsiveForm from "../layout/ResponsiveForm"

/**
 * ResponsiveFormでの条件分岐とループ処理のテスト
 * JSXセクション内での条件分岐とmap関数によるループが正しくレンダリングされるかを検証
 */

// テスト用のデータ配列
const basicItems = [
  { id: 1, name: "項目1", type: "text", required: true },
  { id: 2, name: "項目2", type: "email", required: false },
  { id: 3, name: "項目3", type: "password", required: true },
  { id: 4, name: "項目4", type: "text", required: false },
]

const sectionData = [
  {
    id: 1,
    title: "基本情報",
    fields: [
      { name: "氏名", placeholder: "山田太郎" },
      { name: "年齢", placeholder: "30" },
    ]
  },
  {
    id: 2,
    title: "連絡先",
    fields: [
      { name: "メールアドレス", placeholder: "example@example.com" },
      { name: "電話番号", placeholder: "090-1234-5678" },
    ]
  },
  {
    id: 3,
    title: "追加情報",
    fields: [
      { name: "職業", placeholder: "エンジニア" },
      { name: "趣味", placeholder: "プログラミング" },
      { name: "備考", placeholder: "特記事項" },
    ]
  },
]

const dynamicItems = Array.from({ length: 20 }, (_, i) => ({
  id: i + 1,
  label: `動的項目${i + 1}`,
  fullWidth: i % 7 === 0, // 7つに1つをfullWidthにする
  isImportant: i % 8 === 0, // 8つに1つを重要項目とする
}))

export default function () {

  return (
    <div className="w-full h-full flex border p-4 overflow-auto">
      <ResponsiveForm.Container className="flex-1" labelWidthPx={160}>
        <InnerComponent />
      </ResponsiveForm.Container>
    </div>
  )
}

const InnerComponent = () => {
  const [showOptional, setShowOptional] = useState(true)
  const [showAdvanced, setShowAdvanced] = useState(false)
  const [itemCount, setItemCount] = useState(5)
  const [enabledSections, setEnabledSections] = useState([true, true, false])
  return (
    <>
      {/* コントロールパネル */}
      <ResponsiveForm.Section label="テスト制御パネル" fullWidth>
        <ResponsiveForm.Item label="オプション項目表示">
          <label className="flex items-center gap-2">
            <input
              type="checkbox"
              checked={showOptional}
              onChange={(e) => setShowOptional(e.target.checked)}
            />
            オプション項目を表示
          </label>
        </ResponsiveForm.Item>
        <ResponsiveForm.Item label="高度な設定表示">
          <label className="flex items-center gap-2">
            <input
              type="checkbox"
              checked={showAdvanced}
              onChange={(e) => setShowAdvanced(e.target.checked)}
            />
            高度な設定を表示
          </label>
        </ResponsiveForm.Item>
        <ResponsiveForm.Item label="動的項目数">
          <select
            value={itemCount}
            onChange={(e) => setItemCount(Number(e.target.value))}
            className="border w-full p-1"
          >
            <option value={3}>3個</option>
            <option value={5}>5個</option>
            <option value={10}>10個</option>
            <option value={20}>20個</option>
          </select>
        </ResponsiveForm.Item>
      </ResponsiveForm.Section>

      <ResponsiveForm.Spacer line />

      {/* 基本的な条件分岐テスト */}
      <ResponsiveForm.Section label="条件分岐テスト">
        {/* if文による条件分岐 */}
        {showOptional && (
          <ResponsiveForm.Item label="条件付き項目1">
            <input type="text" className="border w-full" placeholder="showOptionalがtrueの時のみ表示" />
          </ResponsiveForm.Item>
        )}

        {/* 三項演算子による条件分岐 */}
        <ResponsiveForm.Item label="条件切り替え項目">
          {showAdvanced ? (
            <textarea className="border w-full h-20" placeholder="高度な設定用テキストエリア" />
          ) : (
            <input type="text" className="border w-full" placeholder="基本設定用テキストフィールド" />
          )}
        </ResponsiveForm.Item>

        {/* 論理演算子による条件分岐 */}
        {showOptional && showAdvanced && (
          <ResponsiveForm.Item label="両方true時のみ表示" fullWidth>
            <div className="p-4 bg-green-100 border border-green-300 rounded">
              オプション項目と高度な設定の両方が有効な場合のみ表示される項目
            </div>
          </ResponsiveForm.Item>
        )}

        {/* 否定条件 */}
        {!showAdvanced && (
          <ResponsiveForm.Item label="高度な設定無効時表示">
            <input type="text" className="border w-full" placeholder="シンプルモード" />
          </ResponsiveForm.Item>
        )}
      </ResponsiveForm.Section>

      {/* 配列のmapによるループテスト */}
      <ResponsiveForm.Section label="基本的なmapループテスト">
        {basicItems.map((item) => (
          <ResponsiveForm.Item
            key={item.id}
            label={`${item.name}${item.required ? ' *' : ''}`}
            fullWidth={item.type === 'password'}
          >
            <input
              type={item.type}
              className={`border w-full ${item.required ? 'border-red-300' : 'border-gray-300'}`}
              placeholder={`${item.name}を入力してください`}
              required={item.required}
            />
          </ResponsiveForm.Item>
        ))}
      </ResponsiveForm.Section>

      {/* 動的な項目数でのmapテスト */}
      <ResponsiveForm.Section fullWidth label="動的項目数mapテスト">
        {dynamicItems.slice(0, itemCount).map((item) => (
          <ResponsiveForm.Item
            key={item.id}
            label={item.label}
            fullWidth={item.fullWidth}
          >
            <input
              type="text"
              className={`border w-full ${item.isImportant ? 'border-blue-400 bg-blue-50' : 'border-gray-300'}`}
              placeholder={`${item.label}の値`}
            />
            {item.isImportant && (
              <div className="text-xs text-blue-600 mt-1">重要項目</div>
            )}
          </ResponsiveForm.Item>
        ))}
      </ResponsiveForm.Section>

      {/* 条件付きSectionのレンダリング */}
      {sectionData.map((section, index) => {
        const isEnabled = enabledSections[index]
        if (!isEnabled && !showAdvanced) return null

        return (
          <ResponsiveForm.Section
            key={section.id}
            label={`${section.title}${!isEnabled ? ' (無効)' : ''}`}
            fullWidth={section.id === 3} // 3番目のセクションをfullWidthにする
          >
            <ResponsiveForm.Item label={`${section.title}有効/無効`}>
              <label className="flex items-center gap-2">
                <input
                  type="checkbox"
                  checked={isEnabled}
                  onChange={(e) => {
                    const newEnabled = [...enabledSections]
                    newEnabled[index] = e.target.checked
                    setEnabledSections(newEnabled)
                  }}
                />
                このセクションを有効にする
              </label>
            </ResponsiveForm.Item>

            {/* セクション内でのmapとネストした条件分岐 */}
            {isEnabled && section.fields.map((field, fieldIndex) => (
              <ResponsiveForm.Item
                key={`${section.id}-${fieldIndex}`}
                label={field.name}
                fullWidth={field.name === '備考'} // 備考フィールドのみfullWidth
              >
                {field.name === '備考' ? (
                  <textarea
                    className="border w-full h-16"
                    placeholder={field.placeholder}
                  />
                ) : (
                  <input
                    type="text"
                    className="border w-full"
                    placeholder={field.placeholder}
                  />
                )}
              </ResponsiveForm.Item>
            ))}

            {/* 条件付きBreakPoint */}
            {isEnabled && section.id === 2 && <ResponsiveForm.BreakPoint />}
          </ResponsiveForm.Section>
        )
      })}

      {/* フィルタリングを伴うmapテスト */}
      <ResponsiveForm.Section label="フィルタリング付きmapテスト">
        {basicItems
          .filter(item => showOptional || item.required) // オプション項目表示がoffの場合は必須項目のみ
          .map((item) => (
            <ResponsiveForm.Item key={`filtered-${item.id}`} label={`フィルタ済み-${item.name}`}>
              <input
                type={item.type}
                className="border w-full"
                placeholder={`フィルタリングされた${item.name}`}
              />
            </ResponsiveForm.Item>
          ))}
      </ResponsiveForm.Section>

      {/* 複雑な条件とループの組み合わせ */}
      {showAdvanced && (
        <ResponsiveForm.Section label="複雑な条件とループの組み合わせ" fullWidth>
          {/* ネストしたmapと条件分岐 */}
          {sectionData.map((section, sectionIndex) =>
            enabledSections[sectionIndex] && (
              <div key={`complex-${section.id}`} className="mb-4">
                <h4 className="font-bold mb-2">{section.title}詳細</h4>
                {section.fields
                  .filter((_, fieldIndex) => fieldIndex < 2 || showOptional) // 最初の2つまたはオプション表示時のみ
                  .map((field, fieldIndex) => (
                    <ResponsiveForm.Item
                      key={`complex-${section.id}-${fieldIndex}`}
                      label={`詳細-${field.name}`}
                    >
                      <input
                        type="text"
                        className="border w-full"
                        placeholder={`詳細版 ${field.placeholder}`}
                      />
                    </ResponsiveForm.Item>
                  ))}
                {sectionIndex < sectionData.length - 1 && <ResponsiveForm.Spacer />}
              </div>
            )
          )}
        </ResponsiveForm.Section>
      )}

      {/* 条件によるSpacer挿入 */}
      {showOptional && <ResponsiveForm.Spacer line />}

      {/* 最終確認セクション */}
      <ResponsiveForm.Section label="テスト結果確認">
        <ResponsiveForm.Item label="表示項目数" fullWidth>
          <div className="p-3 bg-gray-100 rounded">
            <p>オプション項目: {showOptional ? '表示中' : '非表示'}</p>
            <p>高度な設定: {showAdvanced ? '表示中' : '非表示'}</p>
            <p>動的項目数: {itemCount}個</p>
            <p>有効セクション数: {enabledSections.filter(Boolean).length}個</p>
          </div>
        </ResponsiveForm.Item>
      </ResponsiveForm.Section>
    </>
  )
}
