import React from "react"
import * as Icon from "@heroicons/react/24/solid"

type DropdownSelectorProps = {
  value?: string
  onChange: (value: string) => void
  className?: string
  /** [値, ラベル, レンダラー]のタプルの配列 */
  children: [value: string, label: React.ReactNode, renderer: React.ReactNode][]
}

export const DropdownSelector: React.FC<DropdownSelectorProps> = ({
  value,
  onChange,
  className,
  children
}) => {
  const [isOpen, setIsOpen] = React.useState(false)
  const [highlightedIndex, setHighlightedIndex] = React.useState(0)
  const [maxHeight, setMaxHeight] = React.useState<number | undefined>(undefined)
  const dropdownRef = React.useRef<HTMLDivElement>(null)
  const menuRef = React.useRef<HTMLDivElement>(null)

  // 現在選択されているオプションを取得
  const selectedIndex = children.findIndex(([optionValue]) => optionValue === value)
  const selectedLabel = selectedIndex >= 0 ? children[selectedIndex][1] : null

  // ドロップダウンの外側クリックで閉じる
  React.useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false)
      }
    }

    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [])

  // ドロップダウンがブラウザの幅を超えたときにドロップダウンに縦スクロールを発生させるため、
  // ドロップダウンが開いたときに最大高さを計算
  React.useEffect(() => {
    if (isOpen && menuRef.current) {
      const menuRect = menuRef.current.getBoundingClientRect()
      const viewportHeight = window.innerHeight
      const availableHeight = viewportHeight - menuRect.top - 16 // 16px のマージン
      setMaxHeight(availableHeight)
    }
  }, [isOpen])

  // キーボード操作
  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (!isOpen) {
      if (e.key === 'Enter' || e.key === ' ' || e.key === 'ArrowDown') {
        e.preventDefault()
        setIsOpen(true)
        setHighlightedIndex(0)
      }
      return
    }

    switch (e.key) {
      case 'Escape':
        e.preventDefault()
        setIsOpen(false)
        break
      case 'ArrowDown':
        e.preventDefault()
        setHighlightedIndex(prev => (prev + 1) % children.length)
        break
      case 'ArrowUp':
        e.preventDefault()
        setHighlightedIndex(prev => prev === 0 ? children.length - 1 : prev - 1)
        break
      case 'Enter':
        e.preventDefault()
        onChange(children[highlightedIndex][0])
        setIsOpen(false)
        break
    }
  }

  const handleOptionClick = (optionValue: string, e: React.MouseEvent) => {
    e.preventDefault()
    e.stopPropagation()
    onChange(optionValue)
    setIsOpen(false)
  }

  const handleToggle = () => {
    setIsOpen(!isOpen)
    if (!isOpen) {
      setHighlightedIndex(0)
    }
  }

  return (
    <div className={`relative ${className ?? ''}`} ref={dropdownRef}>
      {/* トリガーボタン */}
      <button
        type="button"
        onClick={handleToggle}
        onKeyDown={handleKeyDown}
        className="flex items-center justify-between w-full px-1 py-px border border-gray-300 bg-white text-left"
      >
        <span className="truncate">
          {selectedLabel || '選択してください'}
        </span>
        <Icon.ChevronDownIcon className="w-4 h-4 ml-2" />
      </button>

      {/* ドロップダウンメニュー */}
      {isOpen && (
        <div
          ref={menuRef}
          className="absolute z-50 w-full mt-1 bg-white border border-gray-300 shadow-lg overflow-y-auto"
          style={{
            // 384px は Tailwind の h-96 の高さに相当
            maxHeight: maxHeight ? `${maxHeight}px` : '384px',
          }}
        >
          {children.map(([optionValue, optionLabel, optionRenderer], index) => (
            <div
              key={optionValue}
              onClick={(e) => handleOptionClick(optionValue, e)}
              className={`cursor-pointer border-b border-gray-100 last:border-b-0 ${index === highlightedIndex ? 'bg-blue-50' : 'hover:bg-gray-50'} ${optionValue === value ? 'bg-blue-100' : ''}`}
              onMouseEnter={() => setHighlightedIndex(index)}
            >
              {optionRenderer}
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
