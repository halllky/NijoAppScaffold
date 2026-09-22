import React from "react"
import { ChevronDownIcon } from "@heroicons/react/24/outline"

/**
 * レイアウトを施しアイコンをつけられるようにしたボタン。
 * サイドボタンのドロップダウンの中身の操作は呼び出し側で実装する。
 */
export function Button(props: {
  /** クリック時処理 */
  onClick?: React.MouseEventHandler
  /** type="submit" にするかどうか。既定は type="button" */
  submit?: boolean
  /** このボタンをクリックしたときに送信するformのid */
  form?: string
  /** 背景塗りつぶし */
  fill?: boolean
  /** 枠をつける。未指定の場合の枠は transparent */
  border?: boolean
  /** heroicons のアイコンを指定 */
  Icon?: React.ElementType
  /** 指定すると children が title になる。アイコンだけのボタンにしたい場合に使う */
  hideText?: boolean
  /** ボタンを押せなくする */
  disabled?: boolean
  /** ボタンを押せなくし、さらに読み込み中であることを示すインジケーターを表示する */
  loading?: boolean
  /** 通常はブロック表示、これを指定するとインライン表示 */
  inline?: boolean
  /** ボタンテキスト */
  children?: React.ReactNode
  /**
   * 配列を指定するとサイドボタンが出現する。
   * 配列の各要素はドロップダウンで表示される。
   * 配列の各要素の内部操作は呼び出し側で定義する。
   */
  sideButton?: React.ReactNode[]
  /** 細かいレイアウトの微調整に使用 */
  className?: string
}) {
  const {
    onClick, submit, form, fill, border, Icon, hideText,
    disabled, loading, inline, children, sideButton, className,
  } = props

  const [dropdownOpen, setDropdownOpen] = React.useState(false)
  const wrapperRef = React.useRef<HTMLDivElement>(null)

  // ドロップダウンが開いている間だけ、外側のクリックと Esc キーで閉じられるようにする。
  // (React の外にある document のイベントとの同期)
  React.useEffect(() => {
    if (!dropdownOpen) return
    const handleMouseDown = (e: MouseEvent) => {
      if (!wrapperRef.current?.contains(e.target as Node)) setDropdownOpen(false)
    }
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') setDropdownOpen(false)
    }
    document.addEventListener('mousedown', handleMouseDown)
    document.addEventListener('keydown', handleKeyDown)
    return () => {
      document.removeEventListener('mousedown', handleMouseDown)
      document.removeEventListener('keydown', handleKeyDown)
    }
  }, [dropdownOpen])

  const isDisabled = disabled || loading
  const title = hideText && typeof children === 'string' ? children : undefined

  // 本体ボタンとサイドボタンで共通のクラス
  const buttonClassName = [
    'flex items-center justify-center gap-1 px-1 py-px text-sm whitespace-nowrap select-none border cursor-pointer',
    'disabled:opacity-50 disabled:cursor-not-allowed',
    fill
      ? `text-white bg-sky-600 enabled:hover:bg-sky-700 ${border ? 'border-sky-800' : 'border-transparent'}`
      : `text-sky-700 bg-transparent enabled:hover:bg-sky-100 ${border ? 'border-sky-600' : 'border-transparent'}`,
  ].join(' ')

  return (
    <div
      ref={wrapperRef}
      className={`relative ${inline ? 'inline-flex' : 'flex'} ${className ?? ''}`}
    >
      {/* 本体ボタン */}
      <button
        type={submit ? 'submit' : 'button'}
        form={form}
        onClick={onClick}
        disabled={isDisabled}
        title={title}
        aria-label={title}
        className={`${buttonClassName} flex-1 min-w-0 ${sideButton ? 'rounded-l rounded-r-none' : 'rounded'}`}
      >
        {loading ? (
          <span className="animate-spin h-4 w-4 shrink-0 border-2 border-current rounded-full border-t-transparent" aria-label="読み込み中" />
        ) : Icon && (
          <Icon className="h-4 w-4 shrink-0" />
        )}
        {!hideText && children}
      </button>

      {/* サイドボタン */}
      {sideButton && (
        <button
          type="button"
          onClick={() => setDropdownOpen(open => !open)}
          disabled={isDisabled}
          aria-label="その他の操作"
          aria-haspopup="true"
          aria-expanded={dropdownOpen}
          // 塗りつぶし＋枠なしのときは、本体との境目が分かるよう1px空ける。それ以外は本体側の枠と重ねない。
          className={`${buttonClassName} rounded-r rounded-l-none ${fill && !border ? 'ml-px' : 'border-l-0'}`}
        >
          <ChevronDownIcon className="h-4 w-4" />
        </button>
      )}

      {/* ドロップダウン */}
      {sideButton && dropdownOpen && (
        // 周囲のUIより手前に表示させるために z-index を指定している。
        // 中の要素がクリックされたら、その操作は呼び出し側に任せてメニューを閉じる。
        <div
          className="absolute right-0 top-full z-50 mt-1 min-w-full w-max flex flex-col py-px bg-white border border-gray-300 rounded shadow-md"
          onClick={() => setDropdownOpen(false)}
        >
          {sideButton.map((item, i) => (
            <React.Fragment key={i}>
              {i > 0 && (
                <hr className="border-t border-gray-300" />
              )}
              {item}
            </React.Fragment>
          ))}
        </div>
      )}
    </div>
  )
}
