import { MemberOwner, ValueMemberDefinitionMap } from "../types";

/**
 * 空データ構造（空配列、空セクション等）のエッジケースを確認するためのテストデータ。
 */
export default function (): [MemberOwner, ValueMemberDefinitionMap] {
  return [{
    members: [
      // 通常のメンバー（比較用）
      {
        physicalName: "normalMember",
        displayName: "通常のメンバー",
        type: "text",
      },

      // 空のセクション
      {
        physicalName: "emptySection",
        displayName: "空のセクション",
        isSection: true,
        members: [],
      },

      // 空の配列
      {
        physicalName: "emptyArray",
        displayName: "空の配列",
        isArray: true,
        onCreateNewItem: () => ({ item: "" }),
        members: [],
      },

      // メンバーが1つだけのセクション
      {
        physicalName: "singleMemberSection",
        displayName: "単一メンバーセクション",
        isSection: true,
        members: [
          {
            physicalName: "singleMember",
            displayName: "単一メンバー",
            type: "text",
          },
        ],
      },

      // メンバーが1つだけの配列
      {
        physicalName: "singleMemberArray",
        displayName: "単一メンバー配列",
        isArray: true,
        onCreateNewItem: () => ({ singleItem: "" }),
        members: [
          {
            physicalName: "singleItem",
            displayName: "単一アイテム",
            type: "text",
          },
        ],
      },

      // ネストした空のセクション
      {
        physicalName: "nestedEmptySection",
        displayName: "ネストした空のセクション",
        isSection: true,
        members: [
          {
            physicalName: "outerMember",
            displayName: "外側メンバー",
            type: "text",
          },
          // 内側の空セクション
          {
            physicalName: "innerEmptySection",
            displayName: "内側の空セクション",
            isSection: true,
            members: [],
          },
          {
            physicalName: "anotherOuterMember",
            displayName: "もう一つの外側メンバー",
            type: "text",
          },
        ],
      },

      // ネストした空の配列
      {
        physicalName: "nestedEmptyArray",
        displayName: "ネストした空の配列",
        isSection: true,
        members: [
          {
            physicalName: "sectionMember",
            displayName: "セクションメンバー",
            type: "text",
          },
          // 内側の空配列
          {
            physicalName: "innerEmptyArray",
            displayName: "内側の空配列",
            isArray: true,
            onCreateNewItem: () => ({}),
            members: [],
          },
        ],
      },

      // 空セクションと空配列を含む配列
      {
        physicalName: "arrayWithEmptyMembers",
        displayName: "空メンバーを含む配列",
        isArray: true,
        onCreateNewItem: () => ({
          text: "",
          emptySection: {},
          emptyArray: []
        }),
        members: [
          {
            physicalName: "text",
            displayName: "テキスト",
            type: "text",
          },
          // 配列内の空セクション
          {
            physicalName: "emptySection",
            displayName: "配列内空セクション",
            isSection: true,
            members: [],
          },
          // 配列内の空配列
          {
            physicalName: "emptyArray",
            displayName: "配列内空配列",
            isArray: true,
            onCreateNewItem: () => ({}),
            members: [],
          },
        ],
      },

      // 複数レベルの空のネスト
      {
        physicalName: "multiLevelEmpty",
        displayName: "複数レベルの空ネスト",
        isSection: true,
        members: [
          {
            physicalName: "level1Text",
            displayName: "レベル1テキスト",
            type: "text",
          },
          {
            physicalName: "level2Section",
            displayName: "レベル2セクション",
            isSection: true,
            members: [
              {
                physicalName: "level2Text",
                displayName: "レベル2テキスト",
                type: "text",
              },
              // レベル3の空セクション
              {
                physicalName: "level3EmptySection",
                displayName: "レベル3空セクション",
                isSection: true,
                members: [],
              },
              // レベル3の空配列
              {
                physicalName: "level3EmptyArray",
                displayName: "レベル3空配列",
                isArray: true,
                onCreateNewItem: () => ({}),
                members: [],
              },
            ],
          },
        ],
      },

      // 無効なonCreateNewItem関数のテスト
      {
        physicalName: "invalidNewItemArray",
        displayName: "無効な新規アイテム配列",
        isArray: true,
        // 空オブジェクトを返すonCreateNewItem
        onCreateNewItem: () => ({}),
        members: [
          {
            physicalName: "item",
            displayName: "アイテム",
            type: "text",
          },
        ],
      },

      // null/undefinedを含む可能性のあるonCreateNewItem
      {
        physicalName: "nullishNewItemArray",
        displayName: "null可能性配列",
        isArray: true,
        onCreateNewItem: () => ({
          text: undefined,
          number: null,
          emptyString: "",
        }),
        members: [
          {
            physicalName: "text",
            displayName: "テキスト（undefined初期値）",
            type: "text",
          },
          {
            physicalName: "number",
            displayName: "数値（null初期値）",
            type: "number",
          },
          {
            physicalName: "emptyString",
            displayName: "空文字初期値",
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
          className="border border-gray-300 px-2 py-1 rounded w-full"
          placeholder="数値を入力してください"
        />
      ),
      getGridColumnDef: ({ member, cellType }) => {
        return cellType.number(member.physicalName, member.displayName ?? member.physicalName, {
          defaultWidth: 120,
        })
      },
    },
  }]
}
