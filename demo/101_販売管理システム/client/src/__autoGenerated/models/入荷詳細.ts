
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : DataStructuresSection.cs
// ※ このファイルを生成したメソッド: Render

import type * as Util from ".."
import * as EnumDefs from "../enum-defs"
import * as 商品 from "./商品"
import * as 従業員Ref from "./従業員Ref"

//#region 型定義
//#region 構造体定義
/** 入荷詳細の構造体 */
export type Structure = {
  入荷ID: string | undefined
  入荷日時: string | undefined
  担当者: 従業員Ref.RefTarget
  備考: string | undefined
  Version: string | undefined
  入荷商品一覧: Structure_入荷商品一覧[]
}
/** 入荷商品一覧の構造体 */
export type Structure_入荷商品一覧 = {
  入荷明細ID: string | undefined
  商品: 商品.RefTarget
  数量: string | undefined
  仕入単価_税抜: string | undefined
  消費税区分: EnumDefs.消費税区分 | undefined
  備考: string | undefined
  Version: string | undefined
}
//#endregion 構造体定義

//#region 構造体新規作成用関数
/** 入荷詳細の構造体の新しいインスタンスを作成します。 */
export function createNewStructure(): Structure {
  return {
    入荷ID: undefined,
    入荷日時: undefined,
    担当者: 従業員Ref.createNewRefTarget(),
    備考: undefined,
    Version: undefined,
    入荷商品一覧: [],
  }
}
/** 入荷商品一覧の構造体の新しいインスタンスを作成します。 */
export function createNewStructure_入荷商品一覧(): Structure_入荷商品一覧 {
  return {
    入荷明細ID: undefined,
    商品: 商品.createNewRefTarget(),
    数量: undefined,
    仕入単価_税抜: undefined,
    消費税区分: undefined,
    備考: undefined,
    Version: undefined,
  }
}
//#endregion 構造体新規作成用関数

