import React, { useEffect, useRef } from "react"

/**
 * 指定した要素の外側でマウスボタンが押されたときにコールバックを呼ぶフック。
 * ドロップダウン等を外側クリックで閉じる用途を想定している。
 *
 * @param ref 内側とみなす要素
 * @param onOutsideClick 外側でマウスボタンが押されたときに呼ばれる関数
 */
export function useOutsideClick(ref: React.RefObject<HTMLElement | null>, onOutsideClick: () => void) {
  // 呼び出し側が毎レンダー新しい関数を渡してもリスナーを付け直さずに済むよう、最新のコールバックを ref で保持する
  const onOutsideClickRef = useRef(onOutsideClick)
  onOutsideClickRef.current = onOutsideClick

  // document のマウスイベントと同期
  useEffect(() => {
    const handleMouseDown = (event: MouseEvent) => {
      if (ref.current && !ref.current.contains(event.target as Node)) {
        onOutsideClickRef.current()
      }
    }
    document.addEventListener("mousedown", handleMouseDown)
    return () => {
      document.removeEventListener("mousedown", handleMouseDown)
    }
  }, [ref])
}
