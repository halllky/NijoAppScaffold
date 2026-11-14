
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : DataStructuresSection.cs
// ※ このファイルを生成したメソッド: Render

import type * as Util from ".."
import * as EnumDefs from "../enum-defs"

//#region 型定義
/** 従業員マスタの検索時の検索条件の型。 */
export type SearchCondition = Util.SearchConditionBaseType<SearchConditionFilter, SortableMember>

/** 従業員マスタの検索時の検索条件の絞り込み条件の型。 */
export type SearchConditionFilter = {
  従業員番号?: string | null
  氏名?: string | null
  従業員マスタの権限: {
    権限区分?: { 'システム管理者'?: boolean | null, '入荷担当'?: boolean | null, '販売担当'?: boolean | null }
  }
  Version?: { from?: string | null; to?: string | null }
}

/** 従業員マスタのメンバーのうちソート可能なものを表すリテラル型 */
export type SortableMember
  = '従業員マスタ.従業員番号'
  | '従業員マスタ.氏名'
  | '従業員マスタ.Version'

/** 従業員マスタのメンバーのうちソート可能なものを文字列で返します。 */
export function getSortableMember(): SortableMember[] {
  return [
    '従業員マスタ.従業員番号',
    '従業員マスタ.氏名',
    '従業員マスタ.Version',
  ]
}

//#region 画面表示用データ
/** 従業員マスタの画面表示用データ。 */
export type DisplayData = {
  /**
   * 画面初期表示から保存までの間でこのインスタンスを識別する番号。
   * GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
   * エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
   */
  instanceId: string
  従業員番号?: string | null
  氏名?: string | null
  /**
   * 楽観排他制御用
   */
  Version?: string | null
  /** 権限 */
  従業員マスタの権限: DisplayData_従業員マスタの権限[]

  /** このデータがDBに保存済みかどうか */
  existsInDatabase: boolean
  /** このデータに更新がかかっているかどうか */
  willBeChanged: boolean
  /** このデータが更新確定時に削除されるかどうか */
  willBeDeleted: boolean
}
/** 権限の画面表示用データ。 */
export type DisplayData_従業員マスタの権限 = {
  /**
   * 画面初期表示から保存までの間でこのインスタンスを識別する番号。
   * GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
   * エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
   */
  instanceId: string
  権限区分?: EnumDefs.権限区分 | null

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
 * 従業員マスタが他の集約から外部参照されるときの従業員マスタの型
 */
export type RefTarget = {
  従業員番号?: string
  氏名?: string
  Version?: string
}

//#endregion 他の集約から参照されるときの画面表示用オブジェクト

//#endregion 型定義


//#region 関数
/** 従業員マスタの検索条件クラスの空オブジェクトを作成して返します。 */
export function createNewSearchCondition(): SearchCondition {
  return {
    filter: {
      従業員番号: '',
      氏名: '',
      従業員マスタの権限: {
        権限区分: { 'システム管理者': false, '入荷担当': false, '販売担当': false },
      },
      Version: { from: '', to: '' },
    },
    sort: [],
    skip: '',
    take: '',
  }
}

//#region 画面表示用データ新規作成用関数
/** 従業員マスタの画面表示用データの新しいインスタンスを作成します。 */
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
    従業員番号: '',
    氏名: '',
    Version: '',
    existsInDatabase: false,
    willBeChanged: true,
    willBeDeleted: false,
    従業員マスタの権限: [],
  }
}
/** 権限の画面表示用データの新しいインスタンスを作成します。 */
export function createNewDisplayData_従業員マスタの権限(): DisplayData_従業員マスタの権限 {
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
    権限区分: null,
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

  // 従業員マスタ の等価比較
  if (areEqual従業員マスタ(left, right, compareFunction)) {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = false;
  } else {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = true;
    areEqual = false;
  }
  
  // 権限 の等価比較
  // 以下いずれかに該当すれば変更ありと判定
  // * 配列の長さが変わっている
  // * 要素の並び替えが発生している（instanceId 基準）
  // * 配列の要素を instanceId で突合しそれぞれの値を比較
  const leftIds = left.従業員マスタの権限?.map(x => x.instanceId) ?? [];
  const rightIds = right.従業員マスタの権限?.map(x => x.instanceId) ?? [];
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
  
  const leftDict = new Map(left.従業員マスタの権限?.map(x => [x.instanceId, x]) ?? []);
  const rightDict = new Map(right.従業員マスタの権限?.map(x => [x.instanceId, x]) ?? []);
  for (const instanceId of Array.from(new Set([...leftIds, ...rightIds]))) {
    const x = leftDict.get(instanceId);
    const y = rightDict.get(instanceId);
    if (!x && y && option?.setRightObjectWillBeChanged) y.willBeChanged = true;
    if (!x || !y) continue;
    if (areEqual従業員マスタの権限(x, y, compareFunction)) {
      if (option?.setRightObjectWillBeChanged) y.willBeChanged = false;
    } else {
      if (option?.setRightObjectWillBeChanged) y.willBeChanged = true;
      areEqual = false;
    }
  }

  return areEqual;

  // ------ 値比較関数 --------

  function areEqual従業員マスタ(left: DisplayData, right: DisplayData, equals: Exclude<Util.DisplayDataDeepEqualOption["compareFunction"], undefined>): boolean {
    if (!equals('Word', left.従業員番号, right.従業員番号)) return false;
    if (!equals('Word', left.氏名, right.氏名)) return false;
    if (!equals('Integer', left.Version, right.Version)) return false;
    if ((left.willBeDeleted ?? false) !== (right.willBeDeleted ?? false)) return false;
    return true;
  }

  function areEqual従業員マスタの権限(left: DisplayData_従業員マスタの権限, right: DisplayData_従業員マスタの権限, equals: Exclude<Util.DisplayDataDeepEqualOption["compareFunction"], undefined>): boolean {
    if (!equals('Enum', left.権限区分, right.権限区分, '権限区分')) return false;
    if ((left.willBeDeleted ?? false) !== (right.willBeDeleted ?? false)) return false;
    return true;
  }
}
//#endregion ディープイコール関数

