import React from "react"
import FormLayout, { type LabelProps } from "@nijo/ui-components/layout/FormLayout"

export default function FormLayoutPatternsDebugging() {

  return (
    <div className="flex flex-col items-start gap-6 p-4">

      {/* === 基本パターン === */}

      <DemoBox title="基本: Item 群とlabelAlign">
        <FormLayout.Root>
          <FormLayout.Field label="氏名">
            <Input placeholder="山田 太郎" />
          </FormLayout.Field>
          <FormLayout.Field label="メール">
            <Input placeholder="taro@example.com" />
          </FormLayout.Field>
          <FormLayout.Field label="都道府県">
            <Select defaultValue="tokyo">
              <option value="tokyo">東京都</option>
              <option value="kanagawa">神奈川県</option>
              <option value="saitama">埼玉県</option>
            </Select>
          </FormLayout.Field>
          <FormLayout.Field label="fullWidth指定" fullWidth>
            <Input placeholder="備考" />
          </FormLayout.Field>
          <FormLayout.Field>
            <div className="text-amber-600 text-sm bg-amber-50 border border-amber-200 rounded-md p-2">
              *** ラベルなしのItem ***
            </div>
          </FormLayout.Field>
        </FormLayout.Root>
      </DemoBox>

      <DemoBox title="labelAlign: 左寄せ vs 右寄せ" selfStretch>
        <div className="flex flex-col gap-4">
          <div className="flex-1">
            <h4 className="mb-2 text-sm font-bold">左寄せ</h4>
            <FormLayout.Root labelWidthPx={160} labelAlign="left">
              <FormLayout.Field label="会社名"><Input /></FormLayout.Field>
              <FormLayout.Field label="部署"><Input /></FormLayout.Field>
            </FormLayout.Root>
          </div>
          <div className="flex-1">
            <h4 className="mb-2 text-sm font-bold">右寄せ(デフォルト)</h4>
            <FormLayout.Root labelWidthPx={160} labelAlign="right">
              <FormLayout.Field label="会社名"><Input /></FormLayout.Field>
              <FormLayout.Field label="部署"><Input /></FormLayout.Field>
            </FormLayout.Root>
          </div>
          <div className="flex-1">
            <h4 className="mb-2 text-sm font-bold">Fieldレベルでの個別指定</h4>
            <FormLayout.Root labelWidthPx={160}>
              <FormLayout.Field label="会社名(左寄せ)" labelAlign="left"><Input /></FormLayout.Field>
              <FormLayout.Field label="部署(右寄せ)" labelAlign="right"><Input /></FormLayout.Field>
            </FormLayout.Root>
          </div>
        </div>
      </DemoBox>

      {/* === Groupパターン === */}

      <DemoBox title="Group: 基本とborder">
        <FormLayout.Root>
          <FormLayout.Section label="基本情報" border>
            <FormLayout.Field label="氏名"><Input /></FormLayout.Field>
            <FormLayout.Field label="メール" labelEnd={<Badge>必須</Badge>}><Input /></FormLayout.Field>
            <FormLayout.Field label="住所" fullWidth><Input /></FormLayout.Field>
          </FormLayout.Section>

          <FormLayout.Section label="borderなし">
            <FormLayout.Field label="項目A"><Input /></FormLayout.Field>
            <FormLayout.Field label="項目B"><Input /></FormLayout.Field>
          </FormLayout.Section>

          <FormLayout.Section border>
            <FormLayout.Field label="ラベルなしGroup"><Input /></FormLayout.Field>
          </FormLayout.Section>
        </FormLayout.Root>
      </DemoBox>

      <DemoBox title="Separator/Spacer配置パターン">
        <FormLayout.Root labelWidthPx={160}>
          <FormLayout.Section label="区切り要素テスト" border>
            <FormLayout.Separator />
            <FormLayout.Field label="先頭Separator後"><Input /></FormLayout.Field>
            <FormLayout.Spacer />
            <FormLayout.Field label="Spacer後"><Input /></FormLayout.Field>
            <FormLayout.Separator />
            <FormLayout.Field label="中間項目"><Input /></FormLayout.Field>
            <FormLayout.Separator />
          </FormLayout.Section>

          <FormLayout.Field label="Root直下項目"><Input /></FormLayout.Field>
          <FormLayout.Spacer />
          <FormLayout.Field label="Root直下Spacer後"><Input /></FormLayout.Field>
        </FormLayout.Root>
      </DemoBox>

      {/* === Responsiveパターン === */}

      <DemoBox title="Responsive: 基本動作" selfStretch>
        <FormLayout.Root>
          <FormLayout.Section label="基本responsive" responsive border>
            <FormLayout.Section>
              <FormLayout.Field label="TEL"><Input placeholder="03-1234-5678" /></FormLayout.Field>
              <FormLayout.Field label="FAX"><Input placeholder="03-9876-5432" /></FormLayout.Field>
            </FormLayout.Section>
            <FormLayout.Section>
              <FormLayout.Field label="携帯"><Input placeholder="090-0000-0000" /></FormLayout.Field>
            </FormLayout.Section>
          </FormLayout.Section>

          <FormLayout.Section responsive border>
            <FormLayout.Section>
              <FormLayout.Field label="ラベルなし1"><Input /></FormLayout.Field>
              <FormLayout.Field label="ラベルなし2"><Input /></FormLayout.Field>
            </FormLayout.Section>
            <FormLayout.Section>
              <FormLayout.Field label="ラベルなし3"><Input /></FormLayout.Field>
            </FormLayout.Section>
          </FormLayout.Section>
        </FormLayout.Root>
      </DemoBox>

      <DemoBox title="Responsive: 数値指定とfullWidth混在" selfStretch>
        <FormLayout.Root>
          <FormLayout.Section label="responsive={3} (3列以上で横並び)" responsive={3} border>
            <FormLayout.Field label="項目1"><Input /></FormLayout.Field>
            <FormLayout.Field label="項目2"><Input /></FormLayout.Field>
            <FormLayout.Field label="項目3"><Input /></FormLayout.Field>
          </FormLayout.Section>

          <FormLayout.Section label="fullWidth混在" responsive border>
            <FormLayout.Section>
              <FormLayout.Field label="郵便番号"><Input placeholder="100-0000" /></FormLayout.Field>
              <FormLayout.Field label="都道府県"><Input placeholder="東京都" /></FormLayout.Field>
              <FormLayout.Spacer />
              <FormLayout.Field label="住所" fullWidth><Input placeholder="千代田区丸の内1-1-1" /></FormLayout.Field>
              <FormLayout.Spacer />
              <FormLayout.Field label="建物名"><Input placeholder="XXビル 10F" /></FormLayout.Field>
            </FormLayout.Section>
            <FormLayout.Field label="項目4" fullWidth>
              <div className="h-48 bg-rose-100 p-2">
                <Input />
                赤背景部分は項目4のコンテンツ
              </div>
            </FormLayout.Field>
          </FormLayout.Section>
        </FormLayout.Root>
      </DemoBox>

      <DemoBox title="Responsive: サイズ境界とvalueWidth影響" selfStretch>
        <div style={{ display: 'flex', gap: '16px' }}>
          <div style={{ width: '300px' }}>
            <h4 style={{ margin: '0 0 8px', fontSize: '14px', fontWeight: 'bold' }}>狭幅(300px)</h4>
            <FormLayout.Root>
              <FormLayout.Section label="狭幅テスト" responsive border>
                <FormLayout.Field label="A"><Input /></FormLayout.Field>
                <FormLayout.Field label="B"><Input /></FormLayout.Field>
                <FormLayout.Field label="C"><Input /></FormLayout.Field>
              </FormLayout.Section>
            </FormLayout.Root>
          </div>
          <div style={{ flex: 1 }}>
            <h4 style={{ margin: '0 0 8px', fontSize: '14px', fontWeight: 'bold' }}>valueWidth=480px</h4>
            <FormLayout.Root valueWidthPx={480}>
              <FormLayout.Section label="大きいvalueWidth" responsive border>
                <FormLayout.Field label="X"><Input /></FormLayout.Field>
                <FormLayout.Field label="Y"><Input /></FormLayout.Field>
                <FormLayout.Field label="Z"><Input /></FormLayout.Field>
              </FormLayout.Section>
            </FormLayout.Root>
          </div>
        </div>
      </DemoBox>

      {/* === ネスト・複雑な構造 === */}

      <DemoBox title="Group ネスト: 3段構造" selfStretch>
        <FormLayout.Root>
          <FormLayout.Section label="レベル1" border>
            <FormLayout.Field label="L1項目"><Input /></FormLayout.Field>
            <FormLayout.Section label="レベル2" border>
              <FormLayout.Field label="L2項目A"><Input /></FormLayout.Field>
              <FormLayout.Section label="レベル3" border>
                <FormLayout.Section responsive>
                  <FormLayout.Section>
                    <FormLayout.Field label="L3項目1"><Input /></FormLayout.Field>
                    <FormLayout.Field label="L3項目2"><Input /></FormLayout.Field>
                  </FormLayout.Section>
                  <FormLayout.Section>
                    <FormLayout.Field label="L3項目3"><Input /></FormLayout.Field>
                    <FormLayout.Field label="L3項目4"><Input /></FormLayout.Field>
                    <FormLayout.Field label="L3項目5"><Input /></FormLayout.Field>
                  </FormLayout.Section>
                </FormLayout.Section>
                <FormLayout.Field label="L3項目6"><Textarea /></FormLayout.Field>
                <FormLayout.Section responsive>
                  <FormLayout.Section>
                    <FormLayout.Field label="L3項目7"><Input /></FormLayout.Field>
                    <FormLayout.Field label="L3項目8"><Input /></FormLayout.Field>
                  </FormLayout.Section>
                  <FormLayout.Section>
                    <FormLayout.Field label="L3項目9"><Input /></FormLayout.Field>
                    <FormLayout.Field label="L3項目10"><Input /></FormLayout.Field>
                    <FormLayout.Field label="L3項目11"><Input /></FormLayout.Field>
                  </FormLayout.Section>
                </FormLayout.Section>
              </FormLayout.Section>
              <FormLayout.Field label="L2項目B"><Input /></FormLayout.Field>
            </FormLayout.Section>
            <FormLayout.Field label="L1項目2" fullWidth><Input /></FormLayout.Field>
          </FormLayout.Section>
        </FormLayout.Root>
      </DemoBox>

      <DemoBox title="Responsive ネスト構造" selfStretch>
        <FormLayout.Root labelWidthPx={56}>
          <FormLayout.Section label="外側responsive" responsive={3}>
            <FormLayout.Section>
              <FormLayout.Field label="A1"><Input /></FormLayout.Field>
              <FormLayout.Field label="A2"><Input /></FormLayout.Field>
              <FormLayout.Field label="A3"><Input /></FormLayout.Field>
            </FormLayout.Section>
            <FormLayout.Section label="内側responsive" responsive border>
              <FormLayout.Section>
                <FormLayout.Field label="Left1"><Input /></FormLayout.Field>
                <FormLayout.Field label="Left2"><Input /></FormLayout.Field>
                <FormLayout.Field label="Left3"><Input /></FormLayout.Field>
              </FormLayout.Section>
              <FormLayout.Section>
                <FormLayout.Field label="Right1"><Input /></FormLayout.Field>
                <FormLayout.Field label="Right2"><Input /></FormLayout.Field>
                <FormLayout.Field label="Right3"><Input /></FormLayout.Field>
                <FormLayout.Field label="Right4"><Input /></FormLayout.Field>
              </FormLayout.Section>
            </FormLayout.Section>
          </FormLayout.Section>
        </FormLayout.Root>
      </DemoBox>

      <DemoBox title="Root > Group > Responsive 構造" selfStretch>
        <FormLayout.Root labelWidthPx={80}>
          <FormLayout.Section label="通常Group内にResponsive" border>
            <FormLayout.Field label="A1"><Input /></FormLayout.Field>
            <FormLayout.Field label="A2"><Input /></FormLayout.Field>
            <FormLayout.Section responsive>
              <FormLayout.Section>
                <FormLayout.Field label="B1"><Input /></FormLayout.Field>
                <FormLayout.Field label="B2"><Input /></FormLayout.Field>
                <FormLayout.Field label="B3"><Input /></FormLayout.Field>
              </FormLayout.Section>
              <FormLayout.Section>
                <FormLayout.Field label="B4"><Textarea /></FormLayout.Field>
                <FormLayout.Field label="B5"><Input /></FormLayout.Field>
              </FormLayout.Section>
            </FormLayout.Section>
            <FormLayout.Field label="C1"><Input /></FormLayout.Field>
          </FormLayout.Section>
        </FormLayout.Root>
      </DemoBox>

      {/* === 設定・カスタマイズ === */}

      <DemoBox title="ラベル幅・値幅の調整" width={1000}>
        <FormLayout.Root labelWidthPx={180} valueWidthPx={300}>
          <FormLayout.Section label="サイズ調整テスト" border>
            <FormLayout.Field label="長いラベルのサンプルABCDEF"><Input /></FormLayout.Field>
            <FormLayout.Field label="短い"><Input /></FormLayout.Field>
            <FormLayout.Field label="さらに長いラベルのサンプルGHIJKLMNOP"><Input /></FormLayout.Field>
          </FormLayout.Section>
        </FormLayout.Root>
      </DemoBox>

      <DemoBox title="カスタムラベルコンポーネント" selfStretch>
        <div className="flex flex-col gap-4">
          <div style={{ flex: 1 }}>
            <h4 style={{ margin: '0 0 8px', fontSize: '14px', fontWeight: 'bold' }}>標準ラベル</h4>
            <FormLayout.Root>
              <FormLayout.Field label="プロジェクト"><Input /></FormLayout.Field>
              <FormLayout.Field label="状態" labelEnd={<Badge>進行中</Badge>}><Input /></FormLayout.Field>
            </FormLayout.Root>
          </div>
          <div style={{ flex: 1 }}>
            <h4 style={{ margin: '0 0 8px', fontSize: '14px', fontWeight: 'bold' }}>カスタムラベル</h4>
            <FormLayout.Root labelComponent={CustomLabel}>
              <FormLayout.Field label="プロジェクト"><Input /></FormLayout.Field>
              <FormLayout.Field label="状態" labelEnd={<Badge>進行中</Badge>}><Input /></FormLayout.Field>
            </FormLayout.Root>
          </div>
        </div>
      </DemoBox>

      {/* === 実用的なフォームパターン === */}

      <DemoBox title="実用例: 複合業務フォーム" selfStretch>
        <FormLayout.Root>
          <FormLayout.Section label="基本情報" border>
            <FormLayout.Field label="氏名" labelEnd={<Badge>必須</Badge>}><Input /></FormLayout.Field>
            <FormLayout.Field label="メールアドレス" labelEnd={<Badge>必須</Badge>}><Input /></FormLayout.Field>
          </FormLayout.Section>

          <FormLayout.Section label="連絡先" responsive border>
            <FormLayout.Field label="電話番号"><Input /></FormLayout.Field>
            <FormLayout.Field label="携帯電話"><Input /></FormLayout.Field>
            <FormLayout.Field label="FAX"><Input /></FormLayout.Field>
          </FormLayout.Section>

          <FormLayout.Section label="住所" border>
            <FormLayout.Section responsive>
              <FormLayout.Field label="郵便番号" labelEnd={<Badge>必須</Badge>}><Input /></FormLayout.Field>
              <FormLayout.Field label="都道府県"><Select><option>東京都</option></Select></FormLayout.Field>
            </FormLayout.Section>
            <FormLayout.Field label="住所" fullWidth><Input /></FormLayout.Field>
            <FormLayout.Field label="建物名・部屋番号" fullWidth><Input /></FormLayout.Field>
          </FormLayout.Section>

          <FormLayout.Separator />

          <FormLayout.Section label="その他" border>
            <FormLayout.Field label="備考" fullWidth><Textarea rows={3} /></FormLayout.Field>
            <FormLayout.Field label="利用規約" fullWidth>
              <label style={{ display: 'flex', gap: '8px', alignItems: 'center' }}>
                <input type="checkbox" />
                <span>利用規約およびプライバシーポリシーに同意します</span>
              </label>
            </FormLayout.Field>
          </FormLayout.Section>
        </FormLayout.Root>
      </DemoBox>

      {/* === エッジケースとエラーパターン === */}

      <DemoBox title="エッジケース: ラベル・内容バリエーション">
        <FormLayout.Root labelWidthPx={280}>
          <FormLayout.Field label="">
            <Input placeholder="空ラベル" />
          </FormLayout.Field>
          <FormLayout.Spacer />
          <FormLayout.Field label="とても長いラベルのサンプルABCDEFGHIJKLMNOPQRSTUVWXYZ">
            <Input placeholder="長いラベル" />
          </FormLayout.Field>
          <FormLayout.Spacer />
          <FormLayout.Field labelEnd={<Badge>labelEndのみ</Badge>}>
            <Input placeholder="label無し、labelEndのみ" />
          </FormLayout.Field>
          <FormLayout.Spacer />
          <FormLayout.Field label="複雑なlabelEnd"
            labelEnd={<>
              <Badge>重要</Badge>
              <Badge>新機能</Badge>
              <span style={{ color: '#777', fontSize: '0.8em' }}>
                詳細な説明がここに入ります
              </span>
            </>}
          >
            <Input />
          </FormLayout.Field>
        </FormLayout.Root>
      </DemoBox>

      <DemoBox title="Item内にGroupの特殊ケース（本来はGroupで囲むべきなのであまりにも壊滅的でなければよしとする）" selfStretch>
        <FormLayout.Root>
          <FormLayout.Field label="A1" fullWidth>
            <FormLayout.Section border>
              <FormLayout.Field label="B1" fullWidth><Input /></FormLayout.Field>
              <FormLayout.Field label="B2" fullWidth><Input /></FormLayout.Field>
            </FormLayout.Section>
          </FormLayout.Field>
          <FormLayout.Field label="A2"><Input /></FormLayout.Field>
        </FormLayout.Root>
      </DemoBox>

      <DemoBox title="Item内にItemの特殊ケース（本来はGroupで囲むべきなのであまりにも壊滅的でなければよしとする）" selfStretch>
        <FormLayout.Root labelWidthPx={60}>
          <FormLayout.Field label="A1">
            <FormLayout.Field label="B1"><Input /></FormLayout.Field>
            <FormLayout.Field label="B2"><Input /></FormLayout.Field>
            <FormLayout.Field label="B3" fullWidth><Input /></FormLayout.Field>
            <FormLayout.Field label="B4"><Input /></FormLayout.Field>
          </FormLayout.Field>
        </FormLayout.Root>
      </DemoBox>

      {/* === レスポンシブ確認用 === */}

      <DemoBox title="レスポンシブ自由伸縮テスト (ブラウザ幅で確認)" selfStretch>
        <FormLayout.Root>
          <FormLayout.Section label="伸縮確認" responsive border>
            <FormLayout.Field label="TEL"><Input /></FormLayout.Field>
            <FormLayout.Field label="FAX"><Input /></FormLayout.Field>
            <FormLayout.Field label="携帯"><Input /></FormLayout.Field>
            <FormLayout.Field label="IP電話"><Input /></FormLayout.Field>
          </FormLayout.Section>
        </FormLayout.Root>
      </DemoBox>

      {/* === labelEnd 大量要素テスト（全LabelRendererケース網羅） === */}

      <LabelEndTest />

    </div>
  )
}

