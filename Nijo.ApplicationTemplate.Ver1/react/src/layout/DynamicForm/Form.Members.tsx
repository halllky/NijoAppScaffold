import React from "react"
import { Member, MemberOwner, ValueMember } from "./types"
import { hasArray } from "./helpers"
import { FormValueMember } from "./Form.ValueMember"
import { FormSection } from "./Form.Section"
import { FormArrayAsForm } from "./Form.ArrayAsForm"
import { FormArrayAsGrid } from "./Form.ArrayAsGrid"
import { DynamicFormSpacer } from "./layout"
import { DynamicFormContext } from "./DynamicFormContext"

/**
 * メンバーをAutoColumnの単位にグルーピングしてレンダリングする。
 */
export const MembersGroupByBreakPoint = ({ owner, ancestorsPath }: {
  owner: MemberOwner
  /** ルートオブジェクトからownerまでのパス */
  ancestorsPath: string
}) => {
  const { isWideLayout } = React.useContext(DynamicFormContext)

  // メンバーを折り返しの単位でグルーピングする
  const groups = React.useMemo(() => {
    // 最初に基本的なグルーピングを行う
    type MemberGroup = {
      /** グリッド配置情報とメンバーの組 */
      members: {
        member: Member
        /** グリッド配置情報（4列レイアウト時に使用） */
        gridPosition?: { gridColumn: number, gridRow: number }
      }[]
      fullWidth: boolean
    }
    const baseGroups = owner.members.reduce((acc, member) => {
      // Child, Children, fullWidth指定のメンバーは横幅いっぱいとる
      if (member.isSection || member.isArray || member.fullWidth) {
        acc.push({
          members: [{ member, gridPosition: undefined }],
          fullWidth: true
        })
        return acc
      }

      // それ以外はグルーピングする
      const lastGroup = acc[acc.length - 1]
      if (lastGroup === undefined || lastGroup.fullWidth) {
        acc.push({
          members: [{ member, gridPosition: undefined }],
          fullWidth: false
        })
      } else {
        lastGroup.members.push({ member, gridPosition: undefined })
      }
      return acc
    }, [] as MemberGroup[])

    // 4列レイアウトの場合、fullWidthでないメンバーに配置情報を追加
    if (!isWideLayout) {
      return baseGroups
    }

    return baseGroups.map(group => {
      if (group.fullWidth) {
        return group
      }

      const totalMembers = group.members.length
      const halfPoint = Math.ceil(totalMembers / 2)

      return {
        ...group,
        members: group.members.map((memberWithPos, index) => {
          // 前半は左側（1-2列目）、後半は右側（3-4列目）
          const isLeftSide = index < halfPoint
          const columnStart = isLeftSide ? 1 : 3
          const rowStart = isLeftSide ? index + 1 : (index - halfPoint) + 1

          return {
            member: memberWithPos.member,
            gridPosition: {
              gridColumn: columnStart,
              gridRow: rowStart,
            },
          }
        }),
      }
    })
  }, [owner.members, isWideLayout])

  return (
    <>
      {groups.map(({ members, fullWidth }, groupIndex) => (
        <React.Fragment key={groupIndex}>

          {/* グルーピングの境界線 */}
          {groupIndex > 0 && (
            <DynamicFormSpacer />
          )}

          {/* グリッドや子フォームなど横幅いっぱいとるもの */}
          {fullWidth && (
            <MemberComponent
              ancestorsPath={ancestorsPath}
              member={members[0].member}
              owner={owner}
              gridColumn={undefined}
              gridRow={undefined}
            />
          )}

          {/* ラベルと値のペアの羅列 */}
          {!fullWidth && members.map((memberWithPos, memberIndex) => (
            <MemberComponent
              key={memberIndex}
              ancestorsPath={ancestorsPath}
              member={memberWithPos.member}
              owner={owner}
              gridColumn={memberWithPos.gridPosition?.gridColumn}
              gridRow={memberWithPos.gridPosition?.gridRow}
            />
          ))}
        </React.Fragment>
      ))}
    </>
  )
}


/** VForm2のラベルと値の組 */
const MemberComponent = ({ owner, member, ancestorsPath, gridColumn, gridRow }: {
  /** ルートオブジェクトからこのメンバーまでのパス */
  ancestorsPath: string
  /** メンバー */
  member: Member
  /** メンバーを保持するオブジェクトのメタデータ */
  owner: MemberOwner
  /** 4列レイアウト時のgrid-column指定 */
  gridColumn?: number
  /** 4列レイアウト時のgrid-row指定 */
  gridRow?: number
}): React.ReactNode => {

  // セクション
  if (member.isSection) {
    return (
      <FormSection
        member={member}
        ancestorsPath={ancestorsPath}
        owner={owner}
      />
    )
  }

  // 配列
  if (member.isArray) {
    return hasArray(member) ? (
      // このメンバーの子孫（直下の子ではなく子孫再帰的に）にさらに配列が含まれるのでフォームのリスト
      <FormArrayAsForm
        member={member}
        ancestorsPath={ancestorsPath}
        owner={owner}
      />
    ) : (
      // グリッドで表示
      <FormArrayAsGrid
        member={member}
        ancestorsPath={ancestorsPath}
        owner={owner}
      />
    )
  }

  // 一般ValueMember
  return (
    <FormValueMember
      member={member}
      owner={owner}
      ancestorsPath={ancestorsPath}
      gridColumn={gridColumn}
      gridRow={gridRow}
    />
  )

}
