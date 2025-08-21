import FormLayout from "../layout/FormLayout"

/**
 * FormLayoutコンポーネントのパターン網羅テスト
 * 5つのコンポーネントの様々な使用パターンをテストする
 */
export default function FormLayoutComponentsTest() {
  return (
    <div className="w-full h-full flex border p-4 overflow-y-auto">
      <FormLayout.Root className="flex-1 w-full" labelWidthPx={150}>

        {/* =========================== */}
        {/* Item コンポーネントのテスト */}
        {/* =========================== */}
        <h2 className="text-lg font-bold text-blue-600 border-b-2 border-blue-600 pb-1 mb-2">
          1. Item コンポーネントのパターン
        </h2>

        {/* Root直下のItem - 通常（横並び） */}
        <FormLayout.Item label="Root直下のItem">
          <input type="text" className="border w-full" placeholder="通常のhorizontal表示" />
        </FormLayout.Item>

        {/* Root直下のItem - vertical */}
        <FormLayout.Item label="Root直下のItem（vertical）" fullWidth>
          <input type="text" className="border w-full" placeholder="vertical指定で下に配置" />
        </FormLayout.Item>

        {/* labelEndプロパティ付きのItem */}
        <FormLayout.Item
          label="labelEnd付きItem"
          labelEnd={<span className="text-red-500 ml-1">*</span>}
        >
          <input type="text" className="border w-full" placeholder="ラベル後に要素追加" />
        </FormLayout.Item>

        {/* ラベルなしのItem */}
        <FormLayout.Item>
          <input type="text" className="border w-full" placeholder="ラベルなしのItem" />
        </FormLayout.Item>

        <FormLayout.Separator />

        {/* =============================== */}
        {/* ItemGroup コンポーネントのテスト */}
        {/* =============================== */}
        <h2 className="text-lg font-bold text-green-600 border-b-2 border-green-600 pb-1 mb-2">
          2. ItemGroup コンポーネントのパターン
        </h2>

        {/* Root直下のItemGroup */}
        <FormLayout.ItemGroupInResponsiveColumn label="Root直下のItemGroup">
          <FormLayout.Item label="グループ内Item1">
            <input type="text" className="border w-full" />
          </FormLayout.Item>
          <FormLayout.Item label="グループ内Item2">
            <input type="text" className="border w-full" />
          </FormLayout.Item>
          <FormLayout.Item label="グループ内vertical Item" fullWidth>
            <textarea className="border w-full h-16" placeholder="グループ内のvertical要素" />
          </FormLayout.Item>
        </FormLayout.ItemGroupInResponsiveColumn>

        {/* labelEnd付きのItemGroup */}
        <FormLayout.ItemGroupInResponsiveColumn
          label="labelEnd付きItemGroup"
          labelEnd={<button className="ml-2 px-2 py-1 bg-blue-500 text-white text-xs rounded">編集</button>}
        >
          <FormLayout.Item label="アイテム1">
            <input type="text" className="border w-full" />
          </FormLayout.Item>
          <FormLayout.Item label="アイテム2">
            <select className="border w-full">
              <option>選択してください</option>
              <option>オプション1</option>
              <option>オプション2</option>
            </select>
          </FormLayout.Item>
        </FormLayout.ItemGroupInResponsiveColumn>

        {/* ラベルなしのItemGroup */}
        < FormLayout.ItemGroupInResponsiveColumn>
          <FormLayout.Item label="ラベルなしグループ内Item1">
            <input type="text" className="border w-full" />
          </FormLayout.Item>
          <FormLayout.Item label="ラベルなしグループ内Item2">
            <input type="text" className="border w-full" />
          </FormLayout.Item>
        </FormLayout.ItemGroupInResponsiveColumn>

        <FormLayout.Separator />

        {/* ================================= */}
        {/* ColumnGroup コンポーネントのテスト */}
        {/* ================================= */}
        <h2 className="text-lg font-bold text-purple-600 border-b-2 border-purple-600 pb-1 mb-2">
          3. ColumnGroup コンポーネントのパターン
        </h2>

        {/* 基本的なColumnGroup（レスポンシブ） */}
        <FormLayout.ResponsiveColumnGroup label="基本的なColumnGroup（レスポンシブ）">
          <FormLayout.ResponsiveColumn>
            <FormLayout.Item label="左カラムItem1">
              <input type="text" className="border w-full" />
            </FormLayout.Item>
            <FormLayout.Item label="左カラムItem2">
              <input type="text" className="border w-full" />
            </FormLayout.Item>
          </FormLayout.ResponsiveColumn>
          <FormLayout.ResponsiveColumn>
            <FormLayout.Item label="右カラムItem1">
              <input type="text" className="border w-full" />
            </FormLayout.Item>
            <FormLayout.Item label="右カラムItem2">
              <input type="text" className="border w-full" />
            </FormLayout.Item>
          </FormLayout.ResponsiveColumn>
        </FormLayout.ResponsiveColumnGroup>

        {/* 3つのColumnを持つColumnGroup */}
        <FormLayout.ResponsiveColumnGroup label="3カラムのColumnGroup">
          <FormLayout.ResponsiveColumn>
            <FormLayout.Item label="第1カラム">
              <input type="text" className="border w-full" />
            </FormLayout.Item>
          </FormLayout.ResponsiveColumn>
          <FormLayout.ResponsiveColumn>
            <FormLayout.Item label="第2カラム">
              <input type="text" className="border w-full" />
            </FormLayout.Item>
          </FormLayout.ResponsiveColumn>
          <FormLayout.ResponsiveColumn>
            <FormLayout.Item label="第3カラム">
              <input type="text" className="border w-full" />
            </FormLayout.Item>
          </FormLayout.ResponsiveColumn>
        </FormLayout.ResponsiveColumnGroup>

        {/* labelEnd付きのColumnGroup */}
        <FormLayout.ResponsiveColumnGroup
          label="labelEnd付きColumnGroup"
          labelEnd={<span className="ml-2 text-sm text-gray-500">（オプション項目）</span>}
        >
          <FormLayout.ResponsiveColumn>
            <FormLayout.Item label="オプション1">
              <input type="checkbox" className="mr-2" />
              <span>チェックボックス</span>
            </FormLayout.Item>
            <FormLayout.Item label="オプション2">
              <input type="radio" name="radio1" className="mr-2" />
              <span>ラジオボタン1</span>
            </FormLayout.Item>
          </FormLayout.ResponsiveColumn>
          <FormLayout.ResponsiveColumn>
            <FormLayout.Item label="オプション3">
              <input type="radio" name="radio1" className="mr-2" />
              <span>ラジオボタン2</span>
            </FormLayout.Item>
            <FormLayout.Item label="オプション4">
              <input type="range" className="w-full" />
            </FormLayout.Item>
          </FormLayout.ResponsiveColumn>
        </FormLayout.ResponsiveColumnGroup>

        {/* ラベルなしのColumnGroup */}
        <FormLayout.ResponsiveColumnGroup>
          <FormLayout.ResponsiveColumn>
            <FormLayout.Item label="ラベルなしグループ項目1">
              <input type="text" className="border w-full" />
            </FormLayout.Item>
          </FormLayout.ResponsiveColumn>
          <FormLayout.ResponsiveColumn>
            <FormLayout.Item label="ラベルなしグループ項目2">
              <input type="text" className="border w-full" />
            </FormLayout.Item>
          </FormLayout.ResponsiveColumn>
        </FormLayout.ResponsiveColumnGroup>

        <FormLayout.Separator />

        {/* ============================ */}
        {/* Column コンポーネントのテスト */}
        {/* ============================ */}
        <h2 className="text-lg font-bold text-orange-600 border-b-2 border-orange-600 pb-1 mb-2">
          4. Column コンポーネントのパターン
        </h2>

        <FormLayout.ResponsiveColumnGroup label="Column内の様々なパターン">
          <FormLayout.ResponsiveColumn>
            <FormLayout.Item label="通常Item">
              <input type="text" className="border w-full" />
            </FormLayout.Item>
            <FormLayout.Item label="vertical Item" fullWidth>
              <textarea className="border w-full h-16" placeholder="vertical指定" />
            </FormLayout.Item>

            {/* Column内のItemGroup */}
            <FormLayout.ItemGroupInResponsiveColumn label="Column内のItemGroup">
              <FormLayout.Item label="ネストされたItem1">
                <input type="text" className="border w-full" />
              </FormLayout.Item>
              <FormLayout.Item label="ネストされたItem2">
                <input type="text" className="border w-full" />
              </FormLayout.Item>
            </FormLayout.ItemGroupInResponsiveColumn>
          </FormLayout.ResponsiveColumn>

          <FormLayout.ResponsiveColumn className="bg-gray-50 p-2 rounded">
            <FormLayout.Item label="カスタムスタイルColumn">
              <input type="text" className="border w-full" placeholder="Column className指定" />
            </FormLayout.Item>
            <FormLayout.Item label="複数の入力要素" fullWidth>
              <div className="space-y-2">
                <input type="text" className="border w-full" placeholder="入力1" />
                <input type="text" className="border w-full" placeholder="入力2" />
                <select className="border w-full">
                  <option>選択肢1</option>
                  <option>選択肢2</option>
                </select>
              </div>
            </FormLayout.Item>
          </FormLayout.ResponsiveColumn>
        </FormLayout.ResponsiveColumnGroup >

        <FormLayout.Separator />

        {/* =============================== */}
        {/* Separator コンポーネントのテスト */}
        {/* =============================== */}
        <h2 className="text-lg font-bold text-red-600 border-b-2 border-red-600 pb-1 mb-2">
          5. Separator コンポーネントのパターン
        </h2>

        <FormLayout.Item label="Separator前の要素">
          <input type="text" className="border w-full" />
        </FormLayout.Item>

        <FormLayout.Separator />

        <FormLayout.Item label="Separator後の要素">
          <input type="text" className="border w-full" />
        </FormLayout.Item>

        <FormLayout.Separator />

        {/* =============================== */}
        {/* 複合パターンのテスト */}
        {/* =============================== */}
        <h2 className="text-lg font-bold text-indigo-600 border-b-2 border-indigo-600 pb-1 mb-2">
          6. 複合パターンのテスト
        </h2>

        {/* 複雑な組み合わせ */}
        <FormLayout.ResponsiveColumnGroup label="複雑な組み合わせパターン">
          <FormLayout.ResponsiveColumn>
            <FormLayout.Item label="基本情報">
              <input type="text" className="border w-full" placeholder="名前" />
            </FormLayout.Item>

            <FormLayout.ItemGroupInResponsiveColumn label="連絡先情報">
              <FormLayout.Item label="メールアドレス">
                <input type="email" className="border w-full" />
              </FormLayout.Item>
              <FormLayout.Item label="電話番号">
                <input type="tel" className="border w-full" />
              </FormLayout.Item>
              <FormLayout.Item label="住所" fullWidth>
                <textarea className="border w-full h-20" placeholder="住所を入力してください" />
              </FormLayout.Item>
            </FormLayout.ItemGroupInResponsiveColumn>
          </FormLayout.ResponsiveColumn>

          <FormLayout.ResponsiveColumn>
            <FormLayout.Item label="年齢">
              <input type="number" className="border w-full" />
            </FormLayout.Item>

            <FormLayout.ItemGroupInResponsiveColumn
              label="設定項目"
              labelEnd={<span className="text-xs text-gray-500">（任意）</span>}
            >
              <FormLayout.Item label="通知設定">
                <div className="space-y-1">
                  <label className="flex items-center">
                    <input type="checkbox" className="mr-2" />
                    メール通知
                  </label>
                  <label className="flex items-center">
                    <input type="checkbox" className="mr-2" />
                    SMS通知
                  </label>
                </div>
              </FormLayout.Item>
              <FormLayout.Item label="プライバシー">
                <select className="border w-full">
                  <option>公開</option>
                  <option>限定公開</option>
                  <option>非公開</option>
                </select>
              </FormLayout.Item>
            </FormLayout.ItemGroupInResponsiveColumn>
          </FormLayout.ResponsiveColumn >
        </FormLayout.ResponsiveColumnGroup >

        <FormLayout.Separator />

        {/* レスポンシブ動作確認用 */}
        <FormLayout.Item label="レスポンシブテスト説明" fullWidth>
          <div className="bg-yellow-50 p-3 border border-yellow-200 rounded">
            <p className="text-sm">
              ⭐ <strong>レスポンシブ動作の確認方法：</strong><br />
              ブラウザの幅を変更してColumnGroupが縦並び↔横並びに切り替わることを確認してください。
              ブレークポイントは (labelWidth + valueWidth) × 2 + 16px で計算されます。
            </p>
          </div>
        </FormLayout.Item>

        <div className="mt-4 p-4 bg-gray-100 rounded">
          <h3 className="font-bold text-gray-700 mb-2">テスト完了</h3>
          <p className="text-sm text-gray-600">
            以上で5つのコンポーネント（Item, ItemGroup, ColumnGroup, Column, Separator）の
            主要なパターンをすべて網羅しました。
          </p>
        </div>

      </FormLayout.Root >
    </div >
  )
}
