import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { useOutsideClick } from '../util';

/**
 * モーダルダイアログ。
 *
 * 手前に浮き上がって表示される。
 * DOMツリー上ではbody直下にポータルで配置される。
 *
 * ESCキー押下や外側クリックでダイアログを閉じる処理は
 * このコンポーネントを呼ぶ側で実装する。
 */
export const ModalDialog = ({ open, onOutsideClick, children, className }: {
  open: boolean
  /**
   * ダイアログの外側がクリックされたときのコールバック。
   * 外側クリックで閉じられるダイアログの場合はここで閉じる処理を行う。
   */
  onOutsideClick?: () => void
  children?: React.ReactNode
  className?: string
}) => {

  const divRef = React.useRef<HTMLDivElement>(null);
  useOutsideClick(divRef, () => {
    onOutsideClick?.()
  }, [onOutsideClick])

  if (!open) return null;

  return ReactDOM.createPortal(
    <div className="fixed inset-0 z-10 flex items-center justify-center">

      {/* シェード */}
      <div className="absolute inset-0 bg-black/25" />

      {/* ダイアログ */}
      <div ref={divRef} className={`relative bg-white ${className ?? ''}`}>
        {children}
      </div>
    </div>,
    document.body,
  )
}
