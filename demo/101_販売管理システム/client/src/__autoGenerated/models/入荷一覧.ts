
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : DataStructuresSection.cs
// ※ このファイルを生成したメソッド: Render

import type * as Util from ".."
import * as EnumDefs from "../enum-defs"
import * as 従業員Ref from "./従業員Ref"

//#region 型定義
/** 入荷一覧の検索時の検索条件の型。 */
export type SearchCondition = Util.SearchConditionBaseType<SearchConditionFilter, SortableMember>

/** 入荷一覧の検索時の検索条件の絞り込み条件の型。 */
export type SearchConditionFilter = {
  入荷ID?: string | null
  入荷日時?: { from?: string | null; to?: string | null }
  担当者: 従業員Ref.SearchConditionFilter
  備考?: string | null
  明細件数?: { from?: string | null; to?: string | null }
  入荷数量合計?: { from?: string | null; to?: string | null }
}

/** 入荷一覧のメンバーのうちソート可能なものを表すリテラル型 */
export type SortableMember
  = '入荷一覧.入荷ID'
  | '入荷一覧.入荷日時'
  | '入荷一覧.担当者.従業員Ref.従業員番号'
  | '入荷一覧.担当者.従業員Ref.氏名'
  | '入荷一覧.備考'
  | '入荷一覧.明細件数'
  | '入荷一覧.入荷数量合計'

/** 入荷一覧のメンバーのうちソート可能なものを文字列で返します。 */
export function getSortableMember(): SortableMember[] {
  return [
    '入荷一覧.入荷ID',
    '入荷一覧.入荷日時',
    '入荷一覧.担当者.従業員Ref.従業員番号',
    '入荷一覧.担当者.従業員Ref.氏名',
    '入荷一覧.備考',
    '入荷一覧.明細件数',
    '入荷一覧.入荷数量合計',
  ]
}

//#region 画面表示用データ
/** 入荷一覧の画面表示用データ。 */
export type DisplayData = {
  /**
   * 画面初期表示から保存までの間でこのインスタンスを識別する番号。
   * GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
   * エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
   */
  instanceId: string
  /**
   * UUID
   */
  入荷ID?: string | null
  入荷日時?: string | null
  担当者: 従業員Ref.RefTarget
  /**
   * 備考
   */
  備考?: string | null
  明細件数?: string | null
  入荷数量合計?: string | null

  /** このデータがDBに保存済みかどうか */
  existsInDatabase: boolean
  /** このデータに更新がかかっているかどうか */
  willBeChanged: boolean
  /** このデータが更新確定時に削除されるかどうか */
  willBeDeleted: boolean
}
//#endregion 画面表示用データ

//#region 他の集約から参照されるときの画面表示用オブジェクト
//#endregion 他の集約から参照されるときの画面表示用オブジェクト

//#endregion 型定義


//#region 関数
/** 入荷一覧の検索条件クラスの空オブジェクトを作成して返します。 */
export function createNewSearchCondition(): SearchCondition {
  return {
    filter: {
      入荷ID: '',
      入荷日時: { from: '', to: '' },
      担当者: {
        従業員番号: '',
        氏名: '',
      },
      備考: '',
      明細件数: { from: '', to: '' },
      入荷数量合計: { from: '', to: '' },
    },
    sort: [],
    skip: '',
    take: '',
  }
}

