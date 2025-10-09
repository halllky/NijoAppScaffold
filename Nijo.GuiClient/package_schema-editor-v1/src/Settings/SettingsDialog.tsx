import React from "react";
import * as ReactRouter from "react-router-dom";
import { useBlockerEx } from "@nijo/ui-components";
import { ModalDialog } from "@nijo/ui-components/layout";
import FormLayout, { LabelProps } from "@nijo/ui-components/layout/FormLayout";
import useEvent from "react-use-event-hook";
import { MainPageOutletContext } from "../MainPage/OutletContext";
import { ProjectOptionPropertyInfo } from "../types";

/**
 * 個人用設定 + プロジェクト設定ダイアログ。
 * クエリパラメータにプロジェクトディレクトリが指定されている場合はプロジェクト設定も表示する。
 *
 * ### プロジェクト設定
 *
 * プロジェクト名などプロジェクト単位の設定を行う。
 *
 * 項目は最終的に nijo.xml のルート要素の属性に保存される。
 * このダイアログでは React Hook Form の値を更新するところまで行い、保存処理は親コンポーネントに任せる。
 *
 * ### 個人用設定
 *
 * ここで設定する項目はユーザー自身の環境にのみ適用され、
 * プロジェクト全体には影響しない。
 *
 * ここで設定された値は localStorage に保存される。
 */
export const SettingsDialog = () => {

  // 閉じる場合、1つ前のヒストリーに戻る。
  // このダイアログは独立したルーティングで定義されているため
  const handleClose = useEvent(() => {
    window.history.back()
  })

  // // 閉じるときに確認ダイアログを出す
  // useBlockerEx(true)

  return (
    <ModalDialog open
      onOutsideClick={handleClose}
      className="w-1/2 max-w-3xl max-h-[80vh] flex flex-col gap-2"
    >

      <FormLayout.Root
        labelComponent={FormLayoutLabel}
        className="flex-1"
        labelWidthPx={248}
      >

        <ProjectSettingSection />

      </FormLayout.Root>

    </ModalDialog>
  )
}

/**
 * プロジェクト設定セクション
 */
const ProjectSettingSection = () => {
  const { watch, setValue, formState } = ReactRouter.useOutletContext<MainPageOutletContext>()

  const projectOptions = watch('projectOptions') || {}
  const projectOptionPropertyInfos = watch('projectOptionPropertyInfos') || []

  const handleValueChange = useEvent((propertyName: string, value: string | boolean | number) => {
    setValue(`projectOptions.${propertyName}` as any, value, { shouldDirty: true })
  })

  return (
    <FormLayout.Section label="プロジェクト設定" border>
      {projectOptionPropertyInfos.map(propInfo => (
        <ProjectSettingField
          key={propInfo.propertyName}
          propertyInfo={propInfo}
          value={projectOptions[propInfo.propertyName]}
          onChange={(value) => handleValueChange(propInfo.propertyName, value)}
        />
      ))}

      {formState.isDirty && (
        <div className="text-sm text-gray-600 mt-4">
          変更があります。保存するにはダイアログを閉じてください。
        </div>
      )}
    </FormLayout.Section>
  )
}

/**
 * 個別の設定項目フィールド
 */
const ProjectSettingField: React.FC<{
  propertyInfo: ProjectOptionPropertyInfo
  value: string | boolean | number | undefined
  onChange: (value: string | boolean | number) => void
}> = ({ propertyInfo, value, onChange }) => {

  const renderInput = () => {
    switch (propertyInfo.propertyType) {
      case 'bool':
        return (
          <input
            type="checkbox"
            checked={Boolean(value)}
            onChange={(e) => onChange(e.target.checked)}
            className="h-4 w-4"
          />
        )
      case 'int':
        return (
          <input
            type="number"
            value={value !== undefined ? Number(value) : ''}
            onChange={(e) => {
              const numValue = parseInt(e.target.value);
              onChange(isNaN(numValue) ? 0 : numValue);
            }}
            placeholder={String(propertyInfo.defaultValue || '0')}
            className="px-1 py-px border border-gray-300"
          />
        )
      default:
        return (
          <input
            type="text"
            value={String(value || '')}
            onChange={(e) => onChange(e.target.value)}
            placeholder={String(propertyInfo.defaultValue || '')}
            className="px-1 py-px border border-gray-300"
          />
        )
    }
  }

  return (
    <FormLayout.Field label={propertyInfo.propertyName}>
      <div className="flex flex-col gap-px">
        {renderInput()}
        <span className="mb-2 text-xs text-gray-500">
          {propertyInfo.description}
        </span>
      </div>
    </FormLayout.Field>
  )
}


/** ルート集約の属性名の表示用 */
const FormLayoutLabel: React.ElementType<LabelProps> = ({ className, ...rest }) => {
  return (
    <FormLayout.DefaultLabel
      {...rest}
      className={`text-sm ${className ?? ''}`}
    />
  )
}
