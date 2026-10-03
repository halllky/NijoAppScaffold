/** 動作確認用に、オブジェクトを JSON で表示する */
export function ValuesPreview({ title, values }: { title: string, values: unknown }) {
  return (
    <div className="flex flex-col gap-1 min-w-0">
      <span className="text-sm text-gray-500 select-none">{title}</span>
      <pre className="p-2 text-xs bg-gray-100 rounded overflow-auto max-h-96">
        {values === undefined ? '（なし）' : JSON.stringify(values, null, 2)}
      </pre>
    </div>
  )
}
