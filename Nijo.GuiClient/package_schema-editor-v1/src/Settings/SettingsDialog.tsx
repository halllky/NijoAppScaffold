import React from "react";
import * as ReactHookForm from "react-hook-form";
import { useBlockerEx } from "@nijo/ui-components";
import { ModalDialog } from "@nijo/ui-components/layout";
import FormLayout, { LabelProps } from "@nijo/ui-components/layout/FormLayout";
import useEvent from "react-use-event-hook";
import { ProjectOptionPropertyInfo, SchemaDefinitionGlobalState } from "../types";
import { usePersonalSettings } from "./usePersonalSettings";
import { PersonalSettings } from "./PersonalSettings";

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
export const SettingsDialog: React.FC<{
  onClose: () => void
  formMethods?: ReactHookForm.UseFormReturn<SchemaDefinitionGlobalState>
}> = ({ onClose, formMethods }) => {

  const { personalSettings, save } = usePersonalSettings()

  const handleClose = useEvent(() => {
    onClose()
  })

  // // 閉じるときに確認ダイアログを出す
  // useBlockerEx(true)

  return (
    <ModalDialog open
      onOutsideClick={handleClose}
      className="w-1/2 min-w-xl max-w-3xl max-h-[80vh] flex flex-col gap-2"
    >

      <FormLayout.Root
        labelComponent={FormLayoutLabel}
        className="flex-1 p-1"
        labelWidthPx={188}
      >

        {formMethods && (
          <>
            <ProjectSettingSection formMethods={formMethods} />
            <FormLayout.Separator />
          </>
        )}

        <PersonalSettingSection
          personalSettings={personalSettings}
          save={save}
        />

      </FormLayout.Root>

    </ModalDialog>
  )
}

/**
 * プロジェクト設定セクション
 */
const ProjectSettingSection: React.FC<{
  formMethods: ReactHookForm.UseFormReturn<SchemaDefinitionGlobalState>
}> = ({ formMethods: { getValues, register } }) => {

  return (
    <FormLayout.Section labelEnd={(
      <div className="flex flex-col">
        <h2 className="text-lg font-bold">プロジェクト設定</h2>
        <span className="text-xs text-gray-600">
          プロジェクト全体に適用される設定項目
        </span>
      </div>
    )}>
      {getValues('projectOptionPropertyInfos').map((propInfo, index) => (
        <ProjectSettingField
          key={propInfo?.propertyName || index}
          propertyInfo={propInfo}
          register={register}
        />
      ))}
    </FormLayout.Section>
  )
}

/**
 * 個別の設定項目フィールド
 */
const ProjectSettingField: React.FC<{
  propertyInfo: ProjectOptionPropertyInfo
  register: ReactHookForm.UseFormRegister<any>
}> = ({ propertyInfo, register }) => {

  const fieldName: ReactHookForm.Path<SchemaDefinitionGlobalState> = `projectOptions.${propertyInfo.propertyName}`

  return (
    <>
      <FormLayout.Field label={propertyInfo.propertyName}>
        <div className="flex flex-col">
          {(() => {
            switch (propertyInfo.propertyType) {
              case 'bool':
                return (
                  <input
                    type="checkbox"
                    {...register(fieldName)}
                    className="h-4 w-4"
                  />
                )
              case 'int':
                return (
                  <input
                    type="number"
                    {...register(fieldName, {
                      valueAsNumber: true,
                      setValueAs: (value: string) => {
                        const numValue = parseInt(value);
                        return isNaN(numValue) ? 0 : numValue;
                      }
                    })}
                    placeholder={String(propertyInfo.defaultValue || '0')}
                    className="px-1 py-px border border-gray-500"
                  />
                )
              default:
                return (
                  <input
                    type="text"
                    {...register(fieldName)}
                    placeholder={String(propertyInfo.defaultValue || '')}
                    className="px-1 py-px border border-gray-500"
                  />
                )
            }
          })()}
          <span className="mb-2 text-xs text-gray-500">
            {propertyInfo.description}
          </span>
        </div>
      </FormLayout.Field>
    </>
  )
}


/**
 * 個人設定セクション
 */
const PersonalSettingSection: React.FC<{
  personalSettings: PersonalSettings
  save: <TPath extends ReactHookForm.Path<PersonalSettings>>(
    path: TPath,
    value: ReactHookForm.PathValue<PersonalSettings, TPath>
  ) => void
}> = ({ personalSettings, save }) => {

  const handleHideGridButtonsChange = useEvent((e: React.ChangeEvent<HTMLInputElement>) => {
    save('hideGridButtons', e.target.checked)
  })

  return (
    <FormLayout.Section labelEnd={(
      <div className="flex flex-col">
        <h2 className="text-lg font-bold">個人用設定</h2>
        <span className="text-xs text-gray-600">
          自身にのみ適用される設定項目
        </span>
      </div>
    )}>
      <FormLayout.Field label="hideGridButtons">
        <div className="flex flex-col gap-px my-2">
          <input
            type="checkbox"
            checked={personalSettings.hideGridButtons ?? false}
            onChange={handleHideGridButtonsChange}
            className="h-4 w-4"
          />
          <span className="text-xs text-gray-500">
            グリッドの操作説明ボタンを非表示にする
          </span>
        </div>
      </FormLayout.Field>
    </FormLayout.Section>
  )
}

/** ルート集約の属性名の表示用 */
const FormLayoutLabel: React.ElementType<LabelProps> = ({ className, ...rest }) => {
  return (
    <FormLayout.DefaultLabel
      {...rest}
      className={`text-sm break-all ${className ?? ''}`}
    />
  )
}
