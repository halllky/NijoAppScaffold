import React from "react"
import * as ReactHookForm from "react-hook-form"
import { ATTR_KEY_PHYSICAL_NAME, type EditingProject } from "../../features/backend"

/**
 * ルート集約1個分のオプショナル属性の入力欄。
 * 属性の一覧と、どの属性がどの入力形式かは、サーバーから受け取った定義が正となる。
 * 物理名は他の項目と並べて常に表示したい項目なので、ここには含めない。
 * 属性の数が多いため、既定では折りたたんだ状態で表示する。
 */
export function OptionalAttrsForm({ path }: {
  /** 編集対象のルート集約のパス */
  path: `rootAggregates.${number}.root`
}) {

  const { register, control } = ReactHookForm.useFormContext<EditingProject>()
  const attrDefs = ReactHookForm.useWatch({ control, name: "optionalAttributes" })

  return (
    <details className="col-span-2">
      <summary className="text-sm text-gray-600 select-none cursor-pointer">
        属性
      </summary>

      {/* 属性ごとの入力欄 */}
      <div className="max-h-64 overflow-y-auto grid grid-cols-[auto_1fr] items-center gap-x-2 gap-y-1 pt-1 pl-4 bg-gray-100">
        {attrDefs.filter(def => def.key !== ATTR_KEY_PHYSICAL_NAME).map(def => {
          const attrPath = `${path}.attrs.${def.key}` as ReactHookForm.Path<EditingProject>
          const id = `${path}.attrs.${def.key}`

          return (
            <React.Fragment key={def.key}>
              <label htmlFor={id} title={def.helpText ?? undefined} className="text-gray-600">
                {def.displayName ?? def.key}
              </label>

              {def.type === "boolean" ? (
                <input {...register(attrPath)} id={id} type="checkbox" className="bg-white justify-self-start" />
              ) : def.type === "select" ? (
                <select {...register(attrPath)} id={id} className="bg-white justify-self-start px-1 border border-gray-300">
                  <option value=""></option>
                  {(def.selectOptions ?? []).map(value => (
                    <option key={value} value={value}>{value}</option>
                  ))}
                </select>
              ) : (
                <input
                  {...register(attrPath)}
                  id={id}
                  type={def.type === "number" ? "number" : "text"}
                  spellCheck={false}
                  autoComplete="off"
                  className="px-1 border border-gray-300 bg-white"
                />
              )}
            </React.Fragment>
          )
        })}
      </div>
    </details>
  )
}
