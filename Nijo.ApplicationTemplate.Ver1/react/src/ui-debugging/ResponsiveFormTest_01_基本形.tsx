import FormLayout from "../layout/FormLayout"

/**
 * FormLayoutの基本的な使い方
 */
export default function () {
  return (
    <div className="w-full h-full flex border p-4 overflow-y-auto">
      <FormLayout.Root className="flex-1 w-full" labelWidthPx={140}>

        {/* Root直下に full width でない要素を並べる例 */}
        <FormLayout.ResponsiveColumnGroup>
          <FormLayout.ResponsiveColumn>
            <FormLayout.Item label="1番目のメンバー">
              <input type="text" className="border w-full" />
            </FormLayout.Item>
            <FormLayout.Item label="2番目のメンバー">
              <input type="text" className="border w-full" />
            </FormLayout.Item>
            <FormLayout.Item label="3番目のメンバー">
              <input type="text" className="border w-full" />
            </FormLayout.Item>
          </FormLayout.ResponsiveColumn>

          <FormLayout.ResponsiveColumn>
            <FormLayout.ItemGroupInResponsiveColumn label="枠で囲まれるセクション">
              <FormLayout.Item label="4番目のメンバー">
                <input type="text" className="border w-full" />
              </FormLayout.Item>
              <FormLayout.Item label="5番目のメンバー">
                <input type="text" className="border w-full" />
              </FormLayout.Item>
              <FormLayout.Item label="6番目のメンバー" fullWidth>
                <input type="text" className="border w-full" placeholder="ItemGroup内の fullWidth メンバー" />
              </FormLayout.Item>
            </FormLayout.ItemGroupInResponsiveColumn>

            <FormLayout.Item label="7番目のメンバー">
              <input type="text" className="border w-full" />
            </FormLayout.Item>
            <FormLayout.Item label="8番目のメンバー">
              <input type="text" className="border w-full" />
            </FormLayout.Item>
          </FormLayout.ResponsiveColumn>
        </FormLayout.ResponsiveColumnGroup>

        {/* ルート直下のItem */}
        <FormLayout.Item label="9番目のメンバー（ルート直下のItem。vertical指定）" fullWidth>
          <input type="text" className="border w-full" />
        </FormLayout.Item>
        <FormLayout.Item label="9.5番目のメンバー">
          <input type="text" className="border w-full" />
          <span className="text-xs text-gray-500">
            ※ ルート直下のItem。vertical指定なし
          </span>
        </FormLayout.Item>

        <FormLayout.ResponsiveColumnGroup label="BreakPointの明示的な指定パターン">
          <FormLayout.ResponsiveColumn>
            <FormLayout.Item label="10番目のメンバー">
              <input type="text" className="border w-full" placeholder="↓ここにブレークポイント" />
            </FormLayout.Item>
          </FormLayout.ResponsiveColumn>

          <FormLayout.ResponsiveColumn>
            <FormLayout.Item label="11番目のメンバー">
              <input type="text" className="border w-full" placeholder="BreakPoint後の要素" />
            </FormLayout.Item>
            <FormLayout.Item label="12番目のメンバー">
              <input type="text" className="border w-full" />
            </FormLayout.Item>
            <FormLayout.Item label="13番目のメンバー">
              <input type="text" className="border w-full" />
            </FormLayout.Item>
          </FormLayout.ResponsiveColumn>
        </FormLayout.ResponsiveColumnGroup>

        {/* 手動でのSpacer挿入パターン */}
        <FormLayout.Separator />

        {/* 複数のSectionが連続するパターン */}
        <FormLayout.ResponsiveColumnGroup label="1番目の連続セクション">
          <FormLayout.ResponsiveColumn>
            <FormLayout.Item label="Section1-1">
              <input type="text" className="border w-full" />
            </FormLayout.Item>
            <FormLayout.Item label="Section1-2">
              <input type="text" className="border w-full" />
            </FormLayout.Item>
          </FormLayout.ResponsiveColumn>
        </FormLayout.ResponsiveColumnGroup>

        <FormLayout.ResponsiveColumnGroup label="2番目の連続セクション">
          <FormLayout.ResponsiveColumn>
            <FormLayout.Item label="Section2-1">
              <input type="text" className="border w-full" />
            </FormLayout.Item>
            <FormLayout.Item label="Section2-2">
              <input type="text" className="border w-full" />
            </FormLayout.Item>
          </FormLayout.ResponsiveColumn>
        </FormLayout.ResponsiveColumnGroup>

        {/* 線付きSpacer */}
        <FormLayout.Separator />

        {/* fullWidth指定されたSectionのテスト */}
        <FormLayout.ResponsiveColumnGroup label="fullWidth指定セクション">
          <FormLayout.ResponsiveColumn>
            <FormLayout.Item label="fullWidthSection内-1">
              <input type="text" className="border w-full" placeholder="fullWidth Section内の要素" />
            </FormLayout.Item>
            <FormLayout.Item label="fullWidthSection内-2">
              <input type="text" className="border w-full" />
            </FormLayout.Item>
          </FormLayout.ResponsiveColumn>
          <FormLayout.ResponsiveColumn>
            <FormLayout.Item label="fullWidthSection内-3">
              <input type="text" className="border w-full" />
            </FormLayout.Item>
            <FormLayout.Item label="fullWidthSection内-4">
              <input type="text" className="border w-full" />
            </FormLayout.Item>
          </FormLayout.ResponsiveColumn>
        </FormLayout.ResponsiveColumnGroup>

        <FormLayout.Item fullWidth label="Root直下のItem">
          <input type="text" className="border w-full" placeholder="Root直下のItem" />
        </FormLayout.Item>

        <FormLayout.ResponsiveColumnGroup>
          <FormLayout.ResponsiveColumn>
            <FormLayout.Item label="fullWidthSection内-3" fullWidth>
              <textarea className="border w-full h-16" placeholder="ここはColumnの中" />
            </FormLayout.Item>
            <FormLayout.Item label="fullWidthSection内-4">
              <input type="text" className="border w-full" />
            </FormLayout.Item>
            <FormLayout.Item label="fullWidthSection内-5">
              <input type="text" className="border w-full" />
            </FormLayout.Item>
          </FormLayout.ResponsiveColumn>
          <FormLayout.ResponsiveColumn>
            <FormLayout.Item label="fullWidthSection内-6">
              <input type="text" className="border w-full" />
            </FormLayout.Item>
          </FormLayout.ResponsiveColumn>
        </FormLayout.ResponsiveColumnGroup>

        {/* Root直下の ItemGroup */}
        <FormLayout.ItemGroupInResponsiveColumn label="Root直下のItemGroup">
          <FormLayout.Item label="混在テスト1">
            <input type="text" className="border w-full" />
          </FormLayout.Item>
          <FormLayout.Item label="混在テスト2">
            <input type="text" className="border w-full" />
          </FormLayout.Item>
        </FormLayout.ItemGroupInResponsiveColumn>

        <FormLayout.Separator />

        <FormLayout.ResponsiveColumnGroup label="通常のセクション（非fullWidth）">
          <FormLayout.ResponsiveColumn>
            <FormLayout.Item label="通常Section-1">
              <input type="text" className="border w-full" />
            </FormLayout.Item>
            <FormLayout.Item label="通常Section-2">
              <input type="text" className="border w-full" />
            </FormLayout.Item>
          </FormLayout.ResponsiveColumn>
        </FormLayout.ResponsiveColumnGroup>

        <FormLayout.Item label="混在テスト2">
          <input type="text" className="border w-full" />
        </FormLayout.Item>

        {/* 未知の要素（通常のdiv）のテスト */}
        <div className="col-span-full p-4 bg-yellow-100 border border-yellow-300 rounded">
          <p>未知の要素（通常のdiv）。fullWidth要素として扱われる。</p>
        </div>

        {/* BreakPointとfullWidthの組み合わせパターン */}
        <FormLayout.ResponsiveColumnGroup>
          <FormLayout.ResponsiveColumn>
            <FormLayout.Item label="組み合わせ1">
              <input type="text" className="border w-full" />
            </FormLayout.Item>
            <FormLayout.Item label="組み合わせ2">
              <input type="text" className="border w-full" />
            </FormLayout.Item>
          </FormLayout.ResponsiveColumn>

          <FormLayout.ResponsiveColumn>
            <FormLayout.Item label="組み合わせ3" fullWidth>
              <textarea className="border w-full h-20" placeholder="BreakPoint使用後のfullWidth要素" />
            </FormLayout.Item>
          </FormLayout.ResponsiveColumn>
        </FormLayout.ResponsiveColumnGroup>

        {/* 最後のグループ */}
        <FormLayout.Item label="最終メンバー1">
          <input type="text" className="border w-full" />
        </FormLayout.Item>
        <FormLayout.Item label="最終メンバー2">
          <input type="text" className="border w-full" />
        </FormLayout.Item>
        <FormLayout.Item label="最終メンバー3">
          <input type="text" className="border w-full" />
        </FormLayout.Item>

      </FormLayout.Root>
    </div>
  )
}
