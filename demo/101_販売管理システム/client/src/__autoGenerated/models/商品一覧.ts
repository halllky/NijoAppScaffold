
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : DataStructuresSection.cs
// ※ このファイルを生成したメソッド: Render

import type * as Util from ".."
import * as EnumDefs from "../enum-defs"

//#region 型定義
/** 商品一覧の検索時の検索条件の型。 */
export type SearchCondition = Util.SearchConditionBaseType<SearchConditionFilter, SortableMember>

/** 商品一覧の検索時の検索条件の絞り込み条件の型。 */
export type SearchConditionFilter = {
  商品SEQ?: { from?: string | null; to?: string | null }
  外部システム側ID?: { from?: string | null; to?: string | null }
  商品名?: string | null
  売値単価_税抜?: { from?: string | null; to?: string | null }
  消費税区分?: { '非課税'?: boolean | null, '一般税率'?: boolean | null, '軽減税率'?: boolean | null }
  在庫数?: { from?: string | null; to?: string | null }
}

/** 商品一覧のメンバーのうちソート可能なものを表すリテラル型 */
export type SortableMember
  = '商品一覧.商品SEQ'
  | '商品一覧.外部システム側ID'
  | '商品一覧.商品名'
  | '商品一覧.売値単価_税抜'
  | '商品一覧.消費税区分'
  | '商品一覧.在庫数'

/** 商品一覧のメンバーのうちソート可能なものを文字列で返します。 */
export function getSortableMember(): SortableMember[] {
  return [
    '商品一覧.商品SEQ',
    '商品一覧.外部システム側ID',
    '商品一覧.商品名',
    '商品一覧.売値単価_税抜',
    '商品一覧.消費税区分',
    '商品一覧.在庫数',
  ]
}

//#region 画面表示用データ
/** 商品一覧の画面表示用データ。 */
export type DisplayData = {
  /**
   * 画面初期表示から保存までの間でこのインスタンスを識別する番号。
   * GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
   * エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
   */
  instanceId: string
  /**
   * 非表示
   */
  商品SEQ?: string | null
  外部システム側ID?: string | null
  商品名?: string | null
  売値単価_税抜?: string | null
  消費税区分?: EnumDefs.消費税区分 | null
  在庫数?: string | null

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
/** 商品一覧の検索条件クラスの空オブジェクトを作成して返します。 */
export function createNewSearchCondition(): SearchCondition {
  return {
    filter: {
      商品SEQ: { from: '', to: '' },
      外部システム側ID: { from: '', to: '' },
      商品名: '',
      売値単価_税抜: { from: '', to: '' },
      消費税区分: { '非課税': false, '一般税率': false, '軽減税率': false },
      在庫数: { from: '', to: '' },
    },
    sort: [],
    skip: '',
    take: '',
  }
}

//#region 画面表示用データ新規作成用関数
/** 商品一覧の画面表示用データの新しいインスタンスを作成します。 */
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
    在庫数: '',
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

  // 商品一覧 の等価比較
  if (areEqual商品一覧(left, right, compareFunction)) {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = false;
  } else {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = true;
    areEqual = false;
  }

  return areEqual;

  // ------ 値比較関数 --------

  function areEqual商品一覧(left: DisplayData, right: DisplayData, equals: Exclude<Util.DisplayDataDeepEqualOption["compareFunction"], undefined>): boolean {
    if (!equals('Integer', left.商品SEQ, right.商品SEQ)) return false;
    if (!equals('Word', left.外部システム側ID, right.外部システム側ID)) return false;
    if (!equals('Word', left.商品名, right.商品名)) return false;
    if (!equals('Decimal', left.売値単価_税抜, right.売値単価_税抜)) return false;
    if (!equals('Enum', left.消費税区分, right.消費税区分, '消費税区分')) return false;
    if (!equals('Integer', left.在庫数, right.在庫数)) return false;
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
    uniqueId: 'cf33b04a-b6a2-4559-b98c-563ddcc76394',
    displayName: '商品一覧',
    comment: '@[商品](a1b2c3d4-e5f6-4a1b-8c2d-3e4f5a6b7c8d) の一覧検索画面のデータ構造。 @[入荷明細](a5e5b615-6cc1-4a50-8cc7-fe910dca543b) を参照し、現在の在庫数も一覧する。\n商品マスタはこのシステム上では編集できない。閲覧のみ。',
    members: ['商品SEQ', '外部システム側ID', '商品名', '売値単価_税抜', '消費税区分', '在庫数'],
  },
  displayData: {
    '商品SEQ': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'b583f019-e406-407e-94db-588a6c19dadc',
      type: 'int',
      displayName: '商品SEQ',
      comment: '非表示',
    },
    '外部システム側ID': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '643fcd6c-1a53-406f-9d58-576b6eb5a3de',
      type: 'word',
      displayName: '外部システム側ID',
      stringSearchBehavior: 'Range',
    },
    '商品名': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '446d95d0-c894-4cbc-9f52-4cb316445c55',
      type: 'word',
      displayName: '商品名',
    },
    '売値単価_税抜': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'bfe5c44a-cc36-4136-b22c-ac6425ec9dce',
      type: 'decimal',
      displayName: '売値単価_税抜',
      isCurrency: true,
    },
    '消費税区分': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '9f805629-7700-4ffa-802b-897d6e85708c',
      enumType: '消費税区分',
      displayName: '消費税区分',
    },
    '在庫数': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'a3cc1a1f-51b5-4fc5-94a4-296f341293e7',
      type: 'int',
      displayName: '在庫数',
    },
  },
  searchCondition: {
    '商品SEQ': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'b583f019-e406-407e-94db-588a6c19dadc',
      type: 'int',
      displayName: '商品SEQ',
      comment: '非表示',
    },
    '外部システム側ID': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '643fcd6c-1a53-406f-9d58-576b6eb5a3de',
      type: 'word',
      displayName: '外部システム側ID',
      stringSearchBehavior: 'Range',
    },
    '商品名': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '446d95d0-c894-4cbc-9f52-4cb316445c55',
      type: 'word',
      displayName: '商品名',
    },
    '売値単価_税抜': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'bfe5c44a-cc36-4136-b22c-ac6425ec9dce',
      type: 'decimal',
      displayName: '売値単価_税抜',
      isCurrency: true,
    },
    '消費税区分': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '9f805629-7700-4ffa-802b-897d6e85708c',
      enumType: '消費税区分',
      displayName: '消費税区分',
    },
    '在庫数': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'a3cc1a1f-51b5-4fc5-94a4-296f341293e7',
      type: 'int',
      displayName: '在庫数',
    },
  },
} as const satisfies Util.AggregateMetadata.Entry

//#endregion メタデータ

//#endregion 関数
