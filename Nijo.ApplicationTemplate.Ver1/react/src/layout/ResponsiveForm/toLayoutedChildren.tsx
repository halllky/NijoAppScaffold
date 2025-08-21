import React from "react"
import { Item, ItemProps } from "./Item"
import { Section, SectionProps } from "./Section"
import { BreakPoint } from "./BreakPoint"
import { Spacer } from "./Spacer"

/**
 * children をグルーピングし、レスポンシブフォームのルールに則って
 * 適切な位置にスペースを挟んだり display: flex や display: grid で囲んだりしたchildrenにして返す。
 *
 * * childernを先頭から順番に列挙し、横幅いっぱいとる要素（fullWidth）が出る度にグルーピングを開始する。
 *   * fullWidth の要素はそれ単独で1つのグループ。
 *   * それ以外の要素は、その要素が出るまでの間を1つのグループとする。
 * * fullWidth になりうる要素は以下
 *   * FormItem (propsでfullWidthが指定されている場合のみ)
 *   * Spacer
 *   * 未知の要素
 * * 2段組みの場合は BreakPoint の位置、または要素数の半分（length / 2）でグループを区切り、それぞれをdisplay:gridのdivで囲む
 */
export const toLayoutedChildren = (
  children: React.ReactNode,
  /** コンテナ全体が2段組みレイアウトを収める程度の横幅を持っていればtrue */
  isWideLayout: boolean,
  /** ラベル列の横幅(px) */
  labelWidth: number,
  /** コンテンツ列の横幅(px) */
  valueWidth: number,
): React.ReactNode => {

  // 計算しやすいデータ構造に変換する
  type GroupedChild = FullWidthChild | NotFullWidthChild
  type FullWidthChild = {
    type: 'fullwidth'
    child: React.ReactNode
    isSpacer?: true
  }
  type NotFullWidthChild = {
    type: 'group'
    /** このグループ内でBreakPointが登場したかどうか */
    occurredBreakPoint: boolean
    /** 2段組みレイアウトの左側のグループ */
    first: React.ReactElement[]
    /** 2段組みレイアウトの右側のグループ。2段組みレイアウトでない場合は未定義 */
    second?: React.ReactElement[]
  }

  const groupedChildren = React.Children.toArray(children).reduce((groups, child) => {
    // 未知の要素
    if (!React.isValidElement(child)) {
      groups.push({ type: 'fullwidth', child })
      return groups
    }

    // BreakPoint
    if (child.type === BreakPoint) {
      const lastGroup = groups[groups.length - 1]
      if (lastGroup?.type === 'group') {
        lastGroup.occurredBreakPoint = true
      } else {
        // グループの中で登場していない（変な位置に指定されている）ブレークポイントは無視
      }
      return groups
    }

    // fullwidthでない要素
    if ((child.type === Section && !(child.props as SectionProps)?.fullWidth) ||
      (child.type === Item && !(child.props as ItemProps)?.fullWidth)) {
      const lastGroup = groups[groups.length - 1]
      if (lastGroup?.type === 'group') {
        if (isWideLayout && lastGroup.occurredBreakPoint) {
          if (!lastGroup.second) lastGroup.second = []
          lastGroup.second.push(child)
        } else {
          lastGroup.first.push(child)
        }
      } else {
        groups.push({ type: 'group', occurredBreakPoint: false, first: [child] })
      }
      return groups
    }

    // Spacer
    if (child.type === Spacer) {
      groups.push({ type: 'fullwidth', child, isSpacer: true })
      return groups
    }

    // 上記以外はすべて fullWidth
    groups.push({ type: 'fullwidth', child })
    return groups
  }, [] as GroupedChild[])

  // ブレークポイントが登場していないグループについて、
  // 全要素が first に入っているので、後半を second に移動する
  if (isWideLayout) {
    for (const group of groupedChildren) {
      if (group.type !== 'group' || group.occurredBreakPoint) continue

      const halfLength = Math.floor(group.first.length / 2)
      const allNodes = [...group.first]
      group.first = allNodes.slice(0, halfLength)
      group.second = allNodes.slice(halfLength)
    }
  }

  return (
    <>
      {groupedChildren.map((group, index) => (
        <React.Fragment key={index}>
          {/* 自動的にSpacerを挿入する */}
          {index > 0
            && !(group as FullWidthChild).isSpacer
            && !(groupedChildren[index - 1] as FullWidthChild).isSpacer
            && (
              <Spacer />
            )}

          {/* full width の要素はそのままレンダリング */}
          {group.type === 'fullwidth' && (
            group.child
          )}

          {/* 1段組みレイアウト */}
          {group.type === 'group' && group.second === undefined && (
            <div style={{
              display: 'grid',
              gridTemplateColumns: `${labelWidth}px 1fr`,
              gap: '4px',
            }}>
              {renderNonFullWidthChild(group.first)}
            </div>
          )}

          {/* 2段組みレイアウト */}
          {group.type === 'group' && group.second !== undefined && (
            <div className="flex gap-2 items-start">

              {/* 1段目 */}
              <div style={{
                display: 'grid',
                gridTemplateColumns: `${labelWidth}px ${valueWidth}px`,
                gap: '4px',
              }}>
                {renderNonFullWidthChild(group.first)}
              </div>

              {/* 2段目 */}
              <div style={{
                display: 'grid',
                gridTemplateColumns: `${labelWidth}px 1fr`,
                gap: '4px',
                flex: '1',
              }}>
                {renderNonFullWidthChild(group.second)}
              </div>
            </div>
          )}
        </React.Fragment>
      ))}
    </>
  )
}

/** full width でない要素のレンダリング。同じ処理が3回出てくるので関数に切り出している */
const renderNonFullWidthChild = (array: React.ReactElement[]): React.ReactNode => {
  return array.map((child, index) => (
    <React.Fragment key={index}>
      {/* セクション前のスペース */}
      {index > 0 && child.type === Section && array[index - 1].type !== Section && (
        <div className="col-span-2 min-h-2 basis-2"></div>
      )}

      {child}

      {/* セクション後のスペース */}
      {index < array.length - 1 && child.type === Section && (
        <div className="col-span-2 min-h-2 basis-2"></div>
      )}
    </React.Fragment>
  ))
}
