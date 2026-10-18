import React from "react"
import { createPortal } from "react-dom"

/** アンカーと吹き出しの間隔（px） */
const ANCHOR_GAP = 4
/** 吹き出しと画面端の最小距離（px） */
const VIEWPORT_MARGIN = 8

/**
 * children にマウスを乗せるかフォーカスすると、content を吹き出しで表示する。
 *
 * HTML の title 属性と違い、吹き出しはマウスカーソルの位置ではなく children の上側（入りきらなければ下側）に出るため、
 * マウスカーソルに隠れない。スクロールや Esc キーで閉じる。
 */
export function Tooltip({ content, children, className }: {
  /** 吹き出しの中身。文字列の改行はそのまま表示される */
  content: React.ReactNode
  /** 吹き出しを出すきっかけになる要素 */
  children: React.ReactNode
  /** children を囲む要素のクラス */
  className?: string
}): React.ReactNode {
  const anchorRef = React.useRef<HTMLSpanElement>(null)
  const popupRef = React.useRef<HTMLDivElement>(null)
  const popupId = React.useId()
  const [isOpen, setIsOpen] = React.useState(false)
  const [position, setPosition] = React.useState<{ top: number, left: number }>()

  const open = () => setIsOpen(true)
  const close = () => {
    setIsOpen(false)
    setPosition(undefined)
  }

  // 吹き出しの大きさは描画してみないと分からないため、描画後・画面に映る前に位置を決める
  React.useLayoutEffect(() => {
    if (!isOpen || !anchorRef.current || !popupRef.current) return
    setPosition(calcPopupPosition(anchorRef.current.getBoundingClientRect(), popupRef.current.getBoundingClientRect()))
  }, [isOpen, content])

  // 開いている間だけ、閉じるきっかけになるイベントを購読する。
  // スクロールやリサイズで吹き出しがアンカーからずれるのを防ぐため、位置を追従させずに閉じる
  React.useEffect(() => {
    if (!isOpen) return
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') close()
    }
    document.addEventListener('keydown', handleKeyDown)
    window.addEventListener('scroll', close, true)
    window.addEventListener('resize', close)
    return () => {
      document.removeEventListener('keydown', handleKeyDown)
      window.removeEventListener('scroll', close, true)
      window.removeEventListener('resize', close)
    }
  }, [isOpen])

  return (
    <>
      <span
        ref={anchorRef}
        tabIndex={0}
        aria-describedby={isOpen ? popupId : undefined}
        onMouseEnter={open}
        onMouseLeave={close}
        onFocus={open}
        onBlur={close}
        className={className}
      >
        {children}
      </span>

      {isOpen && createPortal(
        // Modal の中でも手前に出るよう、Modal より大きい z-index にしている
        <div
          ref={popupRef}
          id={popupId}
          role="tooltip"
          className="fixed z-60 max-w-xs px-2 py-1 rounded bg-gray-800 text-white text-xs shadow whitespace-pre-wrap break-words pointer-events-none"
          style={position ?? { top: 0, left: 0, visibility: 'hidden' }}
        >
          {content}
        </div>,
        document.body
      )}
    </>
  )
}

/**
 * 吹き出しの位置を決める。
 * マウスカーソルは右下に伸びる形なので、カーソルに隠れないようアンカーの上側を優先する。
 * 左端はアンカーにそろえ、画面からはみ出す分だけずらす。
 */
function calcPopupPosition(anchor: DOMRect, popup: DOMRect): { top: number, left: number } {
  const above = anchor.top - ANCHOR_GAP - popup.height
  const top = above >= VIEWPORT_MARGIN
    ? above
    : anchor.bottom + ANCHOR_GAP

  const maxLeft = window.innerWidth - VIEWPORT_MARGIN - popup.width
  const left = Math.max(VIEWPORT_MARGIN, Math.min(anchor.left, maxLeft))

  return { top, left }
}
