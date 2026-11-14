
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : DataStructuresSection.cs
// ※ このファイルを生成したメソッド: Render

import type * as Util from ".."
import * as EnumDefs from "../enum-defs"
import * as 従業員マスタ from "./従業員マスタ"

//#region 型定義
//#region 構造体定義
/** 従業員一括更新Parameterの構造体 */
export type Structure = {
  更新対象従業員一覧: Structure_更新対象従業員一覧[]
}
/** 更新対象従業員一覧の構造体 */
export type Structure_更新対象従業員一覧 = {
  従業員: 従業員マスタ.DisplayData
}
//#endregion 構造体定義

//#region 構造体新規作成用関数
/** 従業員一括更新Parameterの構造体の新しいインスタンスを作成します。 */
export function createNewStructure(): Structure {
  return {
    更新対象従業員一覧: [],
  }
}
/** 更新対象従業員一覧の構造体の新しいインスタンスを作成します。 */
export function createNewStructure_更新対象従業員一覧(): Structure_更新対象従業員一覧 {
  return {
    従業員: 従業員マスタ.createNewDisplayData(),
  }
}
//#endregion 構造体新規作成用関数

//#region 画面表示用データ
/** 従業員一括更新Parameterの画面表示用データ。 */
export type DisplayData = {
  /**
   * 画面初期表示から保存までの間でこのインスタンスを識別する番号。
   * GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
   * エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
   */
  instanceId: string
  /** 更新対象従業員一覧 */
  更新対象従業員一覧: DisplayData_更新対象従業員一覧[]

  /** このデータがDBに保存済みかどうか */
  existsInDatabase: boolean
  /** このデータに更新がかかっているかどうか */
  willBeChanged: boolean
  /** このデータが更新確定時に削除されるかどうか */
  willBeDeleted: boolean
}
/** 更新対象従業員一覧の画面表示用データ。 */
export type DisplayData_更新対象従業員一覧 = {
  /**
   * 画面初期表示から保存までの間でこのインスタンスを識別する番号。
   * GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
   * エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
   */
  instanceId: string
  従業員: 従業員マスタ.DisplayData

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
/** 従業員一括更新Parameterの画面表示用データの新しいインスタンスを作成します。 */
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
    existsInDatabase: false,
    willBeChanged: true,
    willBeDeleted: false,
    更新対象従業員一覧: [],
  }
}
/** 更新対象従業員一覧の画面表示用データの新しいインスタンスを作成します。 */
export function createNewDisplayData_更新対象従業員一覧(): DisplayData_更新対象従業員一覧 {
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
    従業員: 従業員マスタ.createNewDisplayData(),
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

  // 従業員一括更新Parameter の等価比較
  if (areEqual従業員一括更新Parameter(left, right, compareFunction)) {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = false;
  } else {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = true;
    areEqual = false;
  }
  
  // 更新対象従業員一覧 の等価比較
  // 以下いずれかに該当すれば変更ありと判定
  // * 配列の長さが変わっている
  // * 要素の並び替えが発生している（instanceId 基準）
  // * 配列の要素を instanceId で突合しそれぞれの値を比較
  const leftIds = left.更新対象従業員一覧?.map(x => x.instanceId) ?? [];
  const rightIds = right.更新対象従業員一覧?.map(x => x.instanceId) ?? [];
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
  
  const leftDict = new Map(left.更新対象従業員一覧?.map(x => [x.instanceId, x]) ?? []);
  const rightDict = new Map(right.更新対象従業員一覧?.map(x => [x.instanceId, x]) ?? []);
  for (const instanceId of Array.from(new Set([...leftIds, ...rightIds]))) {
    const x = leftDict.get(instanceId);
    const y = rightDict.get(instanceId);
    if (!x && y && option?.setRightObjectWillBeChanged) y.willBeChanged = true;
    if (!x || !y) continue;
    if (areEqual更新対象従業員一覧(x, y, compareFunction)) {
      if (option?.setRightObjectWillBeChanged) y.willBeChanged = false;
    } else {
      if (option?.setRightObjectWillBeChanged) y.willBeChanged = true;
      areEqual = false;
    }
  }

  return areEqual;

  // ------ 値比較関数 --------

  function areEqual従業員一括更新Parameter(left: DisplayData, right: DisplayData, equals: Exclude<Util.DisplayDataDeepEqualOption["compareFunction"], undefined>): boolean {
    if ((left.willBeDeleted ?? false) !== (right.willBeDeleted ?? false)) return false;
    return true;
  }

  function areEqual更新対象従業員一覧(left: DisplayData_更新対象従業員一覧, right: DisplayData_更新対象従業員一覧, equals: Exclude<Util.DisplayDataDeepEqualOption["compareFunction"], undefined>): boolean {
    if (!equals('Word', left.従業員?.従業員番号, right.従業員?.従業員番号)) return false;
    if (!equals('Word', left.従業員?.氏名, right.従業員?.氏名)) return false;
    if (!equals('Integer', left.従業員?.Version, right.従業員?.Version)) return false;
    // 従業員一括更新Parameter.更新対象従業員一覧.従業員.従業員マスタ.権限 はディープイコールの対象となりません。
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
    uniqueId: '28bf1f9e-8b77-4a78-938d-e29578c44da3',
    displayName: '従業員一括更新Parameter',
    members: ['更新対象従業員一覧'],
  },
  displayData: {
    '更新対象従業員一覧': {
      kind: 'children',
      parent: null,
      isOutsideTree: false,
      uniqueId: '4f82b721-bd04-4279-8acb-ed14ab690eaf',
      displayName: '更新対象従業員一覧',
      members: ['更新対象従業員一覧.従業員'],
    },
    '更新対象従業員一覧.従業員': {
      kind: 'ref',
      parent: '更新対象従業員一覧',
      isOutsideTree: false,
      uniqueId: '46c347d6-20f0-453b-814f-45093b7923c7',
      refTo: 'ref-to:従業員マスタ',
      displayName: '従業員',
      refToObject: 'DisplayData',
      members: ['更新対象従業員一覧.従業員.従業員番号', '更新対象従業員一覧.従業員.氏名', '更新対象従業員一覧.従業員.Version', '更新対象従業員一覧.従業員.従業員マスタの権限'],
    },
    '更新対象従業員一覧.従業員.従業員番号': {
      kind: 'value',
      parent: '更新対象従業員一覧.従業員',
      isOutsideTree: true,
      uniqueId: '525a40bd-74c1-4c09-aea1-4cc238e05c59',
      type: 'word',
      displayName: '従業員番号',
      stringSearchBehavior: 'Exact',
    },
    '更新対象従業員一覧.従業員.氏名': {
      kind: 'value',
      parent: '更新対象従業員一覧.従業員',
      isOutsideTree: true,
      uniqueId: '3ea7c1ea-ab69-47a5-9821-f9ad112e65e2',
      type: 'word',
      displayName: '氏名',
    },
    '更新対象従業員一覧.従業員.Version': {
      kind: 'value',
      parent: '更新対象従業員一覧.従業員',
      isOutsideTree: true,
      uniqueId: '8d4bc54f-4621-426d-bbd1-82624c07dc62',
      type: 'int',
      displayName: 'Version',
      comment: '楽観排他制御用',
    },
    '更新対象従業員一覧.従業員.従業員マスタの権限': {
      kind: 'children',
      parent: '更新対象従業員一覧.従業員',
      isOutsideTree: true,
      uniqueId: 'ac564251-087c-4545-8800-032fdd129fef',
      displayName: '従業員マスタの権限',
      members: ['更新対象従業員一覧.従業員.従業員マスタの権限.権限区分'],
    },
    '更新対象従業員一覧.従業員.従業員マスタの権限.権限区分': {
      kind: 'value',
      parent: '更新対象従業員一覧.従業員.従業員マスタの権限',
      isOutsideTree: true,
      uniqueId: 'e999e4e8-b28c-47f5-89f8-d68d08aad589',
      enumType: '権限区分',
      displayName: '権限区分',
    },
  },
} as const satisfies Util.AggregateMetadata.Entry

//#endregion メタデータ

//#endregion 関数
