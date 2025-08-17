import * as ReactHookForm from "react-hook-form"
import { MemberOwner, NoneMember } from "./types"
import React from "react"
import { DynamicFormContext } from "./DynamicFormContext"

/**
 * フォームでNoneMemberを表示するコンポーネント。
 * なおグリッドの列定義での表示は `Form.ArrayAsGrid` で行う。
 */
export const FormNone = ({ member, owner, ancestorsPath }: {
  /** メンバー */
  member: NoneMember
  /** メンバーを保持するオブジェクトのメタデータ */
  owner: MemberOwner
  /** ルートオブジェクトからこのメンバーまでのパス */
  ancestorsPath: string
}) => {

  const { useFormReturn } = React.useContext(DynamicFormContext)

  return (
    <div className={member.fullWidth ? "col-span-full" : "col-span-2"}>
      {member.renderForm?.({
        useFormReturn,
        ancestorsPath,
        owner,
      })}
    </div>
  )
}