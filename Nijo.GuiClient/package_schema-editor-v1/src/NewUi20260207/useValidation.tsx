import React from "react"
import useEvent from "react-use-event-hook"
import * as ReactRouter from "react-router-dom"
import { asTree, SchemaDefinitionGlobalState, XmlElementAttributeName } from "../types"
import { SERVER_DOMAIN } from "../main"
import { NIJOUI_CLIENT_ROUTE_PARAMS } from "../routing"

/** スキーマ定義のバリデーション機能 */
export const useValidation = (
  /** 編集中の最新の値を取得する関数 */
  getEditingValues: () => SchemaDefinitionGlobalState
) => {
  // 短時間で繰り返し実行するとサーバーに負担がかかるため、
  // 最後にリクエストした時間から一定時間以内はリクエストをしないようにする。
  const [isRequestPrevented, setIsRequestPrevented] = React.useState(false)

  // サーバーから返ってくる検証結果。
  // 独特な形をしているのでReact Hook Formのエラーとは別に管理する。
  const [validationResult, setValidationResult] = React.useState<ValidationResult>({})

  const [searchParams] = ReactRouter.useSearchParams()
  const projectDir = searchParams.get(NIJOUI_CLIENT_ROUTE_PARAMS.QUERY_PROJECT_DIR)

  // 入力検証を実行する。
  const trigger: ValidationTriggerFunction = useEvent(async () => {
    // 最後にリクエストした時間から一定時間以内はリクエストをしない
    if (isRequestPrevented) return;
    setIsRequestPrevented(true)
    window.setTimeout(() => {
      setIsRequestPrevented(false)
    }, 1000)

    // サーバーに問い合わせ。ステータスコード202ならエラーあり。200ならエラーなしなのでエラーをクリアする。
    const result = await fetch(`${SERVER_DOMAIN}/api/validate?${NIJOUI_CLIENT_ROUTE_PARAMS.QUERY_PROJECT_DIR}=${encodeURIComponent(projectDir ?? '')}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(getEditingValues()),
    })
    if (result.status === 202) {
      const errors: ValidationResult = await result.json()
      setValidationResult(errors)
    } else {
      setValidationResult({})
    }
  })

  // 特定のXML要素に対する検証結果を取得する処理
  const getValidationResult: GetValidationResultFunction = React.useCallback(id => {
    return validationResult?.[id] ?? { _own: [] }
  }, [validationResult])

  // 検証結果を表形式で表示するときのためのデータを生成する。
  const validationResultList = React.useMemo(() => {
    const state = getEditingValues()
    const xmlElementTrees = state.xmlElementTrees ?? []
    const customAttributes = state.customAttributes ?? []

    // IDから当該要素の情報を引き当てるための辞書
    const elementMap = new Map(xmlElementTrees.flatMap(tree => tree.xmlElements).map(el => [el.uniqueId, el]))
    const treeUtilsMap = new Map(xmlElementTrees.map(tree => [tree, asTree(tree.xmlElements)]))
    const customAttributeMap = new Map(customAttributes.map(ca => [ca.uniqueId, ca]))

    const infos: ValidationResultListItem[] = []

    for (const [id, obj] of Object.entries(validationResult ?? {})) {
      // 集約定義のエラー
      const element = elementMap.get(id)
      if (element) {
        const tree = xmlElementTrees.find(t => t.xmlElements.some(el => el.uniqueId === id))
        if (!tree) continue

        const treeUtils = treeUtilsMap.get(tree)
        if (!treeUtils) continue

        const rootElement = treeUtils.getRoot(element)
        const rootAggregateName = rootElement.localName

        for (const [objKey, attrMessages] of Object.entries(obj)) {

          let attributeName: string
          if (objKey === '_own') {
            // この要素自体に対するエラー
            attributeName = ''
          } else {
            // カスタム属性のエラーならばカスタム属性の名前を、
            // そうでなければ属性名をそのまま使う
            const customAttribute = customAttributes.find(ca => ca.uniqueId === objKey)
            attributeName = customAttribute?.displayName ?? customAttribute?.physicalName ?? objKey
          }

          infos.push(...attrMessages.map(x => ({
            id,
            rootAggregateName: rootAggregateName ?? '',
            elementName: element.localName ?? '',
            attributeName,
            message: x,
          })))
        }
        continue
      }

      // カスタム属性定義のエラー
      const customAttribute = customAttributeMap.get(id as XmlElementAttributeName)
      if (customAttribute) {
        for (const [objKey, attrMessages] of Object.entries(obj)) {
          const attributeName = objKey === '_own' ? '' : objKey
          infos.push(...attrMessages.map(x => ({
            id,
            rootAggregateName: 'カスタム属性',
            elementName: customAttribute.physicalName ?? '',
            attributeName,
            message: x,
          })))
        }
        continue
      }
    }
    return infos
  }, [getEditingValues, validationResult])

  return {
    /** 検証結果を表形式で表示するときのためのデータ */
    validationResultList,
    /** 特定のXML要素に対する検証結果を取得する関数 */
    getValidationResult,
    /** 検証を実行する */
    trigger,
  }
}

/**
 * サーバーから返ってくる検証結果の型。
 * この形は、C#側の ToReactErrorObject で生成されるJsonObjectの型と一致する。
 */
export type ValidationResult = {
  [uniqueId: string]: ValidationResultToElement
}

/**
 * 特定のXML要素に対する検証結果を取得する関数。
 * @param id 対象のXML要素のID
 */
export type GetValidationResultFunction = (id: string) => ValidationResultToElement

/**
 * 検証を実行する関数。
 */
export type ValidationTriggerFunction = () => Promise<void>

/**
 * 検証結果を表形式で表示するときのための1行分のデータ。
 */
export type ValidationResultListItem = {
  id: string
  rootAggregateName: string
  elementName: string
  attributeName: string
  message: string
}

/**
 * サーバーから返ってくる検証結果のうち特定のXML要素に対するもの。
 */
export type ValidationResultToElement = {
  /** この要素自体に対するエラー */
  _own: string[]
  /** この要素の属性に対するエラー */
  [attributeName: string]: string[]
}
