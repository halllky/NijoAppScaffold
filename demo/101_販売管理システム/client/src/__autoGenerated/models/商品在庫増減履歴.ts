
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : DataStructuresSection.cs
// ※ このファイルを生成したメソッド: Render

import type * as Util from ".."
import * as EnumDefs from "../enum-defs"

//#region 型定義
/** 商品在庫増減履歴の検索時の検索条件の型。 */
export type SearchCondition = Util.SearchConditionBaseType<SearchConditionFilter, SortableMember>

/** 商品在庫増減履歴の検索時の検索条件の絞り込み条件の型。 */
export type SearchConditionFilter = {
  商品SEQ?: { from?: string | null; to?: string | null }
  外部システム側ID?: string | null
  商品名?: string | null
  日時?: { from?: string | null; to?: string | null }
  増減数?: { from?: string | null; to?: string | null }
  事由?: string | null
  入荷ID?: string | null
  増減履歴引当元売上一覧: {
    売上SEQ?: { from?: string | null; to?: string | null }
  }
}

/** 商品在庫増減履歴のメンバーのうちソート可能なものを表すリテラル型 */
export type SortableMember
  = '商品在庫増減履歴.商品SEQ'
  | '商品在庫増減履歴.外部システム側ID'
  | '商品在庫増減履歴.商品名'
  | '商品在庫増減履歴.日時'
  | '商品在庫増減履歴.増減数'
  | '商品在庫増減履歴.事由'
  | '商品在庫増減履歴.入荷ID'

/** 商品在庫増減履歴のメンバーのうちソート可能なものを文字列で返します。 */
export function getSortableMember(): SortableMember[] {
  return [
    '商品在庫増減履歴.商品SEQ',
    '商品在庫増減履歴.外部システム側ID',
    '商品在庫増減履歴.商品名',
    '商品在庫増減履歴.日時',
    '商品在庫増減履歴.増減数',
    '商品在庫増減履歴.事由',
    '商品在庫増減履歴.入荷ID',
  ]
}

