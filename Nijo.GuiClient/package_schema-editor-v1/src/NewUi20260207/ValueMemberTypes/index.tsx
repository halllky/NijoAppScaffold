import { Allotment, LayoutPriority } from "allotment"
import * as ReactHookForm from "react-hook-form"
import { SchemaDefinitionGlobalState } from "../../types"
import StaticEnumGrid from "./StaticEnumGrid"

/**
 * 属性種類定義画面。
 *
 * Nijo の ValueMemberType に相当する属性種類を定義する。
 */
export default function (props: {
  formMethods?: ReactHookForm.UseFormReturn<SchemaDefinitionGlobalState>
}) {

  return (
    <div className="relative h-full w-full overflow-auto">

      <div className="flex m-auto max-w-4xl px-4">

        {/* コンテンツ */}
        <div className="flex-1 pt-2 pr-4 border-r border-gray-300">

          <p className="mb-4 text-sm">
            データ構造定義で定義される各種構造体の属性に使用できる属性種類を定義します。
          </p>

          <section>
            <h2 id="string-values" className="text-2xl font-bold">文字列系項目</h2>

            <ul className="list-disc list-inside my-2 space-y-1">
              <li>
                <strong className="mr-2">word (単語型):</strong>
                ものの名前など改行を含まない単語を表す。
              </li>
              <li>
                <strong className="mr-2">description (文章型):</strong>
                コメントや備考などフリーフォーマットの文章。
              </li>
            </ul>
          </section>

          <section className="mt-8">
            <h2 id="numeric-values" className="text-2xl font-bold">数値系項目</h2>

            <ul className="list-disc list-inside my-2 space-y-1">
              <li>
                <strong className="mr-2">int (整数型):</strong>
                数量、回数、順序番号などの整数。
              </li>
              <li>
                <strong className="mr-2">decimal (実数型):</strong>
                金額、重量、割合などの小数を含む数値。
              </li>
              <li>
                <strong className="mr-2">sequence (採番型):</strong>
                DB登録時に自動採番される整数。
              </li>
            </ul>
          </section>

          <section className="mt-8">
            <h2 id="date-values" className="text-2xl font-bold">日付系項目</h2>

            <ul className="list-disc list-inside my-2 space-y-1">
              <li>
                <strong className="mr-2">date (日付型):</strong>
                年月日。時刻は含まない。
              </li>
              <li>
                <strong className="mr-2">datetime (日時型):</strong>
                年月日と時刻。
              </li>
              <li>
                <strong className="mr-2">year (年型):</strong>
                西暦年。
              </li>
              <li>
                <strong className="mr-2">year-month (年月型):</strong>
                年月。
              </li>
            </ul>
          </section>

          <section className="mt-8">
            <h2 id="selection-values" className="text-2xl font-bold">選択・区分系項目</h2>

            <ul className="list-disc list-inside my-2 space-y-1">
              <li>
                <strong className="mr-2">bool (真偽値型):</strong>
                はい/いいえ (True/False)。
              </li>
              <li>
                <strong className="mr-2">静的区分:</strong>
              </li>
            </ul>

            {props.formMethods && (
              <StaticEnumGrid formMethods={props.formMethods} />
            )}

          </section>

          <section className="mt-8 pb-96">
            <h2 id="other-values" className="text-2xl font-bold">その他</h2>

            <ul className="list-disc list-inside my-2 space-y-1">
              <li>
                <strong className="mr-2">bytearray (バイナリ型):</strong>
                ハッシュ化されたパスワードなどのバイナリデータ。
              </li>
            </ul>
          </section>
        </div>

        {/* 目次 */}
        <div className="self-start sticky top-0 basis-48 shrink-0 pt-8 px-2">
          <span className="font-bold text-sm select-none">
            属性種類定義
          </span>
          <SideMenuLink hash="string-values">
            文字列系項目
          </SideMenuLink>
          <SideMenuLink hash="numeric-values">
            数値系項目
          </SideMenuLink>
          <SideMenuLink hash="date-values">
            日付系項目
          </SideMenuLink>
          <SideMenuLink hash="selection-values">
            選択・区分系項目
          </SideMenuLink>
          <SideMenuLink hash="other-values">
            その他
          </SideMenuLink>
        </div>
      </div>
    </div>
  )
}

/**
 * 目次のリンク
 */
function SideMenuLink({ hash, children }: {
  hash: string
  children?: React.ReactNode
}) {

  const handleClick = () => {
    const el = document.getElementById(hash)
    if (el) el.scrollIntoView({ block: 'start', behavior: 'smooth' })
  }

  return (
    <button type="button" onClick={handleClick} className="px-2 py-1 w-full text-left hover:bg-gray-200 dark:hover:bg-gray-700 cursor-pointer select-none">
      {children}
    </button>
  )
}
