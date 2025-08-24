import { DynamicFormProps, ValueMember } from "../types";

/**
 * 複雑なネスト構造（深いネスト、配列とセクションの複雑な組み合わせ）を確認するためのテストデータ。
 */
export default function (): DynamicFormProps {

  // 複数回使いまわすUIレンダリング定義
  const UI_TEXT = (physicalName: string): Partial<ValueMember> => ({
    contents: ({ useFormReturn: { register }, name }) => (
      <input
        type="text"
        {...register(name)}
        className="border border-gray-300 px-2 py-1 rounded w-full text-sm"
      />
    ),
    getGridColumnDef: ({ member, cellType }) => {
      return cellType.text(physicalName, typeof member.label === 'string' ? member.label : physicalName, {
        defaultWidth: 120,
      })
    },
  })

  const UI_SELECT = (physicalName: string, options?: { value: string, label: string }[]): Partial<ValueMember> => ({
    contents: ({ useFormReturn: { register }, name }) => {
      // フィールド名に応じて異なる選択肢を提供
      const getOptions = () => {
        if (options) return [{ value: "", label: "選択してください" }, ...options];

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
      return cellType.text(physicalName, typeof member.label === 'string' ? member.label : physicalName, {
        defaultWidth: 100,
      })
    },
  })

  const UI_TEXTAREA = (physicalName: string): Partial<ValueMember> => ({
    contents: ({ useFormReturn: { register }, name }) => (
      <textarea
        {...register(name)}
        className="border border-gray-300 px-2 py-1 rounded w-full text-sm"
        rows={2}
      />
    ),
    getGridColumnDef: ({ member, cellType }) => {
      return cellType.text(physicalName, typeof member.label === 'string' ? member.label : physicalName, {
        defaultWidth: 150,
      })
    },
  })

  const UI_DATE = (physicalName: string): Partial<ValueMember> => ({
    contents: ({ useFormReturn: { register }, name }) => (
      <input
        type="date"
        {...register(name)}
        className="border border-gray-300 px-2 py-1 rounded w-full text-sm"
      />
    ),
    getGridColumnDef: ({ member, cellType }) => {
      return cellType.date(physicalName, typeof member.label === 'string' ? member.label : physicalName, {
        defaultWidth: 120,
      })
    },
  })

  // データ構造定義
  return {
    root: {
      members: [
        // レベル1: 基本情報セクション
        // ※ オブジェクト直下のプロパティだけでなく
        //    オブジェクト直下の "values" オブジェクトの中にある
        //    プロパティも展開されることを確認する
        {
          physicalName: "basicInfo",
          label: "基本情報",
          type: 'section',
          members: [
            {
              physicalName: "values.name",
              label: "名前",
              ...UI_TEXT("values.name"),
            },
            {
              physicalName: "values.email",
              label: "メールアドレス",
              ...UI_TEXT("values.email"),
            },

            // レベル2: 住所セクション（物理名なしネスト）
            // ※ UI上は個別のセクションに分かれるがデータ上は同じオブジェクトであるパターン
            {
              label: "住所",
              type: 'section',
              members: [
                {
                  physicalName: "values.zipCode",
                  label: "郵便番号",
                  ...UI_TEXT("values.zipCode"),
                },
                {
                  physicalName: "values.prefecture",
                  label: "都道府県",
                  ...UI_TEXT("values.prefecture"),
                },
                {
                  physicalName: "values.city",
                  label: "市区町村",
                  ...UI_TEXT("values.city"),
                },

                // レベル3: 詳細住所セクション（レベル2と同じ観点）
                {
                  label: "詳細住所",
                  type: 'section',
                  members: [
                    {
                      physicalName: "values.street",
                      label: "番地",
                      ...UI_TEXT("values.street"),
                    },
                    {
                      physicalName: "values.building",
                      label: "建物名",
                      ...UI_TEXT("values.building"),
                    },
                    {
                      physicalName: "values.room",
                      label: "部屋番号",
                      ...UI_TEXT("values.room"),
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
          arrayLabel: "連絡先リスト",
          type: 'array',
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
              label: "連絡手段",
              ...UI_SELECT("type"),
            },
            {
              physicalName: "value",
              label: "連絡先",
              ...UI_TEXT("value"),
            },

            // レベル2: 配列内のセクション
            {
              physicalName: "details",
              label: "詳細設定",
              type: 'section',
              members: [
                {
                  physicalName: "priority",
                  label: "優先度",
                  ...UI_SELECT("priority"),
                },
                {
                  physicalName: "notes",
                  label: "備考",
                  ...UI_TEXTAREA("notes"),
                },

                // レベル3: セクション内の配列（配列→セクション→配列のネスト）
                {
                  physicalName: "schedule",
                  arrayLabel: "連絡可能時間",
                  type: 'array',
                  onCreateNewItem: () => ({ day: "", time: "" }),
                  members: [
                    {
                      physicalName: "day",
                      label: "曜日",
                      ...UI_SELECT("day"),
                    },
                    {
                      physicalName: "time",
                      label: "時間帯",
                      ...UI_TEXT("time"),
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
          arrayLabel: "プロジェクト",
          type: 'array',
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
              label: "プロジェクト名",
              ...UI_TEXT("name"),
            },
            {
              physicalName: "status",
              label: "ステータス",
              ...UI_SELECT("status"),
            },

            // レベル2: チーム情報セクション
            {
              physicalName: "team",
              label: "チーム情報",
              type: 'section',
              members: [
                {
                  physicalName: "leader",
                  label: "リーダー",
                  ...UI_TEXT("leader"),
                },

                // レベル3: メンバー配列
                {
                  physicalName: "members",
                  arrayLabel: "メンバー",
                  type: 'array',
                  onCreateNewItem: () => ({ name: "", role: "", skills: [{ name: "", level: "" }] }),
                  members: [
                    {
                      physicalName: "name",
                      label: "名前",
                      ...UI_TEXT("name"),
                    },
                    {
                      physicalName: "role",
                      label: "役割",
                      ...UI_TEXT("role"),
                    },

                    // レベル4: スキル配列（配列→セクション→配列→配列のネスト）
                    {
                      physicalName: "skills",
                      arrayLabel: "スキル",
                      type: 'array',
                      onCreateNewItem: () => ({ name: "", level: "" }),
                      members: [
                        {
                          physicalName: "name",
                          label: "スキル名",
                          ...UI_TEXT("name"),
                        },
                        {
                          physicalName: "level",
                          label: "レベル",
                          ...UI_SELECT("level"),
                        },
                      ],
                    },
                  ],
                },

                // レベル3: ミーティング配列
                {
                  physicalName: "meetings",
                  arrayLabel: "ミーティング",
                  type: 'array',
                  onCreateNewItem: () => ({ date: "", agenda: { topics: [{ title: "", priority: "" }] } }),
                  members: [
                    {
                      physicalName: "date",
                      label: "日付",
                      ...UI_DATE("date"),
                    },

                    // レベル4: アジェンダセクション
                    {
                      physicalName: "agenda",
                      label: "アジェンダ",
                      type: 'section',
                      members: [
                        // レベル5: トピック配列（最も深いネスト）
                        {
                          physicalName: "topics",
                          arrayLabel: "議題",
                          type: 'array',
                          onCreateNewItem: () => ({ title: "", priority: "" }),
                          members: [
                            {
                              physicalName: "title",
                              label: "議題名",
                              ...UI_TEXT("title"),
                            },
                            {
                              physicalName: "priority",
                              label: "優先度",
                              ...UI_SELECT("priority"),
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
  }
}
