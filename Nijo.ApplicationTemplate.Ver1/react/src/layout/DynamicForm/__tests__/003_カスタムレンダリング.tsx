import React from "react";
import { DynamicFormProps, ValueMember } from "../types";

/**
 * カスタムレンダリング機能（renderForm, renderFormLabel等）を確認するためのテストデータ。
 */
export default function (): DynamicFormProps {

  // 複数回使いまわすUIレンダリング定義
  const UI_TEXT = (physicalName: string): Partial<ValueMember> => ({
    renderFormValue: ({ useFormReturn: { register }, name }) => (
      <input
        type="text"
        {...register(name)}
        className="border border-gray-300 px-2 py-1 rounded w-full"
      />
    ),
    getGridColumnDef: ({ member, cellType }) => {
      return cellType.text(physicalName, member.displayName ?? physicalName, {
        defaultWidth: 150,
      })
    },
  })

  // データ構造定義
  return {
    labelWidthPx: 180,
    root: {
      members: [
        // カスタムラベルレンダリングを持つ値メンバー
        {
          physicalName: "customLabelMember",
          fullWidth: true,
          renderFormLabel: ({ name }) => (
            <div className="flex flex-col items-start gap-2">
              <span className="text-blue-600 font-bold">📝 カスタムラベルメンバー</span>
              <span className="text-xs text-gray-500">({name})</span>
            </div>
          ),
          ...UI_TEXT("customLabelMember"),
        },

        // カスタムフォームレンダリングを持つ値メンバー
        {
          physicalName: "customFormMember",
          displayName: "カスタムフォームメンバー",
          renderFormValue: ({ useFormReturn: { register, watch }, name }) => {
            const value = watch(name);
            return (
              <div className="border border-blue-300 rounded p-2">
                <input
                  type="text"
                  {...register(name)}
                  className="w-full border-none outline-none"
                  placeholder="カスタムフォームメンバーを入力してください"
                />
                <div className="text-xs text-gray-500 mt-1">
                  現在の値: {String(value || '（未入力）')}
                </div>
              </div>
            );
          },
          getGridColumnDef: ({ member, cellType }) => {
            return cellType.text("customFormMember", member.displayName ?? "customFormMember", {
              defaultWidth: 150,
            })
          },
        },

        // フルワイドオプションの値メンバー
        {
          physicalName: "fullWidthMember",
          displayName: "フルワイドメンバー",
          fullWidth: true,
          ...UI_TEXT("fullWidthMember"),
        },

        // カスタムレンダリングを持つセクション
        {
          physicalName: "customSection",
          displayName: "カスタムセクション",
          type: 'section',
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
              ...UI_TEXT("nestedMember1"),
            },
            {
              physicalName: "nestedMember2",
              displayName: "ネストメンバー2",
              ...UI_TEXT("nestedMember2"),
            },
          ],
        },

        // カスタムラベルレンダリングを持つセクション
        {
          physicalName: "customLabelSection",
          displayName: "カスタムラベルセクション",
          type: 'section',
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
              ...UI_TEXT("sectionMember1"),
            },
          ],
        },

        // カスタムレンダリングを持つ配列
        {
          physicalName: "customArray",
          displayName: "カスタム配列",
          type: 'array',
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
              ...UI_TEXT("arrayItem"),
            },
          ],
        },

        // カスタムラベルレンダリングを持つ配列
        {
          physicalName: "customLabelArray",
          displayName: "カスタムラベル配列",
          type: 'array',
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
              ...UI_TEXT("item"),
            },
          ],
        },
      ],
    },
  }
}
