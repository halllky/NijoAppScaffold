import { useEffect } from 'react'

/**
 * 指定された要素の外側をクリックしたときにコールバックを実行するフック
 * @param ref 監視する要素のref
 * @param onOutsideClick 外側をクリックしたときのコールバック
 * @param deps 依存配列
 */
export const useOutsideClick = (
    ref: React.RefObject<HTMLElement | null>,
    onOutsideClick: () => void,
    deps: React.DependencyList
) => {
    useEffect(() => {
        const handleClickOutside = (event: MouseEvent) => {
            if (ref.current && !ref.current.contains(event.target as Node)) {
                onOutsideClick()
            }
        }

        document.addEventListener('mousedown', handleClickOutside)
        return () => {
            document.removeEventListener('mousedown', handleClickOutside)
        }
    }, deps)
}
