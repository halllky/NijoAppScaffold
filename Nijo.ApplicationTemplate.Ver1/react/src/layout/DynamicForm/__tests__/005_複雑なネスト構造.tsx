import { MemberOwner, ValueMemberDefinitionMap } from "../types";

/**
 * 複雑なネスト構造（深いネスト、配列とセクションの複雑な組み合わせ）を確認するためのテストデータ。
 */
export default function (): [MemberOwner, ValueMemberDefinitionMap] {
  return [{
    members: [
      // レベル1: 基本情報セクション
      {
        physicalName: "basicInfo",
        displayName: "基本情報",
        isSection: true,
        members: [
          {
            physicalName: "name",
            displayName: "名前",
            type: "text",
          },
          {
            physicalName: "email",
            displayName: "メールアドレス",
            type: "text",
          },

          // レベル2: 住所セクション（ネスト）
          {
            physicalName: "address",
            displayName: "住所",
            isSection: true,
            members: [
              {
                physicalName: "zipCode",
                displayName: "郵便番号",
                type: "text",
              },
              {
                physicalName: "prefecture",
                displayName: "都道府県",
                type: "text",
              },
              {
                physicalName: "city",
                displayName: "市区町村",
                type: "text",
              },

              // レベル3: 詳細住所セクション（さらにネスト）
              {
                physicalName: "detail",
                displayName: "詳細住所",
                isSection: true,
                members: [
                  {
                    physicalName: "street",
                    displayName: "番地",
                    type: "text",
                  },
                  {
                    physicalName: "building",
                    displayName: "建物名",
                    type: "text",
                  },
                  {
                    physicalName: "room",
                    displayName: "部屋番号",
                    type: "text",
                  },
                ],
              },
            ],
          },
        ],
      },

      // レベル1: 連絡先配列
      {
        physicalName: "contacts",
        displayName: "連絡先リスト",
        isArray: true,
        onCreateNewItem: () => ({
          type: "",
          value: "",
          details: {
            priority: "",
            notes: "",
            schedule: [{ day: "", time: "" }]
          }
        }),
        members: [
          {
            physicalName: "type",
            displayName: "連絡手段",
            type: "select",
          },
          {
            physicalName: "value",
            displayName: "連絡先",
            type: "text",
          },

          // レベル2: 配列内のセクション
          {
            physicalName: "details",
            displayName: "詳細設定",
            isSection: true,
            members: [
              {
                physicalName: "priority",
                displayName: "優先度",
                type: "select",
              },
              {
                physicalName: "notes",
                displayName: "備考",
                type: "textarea",
              },

              // レベル3: セクション内の配列（配列→セクション→配列のネスト）
              {
                physicalName: "schedule",
                displayName: "連絡可能時間",
                isArray: true,
                onCreateNewItem: () => ({ day: "", time: "" }),
                members: [
                  {
                    physicalName: "day",
                    displayName: "曜日",
                    type: "select",
                  },
                  {
                    physicalName: "time",
                    displayName: "時間帯",
                    type: "text",
                  },
                ],
              },
            ],
          },
        ],
      },

      // レベル1: プロジェクト配列（より複雑なネスト）
      {
        physicalName: "projects",
        displayName: "プロジェクト",
        isArray: true,
        onCreateNewItem: () => ({
          name: "",
          status: "",
          team: {
            leader: "",
            members: [{ name: "", role: "", skills: [{ name: "", level: "" }] }],
            meetings: [{ date: "", agenda: { topics: [{ title: "", priority: "" }] } }]
          }
        }),
        members: [
          {
            physicalName: "name",
            displayName: "プロジェクト名",
            type: "text",
          },
          {
            physicalName: "status",
            displayName: "ステータス",
            type: "select",
          },

          // レベル2: チーム情報セクション
          {
            physicalName: "team",
            displayName: "チーム情報",
            isSection: true,
            members: [
              {
                physicalName: "leader",
                displayName: "リーダー",
                type: "text",
              },

              // レベル3: メンバー配列
              {
                physicalName: "members",
                displayName: "メンバー",
                isArray: true,
                onCreateNewItem: () => ({ name: "", role: "", skills: [{ name: "", level: "" }] }),
                members: [
                  {
                    physicalName: "name",
                    displayName: "名前",
                    type: "text",
                  },
                  {
                    physicalName: "role",
                    displayName: "役割",
                    type: "text",
                  },

                  // レベル4: スキル配列（配列→セクション→配列→配列のネスト）
                  {
                    physicalName: "skills",
                    displayName: "スキル",
                    isArray: true,
                    onCreateNewItem: () => ({ name: "", level: "" }),
                    members: [
                      {
                        physicalName: "name",
                        displayName: "スキル名",
                        type: "text",
                      },
                      {
                        physicalName: "level",
                        displayName: "レベル",
                        type: "select",
                      },
                    ],
                  },
                ],
              },

              // レベル3: ミーティング配列
              {
                physicalName: "meetings",
                displayName: "ミーティング",
                isArray: true,
                onCreateNewItem: () => ({ date: "", agenda: { topics: [{ title: "", priority: "" }] } }),
                members: [
                  {
                    physicalName: "date",
                    displayName: "日付",
                    type: "date",
                  },

                  // レベル4: アジェンダセクション
                  {
                    physicalName: "agenda",
                    displayName: "アジェンダ",
                    isSection: true,
                    members: [
                      // レベル5: トピック配列（最も深いネスト）
                      {
                        physicalName: "topics",
                        displayName: "議題",
                        isArray: true,
                        onCreateNewItem: () => ({ title: "", priority: "" }),
                        members: [
                          {
                            physicalName: "title",
                            displayName: "議題名",
                            type: "text",
                          },
                          {
                            physicalName: "priority",
                            displayName: "優先度",
                            type: "select",
                          },
                        ],
                      },
                    ],
                  },
                ],
              },
            ],
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
          className="border border-gray-300 px-2 py-1 rounded w-full text-sm"
        />
      ),
      getGridColumnDef: ({ member, cellType }) => {
        return cellType.text(member.physicalName, member.displayName ?? member.physicalName, {
          defaultWidth: 120,
        })
      },
    },

    select: {
      renderForm: ({ useFormReturn: { register }, name, member }) => {
        // フィールド名に応じて異なる選択肢を提供
        const getOptions = () => {
          if (name.includes('type')) {
            return [
              { value: "", label: "選択してください" },
              { value: "phone", label: "電話" },
              { value: "email", label: "メール" },
              { value: "chat", label: "チャット" },
            ];
          }
          if (name.includes('priority')) {
            return [
              { value: "", label: "選択してください" },
              { value: "high", label: "高" },
              { value: "medium", label: "中" },
              { value: "low", label: "低" },
            ];
          }
          if (name.includes('status')) {
            return [
              { value: "", label: "選択してください" },
              { value: "planning", label: "計画中" },
              { value: "active", label: "進行中" },
              { value: "completed", label: "完了" },
              { value: "suspended", label: "中断" },
            ];
          }
          if (name.includes('day')) {
            return [
              { value: "", label: "選択してください" },
              { value: "monday", label: "月曜日" },
              { value: "tuesday", label: "火曜日" },
              { value: "wednesday", label: "水曜日" },
              { value: "thursday", label: "木曜日" },
              { value: "friday", label: "金曜日" },
              { value: "saturday", label: "土曜日" },
              { value: "sunday", label: "日曜日" },
            ];
          }
          if (name.includes('level')) {
            return [
              { value: "", label: "選択してください" },
              { value: "beginner", label: "初心者" },
              { value: "intermediate", label: "中級者" },
              { value: "advanced", label: "上級者" },
              { value: "expert", label: "エキスパート" },
            ];
          }
          return [
            { value: "", label: "選択してください" },
            { value: "option1", label: "選択肢1" },
            { value: "option2", label: "選択肢2" },
          ];
        };

        return (
          <select
            {...register(name)}
            className="border border-gray-300 px-2 py-1 rounded w-full text-sm"
          >
            {getOptions().map(option => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
        );
      },
      getGridColumnDef: ({ member, cellType }) => {
        return cellType.text(member.physicalName, member.displayName ?? member.physicalName, {
          defaultWidth: 100,
        })
      },
    },

    textarea: {
      renderForm: ({ useFormReturn: { register }, name }) => (
        <textarea
          {...register(name)}
          className="border border-gray-300 px-2 py-1 rounded w-full text-sm"
          rows={2}
        />
      ),
      getGridColumnDef: ({ member, cellType }) => {
        return cellType.text(member.physicalName, member.displayName ?? member.physicalName, {
          defaultWidth: 150,
        })
      },
    },

    date: {
      renderForm: ({ useFormReturn: { register }, name }) => (
        <input
          type="date"
          {...register(name)}
          className="border border-gray-300 px-2 py-1 rounded w-full text-sm"
        />
      ),
      getGridColumnDef: ({ member, cellType }) => {
        return cellType.date(member.physicalName, member.displayName ?? member.physicalName, {
          defaultWidth: 120,
        })
      },
    },
  }]
}
