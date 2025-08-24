import { DynamicFormProps, ValueMember } from "../types";

/**
 * セクションと配列の基本的な組み合わせを確認するためのテストデータ。
 */
export default function (): DynamicFormProps {

  // 複数回使いまわすUIレンダリング定義
  const UI_TEXT = (physicalName: string): Partial<ValueMember> => ({
    contents: ({ useFormReturn: { register }, name }) => (
      <input type="text" {...register(name)} className="border border-gray-700 px-1 py-px" />
    ),
    getGridColumnDef: ({ member, cellType }) => {
      return cellType.text(physicalName, typeof member.label === 'string' ? member.label : physicalName, {
      })
    },
  })

  // データ構造定義
  return {
    root: {
      members: [
        // セクション1
        {
          physicalName: "section1",
          label: "セクション1",
          type: 'section',
          members: [
            // セクション1-1
            {
              physicalName: "section1_1",
              label: "セクション1-1",
              type: 'section',
              members: [
                // text型のメンバー1
                {
                  physicalName: "member1",
                  label: "メンバー1",
                  ...UI_TEXT("member1"),
                },
              ],
            },
            // 配列1-2
            {
              physicalName: "array1_2",
              arrayLabel: "配列1-2",
              type: 'array',
              onCreateNewItem: () => ({ member2: "" }),
              members: [
                // text型のメンバー2
                {
                  physicalName: "member2",
                  label: "メンバー2",
                  ...UI_TEXT("member2"),
                },
              ],
            },
          ],
        },
        // 配列2
        {
          physicalName: "array2",
          arrayLabel: "配列2",
          itemLabel: ({ itemIndex }) => <span className="font-bold text-gray-500">配列2 No.{itemIndex + 1}</span>,
          type: 'array',
          onCreateNewItem: () => ({
            section2_1: { member3: "" },
            array2_2: [{ member4: "" }]
          }),
          members: [
            // セクション2-1
            {
              physicalName: "section2_1",
              label: "セクション2-1",
              type: 'section',
              members: [
                // text型のメンバー3
                {
                  physicalName: "member3",
                  label: "メンバー3",
                  ...UI_TEXT("member3"),
                },
              ],
            },
            // 配列2-2
            {
              physicalName: "array2_2",
              arrayLabel: "配列2-2",
              type: 'array',
              onCreateNewItem: () => ({ member4: "" }),
              members: [
                // text型のメンバー4
                {
                  physicalName: "member4",
                  label: "メンバー4",
                  ...UI_TEXT("member4"),
                },
              ],
            },
          ],
        },
      ],
    },
  }
}
