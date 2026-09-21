/**
 * ドット区切りのパス（例: "商品.外部システム側ID"）を使って、行オブジェクトの深い階層にある値を
 * イミュータブルに書き換えた新しいオブジェクトを返す。
 * EditableGrid の textToCell は行オブジェクトのディープクローンを返す必要があるため、
 * structuredClone で行全体を複製した上で対象パスだけ書き換える。
 */
export function setFieldByPath<TRow>(row: TRow, path: string, value: unknown): TRow {
  const clone = structuredClone(row) as Record<string, any>
  const keys = path.split('.')
  let target = clone
  for (let i = 0; i < keys.length - 1; i++) {
    // 途中のオブジェクトが未作成（ref未選択など）の場合はここで作る。
    // 作らないと target が undefined になり、末端への代入で例外になる。
    if (target[keys[i]] === null || target[keys[i]] === undefined) {
      target[keys[i]] = {}
    }
    target = target[keys[i]]
  }
  target[keys[keys.length - 1]] = value
  return clone as TRow
}
