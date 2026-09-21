import * as ReactHookForm from "react-hook-form"
import type { EditingProject } from "../../features/backend"

/**
 * 区分の種類の目次。静的区分の種類と動的区分の種類を列挙し、クリックで各編集欄へ移動する。
 * 名前の編集に追従させるためフォームの値を購読しているので、再レンダリングがこのコンポーネント内で閉じるよう分離している。
 */
export function SideMenu({ onClickStaticEnum, onClickDynamicEnumType }: {
  /** 静的区分の種類がクリックされたとき。引数はその区分のルート要素の uniqueId */
  onClickStaticEnum: (uniqueId: string) => void
  /** 動的区分の種類がクリックされたとき。引数は動的区分の種類の一覧中のインデックス */
  onClickDynamicEnumType: (index: number) => void
}) {

  const { control } = ReactHookForm.useFormContext<EditingProject>()
  const staticEnums = ReactHookForm.useWatch({ control, name: "staticEnums" })
  const dynamicEnumTypes = ReactHookForm.useWatch({ control, name: "dynamicEnumTypes" })

  return (
    <nav className="h-full overflow-y-auto flex flex-col gap-4 p-2 bg-gray-50 border-t border-r border-gray-300">

      {/* 静的区分（列挙体） */}
      <div className="flex flex-col">
        <span className="my-2 text-xs text-gray-700 select-none truncate">
          静的区分（列挙体）
        </span>
        {staticEnums.map(staticEnum => (
          <SideMenuLink key={staticEnum.root.uniqueId} onClick={() => onClickStaticEnum(staticEnum.root.uniqueId)}>
            {staticEnum.root.displayName}
          </SideMenuLink>
        ))}
      </div>

      {/* 動的区分 */}
      <div className="flex flex-col">
        <span className="my-2 text-xs text-gray-700 select-none truncate">
          動的区分（区分マスタ）
        </span>
        {dynamicEnumTypes.map((dynamicEnumType, index) => (
          <SideMenuLink key={dynamicEnumType.uniqueId} onClick={() => onClickDynamicEnumType(index)}>
            {dynamicEnumType.displayName}
          </SideMenuLink>
        ))}
      </div>
    </nav>
  )
}

/** 目次の1項目 */
function SideMenuLink({ onClick, children }: {
  onClick: () => void
  /** 表示名。空の場合はその旨を表示する */
  children: string
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      title={children}
      className="pl-3 pr-1 py-px text-left text-sm truncate hover:bg-gray-200 cursor-pointer select-none"
    >
      {children || <span className="text-gray-400">（名前なし）</span>}
    </button>
  )
}
