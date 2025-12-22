import React from "react"
import * as ReactHookForm from "react-hook-form"

//#region メッセージのデータ構造

/**
 * サーバー側の PresentationContext がクライアント側に返されるときの detail プロパティの構造。
 * エラー、警告、情報メッセージを階層的に保持する。
 */
export type PresentationContextDetail = {
  /** この項目に対して発生したエラーメッセージの一覧。 */
  error?: string[]
  /** この項目に対して発生した警告メッセージの一覧。 */
  warn?: string[]
  /** この項目に対して発生した情報メッセージの一覧。 */
  info?: string[]
  /**
   * 子要素のメッセージ。
   *
   * 配列（nijo.xmlで言う Type="children"）の場合、
   * オブジェクトのキーは半角数字のみで構成されており、
   * 各キーは配列のインデックスを表す。
   */
  children?: { [key: string]: PresentationContextDetail }
}

//#endregion メッセージのデータ構造

//#region 全体

/**
 * メッセージコンテナのコンテキスト。
 * 各画面側で取り扱いやすいAPIを提供する。
 */
export type DetailMessageContextType = {
  /**
   * メッセージ全体を引数の値で置き換える。現在表示されているメッセージはすべてクリアされる。
   * @param detail complexPost のレスポンスとしてサーバーから返されたオブジェクトの detail プロパティの値。
   */
  replaceMessages: (detail: PresentationContextDetail | null | undefined) => void
  /**
   * メッセージ全体に引数の値を追加する。現在表示されているメッセージは保持される。
   * @param detail complexPost のレスポンスとしてサーバーから返されたオブジェクトの detail プロパティの値。
   */
  appendMessages: (detail: PresentationContextDetail | null | undefined) => void
  /**
   * 現在表示されているメッセージをすべてクリアする。
   */
  clearMessages: () => void
}

/**
 * 内部コンテキストの型
 */
type DetailMessageContextTypeInternal = {
  /** nameと、そのnameに対するメッセージ更新関数の紐づけのref */
  setMessageFunctionsByName: React.RefObject<NameRegistration>
  /** どこにも登録されていないnameを表示する関数の一覧のuseStateのsetter */
  setMessageFunctionsUnregistered: React.Dispatch<React.SetStateAction<ForUnregistered>>
}

/** nameごとの登録情報 */
type NameRegistration = Map<string, {
  /** このコンポーネントに表示するメッセージを引数の値で置き換える */
  setMessages: (messages: DetailMessageByField | null) => void
}[]>

/** どこにも登録されていないnameごとの登録情報 */
type ForUnregistered = {
  /** このコンポーネントに表示するメッセージを引数の値で置き換える */
  setMessages: (messages: DetailMessageByField | null) => void
}[]

/**
 * 外部公開コンテキスト
 */
const DetailMessageContext = React.createContext<DetailMessageContextType>({
  replaceMessages: () => { throw new Error('DetailMessageContext を配置してください。') },
  appendMessages: () => { throw new Error('DetailMessageContext を配置してください。') },
  clearMessages: () => { throw new Error('DetailMessageContext を配置してください。') },
})

/**
 * 内部コンテキスト
 */
const DetailMessageContextInternal = React.createContext<DetailMessageContextTypeInternal>({
  setMessageFunctionsByName: { current: new Map() },
  setMessageFunctionsUnregistered: () => { },
})

/**
 * complexPost で detail が返されたときに使用
 */
export function useSetter() {
  return React.useContext(DetailMessageContext)
}

/**
 * Query Model や Command Model の処理で発生した各項目に対するエラー等のメッセージ
 * （complexPost のレスポンスとしてサーバー側の PresentationContext がクライアント側に返した detail プロパティ）
 * を表示するためのコンテキスト。各画面側で取り扱いやすいAPIを提供する。
 */
