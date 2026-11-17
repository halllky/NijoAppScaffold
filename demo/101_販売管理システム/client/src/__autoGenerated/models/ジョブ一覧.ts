
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : DataStructuresSection.cs
// ※ このファイルを生成したメソッド: Render
// ※ このファイルを生成した nijo のバージョン: 0.0.0-dev

import type * as Util from ".."
import * as EnumDefs from "../enum-defs"

//#region 型定義
/** ジョブ一覧の検索時の検索条件の型。 */
export type SearchCondition = Util.SearchConditionBaseType<SearchConditionFilter, SortableMember>

/** ジョブ一覧の検索時の検索条件の絞り込み条件の型。 */
export type SearchConditionFilter = {
  ジョブID?: string | null
  種類?: string | null
  状態?: { '待機'?: boolean | null, '実行中'?: boolean | null, '正常終了'?: boolean | null, '異常終了'?: boolean | null }
  起動したユーザー?: string | null
  登録日時?: { from?: string | null; to?: string | null }
  開始日時?: { from?: string | null; to?: string | null }
  終了日時?: { from?: string | null; to?: string | null }
  結果メッセージ?: string | null
  スプールのファイル: {
    ファイル名?: string | null
  }
}

/** ジョブ一覧のメンバーのうちソート可能なものを表すリテラル型 */
export type SortableMember
  = 'ジョブ一覧.ジョブID'
  | 'ジョブ一覧.種類'
  | 'ジョブ一覧.状態'
  | 'ジョブ一覧.起動したユーザー'
  | 'ジョブ一覧.登録日時'
  | 'ジョブ一覧.開始日時'
  | 'ジョブ一覧.終了日時'
  | 'ジョブ一覧.結果メッセージ'

/** ジョブ一覧のメンバーのうちソート可能なものを文字列で返します。 */
export function getSortableMember(): SortableMember[] {
  return [
    'ジョブ一覧.ジョブID',
    'ジョブ一覧.種類',
    'ジョブ一覧.状態',
    'ジョブ一覧.起動したユーザー',
    'ジョブ一覧.登録日時',
    'ジョブ一覧.開始日時',
    'ジョブ一覧.終了日時',
    'ジョブ一覧.結果メッセージ',
  ]
}

