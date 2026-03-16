import cytoscape from 'cytoscape';
// @ts-ignore このライブラリは型定義を提供していない
import nodeHtmlLabel from "cytoscape-node-html-label";
import { Node as GraphView2Node } from './types';

// cytoscape-node-html-label拡張機能を登録
nodeHtmlLabel(cytoscape);

/** 親ノードのラベルのパディング */
const PARENT_NODE_PADDING = 20;

/** HTMLラベルテンプレートを設定する */
export const setupHtmlLabels = (cyInstance: cytoscape.Core) => {
  (cyInstance as any).nodeHtmlLabel([
    {
      query: 'node', // 全てのノードに適用
      valign: 'center',
      halign: 'center',
      tpl: (data: { label?: string, table?: GraphView2Node["table"] }) => {
        const label = data.label || ''
        // 改行文字（\n または \r\n）を<br>タグに変換
        const htmlLabel = label.replace(/\r\n|\n/g, '<br>')
        if (data.table && (data.table.headers.length > 0 || data.table.rows.length > 0)) {
          const { headers, rows } = data.table
          const headerCellHtml = headers.map(h =>
            `<th style="padding:2px 6px;border:1px solid #c8c8c8;font-size:12px;font-weight:bold;white-space:nowrap;">${h}</th>`
          ).join('')
          const rowsHtml = rows.map(row =>
            `<tr>${row.map(cell =>
              `<td style="padding:2px 6px;border:1px solid #c8c8c8;font-size:12px;white-space:nowrap;">${cell}</td>`
            ).join('')}</tr>`
          ).join('')
          return `<div style="pointer-events:none;display:flex;flex-direction:column;align-items:center;gap:4px;"><div>${htmlLabel}</div><table style="border-collapse:collapse;"><thead><tr>${headerCellHtml}</tr></thead><tbody>${rowsHtml}</tbody></table></div>`

        } else {
          return `<div style="pointer-events: none; max-width: 320px;">${htmlLabel}</div>`
        }
      }
    },
    {
      query: 'node:parent', // 親ノードに適用
      valign: 'top',
      valignBox: 'top',
      halign: 'center',
      tpl: (data: { label: string | undefined }) => {
        const label = data.label || ''
        // 改行文字（\n または \r\n）を<br>タグに変換
        const htmlLabel = label.replace(/\r\n|\n/g, '<br>')
        return `<div style="transform: translateY(-${PARENT_NODE_PADDING}px); text-align: center; pointer-events: none;">${htmlLabel}</div>`
      }
    },
  ] satisfies CytoscapeNodeHtmlParams[])
}

