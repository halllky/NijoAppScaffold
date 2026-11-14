
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : DataStructuresSection.cs
// ※ このファイルを生成したメソッド: Render

import type * as Util from ".."
import * as EnumDefs from "../enum-defs"

//#region 型定義
/** 商品の検索時の検索条件の型。 */
export type SearchCondition = Util.SearchConditionBaseType<SearchConditionFilter, SortableMember>

/** 商品の検索時の検索条件の絞り込み条件の型。 */
export type SearchConditionFilter = {
  商品SEQ?: { from?: string | null; to?: string | null }
  外部システム側ID?: string | null
  商品名?: string | null
  売値単価_税抜?: { from?: string | null; to?: string | null }
  消費税区分?: { '非課税'?: boolean | null, '一般税率'?: boolean | null, '軽減税率'?: boolean | null }
}

/** 商品のメンバーのうちソート可能なものを表すリテラル型 */
export type SortableMember
  = '商品.商品SEQ'
  | '商品.外部システム側ID'
  | '商品.商品名'
  | '商品.売値単価_税抜'
  | '商品.消費税区分'

/** 商品のメンバーのうちソート可能なものを文字列で返します。 */
export function getSortableMember(): SortableMember[] {
  return [
    '商品.商品SEQ',
    '商品.外部システム側ID',
    '商品.商品名',
    '商品.売値単価_税抜',
    '商品.消費税区分',
  ]
}

//#region 画面表示用データ
/** 商品の画面表示用データ。 */
export type DisplayData = {
  /**
   * 画面初期表示から保存までの間でこのインスタンスを識別する番号。
   * GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
   * エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
   */
  instanceId: string
  /**
   * このシステムにおける識別番号。非表示
   */
  商品SEQ?: string | null
  /**
   * 商品コード
   *
   * 外部システム側の識別番号
   */
  外部システム側ID?: string | null
  商品名?: string | null
  /**
   * 必須でないのは、キャンペーン用販促商品など価格が存在しない商品があるため。
   */
  売値単価_税抜?: string | null
  消費税区分?: EnumDefs.消費税区分 | null

  /** このデータがDBに保存済みかどうか */
  existsInDatabase: boolean
  /** このデータに更新がかかっているかどうか */
  willBeChanged: boolean
  /** このデータが更新確定時に削除されるかどうか */
  willBeDeleted: boolean
  /** 楽観排他制御用のバージョニング情報 */
  version: number | null | undefined
}
//#endregion 画面表示用データ

//#region 他の集約から参照されるときの画面表示用オブジェクト
/**
 * 商品が他の集約から外部参照されるときの商品の型
 */
export type RefTarget = {
  商品SEQ?: string
  外部システム側ID?: string
  商品名?: string
  売値単価_税抜?: string
  消費税区分?: EnumDefs.消費税区分
}

//#endregion 他の集約から参照されるときの画面表示用オブジェクト

//#endregion 型定義


//#region 関数
/** 商品の検索条件クラスの空オブジェクトを作成して返します。 */
export function createNewSearchCondition(): SearchCondition {
  return {
    filter: {
      商品SEQ: { from: '', to: '' },
      外部システム側ID: '',
      商品名: '',
      売値単価_税抜: { from: '', to: '' },
      消費税区分: { '非課税': false, '一般税率': false, '軽減税率': false },
    },
    sort: [],
    skip: '',
    take: '',
  }
}

//#region 画面表示用データ新規作成用関数
/** 商品の画面表示用データの新しいインスタンスを作成します。 */
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
    商品SEQ: '',
    外部システム側ID: '',
    商品名: '',
    売値単価_税抜: '',
    消費税区分: null,
    existsInDatabase: false,
    willBeChanged: true,
    willBeDeleted: false,
    version: null,
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

  // 商品 の等価比較
  if (areEqual商品(left, right, compareFunction)) {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = false;
  } else {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = true;
    areEqual = false;
  }

  return areEqual;

  // ------ 値比較関数 --------

  function areEqual商品(left: DisplayData, right: DisplayData, equals: Exclude<Util.DisplayDataDeepEqualOption["compareFunction"], undefined>): boolean {
    if (!equals('Sequence', left.商品SEQ, right.商品SEQ)) return false;
    if (!equals('Word', left.外部システム側ID, right.外部システム側ID)) return false;
    if (!equals('Word', left.商品名, right.商品名)) return false;
    if (!equals('Decimal', left.売値単価_税抜, right.売値単価_税抜)) return false;
    if (!equals('Enum', left.消費税区分, right.消費税区分, '消費税区分')) return false;
    if ((left.willBeDeleted ?? false) !== (right.willBeDeleted ?? false)) return false;
    return true;
  }
}
//#endregion ディープイコール関数