//#region 画面表示用データ
/** ジョブ一覧の画面表示用データ。 */
export type DisplayData = {
  /**
   * 画面初期表示から保存までの間でこのインスタンスを識別する番号。
   * GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
   * エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
   */
  instanceId: string
  ジョブID?: string | null
  種類?: string | null
  状態?: EnumDefs.ジョブの状態 | null
  起動したユーザー?: string | null
  登録日時?: string | null
  開始日時?: string | null
  終了日時?: string | null
  結果メッセージ?: string | null
  /** スプールのファイル */
  スプールのファイル: DisplayData_スプールのファイル[]

  /** このデータがDBに保存済みかどうか */
  existsInDatabase: boolean
  /** このデータに更新がかかっているかどうか */
  willBeChanged: boolean
  /** このデータが更新確定時に削除されるかどうか */
  willBeDeleted: boolean
}
/** スプールのファイルの画面表示用データ。 */
export type DisplayData_スプールのファイル = {
  /**
   * 画面初期表示から保存までの間でこのインスタンスを識別する番号。
   * GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
   * エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
   */
  instanceId: string
  ファイル名?: string | null

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
/** ジョブ一覧の検索条件クラスの空オブジェクトを作成して返します。 */
export function createNewSearchCondition(): SearchCondition {
  return {
    filter: {
      ジョブID: '',
      種類: '',
      状態: { '待機': false, '実行中': false, '正常終了': false, '異常終了': false },
      起動したユーザー: '',
      登録日時: { from: '', to: '' },
      開始日時: { from: '', to: '' },
      終了日時: { from: '', to: '' },
      結果メッセージ: '',
      スプールのファイル: {
        ファイル名: '',
      },
    },
    sort: [],
    skip: '',
    take: '',
  }
}

//#region 画面表示用データ新規作成用関数
/** ジョブ一覧の画面表示用データの新しいインスタンスを作成します。 */
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
    ジョブID: '',
    種類: '',
    状態: null,
    起動したユーザー: '',
    登録日時: '',
    開始日時: '',
    終了日時: '',
    結果メッセージ: '',
    existsInDatabase: false,
    willBeChanged: true,
    willBeDeleted: false,
    スプールのファイル: [],
  }
}
/** スプールのファイルの画面表示用データの新しいインスタンスを作成します。 */
export function createNewDisplayData_スプールのファイル(): DisplayData_スプールのファイル {
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
    ファイル名: '',
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

  // ジョブ一覧 の等価比較
  if (areEqualジョブ一覧(left, right, compareFunction)) {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = false;
  } else {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = true;
    areEqual = false;
  }
  
  // スプールのファイル の等価比較
  // 以下いずれかに該当すれば変更ありと判定
  // * 配列の長さが変わっている
  // * 要素の並び替えが発生している（instanceId 基準）
  // * 配列の要素を instanceId で突合しそれぞれの値を比較
  const leftIds = left.スプールのファイル?.map(x => x.instanceId) ?? [];
  const rightIds = right.スプールのファイル?.map(x => x.instanceId) ?? [];
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
  
  const leftDict = new Map(left.スプールのファイル?.map(x => [x.instanceId, x]) ?? []);
  const rightDict = new Map(right.スプールのファイル?.map(x => [x.instanceId, x]) ?? []);
  for (const instanceId of Array.from(new Set([...leftIds, ...rightIds]))) {
    const x = leftDict.get(instanceId);
    const y = rightDict.get(instanceId);
    if (!x && y && option?.setRightObjectWillBeChanged) y.willBeChanged = true;
    if (!x || !y) continue;
    if (areEqualスプールのファイル(x, y, compareFunction)) {
      if (option?.setRightObjectWillBeChanged) y.willBeChanged = false;
    } else {
      if (option?.setRightObjectWillBeChanged) y.willBeChanged = true;
      areEqual = false;
    }
  }

  return areEqual;

  // ------ 値比較関数 --------

  function areEqualジョブ一覧(left: DisplayData, right: DisplayData, equals: Exclude<Util.DisplayDataDeepEqualOption["compareFunction"], undefined>): boolean {
    if (!equals('Word', left.ジョブID, right.ジョブID)) return false;
    if (!equals('Word', left.種類, right.種類)) return false;
    if (!equals('Enum', left.状態, right.状態, 'ジョブの状態')) return false;
    if (!equals('Word', left.起動したユーザー, right.起動したユーザー)) return false;
    if (!equals('DateTime', left.登録日時, right.登録日時)) return false;
    if (!equals('DateTime', left.開始日時, right.開始日時)) return false;
    if (!equals('DateTime', left.終了日時, right.終了日時)) return false;
    if (!equals('Description', left.結果メッセージ, right.結果メッセージ)) return false;
    if ((left.willBeDeleted ?? false) !== (right.willBeDeleted ?? false)) return false;
    return true;
  }

  function areEqualスプールのファイル(left: DisplayData_スプールのファイル, right: DisplayData_スプールのファイル, equals: Exclude<Util.DisplayDataDeepEqualOption["compareFunction"], undefined>): boolean {
    if (!equals('Word', left.ファイル名, right.ファイル名)) return false;
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
    uniqueId: '9b9830f1-bade-476b-a356-9c789a33d6a3',
    displayName: 'ジョブ一覧',
    comment: '@[ジョブ](380ab309-2f96-4bf0-9500-fd759477bdee) の一覧。システム管理者は全員のジョブを、それ以外のユーザーは自分が起動したジョブだけを返す。\nスプールのファイルは、このサーバーで実行したジョブのものだけを返す。',
    呼び出し可能な利用者: 'ログインユーザー全員',
    members: ['ジョブID', '種類', '状態', '起動したユーザー', '登録日時', '開始日時', '終了日時', '結果メッセージ', 'スプールのファイル'],
  },
  displayData: {
    'ジョブID': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '25bba41f-cab4-4c08-99e0-138ca2689bb8',
      type: 'word',
      displayName: 'ジョブID',
    },
    '種類': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '99d76af8-b761-41b0-b66f-2f179aadbaf1',
      type: 'word',
      displayName: '種類',
    },
    '状態': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '178fa453-30e2-42c7-89a2-444cd56b842f',
      enumType: 'ジョブの状態',
      displayName: '状態',
    },
    '起動したユーザー': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '875fbeb0-79af-4720-b33e-79f0ffb4f2c2',
      type: 'word',
      displayName: '起動したユーザー',
    },
    '登録日時': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '1c0ab617-a0de-480f-ad93-e0d9e43ac5b7',
      type: 'datetime',
      displayName: '登録日時',
    },
    '開始日時': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'da756181-fb3c-4c9e-9527-e4da7402bcec',
      type: 'datetime',
      displayName: '開始日時',
    },
    '終了日時': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '1ed25361-2a02-4db4-930a-13d256d26fb4',
      type: 'datetime',
      displayName: '終了日時',
    },
    '結果メッセージ': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '5d211cf4-fc39-45a8-9eb9-c15c927f919e',
      type: 'description',
      displayName: '結果メッセージ',
    },
    'スプールのファイル': {
      kind: 'children',
      parent: null,
      isOutsideTree: false,
      uniqueId: '4fa27ed9-7afc-47fc-8f8b-894fcc7f5d2f',
      displayName: 'スプールのファイル',
      comment: 'スプールに出力されたファイル。DB ではなくサーバーのフォルダから取得する。',
      members: ['スプールのファイル.ファイル名'],
    },
    'スプールのファイル.ファイル名': {
      kind: 'value',
      parent: 'スプールのファイル',
      isOutsideTree: false,
      uniqueId: 'fa1fe58e-f45c-4fd8-9de5-4ab52d2cd7d3',
      type: 'word',
      displayName: 'ファイル名',
    },
  },
  searchCondition: {
    'ジョブID': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '25bba41f-cab4-4c08-99e0-138ca2689bb8',
      type: 'word',
      displayName: 'ジョブID',
    },
    '種類': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '99d76af8-b761-41b0-b66f-2f179aadbaf1',
      type: 'word',
      displayName: '種類',
    },
    '状態': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '178fa453-30e2-42c7-89a2-444cd56b842f',
      enumType: 'ジョブの状態',
      displayName: '状態',
    },
    '起動したユーザー': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '875fbeb0-79af-4720-b33e-79f0ffb4f2c2',
      type: 'word',
      displayName: '起動したユーザー',
    },
    '登録日時': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '1c0ab617-a0de-480f-ad93-e0d9e43ac5b7',
      type: 'datetime',
      displayName: '登録日時',
    },
    '開始日時': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'da756181-fb3c-4c9e-9527-e4da7402bcec',
      type: 'datetime',
      displayName: '開始日時',
    },
    '終了日時': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '1ed25361-2a02-4db4-930a-13d256d26fb4',
      type: 'datetime',
      displayName: '終了日時',
    },
    '結果メッセージ': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '5d211cf4-fc39-45a8-9eb9-c15c927f919e',
      type: 'description',
      displayName: '結果メッセージ',
    },
    'スプールのファイル': {
      kind: 'child',
      parent: null,
      isOutsideTree: false,
      uniqueId: '4fa27ed9-7afc-47fc-8f8b-894fcc7f5d2f',
      displayName: 'スプールのファイル',
      comment: 'スプールに出力されたファイル。DB ではなくサーバーのフォルダから取得する。',
      members: ['スプールのファイル.ファイル名'],
    },
    'スプールのファイル.ファイル名': {
      kind: 'value',
      parent: 'スプールのファイル',
      isOutsideTree: false,
      uniqueId: 'fa1fe58e-f45c-4fd8-9de5-4ab52d2cd7d3',
      type: 'word',
      displayName: 'ファイル名',
    },
  },
} as const satisfies Util.AggregateMetadata.Entry

//#endregion メタデータ

//#endregion 関数
