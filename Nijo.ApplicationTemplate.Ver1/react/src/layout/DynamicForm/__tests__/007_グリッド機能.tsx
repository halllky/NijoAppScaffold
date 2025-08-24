import React from "react";
import { DynamicFormProps, ValueMember } from "../types";

/**
 * グリッド機能（getGridColumnDef等）を確認するためのテストデータ。
 */
export default function (): DynamicFormProps {

  // 複数回使いまわすUIレンダリング定義
  const UI_TEXT = (physicalName: string): Partial<ValueMember> => ({
    renderFormValue: ({ useFormReturn: { register }, name }) => (
      <input
        type="text"
        {...register(name)}
        className="border border-gray-300 px-2 py-1 rounded w-full"
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
      />
    ),
    getGridColumnDef: ({ member, cellType }) => {
      return cellType.number(physicalName, member.displayName ?? physicalName, {
        defaultWidth: 120,
      })
    },
  })

  const UI_EMAIL = (physicalName: string): Partial<ValueMember> => ({
    renderFormValue: ({ useFormReturn: { register }, name }) => (
      <input
        type="email"
        {...register(name)}
        className="border border-gray-300 px-2 py-1 rounded w-full"
        placeholder="example@domain.com"
      />
    ),
    getGridColumnDef: ({ member, cellType }) => {
      return cellType.text(physicalName, member.displayName ?? physicalName, {
        defaultWidth: 180,
      })
    },
  })

  const UI_BOOLEAN = (physicalName: string): Partial<ValueMember> => ({
    renderFormValue: ({ useFormReturn: { register }, name }) => (
      <label className="flex items-center gap-2">
        <input type="checkbox" {...register(name)} />
        <span>有効</span>
      </label>
    ),
    getGridColumnDef: ({ member, cellType }) => {
      return cellType.boolean(physicalName, member.displayName ?? physicalName, {
        defaultWidth: 80,
      })
    },
  })

  const UI_DATE = (physicalName: string): Partial<ValueMember> => ({
    renderFormValue: ({ useFormReturn: { register }, name }) => (
      <input
        type="date"
        {...register(name)}
        className="border border-gray-300 px-2 py-1 rounded w-full"
      />
    ),
    getGridColumnDef: ({ member, cellType }) => {
      return cellType.date(physicalName, member.displayName ?? physicalName, {
        defaultWidth: 140,
      })
    },
  })

  const UI_TEXTAREA = (physicalName: string): Partial<ValueMember> => ({
    renderFormValue: ({ useFormReturn: { register }, name }) => (
      <textarea
        {...register(name)}
        className="border border-gray-300 px-2 py-1 rounded w-full"
        rows={3}
      />
    ),
    getGridColumnDef: ({ member, cellType }) => {
      return cellType.text(physicalName, member.displayName ?? physicalName, {
        defaultWidth: 200,
        editorOverflow: 'vertical',
      })
    },
  })

  const UI_SELECT = (physicalName: string, options?: { value: string, label: string }[]): Partial<ValueMember> => ({
    renderFormValue: ({ useFormReturn: { register }, name }) => (
      <select
        {...register(name)}
        className="border border-gray-300 px-2 py-1 rounded w-full"
      >
        <option value="">選択してください</option>
        {options ? options.map(option => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        )) : (
          <>
            <option value="draft">下書き</option>
            <option value="review">レビュー中</option>
            <option value="approved">承認済み</option>
            <option value="published">公開済み</option>
          </>
        )}
      </select>
    ),
    getGridColumnDef: ({ member, cellType }) => {
      return cellType.text(physicalName, member.displayName ?? physicalName, {
        defaultWidth: 130,
      })
    },
  })

  // データ構造定義
  return {
    root: {
      members: [
        // 基本的なグリッド表示用の配列
        {
          physicalName: "basicGrid",
          displayName: "基本グリッド",
          type: 'array',
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
              ...UI_TEXT("name"),
            },
            {
              physicalName: "age",
              displayName: "年齢",
              ...UI_NUMBER("age"),
            },
            {
              physicalName: "email",
              displayName: "メールアドレス",
              ...UI_EMAIL("email"),
            },
            {
              physicalName: "isActive",
              displayName: "アクティブ",
              ...UI_BOOLEAN("isActive"),
            },
            {
              physicalName: "joinDate",
              displayName: "入社日",
              ...UI_DATE("joinDate"),
            },
            {
              physicalName: "notes",
              displayName: "備考",
              ...UI_TEXTAREA("notes"),
            },
          ],
        },

        // カスタムグリッド列定義を持つ配列
        {
          physicalName: "customGrid",
          displayName: "カスタムグリッド",
          type: 'array',
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
              renderFormValue: ({ useFormReturn: { register }, name }) => (
                <input
                  type="text"
                  {...register(name)}
                  className="border border-gray-300 px-2 py-1 rounded w-full"
                />
              ),
              getGridColumnDef: ({ member, cellType }) => {
                return cellType.text("product", member.displayName ?? "product", {
                  defaultWidth: 200,
                  required: true,
                  isFixed: true, // 固定列
                })
              },
            },
            {
              physicalName: "price",
              displayName: "価格",
              renderFormValue: ({ useFormReturn: { register }, name }) => (
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
                return cellType.number("price", member.displayName ?? "price", {
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
              renderFormValue: ({ useFormReturn: { register }, name }) => (
                <select
                  {...register(name)}
                  className="border border-gray-300 px-2 py-1 rounded w-full"
                >
                  <option value="">選択してください</option>
                  <option value="electronics">家電</option>
                  <option value="clothing">衣類</option>
                  <option value="food">食品</option>
                  <option value="books">書籍</option>
                </select>
              ),
              getGridColumnDef: ({ member, cellType }) => {
                return cellType.text("category", member.displayName ?? "category", {
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
              renderFormValue: ({ useFormReturn: { register }, name }) => (
                <label className="flex items-center gap-2">
                  <input type="checkbox" {...register(name)} />
                  <span>有効</span>
                </label>
              ),
              getGridColumnDef: ({ member, cellType }) => {
                return cellType.boolean("inStock", member.displayName ?? "inStock", {
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
          type: 'array',
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
              renderFormValue: ({ useFormReturn: { watch }, name }) => {
                const value = watch(name);
                return (
                  <div className="bg-gray-100 px-2 py-1 rounded text-gray-600">
                    {String(value || '')}
                  </div>
                );
              },
              getGridColumnDef: ({ member, cellType }) => {
                return cellType.number("id", member.displayName ?? "id", {
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
              ...UI_TEXT("name"),
            },
            {
              physicalName: "status",
              displayName: "ステータス",
              ...UI_SELECT("status"),
            },
            {
              physicalName: "createdAt",
              displayName: "作成日",
              renderFormValue: ({ useFormReturn: { watch }, name }) => {
                const value = watch(name);
                return (
                  <div className="bg-gray-100 px-2 py-1 rounded text-gray-600">
                    {String(value || '')}
                  </div>
                );
              },
              getGridColumnDef: ({ member, cellType }) => {
                return cellType.date("createdAt", member.displayName ?? "createdAt", {
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
          type: 'array',
          onCreateNewItem: () => ({
            visibleData: "",
            hiddenData: "hidden",
            internalId: Date.now(),
          }),
          members: [
            {
              physicalName: "visibleData",
              displayName: "表示データ",
              ...UI_TEXT("visibleData"),
            },
            {
              physicalName: "hiddenData",
              displayName: "非表示データ",
              renderFormValue: ({ useFormReturn: { register }, name }) => (
                <input
                  type="text"
                  {...register(name)}
                  className="border border-gray-300 px-2 py-1 rounded w-full"
                />
              ),
              getGridColumnDef: ({ member, cellType }) => {
                return cellType.text("hiddenData", member.displayName ?? "hiddenData", {
                  invisible: true, // 非表示
                })
              },
            },
            {
              physicalName: "internalId",
              displayName: "内部ID",
              renderFormValue: ({ useFormReturn: { watch }, name }) => {
                const value = watch(name);
                return (
                  <div className="bg-gray-100 px-2 py-1 rounded text-gray-600">
                    {String(value || '')}
                  </div>
                );
              },
              getGridColumnDef: ({ member, cellType }) => {
                return cellType.number("internalId", member.displayName ?? "internalId", {
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
          type: 'array',
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
              renderFormValue: ({ useFormReturn: { register }, name }) => (
                <input
                  type="text"
                  {...register(name)}
                  className="border border-gray-300 px-2 py-1 rounded w-full"
                />
              ),
              getGridColumnDef: ({ member, cellType }) => {
                return cellType.text("task", member.displayName ?? "task", {
                  defaultWidth: 200,
                  editorOverflow: 'vertical',
                })
              },
            },
            {
              physicalName: "priority",
              displayName: "優先度",
              renderFormValue: ({ useFormReturn: { register }, name }) => (
                <select
                  {...register(name)}
                  className="border border-gray-300 px-2 py-1 rounded w-full"
                >
                  <option value="">選択してください</option>
                  <option value="low">低</option>
                  <option value="medium">中</option>
                  <option value="high">高</option>
                  <option value="urgent">緊急</option>
                </select>
              ),
              getGridColumnDef: ({ member, cellType }) => {
                return cellType.text("priority", member.displayName ?? "priority", {
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
              ...UI_DATE("dueDate"),
            },
            {
              physicalName: "progress",
              displayName: "進捗",
              renderFormValue: ({ useFormReturn: { register, watch }, name }) => {
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
                return cellType.number("progress", member.displayName ?? "progress", {
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
              renderFormValue: ({ useFormReturn: { register }, name }) => (
                <select
                  {...register(name)}
                  className="border border-gray-300 px-2 py-1 rounded w-full"
                >
                  <option value="">選択してください</option>
                  <option value="user1">田中太郎</option>
                  <option value="user2">佐藤花子</option>
                  <option value="user3">鈴木一郎</option>
                  <option value="user4">高橋美咲</option>
                </select>
              ),
              getGridColumnDef: ({ member, cellType }) => {
                return cellType.text("assignee", member.displayName ?? "assignee", {
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
  }
}
