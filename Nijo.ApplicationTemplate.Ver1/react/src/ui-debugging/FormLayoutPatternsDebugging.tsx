import React from "react"
import FormLayout, { type LabelProps } from "../layout/FormLayout"

export default function FormLayoutPatternsDebugging() {

  return (
    <div className="flex flex-col items-start gap-6 p-4">

      {/* === 基本パターン === */}

      <DemoBox title="基本: Item 群とlabelAlign">
        <FormLayout.Root>
          <FormLayout.Item label="氏名">
            <Input placeholder="山田 太郎" />
          </FormLayout.Item>
          <FormLayout.Item label="メール">
            <Input placeholder="taro@example.com" />
          </FormLayout.Item>
          <FormLayout.Item label="都道府県">
            <Select defaultValue="tokyo">
              <option value="tokyo">東京都</option>
              <option value="kanagawa">神奈川県</option>
              <option value="saitama">埼玉県</option>
            </Select>
          </FormLayout.Item>
          <FormLayout.Item label="fullWidth指定" fullWidth>
            <Input placeholder="備考" />
          </FormLayout.Item>
          <FormLayout.Item>
            <div className="text-amber-600 text-sm bg-amber-50 border border-amber-200 rounded-md p-2">
              *** ラベルなしのItem ***
            </div>
          </FormLayout.Item>
        </FormLayout.Root>
      </DemoBox>

      <DemoBox title="labelAlign: 左寄せ vs 右寄せ" selfStretch>
        <div className="flex flex-col gap-4">
          <div className="flex-1">
            <h4 className="mb-2 text-sm font-bold">左寄せ</h4>
            <FormLayout.Root labelAlign="left">
              <FormLayout.Item label="会社名"><Input /></FormLayout.Item>
              <FormLayout.Item label="部署"><Input /></FormLayout.Item>
            </FormLayout.Root>
          </div>
          <div className="flex-1">
            <h4 className="mb-2 text-sm font-bold">右寄せ(デフォルト)</h4>
            <FormLayout.Root labelAlign="right">
              <FormLayout.Item label="会社名"><Input /></FormLayout.Item>
              <FormLayout.Item label="部署"><Input /></FormLayout.Item>
            </FormLayout.Root>
          </div>
        </div>
      </DemoBox>

      {/* === Groupパターン === */}

      <DemoBox title="Group: 基本とborder">
        <FormLayout.Root>
          <FormLayout.Group label="基本情報" border>
            <FormLayout.Item label="氏名"><Input /></FormLayout.Item>
            <FormLayout.Item label="メール" labelEnd={<Badge>必須</Badge>}><Input /></FormLayout.Item>
            <FormLayout.Item label="住所" fullWidth><Input /></FormLayout.Item>
          </FormLayout.Group>

          <FormLayout.Group label="borderなし">
            <FormLayout.Item label="項目A"><Input /></FormLayout.Item>
            <FormLayout.Item label="項目B"><Input /></FormLayout.Item>
          </FormLayout.Group>

          <FormLayout.Group border>
            <FormLayout.Item label="ラベルなしGroup"><Input /></FormLayout.Item>
          </FormLayout.Group>
        </FormLayout.Root>
      </DemoBox>

      <DemoBox title="Separator/Spacer配置パターン">
        <FormLayout.Root labelWidthPx={160}>
          <FormLayout.Group label="区切り要素テスト" border>
            <FormLayout.Separator />
            <FormLayout.Item label="先頭Separator後"><Input /></FormLayout.Item>
            <FormLayout.Spacer />
            <FormLayout.Item label="Spacer後"><Input /></FormLayout.Item>
            <FormLayout.Separator />
            <FormLayout.Item label="中間項目"><Input /></FormLayout.Item>
            <FormLayout.Separator />
          </FormLayout.Group>

          <FormLayout.Item label="Root直下項目"><Input /></FormLayout.Item>
          <FormLayout.Spacer />
          <FormLayout.Item label="Root直下Spacer後"><Input /></FormLayout.Item>
        </FormLayout.Root>
      </DemoBox>

      {/* === Responsiveパターン === */}

      <DemoBox title="Responsive: 基本動作" selfStretch>
        <FormLayout.Root>
          <FormLayout.Group label="基本responsive" responsive border>
            <FormLayout.Group>
              <FormLayout.Item label="TEL"><Input placeholder="03-1234-5678" /></FormLayout.Item>
              <FormLayout.Item label="FAX"><Input placeholder="03-9876-5432" /></FormLayout.Item>
            </FormLayout.Group>
            <FormLayout.Group>
              <FormLayout.Item label="携帯"><Input placeholder="090-0000-0000" /></FormLayout.Item>
            </FormLayout.Group>
          </FormLayout.Group>

          <FormLayout.Group responsive border>
            <FormLayout.Group>
              <FormLayout.Item label="ラベルなし1"><Input /></FormLayout.Item>
              <FormLayout.Item label="ラベルなし2"><Input /></FormLayout.Item>
            </FormLayout.Group>
            <FormLayout.Group>
              <FormLayout.Item label="ラベルなし3"><Input /></FormLayout.Item>
            </FormLayout.Group>
          </FormLayout.Group>
        </FormLayout.Root>
      </DemoBox>

      <DemoBox title="Responsive: 数値指定とfullWidth混在" selfStretch>
        <FormLayout.Root>
          <FormLayout.Group label="responsive={3} (3列以上で横並び)" responsive={3} border>
            <FormLayout.Item label="項目1"><Input /></FormLayout.Item>
            <FormLayout.Item label="項目2"><Input /></FormLayout.Item>
            <FormLayout.Item label="項目3"><Input /></FormLayout.Item>
          </FormLayout.Group>

          <FormLayout.Group label="fullWidth混在" responsive border>
            <FormLayout.Group>
              <FormLayout.Item label="郵便番号"><Input placeholder="100-0000" /></FormLayout.Item>
              <FormLayout.Item label="都道府県"><Input placeholder="東京都" /></FormLayout.Item>
              <FormLayout.Spacer />
              <FormLayout.Item label="住所" fullWidth><Input placeholder="千代田区丸の内1-1-1" /></FormLayout.Item>
              <FormLayout.Spacer />
              <FormLayout.Item label="建物名"><Input placeholder="XXビル 10F" /></FormLayout.Item>
            </FormLayout.Group>
            <FormLayout.Item label="項目4" fullWidth>
              <div className="h-48 bg-rose-100 p-2">
                <Input />
                赤背景部分は項目4のコンテンツ
              </div>
            </FormLayout.Item>
          </FormLayout.Group>
        </FormLayout.Root>
      </DemoBox>

      <DemoBox title="Responsive: サイズ境界とvalueWidth影響" selfStretch>
        <div style={{ display: 'flex', gap: '16px' }}>
          <div style={{ width: '300px' }}>
            <h4 style={{ margin: '0 0 8px', fontSize: '14px', fontWeight: 'bold' }}>狭幅(300px)</h4>
            <FormLayout.Root>
              <FormLayout.Group label="狭幅テスト" responsive border>
                <FormLayout.Item label="A"><Input /></FormLayout.Item>
                <FormLayout.Item label="B"><Input /></FormLayout.Item>
                <FormLayout.Item label="C"><Input /></FormLayout.Item>
              </FormLayout.Group>
            </FormLayout.Root>
          </div>
          <div style={{ flex: 1 }}>
            <h4 style={{ margin: '0 0 8px', fontSize: '14px', fontWeight: 'bold' }}>valueWidth=480px</h4>
            <FormLayout.Root valueWidthPx={480}>
              <FormLayout.Group label="大きいvalueWidth" responsive border>
                <FormLayout.Item label="X"><Input /></FormLayout.Item>
                <FormLayout.Item label="Y"><Input /></FormLayout.Item>
                <FormLayout.Item label="Z"><Input /></FormLayout.Item>
              </FormLayout.Group>
            </FormLayout.Root>
          </div>
        </div>
      </DemoBox>

      {/* === ネスト・複雑な構造 === */}

      <DemoBox title="Group ネスト: 3段構造" selfStretch>
        <FormLayout.Root>
          <FormLayout.Group label="レベル1" border>
            <FormLayout.Item label="L1項目"><Input /></FormLayout.Item>
            <FormLayout.Group label="レベル2" border>
              <FormLayout.Item label="L2項目A"><Input /></FormLayout.Item>
              <FormLayout.Group label="レベル3" border>
                <FormLayout.Group responsive>
                  <FormLayout.Group>
                    <FormLayout.Item label="L3項目1"><Input /></FormLayout.Item>
                    <FormLayout.Item label="L3項目2"><Input /></FormLayout.Item>
                  </FormLayout.Group>
                  <FormLayout.Group>
                    <FormLayout.Item label="L3項目3"><Input /></FormLayout.Item>
                    <FormLayout.Item label="L3項目4"><Input /></FormLayout.Item>
                    <FormLayout.Item label="L3項目5"><Input /></FormLayout.Item>
                  </FormLayout.Group>
                </FormLayout.Group>
                <FormLayout.Item label="L3項目6"><Textarea /></FormLayout.Item>
                <FormLayout.Group responsive>
                  <FormLayout.Group>
                    <FormLayout.Item label="L3項目7"><Input /></FormLayout.Item>
                    <FormLayout.Item label="L3項目8"><Input /></FormLayout.Item>
                  </FormLayout.Group>
                  <FormLayout.Group>
                    <FormLayout.Item label="L3項目9"><Input /></FormLayout.Item>
                    <FormLayout.Item label="L3項目10"><Input /></FormLayout.Item>
                    <FormLayout.Item label="L3項目11"><Input /></FormLayout.Item>
                  </FormLayout.Group>
                </FormLayout.Group>
              </FormLayout.Group>
              <FormLayout.Item label="L2項目B"><Input /></FormLayout.Item>
            </FormLayout.Group>
            <FormLayout.Item label="L1項目2" fullWidth><Input /></FormLayout.Item>
          </FormLayout.Group>
        </FormLayout.Root>
      </DemoBox>

      <DemoBox title="Responsive ネスト構造" selfStretch>
        <FormLayout.Root labelWidthPx={56}>
          <FormLayout.Group label="外側responsive" responsive={3}>
            <FormLayout.Group>
              <FormLayout.Item label="A1"><Input /></FormLayout.Item>
              <FormLayout.Item label="A2"><Input /></FormLayout.Item>
              <FormLayout.Item label="A3"><Input /></FormLayout.Item>
            </FormLayout.Group>
            <FormLayout.Group label="内側responsive" responsive border>
              <FormLayout.Group>
                <FormLayout.Item label="Left1"><Input /></FormLayout.Item>
                <FormLayout.Item label="Left2"><Input /></FormLayout.Item>
                <FormLayout.Item label="Left3"><Input /></FormLayout.Item>
              </FormLayout.Group>
              <FormLayout.Group>
                <FormLayout.Item label="Right1"><Input /></FormLayout.Item>
                <FormLayout.Item label="Right2"><Input /></FormLayout.Item>
                <FormLayout.Item label="Right3"><Input /></FormLayout.Item>
                <FormLayout.Item label="Right4"><Input /></FormLayout.Item>
              </FormLayout.Group>
            </FormLayout.Group>
          </FormLayout.Group>
        </FormLayout.Root>
      </DemoBox>

      <DemoBox title="Root > Group > Responsive 構造" selfStretch>
        <FormLayout.Root labelWidthPx={80}>
          <FormLayout.Group label="通常Group内にResponsive" border>
            <FormLayout.Item label="A1"><Input /></FormLayout.Item>
            <FormLayout.Item label="A2"><Input /></FormLayout.Item>
            <FormLayout.Group responsive>
              <FormLayout.Group>
                <FormLayout.Item label="B1"><Input /></FormLayout.Item>
                <FormLayout.Item label="B2"><Input /></FormLayout.Item>
                <FormLayout.Item label="B3"><Input /></FormLayout.Item>
              </FormLayout.Group>
              <FormLayout.Group>
                <FormLayout.Item label="B4"><Textarea /></FormLayout.Item>
                <FormLayout.Item label="B5"><Input /></FormLayout.Item>
              </FormLayout.Group>
            </FormLayout.Group>
            <FormLayout.Item label="C1"><Input /></FormLayout.Item>
          </FormLayout.Group>
        </FormLayout.Root>
      </DemoBox>

      {/* === 設定・カスタマイズ === */}

      <DemoBox title="ラベル幅・値幅の調整" width={1000}>
        <FormLayout.Root labelWidthPx={180} valueWidthPx={300}>
          <FormLayout.Group label="サイズ調整テスト" border>
            <FormLayout.Item label="長いラベルのサンプルABCDEF"><Input /></FormLayout.Item>
            <FormLayout.Item label="短い"><Input /></FormLayout.Item>
            <FormLayout.Item label="さらに長いラベルのサンプルGHIJKLMNOP"><Input /></FormLayout.Item>
          </FormLayout.Group>
        </FormLayout.Root>
      </DemoBox>

      <DemoBox title="カスタムラベルコンポーネント" selfStretch>
        <div className="flex flex-col gap-4">
          <div style={{ flex: 1 }}>
            <h4 style={{ margin: '0 0 8px', fontSize: '14px', fontWeight: 'bold' }}>標準ラベル</h4>
            <FormLayout.Root>
              <FormLayout.Item label="プロジェクト"><Input /></FormLayout.Item>
              <FormLayout.Item label="状態" labelEnd={<Badge>進行中</Badge>}><Input /></FormLayout.Item>
            </FormLayout.Root>
          </div>
          <div style={{ flex: 1 }}>
            <h4 style={{ margin: '0 0 8px', fontSize: '14px', fontWeight: 'bold' }}>カスタムラベル</h4>
            <FormLayout.Root labelComponent={CustomLabel}>
              <FormLayout.Item label="プロジェクト"><Input /></FormLayout.Item>
              <FormLayout.Item label="状態" labelEnd={<Badge>進行中</Badge>}><Input /></FormLayout.Item>
            </FormLayout.Root>
          </div>
        </div>
      </DemoBox>

      {/* === 実用的なフォームパターン === */}

      <DemoBox title="実用例: 複合業務フォーム" selfStretch>
        <FormLayout.Root>
          <FormLayout.Group label="基本情報" border>
            <FormLayout.Item label="氏名" labelEnd={<Badge>必須</Badge>}><Input /></FormLayout.Item>
            <FormLayout.Item label="メールアドレス" labelEnd={<Badge>必須</Badge>}><Input /></FormLayout.Item>
          </FormLayout.Group>

          <FormLayout.Group label="連絡先" responsive border>
            <FormLayout.Item label="電話番号"><Input /></FormLayout.Item>
            <FormLayout.Item label="携帯電話"><Input /></FormLayout.Item>
            <FormLayout.Item label="FAX"><Input /></FormLayout.Item>
          </FormLayout.Group>

          <FormLayout.Group label="住所" border>
            <FormLayout.Group responsive>
              <FormLayout.Item label="郵便番号" labelEnd={<Badge>必須</Badge>}><Input /></FormLayout.Item>
              <FormLayout.Item label="都道府県"><Select><option>東京都</option></Select></FormLayout.Item>
            </FormLayout.Group>
            <FormLayout.Item label="住所" fullWidth><Input /></FormLayout.Item>
            <FormLayout.Item label="建物名・部屋番号" fullWidth><Input /></FormLayout.Item>
          </FormLayout.Group>

          <FormLayout.Separator />

          <FormLayout.Group label="その他" border>
            <FormLayout.Item label="備考" fullWidth><Textarea rows={3} /></FormLayout.Item>
            <FormLayout.Item label="利用規約" fullWidth>
              <label style={{ display: 'flex', gap: '8px', alignItems: 'center' }}>
                <input type="checkbox" />
                <span>利用規約およびプライバシーポリシーに同意します</span>
              </label>
            </FormLayout.Item>
          </FormLayout.Group>
        </FormLayout.Root>
      </DemoBox>

      {/* === エッジケースとエラーパターン === */}

      <DemoBox title="エッジケース: ラベル・内容バリエーション">
        <FormLayout.Root labelWidthPx={280}>
          <FormLayout.Item label="">
            <Input placeholder="空ラベル" />
          </FormLayout.Item>
          <FormLayout.Spacer />
          <FormLayout.Item label="とても長いラベルのサンプルABCDEFGHIJKLMNOPQRSTUVWXYZ">
            <Input placeholder="長いラベル" />
          </FormLayout.Item>
          <FormLayout.Spacer />
          <FormLayout.Item labelEnd={<Badge>labelEndのみ</Badge>}>
            <Input placeholder="label無し、labelEndのみ" />
          </FormLayout.Item>
          <FormLayout.Spacer />
          <FormLayout.Item label="複雑なlabelEnd"
            labelEnd={<>
              <Badge>重要</Badge>
              <Badge>新機能</Badge>
              <span style={{ color: '#777', fontSize: '0.8em' }}>
                詳細な説明がここに入ります
              </span>
            </>}
          >
            <Input />
          </FormLayout.Item>
        </FormLayout.Root>
      </DemoBox>

      <DemoBox title="Item内にGroupの特殊ケース（本来はGroupで囲むべきなのであまりにも壊滅的でなければよしとする）" selfStretch>
        <FormLayout.Root>
          <FormLayout.Item label="A1" fullWidth>
            <FormLayout.Group border>
              <FormLayout.Item label="B1" fullWidth><Input /></FormLayout.Item>
              <FormLayout.Item label="B2" fullWidth><Input /></FormLayout.Item>
            </FormLayout.Group>
          </FormLayout.Item>
          <FormLayout.Item label="A2"><Input /></FormLayout.Item>
        </FormLayout.Root>
      </DemoBox>

      <DemoBox title="Item内にItemの特殊ケース（本来はGroupで囲むべきなのであまりにも壊滅的でなければよしとする）" selfStretch>
        <FormLayout.Root labelWidthPx={60}>
          <FormLayout.Item label="A1">
            <FormLayout.Item label="B1"><Input /></FormLayout.Item>
            <FormLayout.Item label="B2"><Input /></FormLayout.Item>
            <FormLayout.Item label="B3" fullWidth><Input /></FormLayout.Item>
            <FormLayout.Item label="B4"><Input /></FormLayout.Item>
          </FormLayout.Item>
        </FormLayout.Root>
      </DemoBox>

      {/* === レスポンシブ確認用 === */}

      <DemoBox title="レスポンシブ自由伸縮テスト (ブラウザ幅で確認)" selfStretch>
        <FormLayout.Root>
          <FormLayout.Group label="伸縮確認" responsive border>
            <FormLayout.Item label="TEL"><Input /></FormLayout.Item>
            <FormLayout.Item label="FAX"><Input /></FormLayout.Item>
            <FormLayout.Item label="携帯"><Input /></FormLayout.Item>
            <FormLayout.Item label="IP電話"><Input /></FormLayout.Item>
          </FormLayout.Group>
        </FormLayout.Root>
      </DemoBox>

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
    color: '#135',
    background: '#e8f3ff',
    border: '1px solid #9cc4ff',
    borderRadius: '4px',
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