export function Provider(props: { children: React.ReactNode }) {
  // 状態（name対応）
  const setMessageFunctionsByName = React.useRef<NameRegistration>(new Map())

  // 状態（name未登録分）
  const [unregistered, setUnregistered] = React.useState<ForUnregistered>([])
  const setMessageFunctionsUnregistered = React.useRef<ForUnregistered>(unregistered)
  setMessageFunctionsUnregistered.current = unregistered

  // コンテキスト内に未登録メッセージ表示箇所がどこにも無い場合はプロバイダー直下にメッセージを表示する
  const [otherMessages, setOtherMessages] = React.useState<DetailMessageByField>({})

  // 外部公開API
  const publicContextValue: DetailMessageContextType = React.useMemo(() => {
    const clearMessages = () => {
      for (const arr of setMessageFunctionsByName.current.values()) {
        for (const item of arr) {
          item.setMessages(null)
        }
      }
      for (const item of setMessageFunctionsUnregistered.current) {
        item.setMessages(null)
      }
      setOtherMessages({})
    }

    const replaceMessages = (detail: PresentationContextDetail | null | undefined) => {
      clearMessages()
      appendMessages(detail)
    }

    const appendMessages = (detail: PresentationContextDetail | null | undefined) => {
      // 構造化されたオブジェクトを平坦化
      const flatMap = flattenDetailMessages(detail)

      for (const [name, messageByField] of flatMap) {
        const registeredItems = setMessageFunctionsByName.current.get(name)
        if (registeredItems) {
          // nameが登録されているものを設定
          for (const item of registeredItems) {
            item.setMessages(messageByField)
          }
        } else {
          // 登録されていないnameの場合
          for (const item of setMessageFunctionsUnregistered.current) {
            item.setMessages(messageByField)
          }
          setOtherMessages(prev => ({
            error: [...(prev.error ?? []), ...(messageByField.error ?? [])],
            warn: [...(prev.warn ?? []), ...(messageByField.warn ?? [])],
            info: [...(prev.info ?? []), ...(messageByField.info ?? [])],
          }))
        }
      }
    }

    return { replaceMessages, appendMessages, clearMessages }
  }, [setMessageFunctionsByName, setMessageFunctionsUnregistered])

  // 内部用
  const internalContextValue: DetailMessageContextTypeInternal = React.useMemo(() => ({
    setMessageFunctionsByName,
    setMessageFunctionsUnregistered: setUnregistered,
  }), [setMessageFunctionsByName, setUnregistered])

  return (
    <DetailMessageContextInternal.Provider value={internalContextValue}>
      <DetailMessageContext.Provider value={publicContextValue}>

        {/* コンテキスト内に未登録メッセージ表示箇所がどこにも無い場合はプロバイダー直下にメッセージを表示する */}
        {unregistered.length === 0 && (otherMessages.error || otherMessages.warn || otherMessages.info) && (
          <ul>
            {otherMessages.error && otherMessages.error.map((msg, idx) => (
              <li key={`error-${idx}`} className="text-rose-700">{msg}</li>
            ))
            }
            {otherMessages.warn && otherMessages.warn.map((msg, idx) => (
              <li key={`warn-${idx}`} className="text-amber-700">{msg}</li>
            ))}
            {otherMessages.info && otherMessages.info.map((msg, idx) => (
              <li key={`info-${idx}`} className="text-sky-700">{msg}</li>
            ))}
          </ul>
        )}

        {props.children}
      </DetailMessageContext.Provider>
    </DetailMessageContextInternal.Provider>
  )
}

//#endregion 全体

//#region 項目単位のメッセージ

/**
 * 特定の項目に対するメッセージ（エラー・警告・情報）
 */
type DetailMessageByField = Omit<PresentationContextDetail, 'children'>

/**
 * Query Model や Command Model の処理で発生した各項目に対するエラー等のメッセージ
 * （complexPost のレスポンスとしてサーバー側の PresentationContext がクライアント側に返した detail プロパティ）
 * を表示するためのコンポーネント。
 *
 * react hook form を利用して、そのフォームに存在する項目の名前を型安全に扱うことができる。
 */
export function Of<
  TFieldValues extends ReactHookForm.FieldValues,
  TFieldPath extends ReactHookForm.FieldPath<TFieldValues>
