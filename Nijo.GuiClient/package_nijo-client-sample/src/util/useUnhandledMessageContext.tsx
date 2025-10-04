import { XMarkIcon } from "@heroicons/react/24/outline"
import React from "react"
import { UUID } from "uuidjs"

/**
 * どこにも表示する箇所が無いメッセージの表示を行う
 */
export const useUnhandledMessage = () => {
  return React.useContext(UnhandledMessageContext)
}

/**
 * インラインメッセージのコンテキストのプロバイダー。
 * 画面ルートなど、どこにも表示する箇所が無いメッセージの表示より外側に配置する必要がある。
 */
export const UnhandledMessageContextProvider = (props: { children: React.ReactNode }) => {

  const [displayMessages, setDisplayMessages] = React.useState<UnhandledMessageItem[]>(() => [])

  const outerContextValue = React.useMemo((): UnhandledMessageContextType => ({
    error: (msg: string) => {
      console.error(msg)
      setDisplayMessages(prev => [...prev, { id: UUID.generate(), msg, type: 'error' }])
    },
    warn: (msg: string) => setDisplayMessages(prev => [...prev, { id: UUID.generate(), msg, type: 'warn' }]),
    info: (msg: string) => setDisplayMessages(prev => [...prev, { id: UUID.generate(), msg, type: 'info' }]),
    clear: (deleteIdList?: string[]) => {
      if (deleteIdList && deleteIdList.length > 0) {
        setDisplayMessages(prev => prev.filter(msg => !deleteIdList.includes(msg.id)))
      } else {
        setDisplayMessages([])
      }
    },
  }), [setDisplayMessages])

  return (
    <UnhandledMessageContext.Provider value={outerContextValue}>
      <UnhandledMessageContextInternal.Provider value={displayMessages}>
        {props.children}
      </UnhandledMessageContextInternal.Provider>
    </UnhandledMessageContext.Provider>
  )
}

/**
 * インラインメッセージ。
 * エラーは赤、警告は黄色、情報は青色で表示する。
 *
 * メッセージ1件ごとに削除ボタンがついており、ユーザーが削除ボタンをクリックするとそのメッセージが消える。
 * 2件以上のメッセージがある場合、末尾に「n件のメッセージ」と総数が表示される。その左に「すべてクリアする」ボタンがあり、これをクリックすると全てのメッセージが消える。
 *
 * 同じ種類・同じ文言のメッセージが連続して表示された場合、表示上は1件にまとめられたうえで、 "②" など何件同じメッセージが現れているかが表示される。
 */
export const UnhandledMessage = (props: {
  className?: string
}) => {
  const displayMessages = React.useContext(UnhandledMessageContextInternal)
  const { clear } = React.useContext(UnhandledMessageContext)

  // 同じ種類・同じ文言のメッセージが連続して表示された場合、表示上は1件にまとめられたうえで、 "②" など何件同じメッセージが現れているかが表示される。
  const groupedMessages = React.useMemo(() => {
    return displayMessages.reduce((acc, msg) => {
      const previous = acc[acc.length - 1]
      if (previous && previous.type === msg.type && previous.msg === msg.msg) {
        previous.idList.push(msg.id)
      } else {
        acc.push({ type: msg.type, msg: msg.msg, idList: [msg.id] })
      }
      return acc
    }, [] as { type: 'info' | 'warn' | 'error', msg: string, idList: string[] }[])
  }, [displayMessages])

  const handleClearAll = React.useCallback(() => {
    clear()
  }, [clear])

  if (displayMessages.length === 0) {
    return undefined
  }

  return (
    <div className={`${props.className ?? ''} flex flex-col`}>
      <ul className="flex-1 flex flex-col gap-px overflow-y-auto max-h-40">
        {groupedMessages.map((msg, index) => (
          <li key={index} className={`${getInlineMessageClassName(msg.type)} flex flex-wrap px-1`}>
            {msg.msg}
            {msg.idList.length > 1 && ` (${msg.idList.length} 件)`}
            <div className="flex-1"></div>
            <button type="button" className="text-sm cursor-pointer" title="削除" onClick={() => clear(msg.idList)}>
              <XMarkIcon className="w-4 h-4" />
            </button>
          </li>
        ))}
      </ul>

      {groupedMessages.length > 1 && (
        <div className="flex justify-start gap-2">
          <span className="text-sm">{displayMessages.length}件のメッセージ</span>
          <button className="text-sm cursor-pointer" onClick={handleClearAll}>すべてクリアする</button>
        </div>
      )}
    </div>
  )
}

// --------------------------------------------

const getInlineMessageClassName = (type: 'info' | 'warn' | 'error') => {
  switch (type) {
    case 'info':
      return 'border border-blue-500 text-blue-500 bg-blue-50'
    case 'warn':
      return 'border border-amber-500 text-amber-500 bg-amber-50'
    case 'error':
      return 'border border-rose-500 text-rose-500 bg-rose-50'
  }
}

// --------------------------------------------

const UnhandledMessageContext = React.createContext<UnhandledMessageContextType>({
  error: text => { throw new Error(`メッセージの表示箇所が設定されていません。InlineMessageContextProvider と InlineMessage を配置して下さい。メッセージの内容は以下です。\n${text}`) },
  warn: text => { throw new Error(`メッセージの表示箇所が設定されていません。InlineMessageContextProvider と InlineMessage を配置して下さい。メッセージの内容は以下です。\n${text}`) },
  info: text => { throw new Error(`メッセージの表示箇所が設定されていません。InlineMessageContextProvider と InlineMessage を配置して下さい。メッセージの内容は以下です。\n${text}`) },
  clear: () => { throw new Error(`メッセージの表示箇所が設定されていません。InlineMessageContextProvider と InlineMessage を配置して下さい。`) },
})

const UnhandledMessageContextInternal = React.createContext<UnhandledMessageItem[]>([])

type UnhandledMessageContextType = {
  error: (msg: string) => void
  warn: (msg: string) => void
  info: (msg: string) => void
  clear: (deleteIdList?: string[]) => void
}

type UnhandledMessageItem = {
  id: string
  msg: string
  type: 'info' | 'warn' | 'error'
}
