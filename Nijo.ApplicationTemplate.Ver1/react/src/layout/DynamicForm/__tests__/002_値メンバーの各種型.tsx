import React from "react";
import { MemberOwner, ValueMemberDefinitionMap } from "../types";

/**
 * 様々な値メンバー型（text, number, date, boolean, other）を確認するためのテストデータ。
 */
export default function (): [MemberOwner, ValueMemberDefinitionMap] {
  return [{
    members: [
      // 基本的な値メンバー
      {
        physicalName: "textMember",
        displayName: "テキストメンバー",
        type: "text",
      },
      {
        physicalName: "numberMember",
        displayName: "数値メンバー",
        type: "number",
      },
      {
        physicalName: "dateMember",
        displayName: "日付メンバー",
        type: "date",
      },
      {
        physicalName: "booleanMember",
        displayName: "真偽値メンバー",
        type: "boolean",
      },
      {
        physicalName: "selectMember",
        displayName: "選択メンバー",
        type: "select",
      },
      {
        physicalName: "textareaMember",
        displayName: "テキストエリアメンバー",
        type: "textarea",
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
            type: "text",
          },
          {
            physicalName: "nestedNumber",
            displayName: "ネストされた数値",
            type: "number",
          },
          {
            physicalName: "nestedBoolean",
            displayName: "ネストされた真偽値",
            type: "boolean",
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
            type: "text",
          },
          {
            physicalName: "arrayNumber",
            displayName: "配列数値",
            type: "number",
          },
          {
            physicalName: "arrayDate",
            displayName: "配列日付",
            type: "date",
          },
          {
            physicalName: "arrayBoolean",
            displayName: "配列真偽値",
            type: "boolean",
          },
        ],
      },
    ],
  },

  // メンバー種類定義
  {
    text: {
      renderForm: ({ useFormReturn: { register }, name }) => (
        <input
          type="text"
          {...register(name)}
          className="border border-gray-300 px-2 py-1 rounded"
          placeholder="テキストを入力してください"
        />
      ),
      getGridColumnDef: ({ member, cellType }) => {
        return cellType.text(member.physicalName, member.displayName ?? member.physicalName, {
          defaultWidth: 150,
        })
      },
    },

    number: {
      renderForm: ({ useFormReturn: { register }, name }) => (
        <input
          type="number"
          {...register(name, { valueAsNumber: true })}
          className="border border-gray-300 px-2 py-1 rounded"
          placeholder="数値を入力してください"
        />
      ),
      getGridColumnDef: ({ member, cellType }) => {
        return cellType.number(member.physicalName, member.displayName ?? member.physicalName, {
          defaultWidth: 120,
        })
      },
    },

    date: {
      renderForm: ({ useFormReturn: { register }, name }) => (
        <input
          type="date"
          {...register(name)}
          className="border border-gray-300 px-2 py-1 rounded"
        />
      ),
      getGridColumnDef: ({ member, cellType }) => {
        return cellType.date(member.physicalName, member.displayName ?? member.physicalName, {
          defaultWidth: 140,
        })
      },
    },

    boolean: {
      renderForm: ({ useFormReturn: { register }, name }) => (
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
        return cellType.boolean(member.physicalName, member.displayName ?? member.physicalName, {
          defaultWidth: 80,
        })
      },
    },

    select: {
      renderForm: ({ useFormReturn: { register }, name }) => (
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
        return cellType.text(member.physicalName, member.displayName ?? member.physicalName, {
          defaultWidth: 130,
          getOptions: () => [
            { value: "option1", label: "選択肢1" },
            { value: "option2", label: "選択肢2" },
            { value: "option3", label: "選択肢3" },
          ]
        })
      },
    },

    textarea: {
      renderForm: ({ useFormReturn: { register }, name }) => (
        <textarea
          {...register(name)}
          className="border border-gray-300 px-2 py-1 rounded min-h-20"
          placeholder="複数行のテキストを入力してください"
          rows={3}
        />
      ),
      getGridColumnDef: ({ member, cellType }) => {
        return cellType.text(member.physicalName, member.displayName ?? member.physicalName, {
          defaultWidth: 200,
          editorOverflow: 'vertical',
        })
      },
    },
  }]
}
