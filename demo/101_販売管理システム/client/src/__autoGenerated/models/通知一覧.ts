
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : DataStructuresSection.cs
// ※ このファイルを生成したメソッド: Render
// ※ このファイルを生成した nijo のバージョン: 0.0.0-dev

import type * as Util from ".."
import * as EnumDefs from "../enum-defs"

//#region 型定義
/** 通知一覧の検索時の検索条件の型。 */
export type SearchCondition = Util.SearchConditionBaseType<SearchConditionFilter, SortableMember>

/** 通知一覧の検索時の検索条件の絞り込み条件の型。 */
export type SearchConditionFilter = {
  通知ID?: string | null
  本文?: string | null
  リンク先?: string | null
  既読日時?: { from?: string | null; to?: string | null }
  作成日時?: { from?: string | null; to?: string | null }
}

/** 通知一覧のメンバーのうちソート可能なものを表すリテラル型 */
export type SortableMember
  = '通知一覧.通知ID'
  | '通知一覧.本文'
  | '通知一覧.リンク先'
  | '通知一覧.既読日時'
  | '通知一覧.作成日時'

/** 通知一覧のメンバーのうちソート可能なものを文字列で返します。 */
export function getSortableMember(): SortableMember[] {
  return [
    '通知一覧.通知ID',
    '通知一覧.本文',
    '通知一覧.リンク先',
    '通知一覧.既読日時',
    '通知一覧.作成日時',
  ]
}

//#region 画面表示用データ
/** 通知一覧の画面表示用データ。 */
export type DisplayData = {
  /**
   * 画面初期表示から保存までの間でこのインスタンスを識別する番号。
   * GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
   * エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
   */
  instanceId: string
  通知ID?: string | null
  本文?: string | null
  /**
   * データモデル「通知」のリンク先と同じ形式。
   */
  リンク先?: string | null
  既読日時?: string | null
  作成日時?: string | null

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
/** 通知一覧の検索条件クラスの空オブジェクトを作成して返します。 */
export function createNewSearchCondition(): SearchCondition {
  return {
    filter: {
      通知ID: '',
      本文: '',
      リンク先: '',
      既読日時: { from: '', to: '' },
      作成日時: { from: '', to: '' },
    },
    sort: [],
    skip: '',
    take: '',
  }
}

//#region 画面表示用データ新規作成用関数
/** 通知一覧の画面表示用データの新しいインスタンスを作成します。 */
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
    本文: '',
    リンク先: '',
    既読日時: '',
    作成日時: '',
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

  // 通知一覧 の等価比較
  if (areEqual通知一覧(left, right, compareFunction)) {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = false;
  } else {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = true;
    areEqual = false;
  }

  return areEqual;

  // ------ 値比較関数 --------

  function areEqual通知一覧(left: DisplayData, right: DisplayData, equals: Exclude<Util.DisplayDataDeepEqualOption["compareFunction"], undefined>): boolean {
    if (!equals('Word', left.通知ID, right.通知ID)) return false;
    if (!equals('Description', left.本文, right.本文)) return false;
    if (!equals('Word', left.リンク先, right.リンク先)) return false;
    if (!equals('DateTime', left.既読日時, right.既読日時)) return false;
    if (!equals('DateTime', left.作成日時, right.作成日時)) return false;
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
    uniqueId: 'beffc950-74ba-48ec-b9fb-407d52c12533',
    displayName: '通知一覧',
    comment: '@[通知](309fda4b-bae6-445d-ad8c-a0d3ac8995f1) の一覧。ログインユーザー宛てのものだけを返す。',
    operationAccessLevel: 'ログインユーザー全員',
    members: ['通知ID', '本文', 'リンク先', '既読日時', '作成日時'],
  },
  displayData: {
    '通知ID': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '6a13f7b3-8f6e-4b70-a153-7b03606ded8d',
      type: 'word',
      displayName: '通知ID',
    },
    '本文': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '7163364e-f909-4027-b0b6-88f6bff3abe5',
      type: 'description',
      displayName: '本文',
    },
    'リンク先': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '496b48dc-7667-4de1-a577-11660fc5b321',
      type: 'word',
      displayName: 'リンク先',
      comment: 'データモデル「通知」のリンク先と同じ形式。',
    },
    '既読日時': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '43dc8e05-b3da-4dcb-86c3-0d2c6d9a6b6f',
      type: 'datetime',
      displayName: '既読日時',
    },
    '作成日時': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'c476c441-0fbb-439b-b7d5-dd1c27075f1d',
      type: 'datetime',
      displayName: '作成日時',
    },
  },
  searchCondition: {
    '通知ID': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '6a13f7b3-8f6e-4b70-a153-7b03606ded8d',
      type: 'word',
      displayName: '通知ID',
    },
    '本文': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '7163364e-f909-4027-b0b6-88f6bff3abe5',
      type: 'description',
      displayName: '本文',
    },
    'リンク先': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '496b48dc-7667-4de1-a577-11660fc5b321',
      type: 'word',
      displayName: 'リンク先',
      comment: 'データモデル「通知」のリンク先と同じ形式。',
    },
    '既読日時': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '43dc8e05-b3da-4dcb-86c3-0d2c6d9a6b6f',
      type: 'datetime',
      displayName: '既読日時',
    },
    '作成日時': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'c476c441-0fbb-439b-b7d5-dd1c27075f1d',
      type: 'datetime',
      displayName: '作成日時',
    },
  },
} as const satisfies Util.AggregateMetadata.Entry

//#endregion メタデータ

//#endregion 関数
