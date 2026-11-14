
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : DataStructuresSection.cs
// ※ このファイルを生成したメソッド: Render

import type * as Util from ".."
import * as EnumDefs from "../enum-defs"
import * as 従業員Ref from "./従業員Ref"

//#region 型定義
//#region 構造体定義
/** パスワード再発行Parameterの構造体 */
export type Structure = {
  対象者: 従業員Ref.RefTarget
}
//#endregion 構造体定義

//#region 構造体新規作成用関数
/** パスワード再発行Parameterの構造体の新しいインスタンスを作成します。 */
export function createNewStructure(): Structure {
  return {
    対象者: 従業員Ref.createNewRefTarget(),
  }
}
//#endregion 構造体新規作成用関数

//#region 画面表示用データ
/** パスワード再発行Parameterの画面表示用データ。 */
export type DisplayData = {
  /**
   * 画面初期表示から保存までの間でこのインスタンスを識別する番号。
   * GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
   * エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
   */
  instanceId: string
  対象者: 従業員Ref.RefTarget

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
/** パスワード再発行Parameterの画面表示用データの新しいインスタンスを作成します。 */
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
    対象者: 従業員Ref.createNewRefTarget(),
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

  // パスワード再発行Parameter の等価比較
  if (areEqualパスワード再発行Parameter(left, right, compareFunction)) {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = false;
  } else {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = true;
    areEqual = false;
  }

  return areEqual;

  // ------ 値比較関数 --------

  function areEqualパスワード再発行Parameter(left: DisplayData, right: DisplayData, equals: Exclude<Util.DisplayDataDeepEqualOption["compareFunction"], undefined>): boolean {
    if (!equals('Word', left.対象者?.従業員番号, right.対象者?.従業員番号)) return false;
    if (!equals('Word', left.対象者?.氏名, right.対象者?.氏名)) return false;
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
    uniqueId: 'ca0a28e1-6792-4d8b-ad9e-4a373ba190fc',
    displayName: 'パスワード再発行Parameter',
    members: ['対象者'],
  },
  displayData: {
    '対象者': {
      kind: 'ref',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'c804ad2e-ce78-469c-b58d-447d7561cb86',
      refTo: 'ref-to:従業員Ref',
      displayName: '対象者',
      refToObject: 'RefTarget',
      members: ['対象者.従業員番号', '対象者.氏名'],
    },
    '対象者.従業員番号': {
      kind: 'value',
      parent: '対象者',
      isOutsideTree: true,
      uniqueId: '91f94ec3-0cff-4468-84df-dbba8091f1c8',
      type: 'word',
      displayName: '従業員番号',
      stringSearchBehavior: 'Exact',
    },
    '対象者.氏名': {
      kind: 'value',
      parent: '対象者',
      isOutsideTree: true,
      uniqueId: '5fd0ce8d-d1fc-41cd-9a56-96db91fc7e8b',
      type: 'word',
      displayName: '氏名',
    },
  },
} as const satisfies Util.AggregateMetadata.Entry

//#endregion メタデータ

//#endregion 関数
