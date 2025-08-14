import React from "react"
import { VForm2 } from "../VForm2"
import { Member, MemberOwner } from "./types"
import { hasArray } from "./helpers"
import { FormValueMember } from "./Form.ValueMember"
import { FormSection } from "./Form.Section"
import { FormArrayAsForm } from "./Form.ArrayAsForm"
import { FormArrayAsGrid } from "./Form.ArrayAsGrid"

/**
 * メンバーをAutoColumnの単位にグルーピングしてレンダリングする。
 */
export const MembersGroupByBreakPoint = ({ owner, ancestorsPath }: {
  owner: MemberOwner
  /** ルートオブジェクトからownerまでのパス */
  ancestorsPath: string
}) => {
  // メンバーを折り返しの単位でグルーピングする
  const groups = React.useMemo(() => {
    return owner.members.reduce((acc, member) => {
      // Child, Children は横幅いっぱいとる
      if (member.isSection || member.isArray) {
        acc.push({ members: [member], fullWidth: true })
        return acc
      }

      // それ以外はグルーピングする
      const lastGroup = acc[acc.length - 1]
      if (lastGroup === undefined) {
        acc.push({ members: [member], fullWidth: false })
      } else {
        lastGroup.members.push(member)
      }
      return acc
    }, [] as { members: Member[], fullWidth: boolean }[])
  }, [owner])

  return (
    <>
      {groups.map(({ members, fullWidth }, index) => fullWidth ? (
        <MemberComponent
          key={index}
          ancestorsPath={ancestorsPath}
          member={members[0]}
          owner={owner}
        />
      ) : (
        <VForm2.AutoColumn key={index} childrenCount={members.length}>
          {members.map((member) => (
            <MemberComponent
              key={member.physicalName}
              ancestorsPath={ancestorsPath}
              member={member}
              owner={owner}
            />
          ))}
        </VForm2.AutoColumn>
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
  if (member.type) {
    return (
      <FormValueMember
        member={member}
        owner={owner}
        ancestorsPath={ancestorsPath}
      />
    )
  }
}
