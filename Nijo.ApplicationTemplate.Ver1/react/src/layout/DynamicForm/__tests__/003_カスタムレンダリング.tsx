import React from "react";
import { MemberOwner, ValueMemberDefinitionMap } from "../types";

/**
 * カスタムレンダリング機能（renderForm, renderFormLabel等）を確認するためのテストデータ。
 */
export default function (): [MemberOwner, ValueMemberDefinitionMap] {
  return [{
    members: [
      // カスタムラベルレンダリングを持つ値メンバー
      {
        physicalName: "customLabelMember",
        displayName: "カスタムラベルメンバー",
        type: "text",
        fullWidth: true,
        renderFormLabel: ({ name, member }) => (
          <div className="flex items-center gap-2">
            <span className="text-blue-600 font-bold">📝 {member.displayName}</span>
            <span className="text-xs text-gray-500">({name})</span>
          </div>
        ),
      },

      // カスタムフォームレンダリングを持つ値メンバー
      {
        physicalName: "customFormMember",
        displayName: "カスタムフォームメンバー",
        type: "text",
        renderFormValue: ({ useFormReturn: { register, watch }, name, member }) => {
          const value = watch(name);
          return (
            <div className="border border-blue-300 rounded p-2">
              <input
                type="text"
                {...register(name)}
                className="w-full border-none outline-none"
                placeholder={`${member.displayName}を入力してください`}
              />
              <div className="text-xs text-gray-500 mt-1">
                現在の値: {String(value || '（未入力）')}
              </div>
            </div>
          );
        },
      },

      // フルワイドオプションの値メンバー
      {
        physicalName: "fullWidthMember",
        displayName: "フルワイドメンバー",
        type: "text",
        fullWidth: true,
      },

      // カスタムレンダリングを持つセクション
      {
        physicalName: "customSection",
        displayName: "カスタムセクション",
        isSection: true,
        render: ({ useFormReturn, name, owner }) => (
          <div className="border-2 border-purple-300 rounded-lg p-4 bg-purple-50">
            <h3 className="text-lg font-bold text-purple-800 mb-3">
              🎨 カスタムセクション
            </h3>
            <div className="text-sm text-purple-600 mb-2">
              このセクションはカスタムレンダリングされています
            </div>
            {/* 既定のレンダリングロジックは別途実装されると仮定 */}
            <div className="text-xs text-gray-500">
              セクション名: {name}
            </div>
          </div>
        ),
        members: [
          {
            physicalName: "nestedMember1",
            displayName: "ネストメンバー1",
            type: "text",
          },
          {
            physicalName: "nestedMember2",
            displayName: "ネストメンバー2",
            type: "text",
          },
        ],
      },

      // カスタムラベルレンダリングを持つセクション
      {
        physicalName: "customLabelSection",
        displayName: "カスタムラベルセクション",
        isSection: true,
        renderFormLabel: ({ name, owner }) => (
          <div className="flex items-center gap-2 bg-green-100 px-3 py-2 rounded">
            <span className="text-green-800 font-semibold">🏷️ カスタムラベルセクション</span>
            <span className="text-xs text-green-600">({name})</span>
          </div>
        ),
        members: [
          {
            physicalName: "sectionMember1",
            displayName: "セクションメンバー1",
            type: "text",
          },
        ],
      },

      // カスタムレンダリングを持つ配列
      {
        physicalName: "customArray",
        displayName: "カスタム配列",
        isArray: true,
        onCreateNewItem: () => ({ arrayItem: "" }),
        render: ({ useFormReturn, name, useFieldArrayReturn }) => (
          <div className="border-2 border-orange-300 rounded-lg p-4 bg-orange-50">
            <div className="flex justify-between items-center mb-3">
              <h3 className="text-lg font-bold text-orange-800">
                🗂️ カスタム配列
              </h3>
              <button
                type="button"
                onClick={() => useFieldArrayReturn.append({ arrayItem: "" })}
                className="bg-orange-500 text-white px-3 py-1 rounded text-sm hover:bg-orange-600"
              >
                + 追加
              </button>
            </div>
            <div className="text-sm text-orange-600 mb-2">
              この配列はカスタムレンダリングされています
            </div>
            <div className="text-xs text-gray-500">
              配列名: {name}, アイテム数: {useFieldArrayReturn.fields.length}
            </div>
          </div>
        ),
        members: [
          {
            physicalName: "arrayItem",
            displayName: "配列アイテム",
            type: "text",
          },
        ],
      },

      // カスタムラベルレンダリングを持つ配列
      {
        physicalName: "customLabelArray",
        displayName: "カスタムラベル配列",
        isArray: true,
        onCreateNewItem: () => ({ item: "" }),
        renderFormLabel: ({ name, useFieldArrayReturn }) => (
          <div className="flex items-center justify-between bg-red-100 px-3 py-2 rounded">
            <div className="flex items-center gap-2">
              <span className="text-red-800 font-semibold">📋 カスタムラベル配列</span>
              <span className="text-xs text-red-600">({name})</span>
            </div>
            <span className="text-xs text-red-500">
              {useFieldArrayReturn.fields.length} 件
            </span>
          </div>
        ),
        members: [
          {
            physicalName: "item",
            displayName: "アイテム",
            type: "text",
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
          className="border border-gray-300 px-2 py-1 rounded w-full"
        />
      ),
      getGridColumnDef: ({ member, cellType }) => {
        return cellType.text(member.physicalName, member.displayName ?? member.physicalName, {
          defaultWidth: 150,
        })
      },
    },
  }]
}
