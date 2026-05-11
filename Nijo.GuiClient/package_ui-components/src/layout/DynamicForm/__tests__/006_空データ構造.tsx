import { DynamicFormProps, ValueMember } from "../types";

/**
 * 空データ構造（空配列、空セクション等）のエッジケースを確認するためのテストデータ。
 */
export default function (): DynamicFormProps {

  // 複数回使いまわすUIレンダリング定義
  const UI_TEXT = (physicalName: string): Partial<ValueMember> => ({
    contents: ({ useFormReturn: { register }, name }) => (
      <input
        type="text"
        {...register(name)}
        className="border border-gray-300 px-2 py-1 rounded w-full"
        placeholder="テキストを入力してください"
      />
    ),
    getGridColumnDef: ({ member, cellType }) => {
      return cellType.text(physicalName, typeof member.label === 'string' ? member.label : physicalName, {
        defaultWidth: 150,
      })
    },
  })

  const UI_NUMBER = (physicalName: string): Partial<ValueMember> => ({
    contents: ({ useFormReturn: { register }, name }) => (
      <input
        type="number"
        {...register(name, { valueAsNumber: true })}
        className="border border-gray-300 px-2 py-1 rounded w-full"
        placeholder="数値を入力してください"
      />
    ),
    getGridColumnDef: ({ member, cellType }) => {
      return cellType.number(physicalName, typeof member.label === 'string' ? member.label : physicalName, {
        defaultWidth: 120,
      })
    },
  })

  // データ構造定義
  return {
    root: {
      members: [
        // 通常のメンバー（比較用）
        {
          physicalName: "normalMember",
          label: "通常のメンバー",
          ...UI_TEXT("normalMember"),
        },

        // 空のセクション
        {
          physicalName: "emptySection",
          label: "空のセクション",
          type: 'section',
          members: [],
        },

        // 空の配列
        {
          physicalName: "emptyArray",
          arrayLabel: "空の配列",
          type: 'array',
          onCreateNewItem: () => ({ item: "" }),
          members: [],
        },

        // メンバーが1つだけのセクション
        {
          physicalName: "singleMemberSection",
          label: "単一メンバーセクション",
          type: 'section',
          members: [
            {
              physicalName: "singleMember",
              label: "単一メンバー",
              ...UI_TEXT("singleMember"),
            },
          ],
        },

        // メンバーが1つだけの配列
        {
          physicalName: "singleMemberArray",
          arrayLabel: "単一メンバー配列",
          type: 'array',
          onCreateNewItem: () => ({ singleItem: "" }),
          members: [
            {
              physicalName: "singleItem",
              label: "単一アイテム",
              ...UI_TEXT("singleItem"),
            },
          ],
        },

        // ネストした空のセクション
        {
          physicalName: "nestedEmptySection",
          label: "ネストした空のセクション",
          type: 'section',
          members: [
            {
              physicalName: "outerMember",
              label: "外側メンバー",
              ...UI_TEXT("outerMember"),
            },
            // 内側の空セクション
            {
              physicalName: "innerEmptySection",
              label: "内側の空セクション",
              type: 'section',
              members: [],
            },
            {
              physicalName: "anotherOuterMember",
              label: "もう一つの外側メンバー",
              ...UI_TEXT("anotherOuterMember"),
            },
          ],
        },

        // ネストした空の配列
        {
          physicalName: "nestedEmptyArray",
          label: "ネストした空の配列",
          type: 'section',
          members: [
            {
              physicalName: "sectionMember",
              label: "セクションメンバー",
              ...UI_TEXT("sectionMember"),
            },
            // 内側の空配列
            {
              physicalName: "innerEmptyArray",
              arrayLabel: "内側の空配列",
              type: 'array',
              onCreateNewItem: () => ({}),
              members: [],
            },
          ],
        },

        // 空セクションと空配列を含む配列
        {
          physicalName: "arrayWithEmptyMembers",
          arrayLabel: "空メンバーを含む配列",
          type: 'array',
          onCreateNewItem: () => ({
            text: "",
            emptySection: {},
            emptyArray: []
          }),
          members: [
            {
              physicalName: "text",
              label: "テキスト",
              ...UI_TEXT("text"),
            },
            // 配列内の空セクション
            {
              physicalName: "emptySection",
              label: "配列内空セクション",
              type: 'section',
              members: [],
            },
            // 配列内の空配列
            {
              physicalName: "emptyArray",
              arrayLabel: "配列内空配列",
              type: 'array',
              onCreateNewItem: () => ({}),
              members: [],
            },
          ],
        },

        // 複数レベルの空のネスト
        {
          physicalName: "multiLevelEmpty",
          label: "複数レベルの空ネスト",
          type: 'section',
          members: [
            {
              physicalName: "level1Text",
              label: "レベル1テキスト",
              ...UI_TEXT("level1Text"),
            },
            {
              physicalName: "level2Section",
              label: "レベル2セクション",
              type: 'section',
              members: [
                {
                  physicalName: "level2Text",
                  label: "レベル2テキスト",
                  ...UI_TEXT("level2Text"),
                },
                // レベル3の空セクション
                {
                  physicalName: "level3EmptySection",
                  label: "レベル3空セクション",
                  type: 'section',
                  members: [],
                },
                // レベル3の空配列
                {
                  physicalName: "level3EmptyArray",
                  arrayLabel: "レベル3空配列",
                  type: 'array',
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
          arrayLabel: "無効な新規アイテム配列",
          type: 'array',
          // 空オブジェクトを返すonCreateNewItem
          onCreateNewItem: () => ({}),
          members: [
            {
              physicalName: "item",
              label: "アイテム",
              ...UI_TEXT("item"),
            },
          ],
        },

        // null/undefinedを含む可能性のあるonCreateNewItem
        {
          physicalName: "nullishNewItemArray",
          arrayLabel: "null可能性配列",
          type: 'array',
          onCreateNewItem: () => ({
            text: undefined,
            number: null,
            emptyString: "",
          }),
          members: [
            {
              physicalName: "text",
              label: "テキスト（undefined初期値）",
              ...UI_TEXT("text"),
            },
            {
              physicalName: "number",
              label: "数値（null初期値）",
              ...UI_NUMBER("number"),
            },
            {
              physicalName: "emptyString",
              label: "空文字初期値",
              ...UI_TEXT("emptyString"),
            },
          ],
        },
      ],
    },
  }
}
