
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : DataStructuresSection.cs
// ※ このファイルを生成したメソッド: Render

import type * as Util from ".."
import * as EnumDefs from "../enum-defs"

//#region 型定義
/** 事業所の検索時の検索条件の型。 */
export type SearchCondition = Util.SearchConditionBaseType<SearchConditionFilter, SortableMember>

/** 事業所の検索時の検索条件の絞り込み条件の型。 */
export type SearchConditionFilter = {
  事業所ID?: string | null
  事業所名?: string | null
}

/** 事業所のメンバーのうちソート可能なものを表すリテラル型 */
export type SortableMember
  = '事業所.事業所ID'
  | '事業所.事業所名'

/** 事業所のメンバーのうちソート可能なものを文字列で返します。 */
export function getSortableMember(): SortableMember[] {
  return [
    '事業所.事業所ID',
    '事業所.事業所名',
  ]
}

//#region 画面表示用データ
/** 事業所の画面表示用データ。 */
export type DisplayData = {
  /**
   * 画面初期表示から保存までの間でこのインスタンスを識別する番号。
   * GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
   * エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
   */
  instanceId: string
  /**
   * ビュー内部でハードコードする。
   */
  事業所ID?: string | null
  /**
   * ビュー内部でハードコードする。
   */
  事業所名?: string | null

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
 * 事業所が他の集約から外部参照されるときの事業所の型
 */
export type RefTarget = {
  事業所ID?: string
  事業所名?: string
}

//#endregion 他の集約から参照されるときの画面表示用オブジェクト

//#endregion 型定義


//#region 関数
/** 事業所の検索条件クラスの空オブジェクトを作成して返します。 */
export function createNewSearchCondition(): SearchCondition {
  return {
    filter: {
      事業所ID: '',
      事業所名: '',
    },
    sort: [],
    skip: '',
    take: '',
  }
}

//#region 画面表示用データ新規作成用関数
/** 事業所の画面表示用データの新しいインスタンスを作成します。 */
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
    事業所ID: '',
    事業所名: '',
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

  // 事業所 の等価比較
  if (areEqual事業所(left, right, compareFunction)) {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = false;
  } else {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = true;
    areEqual = false;
  }

  return areEqual;

  // ------ 値比較関数 --------

  function areEqual事業所(left: DisplayData, right: DisplayData, equals: Exclude<Util.DisplayDataDeepEqualOption["compareFunction"], undefined>): boolean {
    if (!equals('Word', left.事業所ID, right.事業所ID)) return false;
    if (!equals('Word', left.事業所名, right.事業所名)) return false;
    if ((left.willBeDeleted ?? false) !== (right.willBeDeleted ?? false)) return false;
    return true;
  }
}
//#endregion ディープイコール関数

//#region 他の集約から参照されるときの画面表示用オブジェクトの新規作成関数
/** 事業所RefTargetを新規作成します。 */
export function createNewRefTarget(): RefTarget {
  return {
    事業所ID: undefined,
    事業所名: undefined,
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
    uniqueId: '0112870a-b512-4b7c-a464-db9a254a4250',
    displayName: '事業所',
    comment: 'ビューの例。\nビューはデータモデルから ref-to で外部参照することができる。\nテーブルからテーブルへの ref-to と異なり、外部キー制約は張られない。',
    mapToView: true,
    members: ['事業所ID', '事業所名'],
  },
  displayData: {
    '事業所ID': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'c185f4b5-ed5a-43ba-a418-effca9791119',
      type: 'word',
      displayName: '事業所ID',
      comment: 'ビュー内部でハードコードする。',
      isKey: true,
    },
    '事業所名': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '25b6c40f-24a7-4c6e-9df5-385149e739b7',
      type: 'word',
      displayName: '事業所名',
      comment: 'ビュー内部でハードコードする。',
    },
  },
  searchCondition: {
    '事業所ID': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'c185f4b5-ed5a-43ba-a418-effca9791119',
      type: 'word',
      displayName: '事業所ID',
      comment: 'ビュー内部でハードコードする。',
      isKey: true,
    },
    '事業所名': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '25b6c40f-24a7-4c6e-9df5-385149e739b7',
      type: 'word',
      displayName: '事業所名',
      comment: 'ビュー内部でハードコードする。',
    },
  },
} as const satisfies Util.AggregateMetadata.Entry

//#endregion メタデータ

//#endregion 関数