// -----------------------------
// サンプルコンポーネント

const Badge = ({ children }: { children: React.ReactNode }) => (
  <span style={{
    fontSize: '0.75rem',
    color: '#0b5',
    border: '1px solid #0b5',
    borderRadius: '4px',
    padding: '0 4px',
    userSelect: 'none',
    whiteSpace: 'nowrap',
  }}>{children}</span>
)

const CustomLabel = ({ children, className }: LabelProps) => (
  <span className={className} style={{
    color: '#f34af3',
    fontWeight: 'bold',
    textDecoration: 'underline',
    padding: '0 6px',
  }}>{children}</span>
)

const DemoBox = ({ title, width, selfStretch, children }: {
  title: string
  width?: number
  selfStretch?: boolean
  children: React.ReactNode
}) => (
  <div className="flex flex-col gap-2 border border-gray-300 rounded-md p-3 bg-gray-50" style={{
    alignSelf: selfStretch ? 'stretch' : undefined,
  }}>
    <div className="flex items-center justify-between">
      <span className="text-sm font-bold text-gray-600">{title}</span>
      {width && <span className="text-xs text-gray-500">width: {width}px</span>}
      {selfStretch && <span className="text-xs text-gray-500">self-stretch</span>}
    </div>
    <div style={{ width: width ? `${width}px` : undefined }}>
      {children}
    </div>
  </div>
)

