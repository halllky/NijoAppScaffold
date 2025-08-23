import React from "react"
import { Member, MemberOwner } from "./types"
import { FormValueMember } from "./Form.ValueMember"
import { FormSection } from "./Form.Section"
import { FormArrayAsForm } from "./Form.ArrayAsForm"
import { FormArrayAsGrid } from "./Form.ArrayAsGrid"
import FormLayout from "../FormLayout"
import { FormLayoutContext } from "../FormLayout/internal-context"

/**
 * メンバーをAutoColumnの単位にグルーピングしてレンダリングする。
 */
export const MembersGroupByBreakPoint = ({ owner, ancestorsPath }: {
  owner: MemberOwner
  /** ルートオブジェクトからownerまでのパス */
  ancestorsPath: string
}) => {
  const { columnCount } = React.useContext(FormLayoutContext)

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
      if (member.type === 'section' || member.type === 'array' || member.fullWidth) {
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
    if (columnCount === 1) {
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
  }, [owner.members, columnCount])

  return groups.map(({ members, fullWidth }, groupIndex) => (
    <React.Fragment key={groupIndex}>
      {groupIndex > 0 && (
        <FormLayout.Separator />
      )}

      {fullWidth ? (
        // FormLayout.Item は各々の MemberComponent で処理
        <MemberComponent
          ancestorsPath={ancestorsPath}
          member={members[0]}
          owner={owner}
        />
      ) : (
        // membersを半分に割って2つのColumnに配置
        <FormLayout.ResponsiveColumnGroup>
          <FormLayout.ResponsiveColumn>
            {members.slice(0, Math.ceil(members.length / 2)).map((member, index) => (
              <MemberComponent
                key={index}
                ancestorsPath={ancestorsPath}
                member={member}
                owner={owner}
              />
            ))}
          </FormLayout.ResponsiveColumn>
          <FormLayout.ResponsiveColumn>
            {members.slice(Math.ceil(members.length / 2)).map((member, index) => (
              <MemberComponent
                key={index}
                ancestorsPath={ancestorsPath}
                member={member}
                owner={owner}
              />
            ))}
          </FormLayout.ResponsiveColumn>
        </FormLayout.ResponsiveColumnGroup>
      )}
    </React.Fragment>
  ))
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
  if (member.type === 'section') {
    return (
      <FormSection
        member={member}
        ancestorsPath={ancestorsPath}
        owner={owner}
      />
    )
  }

  // 配列
  if (member.type === 'array') {
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

/**
 * 子孫に配列が含まれるかを返す
 */
const hasArray = (owner: MemberOwner): boolean => {
  const checkRecursive = (o: MemberOwner): boolean => {
    return o.members.some(member => {
      if (member.type === 'array') {
        return true
      }
      if (member.type === 'section') {
        return checkRecursive(member)
      }
      return false
    })
  }
  return checkRecursive(owner)
}