//#region 画面表示用データ
/** 入荷詳細の画面表示用データ。 */
export type DisplayData = {
  /**
   * 画面初期表示から保存までの間でこのインスタンスを識別する番号。
   * GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
   * エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
   */
  instanceId: string
  /**
   * 非表示。UUID。自動採番
   */
  入荷ID?: string | null
  入荷日時?: string | null
  担当者: 従業員Ref.RefTarget
  備考?: string | null
  /**
   * 楽観排他用
   */
  Version?: string | null
  /** 入荷商品一覧 */
  入荷商品一覧: DisplayData_入荷商品一覧[]

  /** このデータがDBに保存済みかどうか */
  existsInDatabase: boolean
  /** このデータに更新がかかっているかどうか */
  willBeChanged: boolean
  /** このデータが更新確定時に削除されるかどうか */
  willBeDeleted: boolean
}
/** 入荷商品一覧の画面表示用データ。 */
export type DisplayData_入荷商品一覧 = {
  /**
   * 画面初期表示から保存までの間でこのインスタンスを識別する番号。
   * GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
   * エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
   */
  instanceId: string
  入荷明細ID?: string | null
  商品: 商品.RefTarget
  数量?: string | null
  仕入単価_税抜?: string | null
  消費税区分?: EnumDefs.消費税区分 | null
  備考?: string | null
  /**
   * 楽観排他用
   */
  Version?: string | null

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
/** 入荷詳細の画面表示用データの新しいインスタンスを作成します。 */
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
    入荷ID: '',
    入荷日時: '',
    担当者: 従業員Ref.createNewRefTarget(),
    備考: '',
    Version: '',
    existsInDatabase: false,
    willBeChanged: true,
    willBeDeleted: false,
    入荷商品一覧: [],
  }
}
/** 入荷商品一覧の画面表示用データの新しいインスタンスを作成します。 */
export function createNewDisplayData_入荷商品一覧(): DisplayData_入荷商品一覧 {
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
    入荷明細ID: '',
    商品: 商品.createNewRefTarget(),
    数量: '',
    仕入単価_税抜: '',
    消費税区分: null,
    備考: '',
    Version: '',
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

  // 入荷詳細 の等価比較
  if (areEqual入荷詳細(left, right, compareFunction)) {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = false;
  } else {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = true;
    areEqual = false;
  }
  
  // 入荷商品一覧 の等価比較
  // 以下いずれかに該当すれば変更ありと判定
  // * 配列の長さが変わっている
  // * 要素の並び替えが発生している（instanceId 基準）
  // * 配列の要素を instanceId で突合しそれぞれの値を比較
  const leftIds = left.入荷商品一覧?.map(x => x.instanceId) ?? [];
  const rightIds = right.入荷商品一覧?.map(x => x.instanceId) ?? [];
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
  
  const leftDict = new Map(left.入荷商品一覧?.map(x => [x.instanceId, x]) ?? []);
  const rightDict = new Map(right.入荷商品一覧?.map(x => [x.instanceId, x]) ?? []);
  for (const instanceId of Array.from(new Set([...leftIds, ...rightIds]))) {
    const x = leftDict.get(instanceId);
    const y = rightDict.get(instanceId);
    if (!x && y && option?.setRightObjectWillBeChanged) y.willBeChanged = true;
    if (!x || !y) continue;
    if (areEqual入荷商品一覧(x, y, compareFunction)) {
      if (option?.setRightObjectWillBeChanged) y.willBeChanged = false;
    } else {
      if (option?.setRightObjectWillBeChanged) y.willBeChanged = true;
      areEqual = false;
    }
  }

  return areEqual;

  // ------ 値比較関数 --------

  function areEqual入荷詳細(left: DisplayData, right: DisplayData, equals: Exclude<Util.DisplayDataDeepEqualOption["compareFunction"], undefined>): boolean {
    if (!equals('Word', left.入荷ID, right.入荷ID)) return false;
    if (!equals('DateTime', left.入荷日時, right.入荷日時)) return false;
    if (!equals('Word', left.担当者?.従業員番号, right.担当者?.従業員番号)) return false;
    if (!equals('Word', left.担当者?.氏名, right.担当者?.氏名)) return false;
    if (!equals('Description', left.備考, right.備考)) return false;
    if (!equals('Integer', left.Version, right.Version)) return false;
    if ((left.willBeDeleted ?? false) !== (right.willBeDeleted ?? false)) return false;
    return true;
  }

  function areEqual入荷商品一覧(left: DisplayData_入荷商品一覧, right: DisplayData_入荷商品一覧, equals: Exclude<Util.DisplayDataDeepEqualOption["compareFunction"], undefined>): boolean {
    if (!equals('Word', left.入荷明細ID, right.入荷明細ID)) return false;
    if (!equals('Sequence', left.商品?.商品SEQ, right.商品?.商品SEQ)) return false;
    if (!equals('Word', left.商品?.外部システム側ID, right.商品?.外部システム側ID)) return false;
    if (!equals('Word', left.商品?.商品名, right.商品?.商品名)) return false;
    if (!equals('Decimal', left.商品?.売値単価_税抜, right.商品?.売値単価_税抜)) return false;
    if (!equals('Enum', left.商品?.消費税区分, right.商品?.消費税区分, '消費税区分')) return false;
    if (!equals('Integer', left.数量, right.数量)) return false;
    if (!equals('Decimal', left.仕入単価_税抜, right.仕入単価_税抜)) return false;
    if (!equals('Enum', left.消費税区分, right.消費税区分, '消費税区分')) return false;
    if (!equals('Description', left.備考, right.備考)) return false;
    if (!equals('Integer', left.Version, right.Version)) return false;
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
    uniqueId: 'bf2453f1-2a89-4209-ab79-00d526fd15a9',
    displayName: '入荷詳細',
    comment: '入荷詳細画面のデータ構造。',
    members: ['入荷ID', '入荷日時', '担当者', '備考', 'Version', '入荷商品一覧'],
  },
  displayData: {
    '入荷ID': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '3e481d8e-1940-4827-a8fa-f19f31c19b43',
      type: 'word',
      displayName: '入荷ID',
      comment: '非表示。UUID。自動採番',
    },
    '入荷日時': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '99365fd5-0fca-4bc1-94af-2b4c8a44936a',
      type: 'datetime',
      displayName: '入荷日時',
    },
    '担当者': {
      kind: 'ref',
      parent: null,
      isOutsideTree: false,
      uniqueId: '37a8c98a-0d7c-4a59-8898-2958c91710ad',
      refTo: 'ref-to:従業員Ref',
      displayName: '担当者',
      refToObject: 'RefTarget',
      members: ['担当者.従業員番号', '担当者.氏名'],
    },
    '担当者.従業員番号': {
      kind: 'value',
      parent: '担当者',
      isOutsideTree: true,
      uniqueId: '91f94ec3-0cff-4468-84df-dbba8091f1c8',
      type: 'word',
      displayName: '従業員番号',
      stringSearchBehavior: 'Exact',
    },
    '担当者.氏名': {
      kind: 'value',
      parent: '担当者',
      isOutsideTree: true,
      uniqueId: '5fd0ce8d-d1fc-41cd-9a56-96db91fc7e8b',
      type: 'word',
      displayName: '氏名',
    },
    '備考': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'f77e1d87-ff76-4b45-a841-48b37137c19b',
      type: 'description',
      displayName: '備考',
    },
    'Version': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'c09d70de-6d3e-4a97-b578-5f2de2391b2e',
      type: 'int',
      displayName: 'Version',
      comment: '楽観排他用',
    },
    '入荷商品一覧': {
      kind: 'children',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'b16836d6-f14e-4ac9-87da-ca07546c6d37',
      displayName: '入荷商品一覧',
      members: ['入荷商品一覧.入荷明細ID', '入荷商品一覧.商品', '入荷商品一覧.数量', '入荷商品一覧.仕入単価_税抜', '入荷商品一覧.消費税区分', '入荷商品一覧.備考', '入荷商品一覧.Version'],
    },
    '入荷商品一覧.入荷明細ID': {
      kind: 'value',
      parent: '入荷商品一覧',
      isOutsideTree: false,
      uniqueId: 'a0b1c2d3-e4f5-6789-0123-456789abcdef',
      type: 'word',
      displayName: '入荷明細ID',
    },
    '入荷商品一覧.商品': {
      kind: 'ref',
      parent: '入荷商品一覧',
      isOutsideTree: false,
      uniqueId: '23cd27b4-406d-4e5c-988a-8715fcbd4a4d',
      refTo: 'ref-to:商品',
      displayName: '商品',
      refToObject: 'RefTarget',
      members: ['入荷商品一覧.商品.商品SEQ', '入荷商品一覧.商品.外部システム側ID', '入荷商品一覧.商品.商品名', '入荷商品一覧.商品.売値単価_税抜', '入荷商品一覧.商品.消費税区分'],
    },
    '入荷商品一覧.商品.商品SEQ': {
      kind: 'value',
      parent: '入荷商品一覧.商品',
      isOutsideTree: true,
      uniqueId: 'b2c3d4e5-f6a1-4b2c-9d3e-4f5a6b7c8d9e',
      type: 'sequence',
      displayName: '商品SEQ',
      comment: 'このシステムにおける識別番号。非表示',
      isKey: true,
      sequenceName: 'ITEM_SEQ',
    },
    '入荷商品一覧.商品.外部システム側ID': {
      kind: 'value',
      parent: '入荷商品一覧.商品',
      isOutsideTree: true,
      uniqueId: 'f5f225fa-eddd-4cca-ae58-b408e1b34100',
      type: 'word',
      comment: '外部システム側の識別番号',
      displayName: '商品コード',
      isNotNull: true,
      stringSearchBehavior: 'Exact',
    },
    '入荷商品一覧.商品.商品名': {
      kind: 'value',
      parent: '入荷商品一覧.商品',
      isOutsideTree: true,
      uniqueId: 'c3d4e5f6-a1b2-4c3d-0e4f-5a6b7c8d9e0f',
      type: 'word',
      displayName: '商品名',
      isNotNull: true,
    },
    '入荷商品一覧.商品.売値単価_税抜': {
      kind: 'value',
      parent: '入荷商品一覧.商品',
      isOutsideTree: true,
      uniqueId: 'd4e5f6a1-b2c3-4d4e-1f5a-6b7c8d9e0f1a',
      type: 'decimal',
      displayName: '売値単価_税抜',
      comment: '必須でないのは、キャンペーン用販促商品など価格が存在しない商品があるため。',
      isCurrency: true,
      notNegative: true,
    },
    '入荷商品一覧.商品.消費税区分': {
      kind: 'value',
      parent: '入荷商品一覧.商品',
      isOutsideTree: true,
      uniqueId: '1fd1192c-06a3-4aee-b88e-96700d545049',
      enumType: '消費税区分',
      displayName: '消費税区分',
    },
    '入荷商品一覧.数量': {
      kind: 'value',
      parent: '入荷商品一覧',
      isOutsideTree: false,
      uniqueId: '853acbf2-2764-424e-b60d-5ff066f18b9b',
      type: 'int',
      displayName: '数量',
      isQuantity: true,
    },
    '入荷商品一覧.仕入単価_税抜': {
      kind: 'value',
      parent: '入荷商品一覧',
      isOutsideTree: false,
      uniqueId: '1454146f-856e-4a67-b461-17a4fcd1e19e',
      type: 'decimal',
      displayName: '仕入単価_税抜',
      isCurrency: true,
    },
    '入荷商品一覧.消費税区分': {
      kind: 'value',
      parent: '入荷商品一覧',
      isOutsideTree: false,
      uniqueId: 'dc5381ce-29b7-4d23-bb9f-473e1625767d',
      enumType: '消費税区分',
      displayName: '消費税区分',
    },
    '入荷商品一覧.備考': {
      kind: 'value',
      parent: '入荷商品一覧',
      isOutsideTree: false,
      uniqueId: '0e829f4a-05b0-4cac-a7cb-a4ab39578532',
      type: 'description',
      displayName: '備考',
    },
    '入荷商品一覧.Version': {
      kind: 'value',
      parent: '入荷商品一覧',
      isOutsideTree: false,
      uniqueId: '20723b0e-a1c3-4ab5-956c-867f0518aa30',
      type: 'int',
      displayName: 'Version',
      comment: '楽観排他用',
    },
  },
} as const satisfies Util.AggregateMetadata.Entry

//#endregion メタデータ

//#endregion 関数
