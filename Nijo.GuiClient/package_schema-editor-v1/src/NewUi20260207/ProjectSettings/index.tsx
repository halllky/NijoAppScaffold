import React from "react";
import * as ReactHookForm from "react-hook-form";
import FormLayout, { LabelProps } from "@nijo/ui-components/layout/FormLayout";
import useEvent from "react-use-event-hook";
import { ProjectOptionPropertyInfo, SchemaDefinitionGlobalState } from "../../types";
import { usePersonalSettings } from "../../Settings/usePersonalSettings";
import { PersonalSettings } from "../../Settings/PersonalSettings";
import { CustomAttributeSettings } from "../../Settings/CustomAttributeSettings";
import { GetValidationResultFunction, ValidationTriggerFunction } from "../../MainPage/useValidation";

/**
 * プロジェクト設定タブの内容
 */
export const ProjectSettings: React.FC<{
  formMethods: ReactHookForm.UseFormReturn<SchemaDefinitionGlobalState>
}> = ({ formMethods }) => {

  const { personalSettings, save } = usePersonalSettings()
  const customAttributeSettingsElementRef = React.useRef<HTMLDivElement>(null)

  return (
    <div className="p-4 h-full overflow-y-auto">
      <FormLayout.Root
        labelComponent={FormLayoutLabel}
        className="max-w-4xl mx-auto"
        labelWidthPx={240}
      >
        <ProjectSettingSection
          formMethods={formMethods}
          getValidationResult={undefined}
          trigger={undefined}
          customAttributeSettingsElementRef={customAttributeSettingsElementRef}
        />

        <FormLayout.Separator />

        <PersonalSettingSection
          personalSettings={personalSettings}
          save={save}
        />

      </FormLayout.Root>
    </div>
  )
}

/**
 * プロジェクト設定セクション
 */
const ProjectSettingSection: React.FC<{
  formMethods: ReactHookForm.UseFormReturn<SchemaDefinitionGlobalState>
  getValidationResult: GetValidationResultFunction | undefined
  trigger: ValidationTriggerFunction | undefined
  customAttributeSettingsElementRef: React.RefObject<HTMLDivElement | null>
}> = ({ formMethods, getValidationResult, trigger, customAttributeSettingsElementRef }) => {
  const { getValues, register } = formMethods

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

      {/* カスタム属性 */}
      <CustomAttributeSettings
        formMethods={formMethods}
        getValidationResult={getValidationResult}
        trigger={trigger}
        elementRef={customAttributeSettingsElementRef}
      />
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
      {/*
        Note: autoGenerateCode setting is handled in the Header of NewUi20260207,
        so it might not be needed here anymore, or we can keep it for completeness?
        The original SettingsDialog allowed setting it.
        The mockup in NewUi20260207 index.tsx header has a checkbox for 'autoGenerateCode'.
        Having it in both places is fine, or we can remove it here to avoid clutter if it's already prominent.
        However, let's stick to the original implementation first.
        Wait, I see NewUi20260207/index.tsx has `savePersonalSettings('autoGenerateCode', ...)` in the header.
        So duplication is fine or I can just include it.
      */}
      <FormLayout.Field label="autoGenerateCode">
        <div className="flex flex-col gap-px my-2">
          <input
            type="checkbox"
            checked={personalSettings.autoGenerateCode ?? false}
            onChange={(e) => save('autoGenerateCode', e.target.checked)}
            className="h-4 w-4"
          />
          <span className="text-xs text-gray-500">
            保存時にソースコードの自動生成をかけ直す
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
