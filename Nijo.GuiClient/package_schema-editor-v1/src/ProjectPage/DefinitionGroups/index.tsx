import React from "react"
import * as ReactHookForm from "react-hook-form"
import * as Icon from "@heroicons/react/24/solid"
import { UUID } from "uuidjs"
import {
  EditingProject,
  EditingRootAggregate,
} from "../../backend"
import * as UI from '../../UI'
import { SingleGroupEditor } from "./SingleGroupEditor"
import { DefinitionGroupsSection, DefinitionGroupsSetting } from "./types"

export type { DefinitionGroupsSection, DefinitionGroupsSetting }

/**
 * 定数やテンプレート文字列のように、グループ（ルート集約）ごとに要素を並べて定義するセクションの編集画面。
 * グループを縦に並べ、それぞれを編集可能にする。
 */
export function DefinitionGroupsGrid({ section, setting, formMethods }: {
  section: DefinitionGroupsSection
  setting: DefinitionGroupsSetting
  formMethods: ReactHookForm.UseFormReturn<EditingProject>
}) {
  const { control, setValue, getValues } = formMethods
  const groups = ReactHookForm.useWatch({
    control,
    name: section,
  }) ?? []

  const handleAddGroup = () => {
    const newGroup: EditingRootAggregate = {
      uniqueId: UUID.generate(),
      physicalName: "",
      attributes: {},
      uniqueConstraints: [],
      members: [],
    }
    const current = getValues(section) ?? []
    setValue(section, [...current, newGroup])
  }

  return (
    <div className="flex flex-col gap-2 py-2 p-4">
      {groups.map((group, index) => (
        <SingleGroupEditor
          key={group.uniqueId ?? index}
          section={section}
          setting={setting}
          index={index}
          formMethods={formMethods}
        />
      ))}
      <div>
        <UI.Button icon={Icon.PlusIcon} onClick={handleAddGroup}>
          {setting.addButtonText}
        </UI.Button>
      </div>
    </div>
  )
}
