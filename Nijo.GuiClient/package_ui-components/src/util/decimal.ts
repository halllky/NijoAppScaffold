import Decimal from "decimal.js"

/**
 * 全角数字、カンマ区切り、句点が小数点として使われている、
 * などといった表記ゆれを正規化する。
 */
export const normalizeAsNumber = (value: string): string => {
  return value
    .replace(/(\s|　|)/g, '') // 半角スペース、全角スペースを除去
    .replace(/,/g, '') // カンマ区切りを除去
    .replace('。', '.') // 句点は小数点とみなす
    .normalize('NFKC') // 全角数字を半角数字に変換
}

/**
 * 数値や文字列を decimal.js のインスタンスに安全に変換する
 */
export const asDecimalSafety = (value: string | number | Decimal | null | undefined): Decimal | null => {
  if (value instanceof Decimal) return value
  if (value === '' || value === null || value === undefined) return null
  if (typeof value === 'number') return new Decimal(value)
  if (typeof value === 'string') {
    try {
      return new Decimal(normalizeAsNumber(value))
    } catch {
      return null
    }
  }
  return null
}
