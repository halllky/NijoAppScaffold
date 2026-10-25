/**
 * EditableGrid のセルの中身に付ける背景色のクラスを返す。
 * グリッド自体の背景はグレーなので、編集可能なセルだけを白く塗って読み取り専用のセルと見分けられるようにする。
 * このクラスを付ける要素は、セルの幅と高さいっぱいに広がるようにすること。
 *
 * @param isReadOnly EditableGrid の renderBody に渡される、そのセルが読み取り専用かどうか
 * @param hasError そのセルにバリデーションエラーがあるかどうか。ある場合は編集可否に関わらずエラーの色になる
 */
export const getGridCellBgClassName = (isReadOnly: boolean, hasError?: boolean): string => {
  if (hasError) return 'bg-amber-300/50'
  return isReadOnly ? '' : 'bg-white'
}
