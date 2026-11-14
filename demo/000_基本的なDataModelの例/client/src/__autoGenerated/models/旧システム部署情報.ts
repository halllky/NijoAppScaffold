
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : DataStructuresSection.cs
// ※ このファイルを生成したメソッド: Render

import type * as Util from ".."
import * as EnumDefs from "../enum-defs"

//#region 型定義
/** 旧システム部署情報の検索時の検索条件の型。 */
export type SearchCondition = Util.SearchConditionBaseType<SearchConditionFilter, SortableMember>

/** 旧システム部署情報の検索時の検索条件の絞り込み条件の型。 */
export type SearchConditionFilter = {
  旧システムコード?: string | null
  名称?: string | null
}

/** 旧システム部署情報のメンバーのうちソート可能なものを表すリテラル型 */
export type SortableMember
  = '旧システム部署情報.旧システムコード'
  | '旧システム部署情報.名称'

/** 旧システム部署情報のメンバーのうちソート可能なものを文字列で返します。 */
export function getSortableMember(): SortableMember[] {
  return [
    '旧システム部署情報.旧システムコード',
    '旧システム部署情報.名称',
  ]
}

//#region 画面表示用データ
/** 旧システム部署情報の画面表示用データ。 */
export type DisplayData = {
  /**
   * 画面初期表示から保存までの間でこのインスタンスを識別する番号。
   * GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
   * エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
   */
  instanceId: string
  旧システムコード?: string | null
  名称?: string | null

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
 * 旧システム部署情報が他の集約から外部参照されるときの旧システム部署情報の型
 */
export type RefTarget = {
  旧システムコード?: string
  名称?: string
}

//#endregion 他の集約から参照されるときの画面表示用オブジェクト

//#endregion 型定義


//#region 関数
/** 旧システム部署情報の検索条件クラスの空オブジェクトを作成して返します。 */
export function createNewSearchCondition(): SearchCondition {
  return {
    filter: {
      旧システムコード: '',
      名称: '',
    },
    sort: [],
    skip: '',
    take: '',
  }
}

//#region 画面表示用データ新規作成用関数
/** 旧システム部署情報の画面表示用データの新しいインスタンスを作成します。 */
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
    旧システムコード: '',
    名称: '',
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

  // 旧システム部署情報 の等価比較
  if (areEqual旧システム部署情報(left, right, compareFunction)) {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = false;
  } else {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = true;
    areEqual = false;
  }

  return areEqual;

  // ------ 値比較関数 --------

  function areEqual旧システム部署情報(left: DisplayData, right: DisplayData, equals: Exclude<Util.DisplayDataDeepEqualOption["compareFunction"], undefined>): boolean {
    if (!equals('Word', left.旧システムコード, right.旧システムコード)) return false;
    if (!equals('Word', left.名称, right.名称)) return false;
    if ((left.willBeDeleted ?? false) !== (right.willBeDeleted ?? false)) return false;
    return true;
  }
}
//#endregion ディープイコール関数

//#region 他の集約から参照されるときの画面表示用オブジェクトの新規作成関数
/** 旧システム部署情報RefTargetを新規作成します。 */
export function createNewRefTarget(): RefTarget {
  return {
    旧システムコード: undefined,
    名称: undefined,
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
    uniqueId: '202ad425-e9b6-4e28-9984-0f401d445ec1',
    displayName: '旧システム部署情報',
    comment: '非主キーのユニーク制約の外部参照の例',
    members: ['旧システムコード', '名称'],
  },
  displayData: {
    '旧システムコード': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '78f8403b-9d15-424e-91c3-f31bd0aeb562',
      type: 'word',
      displayName: '旧システムコード',
      isKey: true,
    },
    '名称': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '1376a08d-3312-46c6-b51b-d4e70a74908a',
      type: 'word',
      displayName: '名称',
      isNotNull: true,
    },
  },
  searchCondition: {
    '旧システムコード': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '78f8403b-9d15-424e-91c3-f31bd0aeb562',
      type: 'word',
      displayName: '旧システムコード',
      isKey: true,
    },
    '名称': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '1376a08d-3312-46c6-b51b-d4e70a74908a',
      type: 'word',
      displayName: '名称',
      isNotNull: true,
    },
  },
} as const satisfies Util.AggregateMetadata.Entry

//#endregion メタデータ

//#endregion 関数
