import FormLayout from "../layout/FormLayout"

/**
 * FormLayoutコンポーネントのパターン網羅テスト（拡張）
 * 5つの主要コンポーネントの使い方を多角的に確認する
 */
export default function FormLayoutComponentsTest_Extended() {
  return (
    <div className="w-full h-full flex border p-4 overflow-y-auto">
      <FormLayout.Root className="flex-1 w-full" labelWidthPx={96} valueWidthPx={220} labelAlign="right">

        {/* ============== 基本のItem ============== */}
        <FormLayout.Field>
          <h2 className="text-lg font-bold text-sky-600 border-b-2 border-sky-600 pb-1 mb-2">1. Item</h2>
        </FormLayout.Field>

        {/* 横並び（Root直下, 通常） */}
        <FormLayout.Field label="通常のItem">
          <input type="text" className="border w-full" placeholder="横並び（ラベル右）" />
        </FormLayout.Field>

        {/* 縦並び（Root直下, fullWidth） */}
        <FormLayout.Field label="縦並びItem" fullWidth>
          <textarea className="border w-full h-16" placeholder="fullWidth指定で縦並び" />
        </FormLayout.Field>

        {/* ラベルなし */}
        <FormLayout.Field>
          <input type="text" className="border w-full" placeholder="ラベルなしItem" />
        </FormLayout.Field>

        {/* labelEnd 付き */}
        <FormLayout.Field label="必須項目" labelEnd={<span className="text-red-500 ml-1">*</span>}>
          <input type="text" className="border w-full" placeholder="labelEndの表示" />
        </FormLayout.Field>

        <FormLayout.Spacer />

        {/* ========== ItemGroup ========== */}
        <FormLayout.Field>
          <h2 className="text-lg font-bold text-emerald-600 border-b-2 border-emerald-600 pb-1 mb-2">2. ItemGroup</h2>
        </FormLayout.Field>

        {/* Root直下のItemGroup */}
        <FormLayout.Section label="グループ（Root直下）" labelEnd={<span className="text-xs text-gray-500">オプション</span>}>
          <FormLayout.Field label="氏名">
            <input type="text" className="border w-full" />
          </FormLayout.Field>
          <FormLayout.Field label="住所" fullWidth>
            <textarea className="border w-full h-16" />
          </FormLayout.Field>
        </FormLayout.Section>

        {/* Column 内の ItemGroup（推奨の使い方） */}
        <FormLayout.Section label="プロフィール">
          <FormLayout.Section>
            <FormLayout.Section label="基本情報">
              <FormLayout.Field label="メール">
                <input type="email" className="border w-full" />
              </FormLayout.Field>
              <FormLayout.Field label="電話番号">
                <input type="tel" className="border w-full" />
              </FormLayout.Field>
              <FormLayout.Field label="備考" fullWidth>
                <textarea className="border w-full h-16" />
              </FormLayout.Field>
            </FormLayout.Section>
          </FormLayout.Section>

          <FormLayout.Section>
            <FormLayout.Field label="年齢">
              <input type="number" className="border w-full" />
            </FormLayout.Field>
            <FormLayout.Field label="性別">
              <select className="border w-full">
                <option>未選択</option>
                <option>男性</option>
                <option>女性</option>
                <option>その他</option>
              </select>
            </FormLayout.Field>
          </FormLayout.Section>
        </FormLayout.Section>

        <FormLayout.Spacer />

        {/* ========== ResponsiveColumnGroup（2段/縦並び切替） ========== */}
        <FormLayout.Field>
          <h2 className="text-lg font-bold text-purple-600 border-b-2 border-purple-600 pb-1 mb-2">3. ResponsiveColumnGroup</h2>
        </FormLayout.Field>

        {/* ラベルなしのグループ */}
        <FormLayout.Section>
          <FormLayout.Section>
            <FormLayout.Field label="左1">
              <input type="text" className="border w-full" />
            </FormLayout.Field>
            <FormLayout.Field label="左2">
              <input type="text" className="border w-full" />
            </FormLayout.Field>
          </FormLayout.Section>
          <FormLayout.Section>
            <FormLayout.Field label="右1">
              <input type="text" className="border w-full" />
            </FormLayout.Field>
          </FormLayout.Section>
        </FormLayout.Section>

        {/* ラベル + labelEnd あり */}
        <FormLayout.Section label="検索条件" labelEnd={<button className="ml-2 px-2 py-1 bg-blue-500 text-white text-xs rounded">リセット</button>}>
          <FormLayout.Section>
            <FormLayout.Field label="キーワード">
              <input type="text" className="border w-full" />
            </FormLayout.Field>
            <FormLayout.Field label="カテゴリ">
              <select className="border w-full">
                <option>すべて</option>
                <option>カテゴリA</option>
                <option>カテゴリB</option>
              </select>
            </FormLayout.Field>
          </FormLayout.Section>
          <FormLayout.Section>
            <FormLayout.Field label="期間" fullWidth>
              <div className="flex gap-2">
                <input type="date" className="border" />
                <span className="self-center">〜</span>
                <input type="date" className="border" />
              </div>
            </FormLayout.Field>
          </FormLayout.Section>
        </FormLayout.Section>

        <FormLayout.Spacer />

        {/* ========== ResponsiveColumn（カスタムクラス/入れ子） ========== */}
        <FormLayout.Field>
          <h2 className="text-lg font-bold text-orange-600 border-b-2 border-orange-600 pb-1 mb-2">4. ResponsiveColumn</h2>
        </FormLayout.Field>

        <FormLayout.Section label="カラムのバリエーション">
          <FormLayout.Section className="bg-gray-50 p-2 rounded">
            <FormLayout.Field label="説明">
              <input type="text" className="border w-full" placeholder="classNameの適用確認" />
            </FormLayout.Field>
            <FormLayout.Section label="入れ子グループ">
              <FormLayout.Field label="内側1">
                <input type="text" className="border w-full" />
              </FormLayout.Field>
              <FormLayout.Field label="内側2" fullWidth>
                <textarea className="border w-full h-16" />
              </FormLayout.Field>
            </FormLayout.Section>
          </FormLayout.Section>

          <FormLayout.Section>
            <FormLayout.Field label="複数要素" fullWidth>
              <div className="space-y-2">
                <input type="text" className="border w-full" placeholder="入力1" />
                <input type="text" className="border w-full" placeholder="入力2" />
                <select className="border w-full">
                  <option>選択1</option>
                  <option>選択2</option>
                </select>
              </div>
            </FormLayout.Field>
          </FormLayout.Section>
        </FormLayout.Section>

        <FormLayout.Spacer />

        {/* ========== Separator（区切りの使い分け） ========== */}
        <FormLayout.Field>
          <h2 className="text-lg font-bold text-rose-600 border-b-2 border-rose-600 pb-1 mb-2">5. Separator</h2>
        </FormLayout.Field>

        <FormLayout.Field label="前の要素">
          <input type="text" className="border w-full" />
        </FormLayout.Field>
        <FormLayout.Separator />
        <FormLayout.Field label="後の要素">
          <input type="text" className="border w-full" />
        </FormLayout.Field>

        {/* 最後に解像度依存の切替説明 */}
        <FormLayout.Field label="レスポンシブ説明" fullWidth>
          <div className="bg-yellow-50 p-3 border border-yellow-200 rounded">
            <p className="text-sm">
              ブラウザ幅に応じて `ResponsiveColumnGroup` のカラムが縦・横に切り替わります。
            </p>
          </div>
        </FormLayout.Field>

      </FormLayout.Root>
    </div>
  )
}