/** スタイルシート */
export const getStyleSheet = (): cytoscape.CytoscapeOptions['style'] => {
  // テキストの幅を推定する関数
  const canvas = document.createElement('canvas')
  const canvasContext = canvas.getContext('2d')
  if (!canvasContext) return []
  const estimateTextWidth = (text: string) => {
    canvasContext.font = '16px Noto Sans JP'
    return canvasContext.measureText(text).width
  }

  const estimateTableCellWidth = (text: string) => {
    canvasContext.font = '12px Noto Sans JP'
    return canvasContext.measureText(text).width
  }

  // max-widthを考慮してテキストが何行になるかを計算する関数
  const calculateTextLines = (text: string, maxWidth: number = 320): number => {
    if (!text) return 1

    // まず明示的な改行文字で分割
    const explicitLines = text.split(/\r\n|\n/)
    let totalLines = 0

    for (const line of explicitLines) {
      if (!line.trim()) {
        totalLines += 1
        continue
      }

      const lineWidth = estimateTextWidth(line)
      if (lineWidth <= maxWidth) {
        totalLines += 1
      } else {
        // 長すぎる場合は単語単位で折り返し計算
        const words = line.split(/\s+/)
        let currentLineWidth = 0
        let currentLineCount = 1

        for (const word of words) {
          const wordWidth = estimateTextWidth(word + ' ')

          if (currentLineWidth + wordWidth > maxWidth) {
            currentLineCount += 1
            currentLineWidth = wordWidth
          } else {
            currentLineWidth += wordWidth
          }
        }
        totalLines += currentLineCount
      }
    }

    return Math.max(1, totalLines)
  }

  return [{
    selector: 'node',
    css: {
      'shape': 'rectangle',
      'width': (node: cytoscape.NodeSingular) => {
        const labelWidth = estimateTextWidth((node.data('label') as string) || '')
        const table = node.data('table') as GraphView2Node["table"] | undefined
        if (table) {
          const CELL_H_PADDING = 12 // padding: 2px 6px → 6px × 2 = 12px
          const colCount = Math.max(table.headers.length, ...table.rows.map(r => r.length), 0)
          let tableWidth = 0
          for (let col = 0; col < colCount; col++) {
            const cellTexts = [table.headers[col] ?? '', ...table.rows.map(row => row[col] ?? '')]
            const maxCellWidth = Math.max(...cellTexts.map(t => estimateTableCellWidth(t)))
            tableWidth += maxCellWidth + CELL_H_PADDING
          }
          return Math.min(600, Math.max(32, Math.max(labelWidth + 8, tableWidth + 2)))

        } else {
          return Math.min(320, Math.max(32, labelWidth + 8))
        }
      },
      'height': (node: cytoscape.NodeSingular) => {
        const label = (node.data('label') as string) || ''
        const table = node.data('table') as GraphView2Node["table"] | undefined
        if (table) {
          const LINE_HEIGHT = 16 * 1.2
          const labelHeight = LINE_HEIGHT // テーブル付きノードのラベルは1行と仮定
          const TABLE_ROW_HEIGHT = 22 // 12pxフォント + 上下パディング4px + ボーダー2px + 行間
          const tableHeight = TABLE_ROW_HEIGHT * (1 + table.rows.length)
          return Math.max(32, labelHeight + 4 + tableHeight + 8) // 4px gap + 8px上下パディング

        } else {
          // max-widthを考慮してテキストの実際の行数を計算
          const lines = calculateTextLines(label, 320) // HTMLラベルのmax-widthと同じ値
          const lineHeight = 16 * 1.2 // フォントサイズ16px × line-height 1.2
          return Math.max(32, lines * lineHeight + 8) // 最小32px、上下パディング8px
        }
      },
      'color': (node: cytoscape.NodeSingular) => (node.data('color') as string) ?? '#000000',
      'border-width': '1px',
      'border-color': node => (node.data('border-color') as string) ?? '#909090',
      'background-color': node => (node.data('background-color') as string) ?? '#666666',
      'background-opacity': .1,
    },
  }, {
    selector: 'node:selected',
    style: {
      'border-style': 'solid',
      'border-width': '2px',
      'border-color': node => (node.data('border-color:selected') as string) ?? '#0060e6',
    },
  }, {
    selector: 'node:parent', // 子要素をもつノードに適用される
    css: {
      'text-valign': 'top', // ラベルをノードの上部外側に配置
      'padding': `${PARENT_NODE_PADDING}px`, // parentが複数重なるとラベルが重なるので、ノードの上部分に余白を持たせる
      'color': (node: cytoscape.NodeSingular) => (node.data('color:container') as string) ?? '#707070',
    },
  }, {
    selector: 'edge',
    style: {
      'label': 'data(label)',
      'color': '#707070',
      'line-color': edge => edge.data('line-color') ?? '#707070',
      'line-style': edge => edge.data('line-style') ?? 'solid',
      'line-opacity': .5,
      'font-size': '10px',
      'source-arrow-shape': edge => edge.data('sourceEndShape') ?? 'none',
      'target-arrow-shape': edge => edge.data('targetEndShape') ?? 'none',
      'source-arrow-color': edge => edge.data('line-color') ?? '#707070',
      'target-arrow-color': edge => edge.data('line-color') ?? '#707070',
      'curve-style': 'bezier',
      'width': '1px',
      'source-label': (edge: cytoscape.EdgeSingular) => edge.data('sourceEndLabel') ?? '',
      'target-label': (edge: cytoscape.EdgeSingular) => edge.data('targetEndLabel') ?? '',
      'source-text-margin-y': -10,
      'target-text-margin-y': -10,
    },
  }, {
    selector: 'edge:selected',
    style: {
      'label': 'data(label)',
      'color': '#0060e6',
      'line-color': '#0060e6',
      'line-style': 'solid',
      'line-opacity': 1,
      'source-arrow-shape': edge => edge.data('sourceEndShape') ?? 'none',
      'target-arrow-shape': edge => edge.data('targetEndShape') ?? 'none',
      'source-arrow-color': '#0060e6',
      'target-arrow-color': '#0060e6',
      'width': '2px',
      'source-label': (edge: cytoscape.EdgeSingular) => edge.data('sourceEndLabel') ?? '',
      'target-label': (edge: cytoscape.EdgeSingular) => edge.data('targetEndLabel') ?? '',
      'source-text-margin-y': -10,
      'target-text-margin-y': -10,
    },
  }]
}
