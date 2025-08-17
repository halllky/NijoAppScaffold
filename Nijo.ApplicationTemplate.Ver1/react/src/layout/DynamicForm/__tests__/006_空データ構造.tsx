import { MemberOwner, ValueMember } from "../types";

/**
 * 空データ構造（空配列、空セクション等）のエッジケースを確認するためのテストデータ。
 */
export default function (): MemberOwner {

  // 複数回使いまわすUIレンダリング定義
  const UI_TEXT = (physicalName: string): Partial<ValueMember> => ({
    renderFormValue: ({ useFormReturn: { register }, name }) => (
      <input
        type="text"
        {...register(name)}
        className="border border-gray-300 px-2 py-1 rounded w-full"
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
        className="border border-gray-300 px-2 py-1 rounded w-full"
        placeholder="数値を入力してください"
      />
    ),
    getGridColumnDef: ({ member, cellType }) => {
      return cellType.number(physicalName, member.displayName ?? physicalName, {
        defaultWidth: 120,
      })
    },
  })

  // データ構造定義
  return {
    members: [
      // 通常のメンバー（比較用）
      {
        physicalName: "normalMember",
        displayName: "通常のメンバー",
        ...UI_TEXT("normalMember"),
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
            ...UI_TEXT("singleMember"),
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
            ...UI_TEXT("singleItem"),
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
            ...UI_TEXT("outerMember"),
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
            ...UI_TEXT("anotherOuterMember"),
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
            ...UI_TEXT("sectionMember"),
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
            ...UI_TEXT("text"),
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
            ...UI_TEXT("level1Text"),
          },
          {
            physicalName: "level2Section",
            displayName: "レベル2セクション",
            isSection: true,
            members: [
              {
                physicalName: "level2Text",
                displayName: "レベル2テキスト",
                ...UI_TEXT("level2Text"),
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
            ...UI_TEXT("item"),
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
            ...UI_TEXT("text"),
          },
          {
            physicalName: "number",
            displayName: "数値（null初期値）",
            ...UI_NUMBER("number"),
          },
          {
            physicalName: "emptyString",
            displayName: "空文字初期値",
            ...UI_TEXT("emptyString"),
          },
        ],
      },
    ],
  }
}