//#region 画面表示用データ新規作成用関数
/** 入荷一覧の画面表示用データの新しいインスタンスを作成します。 */
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
    明細件数: '',
    入荷数量合計: '',
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

  // 入荷一覧 の等価比較
  if (areEqual入荷一覧(left, right, compareFunction)) {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = false;
  } else {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = true;
    areEqual = false;
  }

  return areEqual;

  // ------ 値比較関数 --------

  function areEqual入荷一覧(left: DisplayData, right: DisplayData, equals: Exclude<Util.DisplayDataDeepEqualOption["compareFunction"], undefined>): boolean {
    if (!equals('Word', left.入荷ID, right.入荷ID)) return false;
    if (!equals('DateTime', left.入荷日時, right.入荷日時)) return false;
    if (!equals('Word', left.担当者?.従業員番号, right.担当者?.従業員番号)) return false;
    if (!equals('Word', left.担当者?.氏名, right.担当者?.氏名)) return false;
    if (!equals('Description', left.備考, right.備考)) return false;
    if (!equals('Integer', left.明細件数, right.明細件数)) return false;
    if (!equals('Integer', left.入荷数量合計, right.入荷数量合計)) return false;
    if ((left.willBeDeleted ?? false) !== (right.willBeDeleted ?? false)) return false;
    return true;
  }
}
//#endregion ディープイコール関数

//#region 他の集約から参照されるときの画面表示用オブジェクトの新規作成関数
//#endregion 他の集約から参照されるときの画面表示用オブジェクトの新規作成関数

//#region メタデータ

/** この集約のメタデータ */
export const metadata = {
  root: {
    kind: 'root',
    isStored: false,
    isQueryable: true,
    uniqueId: 'ca790c30-db44-4439-9452-2a1310a901bb',
    displayName: '入荷一覧',
    comment: '@[入荷](e5f6a1b2-c3d4-4e5f-2a6b-7c8d9e0f1a2b) の一覧検索画面のデータ構造。',
    members: ['入荷ID', '入荷日時', '担当者', '備考', '明細件数', '入荷数量合計'],
  },
  displayData: {
    '入荷ID': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '1f7d351c-4779-45fe-91c0-c21dde51c77e',
      type: 'word',
      displayName: '入荷ID',
      comment: 'UUID',
      stringSearchBehavior: 'Exact',
    },
    '入荷日時': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'b3a66de3-5919-4896-b2c9-a87d321fb4a1',
      type: 'datetime',
      displayName: '入荷日時',
    },
    '担当者': {
      kind: 'ref',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'c8fde618-5c88-4a6b-8b05-dc5dbc93b032',
      refTo: 'ref-to:従業員Ref',
      displayName: '担当者',
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
      uniqueId: '06898967-2069-408d-8094-0abf14d93d46',
      type: 'description',
      displayName: '備考',
      comment: '備考',
    },
    '明細件数': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '47e8ab32-450e-4a0c-997c-d4b4e4627909',
      type: 'int',
      displayName: '明細件数',
    },
    '入荷数量合計': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'f91dd3c1-3e02-485c-9d51-00a6d77a8c97',
      type: 'int',
      displayName: '入荷数量合計',
      isQuantity: true,
    },
  },
  searchCondition: {
    '入荷ID': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '1f7d351c-4779-45fe-91c0-c21dde51c77e',
      type: 'word',
      displayName: '入荷ID',
      comment: 'UUID',
      stringSearchBehavior: 'Exact',
    },
    '入荷日時': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'b3a66de3-5919-4896-b2c9-a87d321fb4a1',
      type: 'datetime',
      displayName: '入荷日時',
    },
    '担当者': {
      kind: 'ref',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'c8fde618-5c88-4a6b-8b05-dc5dbc93b032',
      refTo: 'ref-to:従業員Ref',
      displayName: '担当者',
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
      uniqueId: '06898967-2069-408d-8094-0abf14d93d46',
      type: 'description',
      displayName: '備考',
      comment: '備考',
    },
    '明細件数': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '47e8ab32-450e-4a0c-997c-d4b4e4627909',
      type: 'int',
      displayName: '明細件数',
    },
    '入荷数量合計': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'f91dd3c1-3e02-485c-9d51-00a6d77a8c97',
      type: 'int',
      displayName: '入荷数量合計',
      isQuantity: true,
    },
  },
} as const satisfies Util.AggregateMetadata.Entry

//#endregion メタデータ

//#endregion 関数
