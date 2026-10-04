import React from "react"
import * as RHF from "react-hook-form"
import * as EG2 from "@halllky/editable-grid"
import { findMemberMetadata, type GridBinding } from "../form/FormBinding"
import { assertValueMember, toRegisterRules, type ColumnOptionsBase, type InputPropsBase, type WithFormBinding } from "./InputProps"
import { defineCellColumn, findCellMemberMetadata, toCellFormPath } from "./GridCell"

/*
 * 真偽値の入力欄。
 * フォーム用とグリッド用で以下の仕様を共有する。
 *
 * - クリックまたはスペースキーで値を切り替える。
 * - 値が未設定（null / undefined）の場合は false として表示する。
 * - コピー&ペーストの文字列表現は "true" / "false"。それ以外の文字列の貼り付けは無視する。
 * - 検索条件のフォームでは、「該当する」「該当しない」の2つのチェックボックスになる。
 *   どちらか一方だけを選ぶとその値で絞り込み、両方または何も選ばなければ絞り込まない。
 */

//#region フォーム用

export type CheckBoxProps<TValues extends RHF.FieldValues> = InputPropsBase<TValues> & {
  /** チェックボックスの右側に表示する文字列。未指定の場合はチェックボックスだけを表示する */
  children?: React.ReactNode
}

/** 真偽値の入力欄（フォーム用） */
export function CheckBox<TValues extends RHF.FieldValues>(props: WithFormBinding<CheckBoxProps<TValues>, TValues>): React.ReactNode {
  const { binding, name, isReadOnly, rules, className, children } = props
  const formPath = binding.toFormPath(name)
  assertValueMember(findMemberMetadata(binding.metadata, name), name, 'CheckBox', member => member.type === 'bool')

  // 画面表示用データでは1つのチェックボックス
  if (binding.kind === 'display-data') {
    return (
      <div className={`flex items-center min-h-6 ${className ?? ''}`}>
        <SingleCheckBox
          formPath={formPath}
          control={binding.formMethods.control}
          rules={toRegisterRules({ rules })}
          isReadOnly={isReadOnly}
        >
          {children}
        </SingleCheckBox>
      </div>
    )
  }

  // 検索条件では「該当する」「該当しない」の2つ。項目全体に対する検証は1つ目に付ける
  return (
    <div className={`flex flex-wrap items-center gap-x-3 min-h-6 ${className ?? ''}`}>
      <SingleCheckBox
        formPath={`${formPath}.trueのみ`}
        control={binding.formMethods.control}
        rules={toRegisterRules({ rules, itemValueOf: (_, formValues) => RHF.get(formValues, formPath) })}
        isReadOnly={isReadOnly}
      >
        該当する
      </SingleCheckBox>
      <SingleCheckBox
        formPath={`${formPath}.falseのみ`}
        control={binding.formMethods.control}
        rules={toRegisterRules({ rules: undefined })}
        isReadOnly={isReadOnly}
      >
        該当しない
      </SingleCheckBox>
    </div>
  )
}

/** 1つのチェックボックスと、その右側の文字列 */
function SingleCheckBox({ formPath, control, rules, isReadOnly, children }: {
  formPath: string
  control: RHF.Control<RHF.FieldValues>
  rules: Pick<RHF.RegisterOptions, 'validate'>
  isReadOnly: boolean | undefined
  children?: React.ReactNode
}) {
  const { field } = RHF.useController({ name: formPath, control, rules })

  return (
    <label className={`inline-flex items-center gap-1 select-none ${isReadOnly ? '' : 'cursor-pointer'}`}>
      <input
        type="checkbox"
        ref={field.ref}
        name={field.name}
        checked={field.value === true}
        // チェックボックスには readOnly 属性が効かないので、読み取り専用のときは値を変えないようにする
        onChange={e => { if (!isReadOnly) field.onChange(e.target.checked) }}
        onBlur={field.onBlur}
        aria-readonly={isReadOnly}
        className={isReadOnly ? 'pointer-events-none' : 'cursor-pointer'}
      />
      {children}
    </label>
  )
}

//#endregion フォーム用

//#region グリッド用

export type CheckBoxColumnOptions<TRow> = ColumnOptionsBase<TRow>

/**
 * 真偽値の列（グリッド用）。セルエディタは使わず、セルの選択中のスペースキーとクリックで値を切り替える。
 * コピー&ペーストのために、セルエディタが無くても文字列との変換は定義する。
 */
export function checkBoxColumn<TRow>(
  binding: GridBinding,
  path: RHF.Path<TRow>,
  options?: CheckBoxColumnOptions<TRow>,
): EG2.EditableGridLeafColumn<TRow> {
  assertValueMember(findCellMemberMetadata(binding, path), path, 'checkBox 列', member => member.type === 'bool')

  const toggle = (rowKey: string) => {
    const cellFormPath = toCellFormPath(binding, rowKey, path)
    if (cellFormPath === undefined) return
    const current = binding.form.formMethods.getValues(cellFormPath) === true
    binding.form.formMethods.setValue(cellFormPath, !current, { shouldDirty: true })
  }

  return defineCellColumn(binding, path, options, {
    render: ({ value, rowKey, isReadOnly }) => (
      // セル全体を label にして、セルのどこをクリックしても切り替わるようにする
      <label className={`flex items-start w-full h-full ${isReadOnly ? '' : 'cursor-pointer'}`}>
        <span>
          <input
            type="checkbox"
            tabIndex={-1}
            checked={value === true}
            onChange={() => toggle(rowKey)}
            disabled={isReadOnly}
            className={isReadOnly ? '' : 'cursor-pointer'}
          />
          {/* 行の高さを文字のセルとそろえるため */}
          &nbsp;
        </span>
      </label>
    ),
    toText: value => value === true ? 'true' : 'false',
    fromText: text => {
      const parsed = parseBoolean(text)
      return parsed === undefined ? undefined : { value: parsed }
    },
    onCellKeyDown: ({ row, rowIndex, rowKey, event }) => {
      if (event.key !== ' ' && event.code !== 'Space') return
      // スペースキーをそのままにするとキー入力による編集開始になるので止める
      event.preventDefault()
      // キー操作には読み取り専用かどうかが渡されないので、ここで分かる列単位の指定だけを見る
      const isReadOnly = typeof options?.isReadOnly === 'function'
        ? options.isReadOnly(row as TRow, rowIndex, rowKey)
        : options?.isReadOnly === true
      if (!isReadOnly) toggle(rowKey)
    },
  })
}

//#endregion グリッド用

//#region フォームとグリッドで共有する仕様

/** 貼り付けられた文字列を真偽値として解釈する。解釈できない場合は undefined */
function parseBoolean(text: string): boolean | undefined {
  const normalized = text.trim().toLowerCase()
  if (normalized === 'true') return true
  if (normalized === 'false') return false
  return undefined
}

//#endregion フォームとグリッドで共有する仕様
