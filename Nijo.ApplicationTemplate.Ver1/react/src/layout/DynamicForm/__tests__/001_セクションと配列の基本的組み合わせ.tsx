import { MemberOwner, ValueMember } from "../types";

/**
 * セクションと配列の基本的な組み合わせを確認するためのテストデータ。
 */
export default function (): MemberOwner {

  // 複数回使いまわすUIレンダリング定義
  const UI_TEXT = (physicalName: string): Partial<ValueMember> => ({
    renderFormValue: ({ useFormReturn: { register }, name }) => (
      <input type="text" {...register(name)} className="border border-gray-700 px-1 py-px" />
    ),
    getGridColumnDef: ({ member, cellType }) => {
      return cellType.text(physicalName, member.displayName ?? physicalName, {
      })
    },
  })

  // データ構造定義
  return {
    members: [
      // セクション1
      {
        physicalName: "section1",
        displayName: "セクション1",
        isSection: true,
        members: [
          // セクション1-1
          {
            physicalName: "section1_1",
            displayName: "セクション1-1",
            isSection: true,
            members: [
              // text型のメンバー1
              {
                physicalName: "member1",
                displayName: "メンバー1",
                ...UI_TEXT("member1"),
              },
            ],
          },
          // 配列1-2
          {
            physicalName: "array1_2",
            displayName: "配列1-2",
            isArray: true,
            onCreateNewItem: () => ({ member2: "" }),
            members: [
              // text型のメンバー2
              {
                physicalName: "member2",
                displayName: "メンバー2",
                ...UI_TEXT("member2"),
              },
            ],
          },
        ],
      },
      // 配列2
      {
        physicalName: "array2",
        displayName: "配列2",
        isArray: true,
        onCreateNewItem: () => ({
          section2_1: { member3: "" },
          array2_2: [{ member4: "" }]
        }),
        members: [
          // セクション2-1
          {
            physicalName: "section2_1",
            displayName: "セクション2-1",
            isSection: true,
            members: [
              // text型のメンバー3
              {
                physicalName: "member3",
                displayName: "メンバー3",
                ...UI_TEXT("member3"),
              },
            ],
          },
          // 配列2-2
          {
            physicalName: "array2_2",
            displayName: "配列2-2",
            isArray: true,
            onCreateNewItem: () => ({ member4: "" }),
            members: [
              // text型のメンバー4
              {
                physicalName: "member4",
                displayName: "メンバー4",
                ...UI_TEXT("member4"),
              },
            ],
          },
        ],
      },
    ],
  }
}
