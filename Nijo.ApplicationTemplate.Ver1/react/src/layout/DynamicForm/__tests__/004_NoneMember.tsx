import React from "react";
import { MemberOwner, ValueMemberDefinitionMap } from "../types";

/**
 * NoneMember（特定のプロパティとバインドされないメンバー）を確認するためのテストデータ。
 */
export default function (): [MemberOwner, ValueMemberDefinitionMap] {
  return [{
    members: [
      // 通常のテキストメンバー（比較用）
      {
        physicalName: "normalText",
        displayName: "通常のテキスト",
        type: "text",
      },

      // 静的な説明文を表示するNoneMember
      {
        displayName: "説明文",
        renderForm: () => (
          <div className="bg-blue-50 border border-blue-200 rounded p-3">
            <h4 className="text-blue-800 font-semibold mb-2">📋 重要な説明</h4>
            <p className="text-blue-700 text-sm">
              これは入力フィールドではなく、ユーザーに情報を提供するための静的なコンテンツです。
            </p>
          </div>
        ),
        getGridColumnDef: ({ cellType }) => cellType.other("説明", {
          renderCell: () => (
            <div className="text-blue-600 text-xs">
              📋 説明文
            </div>
          ),
          defaultWidth: 100,
        }),
      },

      // ボタンを表示するNoneMember
      {
        displayName: "アクションボタン",
        renderForm: ({ useFormReturn }) => (
          <div className="flex gap-2">
            <button
              type="button"
              onClick={() => {
                const currentValues = useFormReturn.getValues();
                alert(`現在の値: ${JSON.stringify(currentValues, null, 2)}`);
              }}
              className="bg-green-500 text-white px-3 py-1 rounded text-sm hover:bg-green-600"
            >
              🔍 値を確認
            </button>
            <button
              type="button"
              onClick={() => useFormReturn.reset()}
              className="bg-red-500 text-white px-3 py-1 rounded text-sm hover:bg-red-600"
            >
              🗑️ リセット
            </button>
          </div>
        ),
        getGridColumnDef: ({ cellType }) => cellType.other("アクション", {
          renderCell: (context) => (
            <div className="flex gap-1">
              <button
                type="button"
                onClick={() => alert(`行データ: ${JSON.stringify(context.row.original)}`)}
                className="bg-blue-500 text-white px-2 py-1 rounded text-xs"
              >
                詳細
              </button>
            </div>
          ),
          defaultWidth: 80,
        }),
      },

      // カスタムレイアウトのNoneMember
      {
        displayName: "カスタムレイアウト",
        renderForm: ({ useFormReturn: { watch } }) => {
          const normalTextValue = watch("normalText");
          return (
            <div className="border-2 border-purple-300 rounded-lg p-4 bg-purple-50">
              <h4 className="text-purple-800 font-semibold mb-2">🎨 動的コンテンツ</h4>
              <p className="text-purple-700 text-sm mb-2">
                この部分は他のフィールドの値に応じて表示が変わります。
              </p>
              <div className="bg-white rounded p-2 border border-purple-200">
                <span className="text-xs text-purple-600">normalTextの値: </span>
                <span className="font-mono text-sm">
                  {normalTextValue ? `"${normalTextValue}"` : '（未入力）'}
                </span>
              </div>
            </div>
          );
        },
        getGridColumnDef: ({ cellType }) => cellType.other("カスタム", {
          renderCell: (context) => (
            <div className="text-purple-600 text-xs">
              🎨 {context.row.original.normalText || '空'}
            </div>
          ),
          defaultWidth: 120,
        }),
      },

      // セクション内のNoneMember
      {
        physicalName: "sectionWithNone",
        displayName: "NoneMemberを含むセクション",
        isSection: true,
        members: [
          {
            physicalName: "sectionText",
            displayName: "セクション内テキスト",
            type: "text",
          },

          // セクション内の区切り線
          {
            displayName: "区切り線",
            renderForm: () => (
              <hr className="border-gray-300 my-4" />
            ),
            getGridColumnDef: ({ cellType }) => cellType.other("区切り", {
              renderCell: () => (
                <div className="text-gray-400 text-xs">
                  ─
                </div>
              ),
              defaultWidth: 50,
            }),
          },

          {
            physicalName: "sectionText2",
            displayName: "セクション内テキスト2",
            type: "text",
          },
        ],
      },

      // 配列内のNoneMember
      {
        physicalName: "arrayWithNone",
        displayName: "NoneMemberを含む配列",
        isArray: true,
        onCreateNewItem: () => ({ item: "" }),
        members: [
          {
            physicalName: "item",
            displayName: "アイテム",
            type: "text",
          },

          // 配列内の情報表示
          {
            displayName: "行情報",
            renderForm: ({ name }) => {
              // nameから行インデックスを取得
              const pathParts = name.split('.');
              const index = pathParts[pathParts.length - 1];
              return (
                <div className="text-xs text-gray-500 italic">
                  行番号: {index}
                </div>
              );
            },
            getGridColumnDef: ({ cellType }) => cellType.other("行情報", {
              renderCell: (context) => (
                <div className="text-gray-500 text-xs">
                  #{context.row.index + 1}
                </div>
              ),
              defaultWidth: 60,
            }),
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
