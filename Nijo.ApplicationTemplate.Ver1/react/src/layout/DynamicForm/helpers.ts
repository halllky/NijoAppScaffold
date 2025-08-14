import { MemberOwner } from "./types"

/**
 * 子孫の最大の深さを返す。
 * ネストされたセクションや、子孫に配列を持つ配列がある場合に1ずつ増える。
 * 引数のオブジェクトがそういったメンバーを持たないときの値は0。
 */
export const countFormDepth = (owner: MemberOwner): number => {
  const checkRecursive = (o: MemberOwner, depth: number): number => {
    return o.members.reduce((maxDepth, member) => {
      // ネストされたセクション
      if (member.isSection) {
        return Math.max(maxDepth, checkRecursive(member, depth + 1))
      }
      // 配列は子孫に配列を持つか否かで深さが変わる。
      // 子孫に配列を持たない配列はグリッドで表示されるためそれ以上深くならない。
      if (member.isArray && hasArray(member)) {
        return Math.max(maxDepth, checkRecursive(member, depth + 1))
      }

      return maxDepth
    }, depth)
  }
  return checkRecursive(owner, 0)
}

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