>(props: {
  /** この項目に対して発生したメッセージをこの箇所に表示する。 */
  name: TFieldPath
  /** useForm の control オブジェクト。 name を型安全に扱うために必要。 */
  control: ReactHookForm.Control<TFieldValues>
  className?: string
}) {

  const { setMessageFunctionsByName } = React.useContext(DetailMessageContextInternal)
  const [messages, setMessages] = React.useState<DetailMessageByField | null>(null)

  React.useEffect(() => {
    // このコンポーネントと対象項目のnameを紐づける
    setMessageFunctionsByName.current.set(props.name, [
      ...(setMessageFunctionsByName.current.get(props.name) ?? []),
      { setMessages }
    ])

    // アンマウント時に登録を解除する
    return () => {
      const arr = setMessageFunctionsByName.current.get(props.name)
      if (arr) {
        const newArr = arr.filter(item => item.setMessages !== setMessages)
        if (newArr.length > 0) {
          setMessageFunctionsByName.current.set(props.name, newArr)
        } else {
          setMessageFunctionsByName.current.delete(props.name)
        }
      }
    }
  }, [props.name])

  if (!messages) {
    return null
  }

  return (
    <ul className={props.className}>
      {messages?.error && messages.error.map((msg, idx) => (
        <li key={`error-${idx}`} className="text-rose-700">{msg}</li>
      ))}
      {messages?.warn && messages.warn.map((msg, idx) => (
        <li key={`warn-${idx}`} className="text-amber-700">{msg}</li>
      ))}
      {messages?.info && messages.info.map((msg, idx) => (
        <li key={`info-${idx}`} className="text-sky-700">{msg}</li>
      ))}
    </ul>
  )
}

//#endregion 項目単位のメッセージ

//#region どこにも登録されていないname用

/**
 * Query Model や Command Model の処理で発生した各項目に対するエラー等のメッセージ
 * （complexPost のレスポンスとしてサーバー側の PresentationContext がクライアント側に返した detail プロパティ）
 * のうち、どこにも登録されていないname用に表示するためのコンポーネント。
 *
 * コンテキスト内のどこにもこのコンポーネントが存在しない場合、未登録のメッセージはプロバイダー直下に表示される。
 */
export function Rest(props: { className?: string }) {
  const { setMessageFunctionsUnregistered } = React.useContext(DetailMessageContextInternal)
  const [messages, setMessages] = React.useState<DetailMessageByField | null>(null)

  React.useEffect(() => {
    // このコンポーネントを登録する
    setMessageFunctionsUnregistered(prev => {
      return [...prev, { setMessages }]
    })

    // アンマウント時に登録を解除する
    return () => {
      setMessageFunctionsUnregistered(prev => {
        return prev.filter(item => item.setMessages !== setMessages)
      })
    }
  }, [])

  if (!messages) {
    return null
  }

  return (
    <ul className={props.className}>
      {messages?.error && messages.error.map((msg, idx) => (
        <li key={`error-${idx}`} className="text-rose-700">{msg}</li>
      ))}
      {messages?.warn && messages.warn.map((msg, idx) => (
        <li key={`warn-${idx}`} className="text-amber-700">{msg}</li>
      ))}
      {messages?.info && messages.info.map((msg, idx) => (
        <li key={`info-${idx}`} className="text-sky-700">{msg}</li>
      ))}
    </ul>
  )
}

//#endregion どこにも登録されていないname用

//#region ユーティリティ
/**
 * 詳細メッセージオブジェクトを平坦化する。例:
 *
 * ```
 * // 変換前
 * {
 *   error: ['ルートエラー'],
 *   children: {
 *     '1': {
 *       children: {
 *         aaa: { error: ['エラー1'], info: ['情報1'] }
 *       }
 *     }
 *   }
 * }
 *
 * // 変換後
 * Map {
 *   '': { error: ['ルートエラー'] },
 *   '1.aaa': { error: ['エラー1'], info: ['情報1'] }
 * }
 * ```
 */
function flattenDetailMessages(detail: PresentationContextDetail | null | undefined): [name: string, messages: DetailMessageByField][] {
  const result: [name: string, messages: DetailMessageByField][] = []
  if (!detail) return result

  const traverse = (current: PresentationContextDetail, prefix: string) => {
    const { children, ...messages } = current
    const hasMessage = (messages.error?.length ?? 0) > 0
      || (messages.warn?.length ?? 0) > 0
      || (messages.info?.length ?? 0) > 0

    if (hasMessage) {
      result.push([prefix, messages])
    }

    if (children) {
      for (const key of Object.keys(children)) {
        const nextPrefix = prefix === '' ? key : `${prefix}.${key}`
        traverse(children[key], nextPrefix)
      }
    }
  }

  traverse(detail, '')
  return result
}

//#endregion ユーティリティ
