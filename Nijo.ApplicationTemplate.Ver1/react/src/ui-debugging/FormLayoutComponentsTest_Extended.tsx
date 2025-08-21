import FormLayout from "../layout/FormLayout"

/**
 * FormLayoutコンポーネントのパターン網羅テスト（拡張）
 * 5つの主要コンポーネントの使い方を多角的に確認する
 */
export default function FormLayoutComponentsTest_Extended() {
  return (
    <div className="w-full h-full flex border p-4 overflow-y-auto">
      <FormLayout.Root className="flex-1 w-full" labelWidthPx={140} valueWidthPx={220} labelAlign="right">

        {/* ============== 基本のItem ============== */}
        <h2 className="text-lg font-bold text-sky-600 border-b-2 border-sky-600 pb-1 mb-2">1. Item</h2>

        {/* 横並び（Root直下, 通常） */}
        <FormLayout.Item label="通常のItem">
          <input type="text" className="border w-full" placeholder="横並び（ラベル右）" />
        </FormLayout.Item>

        {/* 縦並び（Root直下, fullWidth） */}
        <FormLayout.Item label="縦並びItem" fullWidth>
          <textarea className="border w-full h-16" placeholder="fullWidth指定で縦並び" />
        </FormLayout.Item>

        {/* ラベルなし */}
        <FormLayout.Item>
          <input type="text" className="border w-full" placeholder="ラベルなしItem" />
        </FormLayout.Item>

        {/* labelEnd 付き */}
        <FormLayout.Item label="必須項目" labelEnd={<span className="text-red-500 ml-1">*</span>}>
          <input type="text" className="border w-full" placeholder="labelEndの表示" />
        </FormLayout.Item>

        <FormLayout.Separator />

        {/* ========== ItemGroupInResponsiveColumn ========== */}
        <h2 className="text-lg font-bold text-emerald-600 border-b-2 border-emerald-600 pb-1 mb-2">2. ItemGroupInResponsiveColumn</h2>

        {/* Root直下のItemGroup（仕様上は許可されないので確認不要） */}
        {/* <FormLayout.ItemGroupInResponsiveColumn label="グループ（Root直下）" labelEnd={<span className="text-xs text-gray-500">オプション</span>}>
          <FormLayout.Item label="氏名">
            <input type="text" className="border w-full" />
          </FormLayout.Item>
          <FormLayout.Item label="住所" fullWidth>
            <textarea className="border w-full h-16" />
          </FormLayout.Item>
        </FormLayout.ItemGroupInResponsiveColumn> */}

        {/* Column 内の ItemGroup（推奨の使い方） */}
        <FormLayout.ResponsiveColumnGroup label="プロフィール">
          <FormLayout.ResponsiveColumn>
            <FormLayout.ItemGroupInResponsiveColumn label="基本情報">
              <FormLayout.Item label="メール">
                <input type="email" className="border w-full" />
              </FormLayout.Item>
              <FormLayout.Item label="電話番号">
                <input type="tel" className="border w-full" />
              </FormLayout.Item>
              <FormLayout.Item label="備考" fullWidth>
                <textarea className="border w-full h-16" />
              </FormLayout.Item>
            </FormLayout.ItemGroupInResponsiveColumn>
          </FormLayout.ResponsiveColumn>

          <FormLayout.ResponsiveColumn>
            <FormLayout.Item label="年齢">
              <input type="number" className="border w-full" />
            </FormLayout.Item>
            <FormLayout.Item label="性別">
              <select className="border w-full">
                <option>未選択</option>
                <option>男性</option>
                <option>女性</option>
                <option>その他</option>
              </select>
            </FormLayout.Item>
          </FormLayout.ResponsiveColumn>
        </FormLayout.ResponsiveColumnGroup>

        <FormLayout.Separator />

        {/* ========== ResponsiveColumnGroup（2段/縦並び切替） ========== */}
        <h2 className="text-lg font-bold text-purple-600 border-b-2 border-purple-600 pb-1 mb-2">3. ResponsiveColumnGroup</h2>

        {/* ラベルなしのグループ */}
        <FormLayout.ResponsiveColumnGroup>
          <FormLayout.ResponsiveColumn>
            <FormLayout.Item label="左1">
              <input type="text" className="border w-full" />
            </FormLayout.Item>
            <FormLayout.Item label="左2">
              <input type="text" className="border w-full" />
            </FormLayout.Item>
          </FormLayout.ResponsiveColumn>
          <FormLayout.ResponsiveColumn>
            <FormLayout.Item label="右1">
              <input type="text" className="border w-full" />
            </FormLayout.Item>
          </FormLayout.ResponsiveColumn>
        </FormLayout.ResponsiveColumnGroup>

        {/* ラベル + labelEnd あり */}
        <FormLayout.ResponsiveColumnGroup label="検索条件" labelEnd={<button className="ml-2 px-2 py-1 bg-blue-500 text-white text-xs rounded">リセット</button>}>
          <FormLayout.ResponsiveColumn>
            <FormLayout.Item label="キーワード">
              <input type="text" className="border w-full" />
            </FormLayout.Item>
            <FormLayout.Item label="カテゴリ">
              <select className="border w-full">
                <option>すべて</option>
                <option>カテゴリA</option>
                <option>カテゴリB</option>
              </select>
            </FormLayout.Item>
          </FormLayout.ResponsiveColumn>
          <FormLayout.ResponsiveColumn>
            <FormLayout.Item label="期間" fullWidth>
              <div className="flex gap-2">
                <input type="date" className="border" />
                <span className="self-center">〜</span>
                <input type="date" className="border" />
              </div>
            </FormLayout.Item>
          </FormLayout.ResponsiveColumn>
        </FormLayout.ResponsiveColumnGroup>

        <FormLayout.Separator />

        {/* ========== ResponsiveColumn（カスタムクラス/入れ子） ========== */}
        <h2 className="text-lg font-bold text-orange-600 border-b-2 border-orange-600 pb-1 mb-2">4. ResponsiveColumn</h2>

        <FormLayout.ResponsiveColumnGroup label="カラムのバリエーション">
          <FormLayout.ResponsiveColumn className="bg-gray-50 p-2 rounded">
            <FormLayout.Item label="説明">
              <input type="text" className="border w-full" placeholder="classNameの適用確認" />
            </FormLayout.Item>
            <FormLayout.ItemGroupInResponsiveColumn label="入れ子グループ">
              <FormLayout.Item label="内側1">
                <input type="text" className="border w-full" />
              </FormLayout.Item>
              <FormLayout.Item label="内側2" fullWidth>
                <textarea className="border w-full h-16" />
              </FormLayout.Item>
            </FormLayout.ItemGroupInResponsiveColumn>
          </FormLayout.ResponsiveColumn>

          <FormLayout.ResponsiveColumn>
            <FormLayout.Item label="複数要素" fullWidth>
              <div className="space-y-2">
                <input type="text" className="border w-full" placeholder="入力1" />
                <input type="text" className="border w-full" placeholder="入力2" />
                <select className="border w-full">
                  <option>選択1</option>
                  <option>選択2</option>
                </select>
              </div>
            </FormLayout.Item>
          </FormLayout.ResponsiveColumn>
        </FormLayout.ResponsiveColumnGroup>

        <FormLayout.Separator />

        {/* ========== Separator（区切りの使い分け） ========== */}
        <h2 className="text-lg font-bold text-rose-600 border-b-2 border-rose-600 pb-1 mb-2">5. Separator</h2>

        <FormLayout.Item label="前の要素">
          <input type="text" className="border w-full" />
        </FormLayout.Item>
        <FormLayout.Separator />
        <FormLayout.Item label="後の要素">
          <input type="text" className="border w-full" />
        </FormLayout.Item>

        {/* 最後に解像度依存の切替説明 */}
        <FormLayout.Item label="レスポンシブ説明" fullWidth>
          <div className="bg-yellow-50 p-3 border border-yellow-200 rounded">
            <p className="text-sm">
              ブラウザ幅に応じて `ResponsiveColumnGroup` のカラムが縦・横に切り替わります。
            </p>
          </div>
        </FormLayout.Item>

      </FormLayout.Root>
    </div>
  )
}
