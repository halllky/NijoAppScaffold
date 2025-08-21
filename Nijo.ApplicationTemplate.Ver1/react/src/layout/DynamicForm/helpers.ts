import { MemberOwner } from "./types"

/**
 * 子孫に配列が含まれるかを返す
 */
export const hasArray = (owner: MemberOwner): boolean => {
  const checkRecursive = (o: MemberOwner): boolean => {
    return o.members.some(member => {
      if (member.isArray) {
        return true
      }
      if (member.isSection) {
        return checkRecursive(member)
      }
      return false
    })
  }
  return checkRecursive(owner)
}