//#region 画面表示用データ
/** 商品在庫増減履歴の画面表示用データ。 */
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
  /**
   * 外部システム側ID
   */
  外部システム側ID?: string | null
  商品名?: string | null
  /**
   * いつ変動が起きたか
   */
  日時?: string | null
  /**
   * 何個増減したか
   */
  増減数?: string | null
  /**
   * 入荷、売上による引当、在庫調整など
   */
  事由?: string | null
  /**
   * 入荷詳細画面へのリンクに使用
   */
  入荷ID?: string | null
  /** 増減履歴引当元売上一覧 */
  増減履歴引当元売上一覧: DisplayData_増減履歴引当元売上一覧[]

  /** このデータがDBに保存済みかどうか */
  existsInDatabase: boolean
  /** このデータに更新がかかっているかどうか */
  willBeChanged: boolean
  /** このデータが更新確定時に削除されるかどうか */
  willBeDeleted: boolean
}
/** 増減履歴引当元売上一覧の画面表示用データ。 */
export type DisplayData_増減履歴引当元売上一覧 = {
  /**
   * 画面初期表示から保存までの間でこのインスタンスを識別する番号。
   * GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
   * エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
   */
  instanceId: string
  /**
   * 売上詳細画面へのリンクに使用
   */
  売上SEQ?: string | null

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
/** 商品在庫増減履歴の検索条件クラスの空オブジェクトを作成して返します。 */
export function createNewSearchCondition(): SearchCondition {
  return {
    filter: {
      商品SEQ: { from: '', to: '' },
      外部システム側ID: '',
      商品名: '',
      日時: { from: '', to: '' },
      増減数: { from: '', to: '' },
      事由: '',
      入荷ID: '',
      増減履歴引当元売上一覧: {
        売上SEQ: { from: '', to: '' },
      },
    },
    sort: [],
    skip: '',
    take: '',
  }
}

//#region 画面表示用データ新規作成用関数
/** 商品在庫増減履歴の画面表示用データの新しいインスタンスを作成します。 */
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
    日時: '',
    増減数: '',
    事由: '',
    入荷ID: '',
    existsInDatabase: false,
    willBeChanged: true,
    willBeDeleted: false,
    増減履歴引当元売上一覧: [],
  }
}
/** 増減履歴引当元売上一覧の画面表示用データの新しいインスタンスを作成します。 */
export function createNewDisplayData_増減履歴引当元売上一覧(): DisplayData_増減履歴引当元売上一覧 {
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
    売上SEQ: '',
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

  // 商品在庫増減履歴 の等価比較
  if (areEqual商品在庫増減履歴(left, right, compareFunction)) {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = false;
  } else {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = true;
    areEqual = false;
  }
  
  // 増減履歴引当元売上一覧 の等価比較
  // 以下いずれかに該当すれば変更ありと判定
  // * 配列の長さが変わっている
  // * 要素の並び替えが発生している（instanceId 基準）
  // * 配列の要素を instanceId で突合しそれぞれの値を比較
  const leftIds = left.増減履歴引当元売上一覧?.map(x => x.instanceId) ?? [];
  const rightIds = right.増減履歴引当元売上一覧?.map(x => x.instanceId) ?? [];
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
  
  const leftDict = new Map(left.増減履歴引当元売上一覧?.map(x => [x.instanceId, x]) ?? []);
  const rightDict = new Map(right.増減履歴引当元売上一覧?.map(x => [x.instanceId, x]) ?? []);
  for (const instanceId of Array.from(new Set([...leftIds, ...rightIds]))) {
    const x = leftDict.get(instanceId);
    const y = rightDict.get(instanceId);
    if (!x && y && option?.setRightObjectWillBeChanged) y.willBeChanged = true;
    if (!x || !y) continue;
    if (areEqual増減履歴引当元売上一覧(x, y, compareFunction)) {
      if (option?.setRightObjectWillBeChanged) y.willBeChanged = false;
    } else {
      if (option?.setRightObjectWillBeChanged) y.willBeChanged = true;
      areEqual = false;
    }
  }

  return areEqual;

  // ------ 値比較関数 --------

  function areEqual商品在庫増減履歴(left: DisplayData, right: DisplayData, equals: Exclude<Util.DisplayDataDeepEqualOption["compareFunction"], undefined>): boolean {
    if (!equals('Integer', left.商品SEQ, right.商品SEQ)) return false;
    if (!equals('Word', left.外部システム側ID, right.外部システム側ID)) return false;
    if (!equals('Word', left.商品名, right.商品名)) return false;
    if (!equals('DateTime', left.日時, right.日時)) return false;
    if (!equals('Integer', left.増減数, right.増減数)) return false;
    if (!equals('Word', left.事由, right.事由)) return false;
    if (!equals('Word', left.入荷ID, right.入荷ID)) return false;
    if ((left.willBeDeleted ?? false) !== (right.willBeDeleted ?? false)) return false;
    return true;
  }

  function areEqual増減履歴引当元売上一覧(left: DisplayData_増減履歴引当元売上一覧, right: DisplayData_増減履歴引当元売上一覧, equals: Exclude<Util.DisplayDataDeepEqualOption["compareFunction"], undefined>): boolean {
    if (!equals('Integer', left.売上SEQ, right.売上SEQ)) return false;
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
    uniqueId: '9604df00-d30a-4033-affd-6d2d625558fa',
    displayName: '商品在庫増減履歴',
    comment: '商品詳細画面で表示される。\nその商品の在庫がどう変動したかを新しい順に表示する。\n@[引当明細](a1b2c3d4-e5f6-4a9b-6c0d-1e2f3a4b5c6d) @[入荷明細](a5e5b615-6cc1-4a50-8cc7-fe910dca543b) @[在庫調整引当明細](93c13d8d-23ba-44d5-b1ca-694c45efeb2b) を組み合わせる。',
    members: ['商品SEQ', '外部システム側ID', '商品名', '日時', '増減数', '事由', '入荷ID', '増減履歴引当元売上一覧'],
  },
  displayData: {
    '商品SEQ': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'fcfd234d-d0fe-48d5-97d3-a9dd16510e13',
      type: 'int',
      displayName: '商品SEQ',
      comment: '非表示',
    },
    '外部システム側ID': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '1ee75f27-4bed-4d90-9b4a-be02f2ee92e7',
      type: 'word',
      displayName: '外部システム側ID',
      comment: '外部システム側ID',
      stringSearchBehavior: 'Exact',
    },
    '商品名': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '201ff093-8a90-4769-b716-a757f8596b64',
      type: 'word',
      displayName: '商品名',
    },
    '日時': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '61b8be67-a146-4d37-8efe-a4532885593c',
      type: 'datetime',
      displayName: '日時',
      comment: 'いつ変動が起きたか',
    },
    '増減数': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '0ce2b819-d092-4203-8ad6-3a489ba1ee18',
      type: 'int',
      displayName: '増減数',
      comment: '何個増減したか',
      isQuantity: true,
    },
    '事由': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'e508c0a0-abb3-4605-8a44-65eb5d17e904',
      type: 'word',
      displayName: '事由',
      comment: '入荷、売上による引当、在庫調整など',
    },
    '入荷ID': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'f657a5d7-c7ac-4851-afa2-c5534e3f4aa8',
      type: 'word',
      displayName: '入荷ID',
      comment: '入荷詳細画面へのリンクに使用',
    },
    '増減履歴引当元売上一覧': {
      kind: 'children',
      parent: null,
      isOutsideTree: false,
      uniqueId: '6352e9fa-d872-495c-b3a3-7942e992d19a',
      displayName: '増減履歴引当元売上一覧',
      comment: '在庫調整からの引当の場合はリンク不要なので売上のみ',
      members: ['増減履歴引当元売上一覧.売上SEQ'],
    },
    '増減履歴引当元売上一覧.売上SEQ': {
      kind: 'value',
      parent: '増減履歴引当元売上一覧',
      isOutsideTree: false,
      uniqueId: '37fc3118-a1d1-4ec5-b302-5c13af362b80',
      type: 'int',
      displayName: '売上SEQ',
      comment: '売上詳細画面へのリンクに使用',
    },
  },
  searchCondition: {
    '商品SEQ': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'fcfd234d-d0fe-48d5-97d3-a9dd16510e13',
      type: 'int',
      displayName: '商品SEQ',
      comment: '非表示',
    },
    '外部システム側ID': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '1ee75f27-4bed-4d90-9b4a-be02f2ee92e7',
      type: 'word',
      displayName: '外部システム側ID',
      comment: '外部システム側ID',
      stringSearchBehavior: 'Exact',
    },
    '商品名': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '201ff093-8a90-4769-b716-a757f8596b64',
      type: 'word',
      displayName: '商品名',
    },
    '日時': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '61b8be67-a146-4d37-8efe-a4532885593c',
      type: 'datetime',
      displayName: '日時',
      comment: 'いつ変動が起きたか',
    },
    '増減数': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '0ce2b819-d092-4203-8ad6-3a489ba1ee18',
      type: 'int',
      displayName: '増減数',
      comment: '何個増減したか',
      isQuantity: true,
    },
    '事由': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'e508c0a0-abb3-4605-8a44-65eb5d17e904',
      type: 'word',
      displayName: '事由',
      comment: '入荷、売上による引当、在庫調整など',
    },
    '入荷ID': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'f657a5d7-c7ac-4851-afa2-c5534e3f4aa8',
      type: 'word',
      displayName: '入荷ID',
      comment: '入荷詳細画面へのリンクに使用',
    },
    '増減履歴引当元売上一覧': {
      kind: 'child',
      parent: null,
      isOutsideTree: false,
      uniqueId: '6352e9fa-d872-495c-b3a3-7942e992d19a',
      displayName: '増減履歴引当元売上一覧',
      comment: '在庫調整からの引当の場合はリンク不要なので売上のみ',
      members: ['増減履歴引当元売上一覧.売上SEQ'],
    },
    '増減履歴引当元売上一覧.売上SEQ': {
      kind: 'value',
      parent: '増減履歴引当元売上一覧',
      isOutsideTree: false,
      uniqueId: '37fc3118-a1d1-4ec5-b302-5c13af362b80',
      type: 'int',
      displayName: '売上SEQ',
      comment: '売上詳細画面へのリンクに使用',
    },
  },
} as const satisfies Util.AggregateMetadata.Entry

//#endregion メタデータ

//#endregion 関数