//#region 他の集約から参照されるときの画面表示用オブジェクトの新規作成関数
/** 商品RefTargetを新規作成します。 */
export function createNewRefTarget(): RefTarget {
  return {
    商品SEQ: undefined,
    外部システム側ID: undefined,
    商品名: undefined,
    売値単価_税抜: undefined,
    消費税区分: undefined,
  }
}

//#endregion 他の集約から参照されるときの画面表示用オブジェクトの新規作成関数

//#region メタデータ

/** この集約のメタデータ */
export const metadata = {
  root: {
    kind: 'root',
    isStored: true,
    isQueryable: true,
    uniqueId: 'a1b2c3d4-e5f6-4a1b-8c2d-3e4f5a6b7c8d',
    displayName: '商品',
    comment: '入荷・販売の対象となる商品マスタデータ。\nこのシステムで登録せず、外部システムから日次で連携される。',
    uniqueConstraints: 'f5f225fa-eddd-4cca-ae58-b408e1b34100;',
    members: ['商品SEQ', '外部システム側ID', '商品名', '売値単価_税抜', '消費税区分'],
  },
  displayData: {
    '商品SEQ': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'b2c3d4e5-f6a1-4b2c-9d3e-4f5a6b7c8d9e',
      type: 'sequence',
      displayName: '商品SEQ',
      comment: 'このシステムにおける識別番号。非表示',
      isKey: true,
      sequenceName: 'ITEM_SEQ',
    },
    '外部システム側ID': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'f5f225fa-eddd-4cca-ae58-b408e1b34100',
      type: 'word',
      comment: '外部システム側の識別番号',
      displayName: '商品コード',
      isNotNull: true,
      stringSearchBehavior: 'Exact',
    },
    '商品名': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'c3d4e5f6-a1b2-4c3d-0e4f-5a6b7c8d9e0f',
      type: 'word',
      displayName: '商品名',
      isNotNull: true,
    },
    '売値単価_税抜': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'd4e5f6a1-b2c3-4d4e-1f5a-6b7c8d9e0f1a',
      type: 'decimal',
      displayName: '売値単価_税抜',
      comment: '必須でないのは、キャンペーン用販促商品など価格が存在しない商品があるため。',
      isCurrency: true,
      notNegative: true,
    },
    '消費税区分': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '1fd1192c-06a3-4aee-b88e-96700d545049',
      enumType: '消費税区分',
      displayName: '消費税区分',
    },
  },
  searchCondition: {
    '商品SEQ': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'b2c3d4e5-f6a1-4b2c-9d3e-4f5a6b7c8d9e',
      type: 'sequence',
      displayName: '商品SEQ',
      comment: 'このシステムにおける識別番号。非表示',
      isKey: true,
      sequenceName: 'ITEM_SEQ',
    },
    '外部システム側ID': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'f5f225fa-eddd-4cca-ae58-b408e1b34100',
      type: 'word',
      comment: '外部システム側の識別番号',
      displayName: '商品コード',
      isNotNull: true,
      stringSearchBehavior: 'Exact',
    },
    '商品名': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'c3d4e5f6-a1b2-4c3d-0e4f-5a6b7c8d9e0f',
      type: 'word',
      displayName: '商品名',
      isNotNull: true,
    },
    '売値単価_税抜': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'd4e5f6a1-b2c3-4d4e-1f5a-6b7c8d9e0f1a',
      type: 'decimal',
      displayName: '売値単価_税抜',
      comment: '必須でないのは、キャンペーン用販促商品など価格が存在しない商品があるため。',
      isCurrency: true,
      notNegative: true,
    },
    '消費税区分': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '1fd1192c-06a3-4aee-b88e-96700d545049',
      enumType: '消費税区分',
      displayName: '消費税区分',
    },
  },
} as const satisfies Util.AggregateMetadata.Entry

//#endregion メタデータ

//#endregion 関数