const Input = (props: React.InputHTMLAttributes<HTMLInputElement>) => (
  <input {...props} style={{
    ...props.style,
    width: '100%',
    padding: '1px 8px',
    border: '1px solid #ccc',
    borderRadius: '4px',
    background: '#fff',
  }} />
)

const Textarea = (props: React.TextareaHTMLAttributes<HTMLTextAreaElement>) => (
  <textarea {...props} style={{
    ...props.style,
    width: '100%',
    padding: '2px 8px',
    border: '1px solid #ccc',
    borderRadius: '4px',
    background: '#fff',
  }} />
)

const Select = (props: React.SelectHTMLAttributes<HTMLSelectElement>) => (
  <select {...props} style={{
    ...props.style,
    width: '100%',
    padding: '1px 8px',
    border: '1px solid #ccc',
    borderRadius: '4px',
    background: '#fff',
  }} />
)

// -----------------------------
// labelEnd 大量要素テスト

const LabelEndTest = () => {

  const [type, setType] = React.useState<'少なめ' | '中ぐらい' | '多め'>('少なめ')

  return (<>
    <DemoBox title="labelEnd に大量の要素が配置されたときにレイアウトが問題ないか" selfStretch>
      <FormLayout.Root>
        <FormLayout.Field fullWidth>
          <div className="flex gap-4 items-center">
            <label className="flex gap-1 items-center">
              <input type="radio" name="type" value="少なめ" checked={type === '少なめ'} onChange={() => setType('少なめ')} />
              少なめ
            </label>
            <label className="flex gap-1 items-center">
              <input type="radio" name="type" value="中ぐらい" checked={type === '中ぐらい'} onChange={() => setType('中ぐらい')} />
              中ぐらい
            </label>
            <label className="flex gap-1 items-center">
              <input type="radio" name="type" value="多め" checked={type === '多め'} onChange={() => setType('多め')} />
              多め
            </label>
          </div>
        </FormLayout.Field>

        <FormLayout.Separator />

        <FormLayout.Section label="responsive container" responsive>
          <FormLayout.Field
            label="ケース1: Field の responsive-container 内"
            labelEnd={<LargeLabelEndButtons type={type} />}
          >
            <Input placeholder="responsive-container内のField" />
          </FormLayout.Field>
        </FormLayout.Section>

        <FormLayout.Section label="2-cols-grid" border>
          <FormLayout.Field
            label="ケース2: Field の 2-cols-grid 内"
            labelEnd={<LargeLabelEndButtons type={type} />}
          >
            <Input placeholder="2-cols-grid内のField" />
          </FormLayout.Field>

          <FormLayout.Field
            label="ケース3: Field の fullWidth"
            labelEnd={<LargeLabelEndButtons type={type} />}
            fullWidth
          >
            <Input placeholder="fullWidthのField" />
          </FormLayout.Field>
        </FormLayout.Section>

        <FormLayout.Field label="ケース4: Field の通常ケース（Item内Item）">
          <FormLayout.Field
            label="大量labelEnd"
            labelEnd={<LargeLabelEndButtons type={type} />}
          >
            <Input placeholder="Field通常ケース" />
          </FormLayout.Field>
        </FormLayout.Field>
      </FormLayout.Root>
    </DemoBox>

    <DemoBox title="labelEnd 大量要素テスト: Section responsive + 2-cols-grid" selfStretch>
      <FormLayout.Root>
        <FormLayout.Section
          label="ケース5: Section の responsive"
          labelEnd={<LargeLabelEndButtons type={type} />}
          responsive
          border
        >
          <FormLayout.Field label="項目1"><Input /></FormLayout.Field>
          <FormLayout.Field label="項目2"><Input /></FormLayout.Field>
          <FormLayout.Field label="項目3"><Input /></FormLayout.Field>
        </FormLayout.Section>

        <FormLayout.Section
          label="ケース6: Section の通常（2-cols-grid）"
          labelEnd={<LargeLabelEndButtons type={type} />}
          border
        >
          <FormLayout.Field label="項目A"><Input /></FormLayout.Field>
          <FormLayout.Field label="項目B"><Input /></FormLayout.Field>
          <FormLayout.Field label="項目C"><Input /></FormLayout.Field>
        </FormLayout.Section>
      </FormLayout.Root>
    </DemoBox>
  </>)
}