//#region 他の集約から参照されるときの画面表示用オブジェクトの新規作成関数
/** 従業員マスタRefTargetを新規作成します。 */
export function createNewRefTarget(): RefTarget {
  return {
    従業員番号: undefined,
    氏名: undefined,
    Version: undefined,
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
    uniqueId: '7508f4ed-a9ba-4c3d-b258-eb328affebdc',
    displayName: '従業員マスタ',
    comment: '@[従業員](659bf33a-c624-4e67-96a9-460809d70995) のマスタ画面表示用データ。',
    members: ['従業員番号', '氏名', '従業員マスタの権限', 'Version'],
  },
  displayData: {
    '従業員番号': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '525a40bd-74c1-4c09-aea1-4cc238e05c59',
      type: 'word',
      displayName: '従業員番号',
      stringSearchBehavior: 'Exact',
    },
    '氏名': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '3ea7c1ea-ab69-47a5-9821-f9ad112e65e2',
      type: 'word',
      displayName: '氏名',
    },
    'Version': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '8d4bc54f-4621-426d-bbd1-82624c07dc62',
      type: 'int',
      displayName: 'Version',
      comment: '楽観排他制御用',
    },
    '従業員マスタの権限': {
      kind: 'children',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'ac564251-087c-4545-8800-032fdd129fef',
      displayName: '従業員マスタの権限',
      members: ['従業員マスタの権限.権限区分'],
    },
    '従業員マスタの権限.権限区分': {
      kind: 'value',
      parent: '従業員マスタの権限',
      isOutsideTree: false,
      uniqueId: 'e999e4e8-b28c-47f5-89f8-d68d08aad589',
      enumType: '権限区分',
      displayName: '権限区分',
    },
  },
  searchCondition: {
    '従業員番号': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '525a40bd-74c1-4c09-aea1-4cc238e05c59',
      type: 'word',
      displayName: '従業員番号',
      stringSearchBehavior: 'Exact',
    },
    '氏名': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '3ea7c1ea-ab69-47a5-9821-f9ad112e65e2',
      type: 'word',
      displayName: '氏名',
    },
    '従業員マスタの権限': {
      kind: 'child',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'ac564251-087c-4545-8800-032fdd129fef',
      displayName: '従業員マスタの権限',
      members: ['従業員マスタの権限.権限区分'],
    },
    '従業員マスタの権限.権限区分': {
      kind: 'value',
      parent: '従業員マスタの権限',
      isOutsideTree: false,
      uniqueId: 'e999e4e8-b28c-47f5-89f8-d68d08aad589',
      enumType: '権限区分',
      displayName: '権限区分',
    },
    'Version': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '8d4bc54f-4621-426d-bbd1-82624c07dc62',
      type: 'int',
      displayName: 'Version',
      comment: '楽観排他制御用',
    },
  },
} as const satisfies Util.AggregateMetadata.Entry

//#endregion メタデータ

//#endregion 関数
