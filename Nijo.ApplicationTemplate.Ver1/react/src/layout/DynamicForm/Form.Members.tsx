import React from "react"
import { Member, MemberOwner } from "./types"
import { hasArray } from "./helpers"
import { FormValueMember } from "./Form.ValueMember"
import { FormSection } from "./Form.Section"
import { FormArrayAsForm } from "./Form.ArrayAsForm"
import { FormArrayAsGrid } from "./Form.ArrayAsGrid"
import ResponsiveForm from "../ResponsiveForm"
import { ResponsiveFormContext } from "../ResponsiveForm/ResponsiveFormContext"

/**
 * メンバーをAutoColumnの単位にグルーピングしてレンダリングする。
 */
export const MembersGroupByBreakPoint = ({ owner, ancestorsPath }: {
  owner: MemberOwner
  /** ルートオブジェクトからownerまでのパス */
  ancestorsPath: string
}) => {
  const { isWideLayout } = React.useContext(ResponsiveFormContext)

  // メンバーを折り返しの単位でグルーピングする
  const groups = React.useMemo(() => {
    // 最初に基本的なグルーピングを行う
    type MemberGroup = {
      /** グリッド配置情報とメンバーの組 */
      members: Member[]
      fullWidth: boolean
    }
    const baseGroups = owner.members.reduce((acc, member) => {
      // Child, Children, fullWidth指定のメンバーは横幅いっぱいとる
      if (member.isSection || member.isArray || member.fullWidth) {
        acc.push({
          members: [member],
          fullWidth: true
        })
        return acc
      }

      // それ以外はグルーピングする
      const lastGroup = acc[acc.length - 1]
      if (lastGroup === undefined || lastGroup.fullWidth) {
        acc.push({
          members: [member],
          fullWidth: false
        })
      } else {
        lastGroup.members.push(member)
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

      return {
        ...group,
        members: group.members,
      }
    })
  }, [owner.members, isWideLayout])

  return (
    <>
      {groups.flatMap(g => g.members.map(m => ({ member: m, fullWidth: g.fullWidth }))).map(({ member, fullWidth }, index) => (
        <ResponsiveForm.Item
          key={index}
          fullWidth={fullWidth}
        >
          <MemberComponent
            ancestorsPath={ancestorsPath}
            member={member}
            owner={owner}
          />
        </ResponsiveForm.Item>
      ))}
    </>
  )
}


/** VForm2のラベルと値の組 */
const MemberComponent = ({ owner, member, ancestorsPath }: {
  /** ルートオブジェクトからこのメンバーまでのパス */
  ancestorsPath: string
  /** メンバー */
  member: Member
  /** メンバーを保持するオブジェクトのメタデータ */
  owner: MemberOwner
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
    />
  )

}
