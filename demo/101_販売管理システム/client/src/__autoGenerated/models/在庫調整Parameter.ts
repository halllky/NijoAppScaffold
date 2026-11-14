
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : DataStructuresSection.cs
// ※ このファイルを生成したメソッド: Render

import type * as Util from ".."
import * as EnumDefs from "../enum-defs"
import * as 商品 from "./商品"

//#region 型定義
//#region 構造体定義
/** 在庫調整Parameterの構造体 */
export type Structure = {
  商品: 商品.RefTarget
  増減数: string | undefined
  絶対数: string | undefined
  在庫調整理由: string | undefined
}
//#endregion 構造体定義

//#region 構造体新規作成用関数
/** 在庫調整Parameterの構造体の新しいインスタンスを作成します。 */
export function createNewStructure(): Structure {
  return {
    商品: 商品.createNewRefTarget(),
    増減数: undefined,
    絶対数: undefined,
    在庫調整理由: undefined,
  }
}
//#endregion 構造体新規作成用関数

//#region 画面表示用データ
/** 在庫調整Parameterの画面表示用データ。 */
export type DisplayData = {
  /**
   * 画面初期表示から保存までの間でこのインスタンスを識別する番号。
   * GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
   * エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
   */
  instanceId: string
  商品: 商品.RefTarget
  増減数?: string | null
  絶対数?: string | null
  在庫調整理由?: string | null

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
/** 在庫調整Parameterの画面表示用データの新しいインスタンスを作成します。 */
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
    商品: 商品.createNewRefTarget(),
    増減数: '',
    絶対数: '',
    在庫調整理由: '',
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

  // 在庫調整Parameter の等価比較
  if (areEqual在庫調整Parameter(left, right, compareFunction)) {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = false;
  } else {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = true;
    areEqual = false;
  }

  return areEqual;

  // ------ 値比較関数 --------

  function areEqual在庫調整Parameter(left: DisplayData, right: DisplayData, equals: Exclude<Util.DisplayDataDeepEqualOption["compareFunction"], undefined>): boolean {
    if (!equals('Sequence', left.商品?.商品SEQ, right.商品?.商品SEQ)) return false;
    if (!equals('Word', left.商品?.外部システム側ID, right.商品?.外部システム側ID)) return false;
    if (!equals('Word', left.商品?.商品名, right.商品?.商品名)) return false;
    if (!equals('Decimal', left.商品?.売値単価_税抜, right.商品?.売値単価_税抜)) return false;
    if (!equals('Enum', left.商品?.消費税区分, right.商品?.消費税区分, '消費税区分')) return false;
    if (!equals('Integer', left.増減数, right.増減数)) return false;
    if (!equals('Integer', left.絶対数, right.絶対数)) return false;
    if (!equals('Description', left.在庫調整理由, right.在庫調整理由)) return false;
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
    uniqueId: '2555dc15-3ed7-48cb-9e78-0e7b09de86a3',
    displayName: '在庫調整Parameter',
    comment: '在庫調整の入力パラメータ。\n棚卸など商品の絶対数を正とする場合は絶対数を、返品や破損など増減数で調整する場合は増減数を指定する。',
    members: ['商品', '増減数', '絶対数', '在庫調整理由'],
  },
  displayData: {
    '商品': {
      kind: 'ref',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'cb60d0f6-8dcb-4c4a-bc62-31aa338c4034',
      refTo: 'ref-to:商品',
      displayName: '商品',
      refToObject: 'RefTarget',
      members: ['商品.商品SEQ', '商品.外部システム側ID', '商品.商品名', '商品.売値単価_税抜', '商品.消費税区分'],
    },
    '商品.商品SEQ': {
      kind: 'value',
      parent: '商品',
      isOutsideTree: true,
      uniqueId: 'b2c3d4e5-f6a1-4b2c-9d3e-4f5a6b7c8d9e',
      type: 'sequence',
      displayName: '商品SEQ',
      comment: 'このシステムにおける識別番号。非表示',
      isKey: true,
      sequenceName: 'ITEM_SEQ',
    },
    '商品.外部システム側ID': {
      kind: 'value',
      parent: '商品',
      isOutsideTree: true,
      uniqueId: 'f5f225fa-eddd-4cca-ae58-b408e1b34100',
      type: 'word',
      comment: '外部システム側の識別番号',
      displayName: '商品コード',
      isNotNull: true,
      stringSearchBehavior: 'Exact',
    },
    '商品.商品名': {
      kind: 'value',
      parent: '商品',
      isOutsideTree: true,
      uniqueId: 'c3d4e5f6-a1b2-4c3d-0e4f-5a6b7c8d9e0f',
      type: 'word',
      displayName: '商品名',
      isNotNull: true,
    },
    '商品.売値単価_税抜': {
      kind: 'value',
      parent: '商品',
      isOutsideTree: true,
      uniqueId: 'd4e5f6a1-b2c3-4d4e-1f5a-6b7c8d9e0f1a',
      type: 'decimal',
      displayName: '売値単価_税抜',
      comment: '必須でないのは、キャンペーン用販促商品など価格が存在しない商品があるため。',
      isCurrency: true,
      notNegative: true,
    },
    '商品.消費税区分': {
      kind: 'value',
      parent: '商品',
      isOutsideTree: true,
      uniqueId: '1fd1192c-06a3-4aee-b88e-96700d545049',
      enumType: '消費税区分',
      displayName: '消費税区分',
    },
    '増減数': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '54379682-e77d-4da1-98e4-812ef8d6ad43',
      type: 'int',
      displayName: '増減数',
      isQuantity: true,
    },
    '絶対数': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'a54a1771-b626-4353-ba03-7db45e9d4f61',
      type: 'int',
      displayName: '絶対数',
      isQuantity: true,
    },
    '在庫調整理由': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '53517d58-6e73-4d70-9267-5813bc3bba85',
      type: 'description',
      displayName: '在庫調整理由',
    },
  },
} as const satisfies Util.AggregateMetadata.Entry

//#endregion メタデータ

//#endregion 関数
