import React from "react";
import { MemberOwner, ValueMemberDefinitionMap } from "../types";

/**
 * グリッド機能（getGridColumnDef等）を確認するためのテストデータ。
 */
export default function (): [MemberOwner, ValueMemberDefinitionMap] {
  return [{
    members: [
      // 基本的なグリッド表示用の配列
      {
        physicalName: "basicGrid",
        displayName: "基本グリッド",
        isArray: true,
        onCreateNewItem: () => ({
          name: "",
          age: 0,
          email: "",
          isActive: false,
          joinDate: "",
          notes: "",
        }),
        members: [
          {
            physicalName: "name",
            displayName: "名前",
            type: "text",
          },
          {
            physicalName: "age",
            displayName: "年齢",
            type: "number",
          },
          {
            physicalName: "email",
            displayName: "メールアドレス",
            type: "email",
          },
          {
            physicalName: "isActive",
            displayName: "アクティブ",
            type: "boolean",
          },
          {
            physicalName: "joinDate",
            displayName: "入社日",
            type: "date",
          },
          {
            physicalName: "notes",
            displayName: "備考",
            type: "textarea",
          },
        ],
      },

      // カスタムグリッド列定義を持つ配列
      {
        physicalName: "customGrid",
        displayName: "カスタムグリッド",
        isArray: true,
        onCreateNewItem: () => ({
          product: "",
          price: 0,
          category: "",
          inStock: false,
        }),
        members: [
          {
            physicalName: "product",
            displayName: "商品名",
            type: "text",
            getGridColumnDef: ({ member, cellType }) => {
              return cellType.text(member.physicalName, member.displayName ?? member.physicalName, {
                defaultWidth: 200,
                required: true,
                isFixed: true, // 固定列
              })
            },
          },
          {
            physicalName: "price",
            displayName: "価格",
            type: "currency",
            getGridColumnDef: ({ member, cellType }) => {
              return cellType.number(member.physicalName, member.displayName ?? member.physicalName, {
                defaultWidth: 120,
                required: true,
                renderCell: (context) => (
                  <div className="text-right font-mono">
                    ¥{Number(context.getValue()).toLocaleString()}
                  </div>
                ),
              })
            },
          },
          {
            physicalName: "category",
            displayName: "カテゴリ",
            type: "select",
            getGridColumnDef: ({ member, cellType }) => {
              return cellType.text(member.physicalName, member.displayName ?? member.physicalName, {
                defaultWidth: 140,
                getOptions: () => [
                  { value: "electronics", label: "家電" },
                  { value: "clothing", label: "衣類" },
                  { value: "food", label: "食品" },
                  { value: "books", label: "書籍" },
                ],
              })
            },
          },
          {
            physicalName: "inStock",
            displayName: "在庫あり",
            type: "boolean",
            getGridColumnDef: ({ member, cellType }) => {
              return cellType.boolean(member.physicalName, member.displayName ?? member.physicalName, {
                defaultWidth: 100,
                renderCell: (context) => (
                  <div className="text-center">
                    {context.getValue() ? (
                      <span className="text-green-600 font-bold">✓ あり</span>
                    ) : (
                      <span className="text-red-600">✗ なし</span>
                    )}
                  </div>
                ),
              })
            },
          },
        ],
      },

      // 読み取り専用列を含むグリッド
      {
        physicalName: "readOnlyGrid",
        displayName: "読み取り専用グリッド",
        isArray: true,
        onCreateNewItem: () => ({
          id: Math.floor(Math.random() * 10000),
          name: "",
          status: "draft",
          createdAt: new Date().toISOString().split('T')[0],
        }),
        members: [
          {
            physicalName: "id",
            displayName: "ID",
            type: "readonly",
            getGridColumnDef: ({ member, cellType }) => {
              return cellType.number(member.physicalName, member.displayName ?? member.physicalName, {
                defaultWidth: 80,
                isReadOnly: true,
                renderCell: (context) => (
                  <div className="text-gray-500 font-mono text-sm">
                    #{context.row.original.id}
                  </div>
                ),
              })
            },
          },
          {
            physicalName: "name",
            displayName: "名前",
            type: "text",
          },
          {
            physicalName: "status",
            displayName: "ステータス",
            type: "select",
          },
          {
            physicalName: "createdAt",
            displayName: "作成日",
            type: "readonly",
            getGridColumnDef: ({ member, cellType }) => {
              return cellType.date(member.physicalName, member.displayName ?? member.physicalName, {
                defaultWidth: 120,
                isReadOnly: true,
              })
            },
          },
        ],
      },

      // 非表示列を含むグリッド
      {
        physicalName: "hiddenColumnGrid",
        displayName: "非表示列グリッド",
        isArray: true,
        onCreateNewItem: () => ({
          visibleData: "",
          hiddenData: "hidden",
          internalId: Date.now(),
        }),
        members: [
          {
            physicalName: "visibleData",
            displayName: "表示データ",
            type: "text",
          },
          {
            physicalName: "hiddenData",
            displayName: "非表示データ",
            type: "text",
            getGridColumnDef: ({ member, cellType }) => {
              return cellType.text(member.physicalName, member.displayName ?? member.physicalName, {
                invisible: true, // 非表示
              })
            },
          },
          {
            physicalName: "internalId",
            displayName: "内部ID",
            type: "readonly",
            getGridColumnDef: ({ member, cellType }) => {
              return cellType.number(member.physicalName, member.displayName ?? member.physicalName, {
                invisible: true, // 非表示
              })
            },
          },
        ],
      },

      // 複雑な編集機能を持つグリッド
      {
        physicalName: "advancedEditGrid",
        displayName: "高度編集グリッド",
        isArray: true,
        onCreateNewItem: () => ({
          task: "",
          priority: "",
          dueDate: "",
          progress: 0,
          assignee: "",
        }),
        members: [
          {
            physicalName: "task",
            displayName: "タスク",
            type: "text",
            getGridColumnDef: ({ member, cellType }) => {
              return cellType.text(member.physicalName, member.displayName ?? member.physicalName, {
                defaultWidth: 200,
                editorOverflow: 'vertical',
              })
            },
          },
          {
            physicalName: "priority",
            displayName: "優先度",
            type: "select",
            getGridColumnDef: ({ member, cellType }) => {
              return cellType.text(member.physicalName, member.displayName ?? member.physicalName, {
                defaultWidth: 100,
                getOptions: () => [
                  { value: "low", label: "低" },
                  { value: "medium", label: "中" },
                  { value: "high", label: "高" },
                  { value: "urgent", label: "緊急" },
                ],
                renderCell: (context) => {
                  const value = context.getValue() as string;
                  const colors = {
                    low: "text-gray-600",
                    medium: "text-blue-600",
                    high: "text-orange-600",
                    urgent: "text-red-600",
                  };
                  return (
                    <div className={`font-semibold ${colors[value as keyof typeof colors] || 'text-gray-600'}`}>
                      {value === 'low' && '低'}
                      {value === 'medium' && '中'}
                      {value === 'high' && '高'}
                      {value === 'urgent' && '緊急'}
                    </div>
                  );
                },
              })
            },
          },
          {
            physicalName: "dueDate",
            displayName: "期限",
            type: "date",
          },
          {
            physicalName: "progress",
            displayName: "進捗",
            type: "progress",
            getGridColumnDef: ({ member, cellType }) => {
              return cellType.number(member.physicalName, member.displayName ?? member.physicalName, {
                defaultWidth: 120,
                renderCell: (context) => {
                  const value = Number(context.getValue()) || 0;
                  return (
                    <div className="w-full">
                      <div className="flex justify-between text-xs mb-1">
                        <span>{value}%</span>
                      </div>
                      <div className="w-full bg-gray-200 rounded-full h-2">
                        <div
                          className="bg-blue-600 h-2 rounded-full transition-all"
                          style={{ width: `${Math.min(100, Math.max(0, value))}%` }}
                        ></div>
                      </div>
                    </div>
                  );
                },
              })
            },
          },
          {
            physicalName: "assignee",
            displayName: "担当者",
            type: "select",
            getGridColumnDef: ({ member, cellType }) => {
              return cellType.text(member.physicalName, member.displayName ?? member.physicalName, {
                defaultWidth: 130,
                getOptions: () => [
                  { value: "user1", label: "田中太郎" },
                  { value: "user2", label: "佐藤花子" },
                  { value: "user3", label: "鈴木一郎" },
                  { value: "user4", label: "高橋美咲" },
                ],
              })
            },
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

    number: {
      renderForm: ({ useFormReturn: { register }, name }) => (
        <input
          type="number"
          {...register(name, { valueAsNumber: true })}
          className="border border-gray-300 px-2 py-1 rounded w-full"
        />
      ),
      getGridColumnDef: ({ member, cellType }) => {
        return cellType.number(member.physicalName, member.displayName ?? member.physicalName, {
          defaultWidth: 120,
        })
      },
    },

    email: {
      renderForm: ({ useFormReturn: { register }, name }) => (
        <input
          type="email"
          {...register(name)}
          className="border border-gray-300 px-2 py-1 rounded w-full"
          placeholder="example@domain.com"
        />
      ),
      getGridColumnDef: ({ member, cellType }) => {
        return cellType.text(member.physicalName, member.displayName ?? member.physicalName, {
          defaultWidth: 180,
        })
      },
    },

    boolean: {
      renderForm: ({ useFormReturn: { register }, name }) => (
        <label className="flex items-center gap-2">
          <input type="checkbox" {...register(name)} />
          <span>有効</span>
        </label>
      ),
      getGridColumnDef: ({ member, cellType }) => {
        return cellType.boolean(member.physicalName, member.displayName ?? member.physicalName, {
          defaultWidth: 80,
        })
      },
    },

    date: {
      renderForm: ({ useFormReturn: { register }, name }) => (
        <input
          type="date"
          {...register(name)}
          className="border border-gray-300 px-2 py-1 rounded w-full"
        />
      ),
      getGridColumnDef: ({ member, cellType }) => {
        return cellType.date(member.physicalName, member.displayName ?? member.physicalName, {
          defaultWidth: 140,
        })
      },
    },

    textarea: {
      renderForm: ({ useFormReturn: { register }, name }) => (
        <textarea
          {...register(name)}
          className="border border-gray-300 px-2 py-1 rounded w-full"
          rows={3}
        />
      ),
      getGridColumnDef: ({ member, cellType }) => {
        return cellType.text(member.physicalName, member.displayName ?? member.physicalName, {
          defaultWidth: 200,
          editorOverflow: 'vertical',
        })
      },
    },

    currency: {
      renderForm: ({ useFormReturn: { register }, name }) => (
        <div className="flex items-center">
          <span className="text-gray-500 mr-1">¥</span>
          <input
            type="number"
            {...register(name, { valueAsNumber: true })}
            className="border border-gray-300 px-2 py-1 rounded w-full"
            min="0"
          />
        </div>
      ),
      getGridColumnDef: ({ member, cellType }) => {
        return cellType.number(member.physicalName, member.displayName ?? member.physicalName, {
          defaultWidth: 120,
        })
      },
    },

    select: {
      renderForm: ({ useFormReturn: { register }, name }) => (
        <select
          {...register(name)}
          className="border border-gray-300 px-2 py-1 rounded w-full"
        >
          <option value="">選択してください</option>
          <option value="draft">下書き</option>
          <option value="review">レビュー中</option>
          <option value="approved">承認済み</option>
          <option value="published">公開済み</option>
        </select>
      ),
      getGridColumnDef: ({ member, cellType }) => {
        return cellType.text(member.physicalName, member.displayName ?? member.physicalName, {
          defaultWidth: 130,
        })
      },
    },

    readonly: {
      renderForm: ({ useFormReturn: { watch }, name }) => {
        const value = watch(name);
        return (
          <div className="bg-gray-100 px-2 py-1 rounded text-gray-600">
            {String(value || '')}
          </div>
        );
      },
      getGridColumnDef: ({ member, cellType }) => {
        return cellType.text(member.physicalName, member.displayName ?? member.physicalName, {
          defaultWidth: 120,
          isReadOnly: true,
        })
      },
    },

    progress: {
      renderForm: ({ useFormReturn: { register, watch }, name }) => {
        const value = watch(name) as number || 0;
        return (
          <div>
            <input
              type="range"
              {...register(name, { valueAsNumber: true })}
              min="0"
              max="100"
              className="w-full"
            />
            <div className="text-center text-sm text-gray-600 mt-1">
              {value}%
            </div>
          </div>
        );
      },
      getGridColumnDef: ({ member, cellType }) => {
        return cellType.number(member.physicalName, member.displayName ?? member.physicalName, {
          defaultWidth: 120,
        })
      },
    },
  }]
}
