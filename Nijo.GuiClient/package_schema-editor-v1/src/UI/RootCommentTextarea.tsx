import * as ReactHookForm from "react-hook-form"
import { EditingProject, SectionName } from "../backend"
import { MentionableTextarea } from "./Mention"
import { useMentionSuggestions } from "./useMentionSuggestions"

/**
 * ルート要素（データ構造のルート集約、静的区分、定数やテンプレート文字列のグループなど）のコメントの入力欄。
 * nijo.xml ではルート要素の直前のコメントになり、生成されるソースのドキュメントコメントにも出る。
 * メンションで他の要素を参照できる。
 */
export function RootCommentTextarea({ section, index, formMethods: { control, getValues }, onChange, className }: {
  section: SectionName
  index: number
  formMethods: Pick<ReactHookForm.UseFormReturn<EditingProject>, 'control' | 'getValues'>
  /** 入力内容が変わったときに、フォームの値の更新に加えて行う処理 */
  onChange?: (value: string) => void
  className?: string
}) {
  const getMentionSuggestions = useMentionSuggestions(getValues)

  return (
    <ReactHookForm.Controller
      control={control}
      name={`${section}.${index}.comment`}
      render={({ field }) => (
        <div className={`max-h-64 overflow-auto border border-gray-700 px-1 bg-white ${className ?? ''}`}>
          <MentionableTextarea
            {...field}
            value={field.value ?? undefined}
            onChange={value => {
              field.onChange(value)
              onChange?.(value)
            }}
            getSuggestions={getMentionSuggestions}
            className="w-full"
            placeholder="コメントを入力..."
          />
        </div>
      )}
    />
  )
}
