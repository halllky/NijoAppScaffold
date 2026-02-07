import cytoscape from 'cytoscape';
// @ts-ignore このライブラリは型定義を提供していない
import nodeHtmlLabel from "cytoscape-node-html-label";

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
      tpl: (data: { label: string | undefined }) => {
        const label = data.label || ''
        // 改行文字（\n または \r\n）を<br>タグに変換
        const htmlLabel = label.replace(/\r\n|\n/g, '<br>')
        return `<div style="pointer-events: none; max-width: 320px;">${htmlLabel}</div>`
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
        const maxTextLength = estimateTextWidth(node.data('label') as string)
        return Math.min(320, Math.max(32, maxTextLength + 8))
      },
      'height': (node: cytoscape.NodeSingular) => {
        // max-widthを考慮してテキストの実際の行数を計算
        const label = (node.data('label') as string) || ''
        const lines = calculateTextLines(label, 320) // HTMLラベルのmax-widthと同じ値
        const lineHeight = 16 * 1.2 // フォントサイズ16px × line-height 1.2
        return Math.max(32, lines * lineHeight + 8) // 最小32px、上下パディング8px
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
      'border-style': 'dashed',
      'border-width': '1px',
      'border-color': node => (node.data('border-color:selected') as string) ?? '#FF4F02',
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
      'color': '#FF4F02',
      'line-color': '#FF4F02',
      'line-style': 'dashed',
      'source-arrow-shape': edge => edge.data('sourceEndShape') ?? 'none',
      'target-arrow-shape': edge => edge.data('targetEndShape') ?? 'none',
      'source-arrow-color': '#FF4F02',
      'target-arrow-color': '#FF4F02',
      'width': '2px',
      'source-label': (edge: cytoscape.EdgeSingular) => edge.data('sourceEndLabel') ?? '',
      'target-label': (edge: cytoscape.EdgeSingular) => edge.data('targetEndLabel') ?? '',
      'source-text-margin-y': -10,
      'target-text-margin-y': -10,
    },
  }]
}
