import { MemberOwner, ValueMemberDefinitionMap } from "../types";

/**
 * セクションと配列の基本的な組み合わせを確認するためのテストデータ。
 */
export default function (): [MemberOwner, ValueMemberDefinitionMap] {
  return [{
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
                type: "text",
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
                type: "text",
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
                type: "text",
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
                type: "text",
              },
            ],
          },
        ],
      },
    ],
  },

  // メンバー種類定義。
  // このデータ構造ではセクションと配列の基本的な形を確認するため必要最低限のメンバー種類定義のみ。
  {
    text: {
      renderForm: ({ useFormReturn: { register }, name }) => (
        <input type="text" {...register(name)} />
      ),
      getGridColumnDef: ({ cellType }) => cellType.text("", ""),
    },
  }]
}