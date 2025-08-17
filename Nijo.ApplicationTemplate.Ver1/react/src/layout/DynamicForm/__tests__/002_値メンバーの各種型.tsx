import { MemberOwner, ValueMember } from "../types";

/**
 * 様々な値メンバー型（text, number, date, boolean, other）を確認するためのテストデータ。
 */
export default function (): MemberOwner {

  // 複数回使いまわすUIレンダリング定義
  const UI_TEXT = (physicalName: string): Partial<ValueMember> => ({
    renderFormValue: ({ useFormReturn: { register }, name }) => (
      <input
        type="text"
        {...register(name)}
        className="border border-gray-300 px-2 py-1 rounded"
        placeholder="テキストを入力してください"
      />
    ),
    getGridColumnDef: ({ member, cellType }) => {
      return cellType.text(physicalName, member.displayName ?? physicalName, {
        defaultWidth: 150,
      })
    },
  })

  const UI_NUMBER = (physicalName: string): Partial<ValueMember> => ({
    renderFormValue: ({ useFormReturn: { register }, name }) => (
      <input
        type="number"
        {...register(name, { valueAsNumber: true })}
        className="border border-gray-300 px-2 py-1 rounded"
        placeholder="数値を入力してください"
      />
    ),
    getGridColumnDef: ({ member, cellType }) => {
      return cellType.number(physicalName, member.displayName ?? physicalName, {
        defaultWidth: 120,
      })
    },
  })

  const UI_DATE = (physicalName: string): Partial<ValueMember> => ({
    renderFormValue: ({ useFormReturn: { register }, name }) => (
      <input
        type="date"
        {...register(name)}
        className="border border-gray-300 px-2 py-1 rounded"
      />
    ),
    getGridColumnDef: ({ member, cellType }) => {
      return cellType.date(physicalName, member.displayName ?? physicalName, {
        defaultWidth: 140,
      })
    },
  })

  const UI_BOOLEAN = (physicalName: string): Partial<ValueMember> => ({
    renderFormValue: ({ useFormReturn: { register }, name }) => (
      <label className="flex items-center gap-2">
        <input
          type="checkbox"
          {...register(name)}
          className="rounded"
        />
        <span>チェックする</span>
      </label>
    ),
    getGridColumnDef: ({ member, cellType }) => {
      return cellType.boolean(physicalName, member.displayName ?? physicalName, {
        defaultWidth: 80,
      })
    },
  })

  const UI_SELECT = (physicalName: string): Partial<ValueMember> => ({
    renderFormValue: ({ useFormReturn: { register }, name }) => (
      <select
        {...register(name)}
        className="border border-gray-300 px-2 py-1 rounded"
      >
        <option value="">選択してください</option>
        <option value="option1">選択肢1</option>
        <option value="option2">選択肢2</option>
        <option value="option3">選択肢3</option>
      </select>
    ),
    getGridColumnDef: ({ member, cellType }) => {
      return cellType.text(physicalName, member.displayName ?? physicalName, {
        defaultWidth: 130,
        getOptions: () => [
          { value: "option1", label: "選択肢1" },
          { value: "option2", label: "選択肢2" },
          { value: "option3", label: "選択肢3" },
        ]
      })
    },
  })

  const UI_TEXTAREA = (physicalName: string): Partial<ValueMember> => ({
    fullWidth: true,
    renderFormValue: ({ useFormReturn: { register }, name }) => (
      <textarea
        {...register(name)}
        className="border border-gray-300 px-2 py-1 rounded w-full"
        placeholder="複数行のテキストを入力してください"
        rows={3}
      />
    ),
    getGridColumnDef: ({ member, cellType }) => {
      return cellType.text(physicalName, member.displayName ?? physicalName, {
        defaultWidth: 200,
        editorOverflow: 'vertical',
      })
    },
  })

  // データ構造定義
  return {
    members: [
      // 基本的な値メンバー
      {
        physicalName: "textMember",
        displayName: "テキストメンバー",
        ...UI_TEXT("textMember"),
      },
      {
        physicalName: "numberMember",
        displayName: "数値メンバー",
        ...UI_NUMBER("numberMember"),
      },
      {
        physicalName: "dateMember",
        displayName: "日付メンバー",
        ...UI_DATE("dateMember"),
      },
      {
        physicalName: "booleanMember",
        displayName: "真偽値メンバー",
        ...UI_BOOLEAN("booleanMember"),
      },
      {
        physicalName: "selectMember",
        displayName: "選択メンバー",
        ...UI_SELECT("selectMember"),
      },
      {
        physicalName: "textareaMember",
        displayName: "テキストエリアメンバー",
        ...UI_TEXTAREA("textareaMember"),
      },

      // セクション内の様々な型
      {
        physicalName: "typeSection",
        displayName: "型のセクション",
        isSection: true,
        members: [
          {
            physicalName: "nestedText",
            displayName: "ネストされたテキスト",
            ...UI_TEXT("nestedText"),
          },
          {
            physicalName: "nestedNumber",
            displayName: "ネストされた数値",
            ...UI_NUMBER("nestedNumber"),
          },
          {
            physicalName: "nestedBoolean",
            displayName: "ネストされた真偽値",
            ...UI_BOOLEAN("nestedBoolean"),
          },
        ],
      },

      // 配列内の様々な型
      {
        physicalName: "typeArray",
        displayName: "型の配列",
        isArray: true,
        onCreateNewItem: () => ({
          arrayText: "",
          arrayNumber: 0,
          arrayDate: "",
          arrayBoolean: false,
        }),
        members: [
          {
            physicalName: "arrayText",
            displayName: "配列テキスト",
            ...UI_TEXT("arrayText"),
          },
          {
            physicalName: "arrayNumber",
            displayName: "配列数値",
            ...UI_NUMBER("arrayNumber"),
          },
          {
            physicalName: "arrayDate",
            displayName: "配列日付",
            ...UI_DATE("arrayDate"),
          },
          {
            physicalName: "arrayBoolean",
            displayName: "配列真偽値",
            ...UI_BOOLEAN("arrayBoolean"),
          },
        ],
      },
    ],
  }
}
