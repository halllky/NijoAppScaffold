
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : DataStructuresSection.cs
// ※ このファイルを生成したメソッド: Render

import type * as Util from ".."
import * as EnumDefs from "../enum-defs"

//#region 型定義
//#region 構造体定義
/** ログインユーザー情報の構造体 */
export type Structure = {
  従業員番号: string | undefined
  氏名: string | undefined
  ログインユーザー情報の権限: Structure_ログインユーザー情報の権限[]
}
/** 権限の構造体 */
export type Structure_ログインユーザー情報の権限 = {
  権限区分: EnumDefs.権限区分 | undefined
}
//#endregion 構造体定義

//#region 構造体新規作成用関数
/** ログインユーザー情報の構造体の新しいインスタンスを作成します。 */
export function createNewStructure(): Structure {
  return {
    従業員番号: undefined,
    氏名: undefined,
    ログインユーザー情報の権限: [],
  }
}
/** 権限の構造体の新しいインスタンスを作成します。 */
export function createNewStructure_ログインユーザー情報の権限(): Structure_ログインユーザー情報の権限 {
  return {
    権限区分: undefined,
  }
}
//#endregion 構造体新規作成用関数

//#region 画面表示用データ
/** ログインユーザー情報の画面表示用データ。 */
export type DisplayData = {
  /**
   * 画面初期表示から保存までの間でこのインスタンスを識別する番号。
   * GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
   * エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
   */
  instanceId: string
  従業員番号?: string | null
  氏名?: string | null
  /** 権限 */
  ログインユーザー情報の権限: DisplayData_ログインユーザー情報の権限[]

  /** このデータがDBに保存済みかどうか */
  existsInDatabase: boolean
  /** このデータに更新がかかっているかどうか */
  willBeChanged: boolean
  /** このデータが更新確定時に削除されるかどうか */
  willBeDeleted: boolean
}
/** 権限の画面表示用データ。 */
export type DisplayData_ログインユーザー情報の権限 = {
  /**
   * 画面初期表示から保存までの間でこのインスタンスを識別する番号。
   * GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
   * エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
   */
  instanceId: string
  権限区分?: EnumDefs.権限区分 | null

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
/** ログインユーザー情報の画面表示用データの新しいインスタンスを作成します。 */
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
    従業員番号: '',
    氏名: '',
    existsInDatabase: false,
    willBeChanged: true,
    willBeDeleted: false,
    ログインユーザー情報の権限: [],
  }
}
/** 権限の画面表示用データの新しいインスタンスを作成します。 */
export function createNewDisplayData_ログインユーザー情報の権限(): DisplayData_ログインユーザー情報の権限 {
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
    権限区分: null,
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

  // ログインユーザー情報 の等価比較
  if (areEqualログインユーザー情報(left, right, compareFunction)) {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = false;
  } else {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = true;
    areEqual = false;
  }
  
  // 権限 の等価比較
  // 以下いずれかに該当すれば変更ありと判定
  // * 配列の長さが変わっている
  // * 要素の並び替えが発生している（instanceId 基準）
  // * 配列の要素を instanceId で突合しそれぞれの値を比較
  const leftIds = left.ログインユーザー情報の権限?.map(x => x.instanceId) ?? [];
  const rightIds = right.ログインユーザー情報の権限?.map(x => x.instanceId) ?? [];
  if (leftIds.length !== rightIds.length) {
    areEqual = false;
  } else {
    for (let i = 0; i < leftIds.length; i++) {
      if (leftIds[i] !== rightIds[i]) {
        areEqual = false;
        break;
      }
    }
  }
  
  const leftDict = new Map(left.ログインユーザー情報の権限?.map(x => [x.instanceId, x]) ?? []);
  const rightDict = new Map(right.ログインユーザー情報の権限?.map(x => [x.instanceId, x]) ?? []);
  for (const instanceId of Array.from(new Set([...leftIds, ...rightIds]))) {
    const x = leftDict.get(instanceId);
    const y = rightDict.get(instanceId);
    if (!x && y && option?.setRightObjectWillBeChanged) y.willBeChanged = true;
    if (!x || !y) continue;
    if (areEqualログインユーザー情報の権限(x, y, compareFunction)) {
      if (option?.setRightObjectWillBeChanged) y.willBeChanged = false;
    } else {
      if (option?.setRightObjectWillBeChanged) y.willBeChanged = true;
      areEqual = false;
    }
  }

  return areEqual;

  // ------ 値比較関数 --------

  function areEqualログインユーザー情報(left: DisplayData, right: DisplayData, equals: Exclude<Util.DisplayDataDeepEqualOption["compareFunction"], undefined>): boolean {
    if (!equals('Word', left.従業員番号, right.従業員番号)) return false;
    if (!equals('Word', left.氏名, right.氏名)) return false;
    if ((left.willBeDeleted ?? false) !== (right.willBeDeleted ?? false)) return false;
    return true;
  }

  function areEqualログインユーザー情報の権限(left: DisplayData_ログインユーザー情報の権限, right: DisplayData_ログインユーザー情報の権限, equals: Exclude<Util.DisplayDataDeepEqualOption["compareFunction"], undefined>): boolean {
    if (!equals('Enum', left.権限区分, right.権限区分, '権限区分')) return false;
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
    uniqueId: 'f594d8ac-f7ff-44ca-878f-fcbfd1e79ee1',
    displayName: 'ログインユーザー情報',
    members: ['従業員番号', '氏名', 'ログインユーザー情報の権限'],
  },
  displayData: {
    '従業員番号': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'c608a333-3561-445b-98e4-033f00baa5d1',
      type: 'word',
      displayName: '従業員番号',
    },
    '氏名': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '59db479e-8398-429e-928c-53639f6e6f19',
      type: 'word',
      displayName: '氏名',
    },
    'ログインユーザー情報の権限': {
      kind: 'children',
      parent: null,
      isOutsideTree: false,
      uniqueId: '41ff739a-6477-4f0f-bda4-d65e5a7642ed',
      displayName: 'ログインユーザー情報の権限',
      members: ['ログインユーザー情報の権限.権限区分'],
    },
    'ログインユーザー情報の権限.権限区分': {
      kind: 'value',
      parent: 'ログインユーザー情報の権限',
      isOutsideTree: false,
      uniqueId: 'd322cef3-4a00-4242-8243-1d72b93c768b',
      enumType: '権限区分',
      displayName: '権限区分',
    },
  },
} as const satisfies Util.AggregateMetadata.Entry

//#endregion メタデータ

//#endregion 関数
