import React from "react"
import * as ReactHookForm from "react-hook-form"
import { MULTI_VIEW_DETAIL_LINK_BEHAVIORS, type EditingProject } from "../../features/backend"
import { ValueObjectEditor } from "./ValueObjectEditor"

/**
 * 基本設定の編集画面。
 * スキーマ定義のルート要素に設定される、アプリケーション全体に関わる設定と値オブジェクトを編集する。
 * 編集対象のデータは親のフォームのコンテキストから取得する。
 */
export function AppSettingsPane() {

  return (
    <div className="w-full h-full overflow-y-auto">
      <div className="h-full max-w-4xl flex flex-col gap-8 p-2">

        {/* 基本 */}
        <section className="flex flex-col gap-4">
          <h2 className="font-bold text-xl select-none">
            基本
          </h2>
          <SettingTable>
            <TextSetting name="config.RootNamespace" label="ルート名前空間">
              生成されるソースコードの名前空間。スキーマ定義のXMLのルート要素の名前にもなる。
            </TextSetting>
            <TextSetting name="config.DbContextName" label="DbContext名">
              Entity Framework Core の DbContext のクラス名。
            </TextSetting>
          </SettingTable>
        </section>

        {/* DBカラム名 */}
        <section className="flex flex-col gap-4">
          <h2 className="font-bold text-xl select-none">
            DBカラム名
          </h2>
          <span className="text-xs text-gray-700">
            すべてのテーブルに共通で追加されるカラムの名前。未指定の場合はそのカラムが作られない。
          </span>
          <SettingTable>
            <TextSetting name="config.CreateUserDbColumnName" label="作成者" />
            <TextSetting name="config.UpdateUserDbColumnName" label="更新者" />
            <TextSetting name="config.CreatedAtDbColumnName" label="作成時刻" />
            <TextSetting name="config.UpdatedAtDbColumnName" label="更新時刻" />
            <TextSetting name="config.VersionDbColumnName" label="バージョン">
              楽観排他制御に使われるカラム。
            </TextSetting>
          </SettingTable>
        </section>

        {/* 添付ファイル */}
        <section className="flex flex-col gap-4">
          <h2 className="font-bold text-xl select-none">
            添付ファイル
          </h2>
          <SettingTable>
            <NumberSetting name="config.MaxFileSizeMB" label="上限サイズ(MB)">
              添付可能なファイル1個あたりの上限サイズ。
            </NumberSetting>
            <NumberSetting name="config.MaxTotalFileSizeMB" label="合計上限サイズ(MB)">
              一度に複数ファイルを添付する際の合計の上限サイズ。
            </NumberSetting>
            <TextSetting name="config.AttachmentFileExtensions" label="添付可能な拡張子">
              セミコロン区切りで複数指定できる。例: doc;docx;xls;xlsx;pdf
            </TextSetting>
          </SettingTable>
        </section>

        {/* 画面 */}
        <section className="flex flex-col gap-4">
          <h2 className="font-bold text-xl select-none">
            画面
          </h2>
          <SettingTable>
            <TextSetting name="config.ButtonColor" label="ボタンの色">
              Tailwind CSS で定義されている色名のみ有効。未指定の場合は cyan。
            </TextSetting>
            <CheckBoxSetting name="config.UseWijmo" label="Wijmo(FlexGrid)を使用する" />
            <SelectSetting
              name="config.MultiViewDetailLinkBehavior"
              label="一覧画面の詳細リンクの挙動"
              options={MULTI_VIEW_DETAIL_LINK_BEHAVIORS}
            />
            <CheckBoxSetting name="config.VFormRefItemIsNotWide" label="参照先の欄をフォーム横幅いっぱいにしない" />
            <NumberSetting name="config.VFormMaxColumnCount" label="フォームの最大列数">
              レスポンシブに変化する列数の上限。
            </NumberSetting>
            <NumberSetting name="config.VFormMaxMemberCount" label="フォームの最大項目数">
              これより多くの項目を持つ集約が定義された場合はレイアウトが崩れる。
            </NumberSetting>
            <NumberSetting name="config.VFormThreshold" label="フォームの列数の切替閾値(px)" />
          </SettingTable>
        </section>

        {/* コード生成 */}
        <section className="flex flex-col gap-4">
          <h2 className="font-bold text-xl select-none">
            コード生成
          </h2>
          <SettingTable>
            <CheckBoxSetting name="config.GenerateUnusedRefToModules" label="使われていない参照関連部品も生成する">
              どの集約からも参照されていない参照関連部品を生成するかどうか。
            </CheckBoxSetting>
            <CheckBoxSetting name="config.DisableLocalRepository" label="一時保存を使用しない" />
            <CheckBoxSetting name="config.UseBatchUpdateVersion2" label="一括更新処理にバージョン2を使用する">
              一括更新処理関連で生成されるソースコードとして、よりシンプルな処理を使用する。
            </CheckBoxSetting>
          </SettingTable>
        </section>

        {/* 値オブジェクト */}
        <section className="flex flex-col gap-4">
          <h2 className="font-bold text-xl select-none">
            値オブジェクト
          </h2>
          <span className="text-xs text-gray-700">
            文字列型の項目に固有の型を与えるためのもの。<br />
            項目の「種類」の欄にここで定義した名前を入力すると、その項目が値オブジェクト型になる。
          </span>
          <ValueObjectEditor />
        </section>

        {/* スクロール用の空白 */}
        <div className="min-h-[50%]"></div>
      </div>
    </div>
  )
}

