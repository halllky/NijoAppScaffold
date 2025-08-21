import React from "react"
import ResponsiveForm from "../layout/ResponsiveForm"

/**
 * ResponsiveFormで中継コンポーネントを使用するテスト
 * toLayoutedChildren が React.Children.toArray で中継コンポーネントを正しく処理できるかを検証
 */

// 単一のFormItemを返すコンポーネント
const SingleFormItem = ({ label, placeholder }: { label: string; placeholder?: string }) => {
  return (
    <ResponsiveForm.Item label={label}>
      <input type="text" className="border w-full" placeholder={placeholder} />
    </ResponsiveForm.Item>
  )
}

// 単一のSectionを返すコンポーネント
const SingleSection = ({ label, children }: { label: string; children: React.ReactNode }) => {
  return (
    <ResponsiveForm.Section label={label}>
      {children}
    </ResponsiveForm.Section>
  )
}

// 複数のFormItemを返すコンポーネント（Fragment使用）
const MultipleFormItemsFragment = ({ prefix }: { prefix: string }) => {
  return (
    <>
      <ResponsiveForm.Item label={`${prefix}-1`}>
        <input type="text" className="border w-full" placeholder="Fragment内の要素1" />
      </ResponsiveForm.Item>
      <ResponsiveForm.Item label={`${prefix}-2`}>
        <input type="text" className="border w-full" placeholder="Fragment内の要素2" />
      </ResponsiveForm.Item>
    </>
  )
}

// 複数のFormItemを返すコンポーネント（配列使用）
const MultipleFormItemsArray = ({ prefix }: { prefix: string }) => {
  return [
    <ResponsiveForm.Item key="1" label={`${prefix}-配列1`}>
      <input type="text" className="border w-full" placeholder="配列内の要素1" />
    </ResponsiveForm.Item>,
    <ResponsiveForm.Item key="2" label={`${prefix}-配列2`}>
      <input type="text" className="border w-full" placeholder="配列内の要素2" />
    </ResponsiveForm.Item>,
  ]
}

// 条件付きでFormItemを返すコンポーネント
const ConditionalFormItem = ({ show, label }: { show: boolean; label: string }) => {
  if (!show) return null

  return (
    <ResponsiveForm.Item label={label}>
      <input type="text" className="border w-full" placeholder="条件付き表示要素" />
    </ResponsiveForm.Item>
  )
}

// fullWidthのFormItemを返すコンポーネント
const FullWidthFormItem = ({ label, placeholder }: { label: string; placeholder?: string }) => {
  return (
    <ResponsiveForm.Item label={label} fullWidth>
      <textarea className="border w-full h-20" placeholder={placeholder || "fullWidth要素"} />
    </ResponsiveForm.Item>
  )
}

// BreakPointを含むコンポーネント
const FormItemWithBreakPoint = ({ label }: { label: string }) => {
  return (
    <>
      <ResponsiveForm.Item label={label}>
        <input type="text" className="border w-full" placeholder="BreakPoint前の要素" />
      </ResponsiveForm.Item>
      <ResponsiveForm.BreakPoint />
    </>
  )
}

// ネストした中継コンポーネント
const NestedComponent = ({ prefix }: { prefix: string }) => {
  return (
    <div>
      <SingleFormItem label={`${prefix}-ネスト1`} placeholder="ネストコンポーネント内" />
      <MultipleFormItemsFragment prefix={`${prefix}-ネスト`} />
    </div>
  )
}

// fullWidth Sectionを返すコンポーネント
const FullWidthSection = ({ label, children }: { label: string; children: React.ReactNode }) => {
  return (
    <ResponsiveForm.Section label={label} fullWidth>
      {children}
    </ResponsiveForm.Section>
  )
}

// 複雑な構造のコンポーネント
const ComplexComponent = ({ prefix }: { prefix: string }) => {
  return (
    <>
      <SingleFormItem label={`${prefix}-複雑1`} />
      <ResponsiveForm.Spacer />
      <SingleSection label={`${prefix}-複雑セクション`}>
        <SingleFormItem label={`${prefix}-セクション内1`} />
        <FullWidthFormItem label={`${prefix}-セクション内fullWidth`} />
      </SingleSection>
      <MultipleFormItemsFragment prefix={`${prefix}-複雑`} />
    </>
  )
}

export default function () {
  return (
    <div className="w-full h-full flex border p-4 overflow-auto">
      <ResponsiveForm.Container className="flex-1" labelWidthPx={150}>

        {/* 基本パターン: 単一要素を返すコンポーネント */}
        <SingleFormItem label="単一コンポーネント1" placeholder="中継コンポーネント経由" />
        <SingleFormItem label="単一コンポーネント2" />
        <SingleFormItem label="単一コンポーネント3" />

        {/* Sectionを返すコンポーネント */}
        <SingleSection label="中継Section">
          <SingleFormItem label="Section内-1" />
          <SingleFormItem label="Section内-2" />
        </SingleSection>

        {/* 複数要素を返すコンポーネント（Fragment） */}
        <MultipleFormItemsFragment prefix="Fragment" />

        {/* fullWidth要素を返すコンポーネント */}
        <FullWidthFormItem label="中継fullWidth" placeholder="中継コンポーネント経由のfullWidth" />

        {/* 複数要素を返すコンポーネント（配列） */}
        <MultipleFormItemsArray prefix="配列" />

        {/* 条件付きコンポーネント */}
        <ConditionalFormItem show={true} label="条件付き表示" />
        <ConditionalFormItem show={false} label="条件付き非表示" />
        <SingleFormItem label="条件後の要素" />

        {/* BreakPointを含むコンポーネント */}
        <FormItemWithBreakPoint label="BreakPoint付き" />
        <SingleFormItem label="BreakPoint後1" />
        <SingleFormItem label="BreakPoint後2" />

        {/* 手動Spacer */}
        <ResponsiveForm.Spacer line />

        {/* fullWidth Sectionを返すコンポーネント */}
        <FullWidthSection label="中継fullWidthSection">
          <SingleFormItem label="fullWidthSection内-1" />
          <SingleFormItem label="fullWidthSection内-2" />
          <FullWidthFormItem label="Section内fullWidth" />
        </FullWidthSection>

        {/* ネストした中継コンポーネント */}
        <NestedComponent prefix="ネスト" />

        {/* 複雑な構造のコンポーネント */}
        <ComplexComponent prefix="複雑" />

        {/* 直接配置との混在 */}
        <ResponsiveForm.Item label="直接配置1">
          <input type="text" className="border w-full" placeholder="直接配置要素" />
        </ResponsiveForm.Item>
        <SingleFormItem label="中継配置" placeholder="中継コンポーネント" />
        <ResponsiveForm.Item label="直接配置2">
          <input type="text" className="border w-full" placeholder="直接配置要素" />
        </ResponsiveForm.Item>

        {/* 最終グループ */}
        <MultipleFormItemsFragment prefix="最終" />
        <SingleFormItem label="最終要素" placeholder="テスト完了" />

      </ResponsiveForm.Container>
    </div>
  )
}