/** labelEnd に配置するボタン群 */
const LargeLabelEndButtons = ({ type }: {
  type: '少なめ' | '中ぐらい' | '多め'
}) => {
  if (type === '少なめ') return (
    <>
      <div className="flex-1"></div>
      <button type="button" className="px-1 border border-blue-500 text-blue-500">編集</button>
      <button type="button" className="px-1 border border-blue-500 text-blue-500">削除</button>
    </>
  )

  if (type === '中ぐらい') return (
    <>
      <div className="flex-1"></div>
      <button type="button" className="px-1 border border-blue-500 text-blue-500">編集</button>
      <button type="button" className="px-1 border border-blue-500 text-blue-500">複写</button>
      <button type="button" className="px-1 border border-orange-500 text-orange-500">印刷</button>
      <button type="button" className="px-1 border border-orange-500 text-orange-500">メール</button>
      <div className="basis-4"></div>
      <button type="button" className="px-1 border border-blue-500 text-blue-500">削除</button>
      <button type="button" className="px-1 border border-green-500 text-green-500">保存</button>
    </>
  )

  return (
    <>
      <div className="flex-1"></div>
      <button type="button" className="px-1 border border-blue-500 text-blue-500">新規</button>
      <button type="button" className="px-1 border border-blue-500 text-blue-500">編集</button>
      <button type="button" className="px-1 border border-blue-500 text-blue-500">複写</button>
      <button type="button" className="px-1 border border-blue-500 text-blue-500">移動</button>
      <button type="button" className="px-1 border border-red-500 text-red-500">削除</button>
      <div className="basis-6"></div>
      <button type="button" className="px-1 border border-green-500 text-green-500">保存</button>
      <button type="button" className="px-1 border border-green-500 text-green-500">確定</button>
      <div className="basis-6"></div>
      <button type="button" className="px-1 border border-orange-500 text-orange-500">印刷</button>
      <button type="button" className="px-1 border border-orange-500 text-orange-500">プレビュー</button>
      <button type="button" className="px-1 border border-orange-500 text-orange-500">メール</button>
      <button type="button" className="px-1 border border-orange-500 text-orange-500">エクスポート</button>
      <div className="basis-4"></div>
      <button type="button" className="px-1 border border-gray-500 text-gray-500">キャンセル</button>
      <button type="button" className="px-1 border border-gray-500 text-gray-500">閉じる</button>
    </>
  )

}
