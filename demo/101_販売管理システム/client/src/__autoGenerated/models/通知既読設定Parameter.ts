
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : DataStructuresSection.cs
// ※ このファイルを生成したメソッド: Render
// ※ このファイルを生成した nijo のバージョン: 0.0.0-dev

import type * as Util from ".."
import * as EnumDefs from "../enum-defs"

//#region 型定義
//#region 構造体定義
/** 通知既読設定Parameterの構造体 */
export type Structure = {
  通知ID: string | undefined
  既読: boolean | undefined
}
//#endregion 構造体定義

//#region 構造体新規作成用関数
/** 通知既読設定Parameterの構造体の新しいインスタンスを作成します。 */
export function createNewStructure(): Structure {
  return {
    通知ID: undefined,
    既読: undefined,
  }
}
//#endregion 構造体新規作成用関数

//#region 画面表示用データ
/** 通知既読設定Parameterの画面表示用データ。 */
export type DisplayData = {
  /**
   * 画面初期表示から保存までの間でこのインスタンスを識別する番号。
   * GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
   * エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
   */
  instanceId: string
  通知ID?: string | null
  /**
   * true なら既読に、false なら未読にする。
   */
  既読?: boolean | null

  /** このデータがDBに保存済みかどうか */
  existsInDatabase: boolean
  /** このデータに更新がかかっているかどうか */
  willBeChanged: boolean
  /** このデータが更新確定時に削除されるかどうか */
  willBeDeleted: boolean
}
//#endregion 画面表示用データ

//#endregion 型定義


//#region 関数
//#region 画面表示用データ新規作成用関数
/** 通知既読設定Parameterの画面表示用データの新しいインスタンスを作成します。 */
export function createNewDisplayData(): DisplayData {
  // タイムスタンプ(ミリ秒) + ランダム文字列 で一意のIDを生成
  function generateRandomUniqueId(): string {
    return [
      Date.now().toString(36).substring(0, 8),
      Math.random().toString(36).substring(2, 6),
      Math.random().toString(36).substring(2, 6),
      Math.random().toString(36).substring(2, 6),
      Math.random().toString(36).replace('.', ''),
    ].join('-')
  }

  return {
    instanceId: generateRandomUniqueId(),
    通知ID: '',
    既読: false,
    existsInDatabase: false,
    willBeChanged: true,
    willBeDeleted: false,
  }
}
//#endregion 画面表示用データ新規作成用関数

//#region ディープイコール関数
/**
 * 2つの画面表示用オブジェクトをディープイコールで比較し、等しい場合はtrueを返す関数。
 *
 * * 以下は差があっても変更なしと判定します。
 *   * ネストされたオブジェクトや配列の参照等価性
 *   * existsInDatabase の差分
 *   * willBeChanged の差分
 * * 以下は変更ありと判定します。
 *   * 配列の要素の並び替え。 instanceId の順番で比較します。
 *   * willBeDeleted の差分
 * * 以下はオプションでルールを変更できます。
 *   * stringやnumberの値比較ルール。
 *     例えばnullとundefinedと空文字を同じとみなすかどうかなど。
 *     未指定の場合は Object.is が使用されます。
 *
 * @param left 比較対象のオブジェクト
 * @param right 比較対象のオブジェクト。オプションを指定すると、差分があるオブジェクトの willBeChanged を自動的に変更することもできます。
 * @param option 比較ルールのオプション
 */
export function deepEqual(left: DisplayData, right: DisplayData, option?: Util.DisplayDataDeepEqualOption): boolean {
  const compareFunction: Exclude<Util.DisplayDataDeepEqualOption["compareFunction"], undefined> = option?.compareFunction ?? ((_, left, right, __?, ___?) => Object.is(left, right));
  let areEqual = true;

  // 通知既読設定Parameter の等価比較
  if (areEqual通知既読設定Parameter(left, right, compareFunction)) {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = false;
  } else {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = true;
    areEqual = false;
  }

  return areEqual;

  // ------ 値比較関数 --------

  function areEqual通知既読設定Parameter(left: DisplayData, right: DisplayData, equals: Exclude<Util.DisplayDataDeepEqualOption["compareFunction"], undefined>): boolean {
    if (!equals('Word', left.通知ID, right.通知ID)) return false;
    if (!equals('Boolean', left.既読, right.既読)) return false;
    if ((left.willBeDeleted ?? false) !== (right.willBeDeleted ?? false)) return false;
    return true;
  }
}
//#endregion ディープイコール関数

//#region メタデータ

/** この集約のメタデータ */
export const metadata = {
  root: {
    kind: 'root',
    isStored: false,
    isQueryable: false,
    uniqueId: '67e95f3a-6a95-4ae0-8069-b2fbdc68e430',
    displayName: '通知既読設定Parameter',
    members: ['通知ID', '既読'],
  },
  displayData: {
    '通知ID': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'da562d73-d355-45a3-86d6-8f3a9f265617',
      type: 'word',
      displayName: '通知ID',
    },
    '既読': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'aff0fac4-e312-4b39-bfce-385ef5ec897e',
      type: 'bool',
      displayName: '既読',
      comment: 'true なら既読に、false なら未読にする。',
    },
  },
} as const satisfies Util.AggregateMetadata.Entry

//#endregion メタデータ

//#endregion 関数
