import ResponsiveForm from "../layout/ResponsiveForm"

/**
 * ResponsiveFormの基本的な使い方
 */
export default function () {
  return (
    <div className="w-full h-full flex border p-4 overflow-auto">
      <ResponsiveForm.Container className="flex-1" labelWidthPx={140}>

        {/* Root直下に full width でない要素を並べる例。明示的な BreakPoint の指定を行なわないパターン */}
        <ResponsiveForm.Item label="1番目のメンバー">
          <input type="text" className="border w-full" />
        </ResponsiveForm.Item>
        <ResponsiveForm.Item label="2番目のメンバー">
          <input type="text" className="border w-full" />
        </ResponsiveForm.Item>
        <ResponsiveForm.Item label="3番目のメンバー">
          <input type="text" className="border w-full" />
        </ResponsiveForm.Item>

        <ResponsiveForm.Section label="枠で囲まれるセクション">
          <ResponsiveForm.Item label="4番目のメンバー">
            <input type="text" className="border w-full" />
          </ResponsiveForm.Item>
          <ResponsiveForm.Item label="5番目のメンバー">
            <input type="text" className="border w-full" />
          </ResponsiveForm.Item>
          <ResponsiveForm.Item label="6番目のメンバー" fullWidth>
            <input type="text" className="border w-full" placeholder="Section内の fullWidth メンバー" />
          </ResponsiveForm.Item>
        </ResponsiveForm.Section>

        <ResponsiveForm.Item label="7番目のメンバー">
          <input type="text" className="border w-full" />
        </ResponsiveForm.Item>
        <ResponsiveForm.Item label="8番目のメンバー">
          <input type="text" className="border w-full" />
        </ResponsiveForm.Item>

        {/* full width */}
        <ResponsiveForm.Item label="9番目のメンバー" fullWidth>
          <input type="text" className="border w-full" />
        </ResponsiveForm.Item>

        {/* BreakPointの明示的な指定パターン */}
        <ResponsiveForm.Item label="10番目のメンバー">
          <input type="text" className="border w-full" placeholder="↓ここにブレークポイント" />
        </ResponsiveForm.Item>
        <ResponsiveForm.BreakPoint />
        <ResponsiveForm.Item label="11番目のメンバー">
          <input type="text" className="border w-full" placeholder="BreakPoint後の要素" />
        </ResponsiveForm.Item>
        <ResponsiveForm.Item label="12番目のメンバー">
          <input type="text" className="border w-full" />
        </ResponsiveForm.Item>
        <ResponsiveForm.Item label="13番目のメンバー">
          <input type="text" className="border w-full" />
        </ResponsiveForm.Item>

        {/* 手動でのSpacer挿入パターン */}
        <ResponsiveForm.Spacer />

        {/* 複数のSectionが連続するパターン */}
        <ResponsiveForm.Section label="1番目の連続セクション">
          <ResponsiveForm.Item label="Section1-1">
            <input type="text" className="border w-full" />
          </ResponsiveForm.Item>
          <ResponsiveForm.Item label="Section1-2">
            <input type="text" className="border w-full" />
          </ResponsiveForm.Item>
        </ResponsiveForm.Section>

        <ResponsiveForm.Section label="2番目の連続セクション">
          <ResponsiveForm.Item label="Section2-1">
            <input type="text" className="border w-full" />
          </ResponsiveForm.Item>
          <ResponsiveForm.Item label="Section2-2">
            <input type="text" className="border w-full" />
          </ResponsiveForm.Item>
        </ResponsiveForm.Section>

        {/* 線付きSpacer */}
        <ResponsiveForm.Spacer line />

        {/* fullWidth指定されたSectionのテスト */}
        <ResponsiveForm.Section label="fullWidth指定セクション" fullWidth>
          <ResponsiveForm.Item label="fullWidthSection内-1">
            <input type="text" className="border w-full" placeholder="fullWidth Section内の要素" />
          </ResponsiveForm.Item>
          <ResponsiveForm.Item label="fullWidthSection内-2">
            <input type="text" className="border w-full" />
          </ResponsiveForm.Item>
          <ResponsiveForm.Item label="fullWidthSection内-3" fullWidth>
            <textarea className="border w-full h-16" placeholder="Section内のfullWidth要素" />
          </ResponsiveForm.Item>
          <ResponsiveForm.Item label="fullWidthSection内-4">
            <input type="text" className="border w-full" />
          </ResponsiveForm.Item>
          <ResponsiveForm.Item label="fullWidthSection内-5">
            <input type="text" className="border w-full" />
          </ResponsiveForm.Item>
        </ResponsiveForm.Section>

        <ResponsiveForm.Spacer line />

        {/* 通常のSectionとfullWidthセクションの混在テスト */}
        <ResponsiveForm.Item label="混在テスト1">
          <input type="text" className="border w-full" />
        </ResponsiveForm.Item>

        <ResponsiveForm.Section label="通常のセクション（非fullWidth）">
          <ResponsiveForm.Item label="通常Section-1">
            <input type="text" className="border w-full" />
          </ResponsiveForm.Item>
          <ResponsiveForm.Item label="通常Section-2">
            <input type="text" className="border w-full" />
          </ResponsiveForm.Item>
        </ResponsiveForm.Section>

        <ResponsiveForm.Item label="混在テスト2">
          <input type="text" className="border w-full" />
        </ResponsiveForm.Item>

        {/* 未知の要素（通常のdiv）のテスト */}
        <div className="col-span-full p-4 bg-yellow-100 border border-yellow-300 rounded">
          <p>未知の要素（通常のdiv）。fullWidth要素として扱われる。</p>
        </div>

        {/* BreakPointとfullWidthの組み合わせパターン */}
        <ResponsiveForm.Item label="組み合わせ1">
          <input type="text" className="border w-full" />
        </ResponsiveForm.Item>
        <ResponsiveForm.BreakPoint />
        <ResponsiveForm.Item label="組み合わせ2">
          <input type="text" className="border w-full" />
        </ResponsiveForm.Item>
        <ResponsiveForm.Item label="組み合わせ3" fullWidth>
          <textarea className="border w-full h-20" placeholder="BreakPoint使用後のfullWidth要素" />
        </ResponsiveForm.Item>

        {/* 最後のグループ */}
        <ResponsiveForm.Item label="最終メンバー1">
          <input type="text" className="border w-full" />
        </ResponsiveForm.Item>
        <ResponsiveForm.Item label="最終メンバー2">
          <input type="text" className="border w-full" />
        </ResponsiveForm.Item>
        <ResponsiveForm.Item label="最終メンバー3">
          <input type="text" className="border w-full" />
        </ResponsiveForm.Item>

      </ResponsiveForm.Container>
    </div>
  )
}
