
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : DataStructuresSection.cs
// ※ このファイルを生成したメソッド: Render
// ※ このファイルを生成した nijo のバージョン: 0.0.0-dev

import type * as Util from ".."
import * as EnumDefs from "../enum-defs"

//#region 型定義
/** 添付ファイルRefの検索時の検索条件の型。 */
export type SearchCondition = Util.SearchConditionBaseType<SearchConditionFilter, SortableMember>

/** 添付ファイルRefの検索時の検索条件の絞り込み条件の型。 */
export type SearchConditionFilter = {
  識別子?: string | null
  名前?: string | null
  サイズ?: { from?: string | null; to?: string | null }
  種類?: string | null
}

/** 添付ファイルRefのメンバーのうちソート可能なものを表すリテラル型 */
export type SortableMember
  = '添付ファイルRef.識別子'
  | '添付ファイルRef.名前'
  | '添付ファイルRef.サイズ'
  | '添付ファイルRef.種類'

/** 添付ファイルRefのメンバーのうちソート可能なものを文字列で返します。 */
export function getSortableMember(): SortableMember[] {
  return [
    '添付ファイルRef.識別子',
    '添付ファイルRef.名前',
    '添付ファイルRef.サイズ',
    '添付ファイルRef.種類',
  ]
}

//#region 画面表示用データ
/** 添付ファイルRefの画面表示用データ。 */
export type DisplayData = {
  /**
   * 画面初期表示から保存までの間でこのインスタンスを識別する番号。
   * GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
   * エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
   */
  instanceId: string
  識別子?: string | null
  名前?: string | null
  サイズ?: string | null
  種類?: string | null

  /** このデータがDBに保存済みかどうか */
  existsInDatabase: boolean
  /** このデータに更新がかかっているかどうか */
  willBeChanged: boolean
  /** このデータが更新確定時に削除されるかどうか */
  willBeDeleted: boolean
}
//#endregion 画面表示用データ

//#region 他の集約から参照されるときの画面表示用オブジェクト
/**
 * 添付ファイルRefが他の集約から外部参照されるときの添付ファイルRefの型
 */
export type RefTarget = {
  識別子?: string
  名前?: string
  サイズ?: string
  種類?: string
}

//#endregion 他の集約から参照されるときの画面表示用オブジェクト

//#endregion 型定義


//#region 関数
/** 添付ファイルRefの検索条件クラスの空オブジェクトを作成して返します。 */
export function createNewSearchCondition(): SearchCondition {
  return {
    filter: {
      識別子: '',
      名前: '',
      サイズ: { from: '', to: '' },
      種類: '',
    },
    sort: [],
    skip: '',
    take: '',
  }
}

//#region 画面表示用データ新規作成用関数
/** 添付ファイルRefの画面表示用データの新しいインスタンスを作成します。 */
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
    識別子: '',
    名前: '',
    サイズ: '',
    種類: '',
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

  // 添付ファイルRef の等価比較
  if (areEqual添付ファイルRef(left, right, compareFunction)) {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = false;
  } else {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = true;
    areEqual = false;
  }

  return areEqual;

  // ------ 値比較関数 --------

  function areEqual添付ファイルRef(left: DisplayData, right: DisplayData, equals: Exclude<Util.DisplayDataDeepEqualOption["compareFunction"], undefined>): boolean {
    if (!equals('Word', left.識別子, right.識別子)) return false;
    if (!equals('Word', left.名前, right.名前)) return false;
    if (!equals('Integer', left.サイズ, right.サイズ)) return false;
    if (!equals('Word', left.種類, right.種類)) return false;
    if ((left.willBeDeleted ?? false) !== (right.willBeDeleted ?? false)) return false;
    return true;
  }
}
//#endregion ディープイコール関数

//#region 他の集約から参照されるときの画面表示用オブジェクトの新規作成関数
/** 添付ファイルRefRefTargetを新規作成します。 */
export function createNewRefTarget(): RefTarget {
  return {
    識別子: undefined,
    名前: undefined,
    サイズ: undefined,
    種類: undefined,
  }
}

//#endregion 他の集約から参照されるときの画面表示用オブジェクトの新規作成関数

//#region メタデータ

/** この集約のメタデータ */
export const metadata = {
  root: {
    kind: 'root',
    isStored: false,
    isQueryable: true,
    uniqueId: '0aeb194b-3f9f-44f5-87af-a5366c7f3789',
    displayName: '添付ファイルRef',
    comment: '画面のデータとコマンドの引数から @[添付ファイル](5ce0a960-5dfa-4ae8-ad75-23bb12a8ed6d) を参照するときの参照先。\n単独で検索させると他人のファイルの名前まで見えるので、画面からは呼べないようにしている。',
    operationAccessLevel: '内部処理のみ',
    members: ['識別子', '名前', 'サイズ', '種類'],
  },
  displayData: {
    '識別子': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '201f61a2-2816-400d-8297-936521a057c1',
      type: 'word',
      displayName: '識別子',
      stringSearchBehavior: 'Exact',
    },
    '名前': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'e6003b6b-f7c7-422c-8d9c-823d4cd55e74',
      type: 'word',
      displayName: '名前',
    },
    'サイズ': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'f6767376-4ba4-422c-801b-59a97c8d9c4d',
      type: 'int',
      displayName: 'サイズ',
    },
    '種類': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '5739ba3c-a233-4523-acc9-0247b54f2a7a',
      type: 'word',
      displayName: '種類',
    },
  },
  searchCondition: {
    '識別子': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '201f61a2-2816-400d-8297-936521a057c1',
      type: 'word',
      displayName: '識別子',
      stringSearchBehavior: 'Exact',
    },
    '名前': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'e6003b6b-f7c7-422c-8d9c-823d4cd55e74',
      type: 'word',
      displayName: '名前',
    },
    'サイズ': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'f6767376-4ba4-422c-801b-59a97c8d9c4d',
      type: 'int',
      displayName: 'サイズ',
    },
    '種類': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '5739ba3c-a233-4523-acc9-0247b54f2a7a',
      type: 'word',
      displayName: '種類',
    },
  },
} as const satisfies Util.AggregateMetadata.Entry

//#endregion メタデータ

//#endregion 関数