/** 設定項目を「ラベル / 入力欄 / 説明」の3列に揃えて並べる */
function SettingTable({ children }: {
  children?: React.ReactNode
}) {
  return (
    <div className="grid grid-cols-[auto_auto_1fr] items-center gap-x-4 gap-y-2 text-sm">
      {children}
    </div>
  )
}

/** 設定項目のラベルと説明。入力欄そのものは呼び出し側が children として渡す */
function SettingRow({ name, label, description, children }: {
  name: string
  label: string
  description?: React.ReactNode
  children?: React.ReactNode
}) {
  return (
    <>
      <label htmlFor={name} className="text-gray-600">
        {label}
      </label>
      {children}
      <span className="text-xs text-gray-500">
        {description}
      </span>
    </>
  )
}

/** 文字列の設定項目。値が空の場合は未指定として扱われる */
function TextSetting({ name, label, children }: {
  name: ReactHookForm.Path<EditingProject>
  label: string
  children?: React.ReactNode
}) {
  const { register } = ReactHookForm.useFormContext<EditingProject>()

  return (
    <SettingRow name={name} label={label} description={children}>
      <input
        {...register(name)}
        id={name}
        spellCheck={false}
        autoComplete="off"
        className="w-64 px-1 border border-gray-300"
      />
    </SettingRow>
  )
}

/** 数値の設定項目。値が空の場合は未指定として扱われる */
function NumberSetting({ name, label, children }: {
  name: ReactHookForm.Path<EditingProject>
  label: string
  children?: React.ReactNode
}) {
  const { register } = ReactHookForm.useFormContext<EditingProject>()

  return (
    <SettingRow name={name} label={label} description={children}>
      <input
        // 空欄を「未指定」として保ちたいため、valueAsNumber（空欄が NaN になる）ではなく自前で変換する
        {...register(name, { setValueAs: value => value === "" || value === null ? null : Number(value) })}
        id={name}
        type="number"
        className="w-64 px-1 border border-gray-300"
      />
    </SettingRow>
  )
}

/** 真偽値の設定項目 */
function CheckBoxSetting({ name, label, children }: {
  name: ReactHookForm.Path<EditingProject>
  label: string
  children?: React.ReactNode
}) {
  const { register } = ReactHookForm.useFormContext<EditingProject>()

  return (
    <SettingRow name={name} label={label} description={children}>
      <input
        {...register(name)}
        id={name}
        type="checkbox"
        className="justify-self-start"
      />
    </SettingRow>
  )
}

/** 決まった選択肢から選ぶ設定項目 */
function SelectSetting({ name, label, options, children }: {
  name: ReactHookForm.Path<EditingProject>
  label: string
  options: readonly { value: string, displayName: string }[]
  children?: React.ReactNode
}) {
  const { register } = ReactHookForm.useFormContext<EditingProject>()

  return (
    <SettingRow name={name} label={label} description={children}>
      <select
        {...register(name)}
        id={name}
        className="w-64 px-1 border border-gray-300"
      >
        {options.map(opt => (
          <option key={opt.value} value={opt.value}>{opt.displayName}</option>
        ))}
      </select>
    </SettingRow>
  )
}
