import React from "react";
import { DynamicForm, MemberOwner, DynamicFormRef } from "../layout";
import { Allotment, LayoutPriority } from "allotment";

export default function DynamicFormDebugging({ getSchema }: {
  getSchema: () => MemberOwner
}) {
  // スキーマ定義
  const schema = React.useMemo(() => {
    return getSchema()
  }, [getSchema])

  // 定期的に値をJSONにして表示
  const formRef = React.useRef<DynamicFormRef>(null)
  const [valueJson, setValueJson] = React.useState<string>("LOADING...")
  React.useEffect(() => {
    const interval = setInterval(() => {
      setValueJson(JSON.stringify(formRef.current?.useFormReturn.getValues(), null, 2))
    }, 500)
    return () => clearInterval(interval)
  }, [])

  return (
    <Allotment
      separator={false}
      proportionalLayout={false}
      className="flex-1"
    >

      {/* フォーム */}
      <Allotment.Pane className="px-1">
        <div className="h-full overflow-y-scroll">
          <DynamicForm
            ref={formRef}
            root={schema}
          />
        </div>
      </Allotment.Pane>

      {/* フォームの現在の値のJSON */}
      <Allotment.Pane preferredSize={320} priority={LayoutPriority.Low} className="px-1">
        <pre className="h-full p-px overflow-y-scroll text-sm text-white bg-gray-700">
          {valueJson}
        </pre>
      </Allotment.Pane>
    </Allotment>
  )
}