
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : DataStructuresSection.cs
// ※ このファイルを生成したメソッド: Render

import type * as Util from ".."
import * as EnumDefs from "../enum-defs"
import * as 事業所 from "./事業所"
import * as 旧システム部署情報 from "./旧システム部署情報"
import * as 汎用マスタ from "./汎用マスタ"

//#region 型定義
/** 部署の検索時の検索条件の型。 */
export type SearchCondition = Util.SearchConditionBaseType<SearchConditionFilter, SortableMember>

/** 部署の検索時の検索条件の絞り込み条件の型。 */
export type SearchConditionFilter = {
  部署ID?: { from?: string | null; to?: string | null }
  部署名?: string | null
  事業所: 事業所.SearchConditionFilter
  課: {
    コード?: string | null
    旧システムコード: 旧システム部署情報.SearchConditionFilter
    課名称?: string | null
    係: {
      連番?: { from?: string | null; to?: string | null }
      係名称?: string | null
      勤怠管理区分: 汎用マスタ.SearchConditionFilter
    }
  }
}

/** 部署のメンバーのうちソート可能なものを表すリテラル型 */
export type SortableMember
  = '部署.部署ID'
  | '部署.部署名'
  | '部署.事業所.事業所.事業所ID'
  | '部署.事業所.事業所.事業所名'

/** 部署のメンバーのうちソート可能なものを文字列で返します。 */
export function getSortableMember(): SortableMember[] {
  return [
    '部署.部署ID',
    '部署.部署名',
    '部署.事業所.事業所.事業所ID',
    '部署.事業所.事業所.事業所名',
  ]
}

//#region 画面表示用データ
/** 部署の画面表示用データ。 */
export type DisplayData = {
  /**
   * 画面初期表示から保存までの間でこのインスタンスを識別する番号。
   * GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
   * エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
   */
  instanceId: string
  部署ID?: string | null
  部署名?: string | null
  事業所: 事業所.RefTarget
  /** 課 */
  課: DisplayData_課[]

  /** このデータがDBに保存済みかどうか */
  existsInDatabase: boolean
  /** このデータに更新がかかっているかどうか */
  willBeChanged: boolean
  /** このデータが更新確定時に削除されるかどうか */
  willBeDeleted: boolean
  /** 楽観排他制御用のバージョニング情報 */
  version: number | null | undefined
}
/** 課の画面表示用データ。 */
export type DisplayData_課 = {
  /**
   * 画面初期表示から保存までの間でこのインスタンスを識別する番号。
   * GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
   * エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
   */
  instanceId: string
  コード?: string | null
  /**
   * 非主キーの外部参照のユニーク制約の例
   */
  旧システムコード: 旧システム部署情報.RefTarget
  課名称?: string | null
  /** 係 */
  係: DisplayData_課_係[]

  /** このデータがDBに保存済みかどうか */
  existsInDatabase: boolean
  /** このデータに更新がかかっているかどうか */
  willBeChanged: boolean
  /** このデータが更新確定時に削除されるかどうか */
  willBeDeleted: boolean
}
/** 係の画面表示用データ。 */
export type DisplayData_課_係 = {
  /**
   * 画面初期表示から保存までの間でこのインスタンスを識別する番号。
   * GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
   * エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
   */
  instanceId: string
  連番?: string | null
  係名称?: string | null
  勤怠管理区分: 汎用マスタ.RefTarget

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
 * 部署が他の集約から外部参照されるときの部署の型
 */
export type RefTarget = {
  部署ID?: string
  部署名?: string
  事業所: 事業所.RefTarget
}

//#endregion 他の集約から参照されるときの画面表示用オブジェクト

//#endregion 型定義


//#region 関数
/** 部署の検索条件クラスの空オブジェクトを作成して返します。 */
export function createNewSearchCondition(): SearchCondition {
  return {
    filter: {
      部署ID: { from: '', to: '' },
      部署名: '',
      事業所: {
        事業所ID: '',
        事業所名: '',
      },
      課: {
        コード: '',
        旧システムコード: {
          旧システムコード: '',
          名称: '',
        },
        課名称: '',
        係: {
          連番: { from: '', to: '' },
          係名称: '',
          勤怠管理区分: {
            区分値: '',
            表示名称: '',
          },
        },
      },
    },
    sort: [],
    skip: '',
    take: '',
  }
}

//#region 画面表示用データ新規作成用関数
/** 部署の画面表示用データの新しいインスタンスを作成します。 */
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
    部署ID: '',
    部署名: '',
    事業所: 事業所.createNewRefTarget(),
    existsInDatabase: false,
    willBeChanged: true,
    willBeDeleted: false,
    version: null,
    課: [],
  }
}
/** 課の画面表示用データの新しいインスタンスを作成します。 */
export function createNewDisplayData_課(): DisplayData_課 {
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
    コード: '',
    旧システムコード: 旧システム部署情報.createNewRefTarget(),
    課名称: '',
    existsInDatabase: false,
    willBeChanged: true,
    willBeDeleted: false,
    係: [],
  }
}
/** 係の画面表示用データの新しいインスタンスを作成します。 */
export function createNewDisplayData_課_係(): DisplayData_課_係 {
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
    連番: '',
    係名称: '',
    勤怠管理区分: 汎用マスタ.createNewRefTarget(),
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

  // 部署 の等価比較
  if (areEqual部署(left, right, compareFunction)) {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = false;
  } else {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = true;
    areEqual = false;
  }
  
  // 課 の等価比較
  // 以下いずれかに該当すれば変更ありと判定
  // * 配列の長さが変わっている
  // * 要素の並び替えが発生している（instanceId 基準）
  // * 配列の要素を instanceId で突合しそれぞれの値を比較
  const leftIds = left.課?.map(x => x.instanceId) ?? [];
  const rightIds = right.課?.map(x => x.instanceId) ?? [];
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
  
  const leftDict = new Map(left.課?.map(x => [x.instanceId, x]) ?? []);
  const rightDict = new Map(right.課?.map(x => [x.instanceId, x]) ?? []);
  for (const instanceId of Array.from(new Set([...leftIds, ...rightIds]))) {
    const x = leftDict.get(instanceId);
    const y = rightDict.get(instanceId);
    if (!x && y && option?.setRightObjectWillBeChanged) y.willBeChanged = true;
    if (!x || !y) continue;
    if (areEqual課(x, y, compareFunction)) {
      if (option?.setRightObjectWillBeChanged) y.willBeChanged = false;
    } else {
      if (option?.setRightObjectWillBeChanged) y.willBeChanged = true;
      areEqual = false;
    }
  
    // 係 の等価比較
    // 以下いずれかに該当すれば変更ありと判定
    // * 配列の長さが変わっている
    // * 要素の並び替えが発生している（instanceId 基準）
    // * 配列の要素を instanceId で突合しそれぞれの値を比較
    const leftIds2 = x.係?.map(x => x.instanceId) ?? [];
    const rightIds2 = y.係?.map(x => x.instanceId) ?? [];
    if (leftIds2.length !== rightIds2.length) {
      areEqual = false;
    } else {
      for (let i = 0; i < leftIds2.length; i++) {
        if (leftIds2[i] !== rightIds2[i]) {
          areEqual = false;
          break;
        }
      }
    }
    
    const leftDict2 = new Map(x.係?.map(x => [x.instanceId, x]) ?? []);
    const rightDict2 = new Map(y.係?.map(x => [x.instanceId, x]) ?? []);
    for (const instanceId2 of Array.from(new Set([...leftIds2, ...rightIds2]))) {
      const x2 = leftDict2.get(instanceId2);
      const y2 = rightDict2.get(instanceId2);
      if (!x2 && y2 && option?.setRightObjectWillBeChanged) y2.willBeChanged = true;
      if (!x2 || !y2) continue;
      if (areEqual課_係(x2, y2, compareFunction)) {
        if (option?.setRightObjectWillBeChanged) y2.willBeChanged = false;
      } else {
        if (option?.setRightObjectWillBeChanged) y2.willBeChanged = true;
        areEqual = false;
      }
    }
  }

  return areEqual;

  // ------ 値比較関数 --------

  function areEqual部署(left: DisplayData, right: DisplayData, equals: Exclude<Util.DisplayDataDeepEqualOption["compareFunction"], undefined>): boolean {
    if (!equals('Integer', left.部署ID, right.部署ID)) return false;
    if (!equals('Word', left.部署名, right.部署名)) return false;
    if (!equals('Word', left.事業所?.事業所ID, right.事業所?.事業所ID)) return false;
    if (!equals('Word', left.事業所?.事業所名, right.事業所?.事業所名)) return false;
    if ((left.willBeDeleted ?? false) !== (right.willBeDeleted ?? false)) return false;
    return true;
  }

  function areEqual課(left: DisplayData_課, right: DisplayData_課, equals: Exclude<Util.DisplayDataDeepEqualOption["compareFunction"], undefined>): boolean {
    if (!equals('Word', left.コード, right.コード)) return false;
    if (!equals('Word', left.旧システムコード?.旧システムコード, right.旧システムコード?.旧システムコード)) return false;
    if (!equals('Word', left.旧システムコード?.名称, right.旧システムコード?.名称)) return false;
    if (!equals('Word', left.課名称, right.課名称)) return false;
    if ((left.willBeDeleted ?? false) !== (right.willBeDeleted ?? false)) return false;
    return true;
  }

  function areEqual課_係(left: DisplayData_課_係, right: DisplayData_課_係, equals: Exclude<Util.DisplayDataDeepEqualOption["compareFunction"], undefined>): boolean {
    if (!equals('Integer', left.連番, right.連番)) return false;
    if (!equals('Word', left.係名称, right.係名称)) return false;
    if (!equals('Word', left.勤怠管理区分?.区分値, right.勤怠管理区分?.区分値)) return false;
    if (!equals('Word', left.勤怠管理区分?.表示名称, right.勤怠管理区分?.表示名称)) return false;
    if ((left.willBeDeleted ?? false) !== (right.willBeDeleted ?? false)) return false;
    return true;
  }
}
//#endregion ディープイコール関数

//#region 他の集約から参照されるときの画面表示用オブジェクトの新規作成関数
/** 部署RefTargetを新規作成します。 */
export function createNewRefTarget(): RefTarget {
  return {
    部署ID: undefined,
    部署名: undefined,
    事業所: {
      事業所ID: undefined,
      事業所名: undefined,
    },
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
    uniqueId: '4c96eb60-249d-414c-8e3e-4b5a981fa8c6',
    displayName: '部署',
    useSoftDelete: true,
    members: ['部署ID', '部署名', '事業所', '課'],
  },
  displayData: {
    '部署ID': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '2d52d3fd-0e35-4371-8ebf-7c70d8a2e1c6',
      type: 'int',
      displayName: '部署ID',
      isKey: true,
    },
    '部署名': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '910a581f-4337-423f-958b-c9975a2551e2',
      type: 'word',
      displayName: '部署名',
      isNotNull: true,
    },
    '事業所': {
      kind: 'ref',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'ce9290d5-a160-4e13-82db-bb3df15e6bc3',
      refTo: 'ref-to:事業所',
      displayName: '事業所',
      members: ['事業所.事業所ID', '事業所.事業所名'],
    },
    '事業所.事業所ID': {
      kind: 'value',
      parent: '事業所',
      isOutsideTree: true,
      uniqueId: 'c185f4b5-ed5a-43ba-a418-effca9791119',
      type: 'word',
      displayName: '事業所ID',
      comment: 'ビュー内部でハードコードする。',
      isKey: true,
    },
    '事業所.事業所名': {
      kind: 'value',
      parent: '事業所',
      isOutsideTree: true,
      uniqueId: '25b6c40f-24a7-4c6e-9df5-385149e739b7',
      type: 'word',
      displayName: '事業所名',
      comment: 'ビュー内部でハードコードする。',
    },
    '課': {
      kind: 'children',
      parent: null,
      isOutsideTree: false,
      uniqueId: '51d08ec1-ddd8-4822-bf2b-91c458ee7e62',
      displayName: '課',
      uniqueConstraints: '1f6febf0-adfe-4377-8749-9eb7223ef14c;',
      members: ['課.コード', '課.旧システムコード', '課.課名称', '課.係'],
    },
    '課.コード': {
      kind: 'value',
      parent: '課',
      isOutsideTree: false,
      uniqueId: '1e4a365b-7b39-46da-8be0-de2d7bb75241',
      type: 'word',
      displayName: 'コード',
      isKey: true,
    },
    '課.旧システムコード': {
      kind: 'ref',
      parent: '課',
      isOutsideTree: false,
      uniqueId: '1f6febf0-adfe-4377-8749-9eb7223ef14c',
      refTo: 'ref-to:旧システム部署情報',
      displayName: '旧システムコード',
      comment: '非主キーの外部参照のユニーク制約の例',
      members: ['課.旧システムコード.旧システムコード', '課.旧システムコード.名称'],
    },
    '課.旧システムコード.旧システムコード': {
      kind: 'value',
      parent: '課.旧システムコード',
      isOutsideTree: true,
      uniqueId: '78f8403b-9d15-424e-91c3-f31bd0aeb562',
      type: 'word',
      displayName: '旧システムコード',
      isKey: true,
    },
    '課.旧システムコード.名称': {
      kind: 'value',
      parent: '課.旧システムコード',
      isOutsideTree: true,
      uniqueId: '1376a08d-3312-46c6-b51b-d4e70a74908a',
      type: 'word',
      displayName: '名称',
      isNotNull: true,
    },
    '課.課名称': {
      kind: 'value',
      parent: '課',
      isOutsideTree: false,
      uniqueId: 'f8667bbb-c921-46ec-869b-b989649634d5',
      type: 'word',
      displayName: '課名称',
      isNotNull: true,
    },
    '課.係': {
      kind: 'children',
      parent: '課',
      isOutsideTree: false,
      uniqueId: '3d203063-3cf5-4acd-ba63-ecbe5701782d',
      displayName: '係',
      members: ['課.係.連番', '課.係.係名称', '課.係.勤怠管理区分'],
    },
    '課.係.連番': {
      kind: 'value',
      parent: '課.係',
      isOutsideTree: false,
      uniqueId: '72f8ff2c-a014-428b-8a74-1b50d033cd94',
      type: 'int',
      displayName: '連番',
      isKey: true,
    },
    '課.係.係名称': {
      kind: 'value',
      parent: '課.係',
      isOutsideTree: false,
      uniqueId: '61e72ff4-9992-4a83-a546-0fb4c93ef3fa',
      type: 'word',
      displayName: '係名称',
      isNotNull: true,
    },
    '課.係.勤怠管理区分': {
      kind: 'ref',
      parent: '課.係',
      isOutsideTree: false,
      uniqueId: '1c9be8ce-0d9d-41c0-a3b4-d56aac06b0f2',
      refTo: 'ref-to:汎用マスタ',
      displayName: '勤怠管理区分',
      category: '勤怠管理区分',
      members: ['課.係.勤怠管理区分.区分値', '課.係.勤怠管理区分.表示名称'],
    },
    '課.係.勤怠管理区分.区分値': {
      kind: 'value',
      parent: '課.係.勤怠管理区分',
      isOutsideTree: true,
      uniqueId: 'd53ec0af-86df-4951-97f1-32ef5fcc7dc0',
      type: 'word',
      displayName: '区分値',
      isKey: true,
    },
    '課.係.勤怠管理区分.表示名称': {
      kind: 'value',
      parent: '課.係.勤怠管理区分',
      isOutsideTree: true,
      uniqueId: '062655d6-b3bb-422b-9c9f-1543c83cba6e',
      type: 'word',
      displayName: '表示名称',
    },
  },
  searchCondition: {
    '部署ID': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '2d52d3fd-0e35-4371-8ebf-7c70d8a2e1c6',
      type: 'int',
      displayName: '部署ID',
      isKey: true,
    },
    '部署名': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '910a581f-4337-423f-958b-c9975a2551e2',
      type: 'word',
      displayName: '部署名',
      isNotNull: true,
    },
    '事業所': {
      kind: 'ref',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'ce9290d5-a160-4e13-82db-bb3df15e6bc3',
      refTo: 'ref-to:事業所',
      displayName: '事業所',
      members: ['事業所.事業所ID', '事業所.事業所名'],
    },
    '事業所.事業所ID': {
      kind: 'value',
      parent: '事業所',
      isOutsideTree: true,
      uniqueId: 'c185f4b5-ed5a-43ba-a418-effca9791119',
      type: 'word',
      displayName: '事業所ID',
      comment: 'ビュー内部でハードコードする。',
      isKey: true,
    },
    '事業所.事業所名': {
      kind: 'value',
      parent: '事業所',
      isOutsideTree: true,
      uniqueId: '25b6c40f-24a7-4c6e-9df5-385149e739b7',
      type: 'word',
      displayName: '事業所名',
      comment: 'ビュー内部でハードコードする。',
    },
    '課': {
      kind: 'child',
      parent: null,
      isOutsideTree: false,
      uniqueId: '51d08ec1-ddd8-4822-bf2b-91c458ee7e62',
      displayName: '課',
      uniqueConstraints: '1f6febf0-adfe-4377-8749-9eb7223ef14c;',
      members: ['課.コード', '課.旧システムコード', '課.課名称', '課.係'],
    },
    '課.コード': {
      kind: 'value',
      parent: '課',
      isOutsideTree: false,
      uniqueId: '1e4a365b-7b39-46da-8be0-de2d7bb75241',
      type: 'word',
      displayName: 'コード',
      isKey: true,
    },
    '課.旧システムコード': {
      kind: 'ref',
      parent: '課',
      isOutsideTree: false,
      uniqueId: '1f6febf0-adfe-4377-8749-9eb7223ef14c',
      refTo: 'ref-to:旧システム部署情報',
      displayName: '旧システムコード',
      comment: '非主キーの外部参照のユニーク制約の例',
      members: ['課.旧システムコード.旧システムコード', '課.旧システムコード.名称'],
    },
    '課.旧システムコード.旧システムコード': {
      kind: 'value',
      parent: '課.旧システムコード',
      isOutsideTree: true,
      uniqueId: '78f8403b-9d15-424e-91c3-f31bd0aeb562',
      type: 'word',
      displayName: '旧システムコード',
      isKey: true,
    },
    '課.旧システムコード.名称': {
      kind: 'value',
      parent: '課.旧システムコード',
      isOutsideTree: true,
      uniqueId: '1376a08d-3312-46c6-b51b-d4e70a74908a',
      type: 'word',
      displayName: '名称',
      isNotNull: true,
    },
    '課.課名称': {
      kind: 'value',
      parent: '課',
      isOutsideTree: false,
      uniqueId: 'f8667bbb-c921-46ec-869b-b989649634d5',
      type: 'word',
      displayName: '課名称',
      isNotNull: true,
    },
    '課.係': {
      kind: 'child',
      parent: '課',
      isOutsideTree: false,
      uniqueId: '3d203063-3cf5-4acd-ba63-ecbe5701782d',
      displayName: '係',
      members: ['課.係.連番', '課.係.係名称', '課.係.勤怠管理区分'],
    },
    '課.係.連番': {
      kind: 'value',
      parent: '課.係',
      isOutsideTree: false,
      uniqueId: '72f8ff2c-a014-428b-8a74-1b50d033cd94',
      type: 'int',
      displayName: '連番',
      isKey: true,
    },
    '課.係.係名称': {
      kind: 'value',
      parent: '課.係',
      isOutsideTree: false,
      uniqueId: '61e72ff4-9992-4a83-a546-0fb4c93ef3fa',
      type: 'word',
      displayName: '係名称',
      isNotNull: true,
    },
    '課.係.勤怠管理区分': {
      kind: 'ref',
      parent: '課.係',
      isOutsideTree: false,
      uniqueId: '1c9be8ce-0d9d-41c0-a3b4-d56aac06b0f2',
      refTo: 'ref-to:汎用マスタ',
      displayName: '勤怠管理区分',
      category: '勤怠管理区分',
      members: ['課.係.勤怠管理区分.区分値', '課.係.勤怠管理区分.表示名称'],
    },
    '課.係.勤怠管理区分.区分値': {
      kind: 'value',
      parent: '課.係.勤怠管理区分',
      isOutsideTree: true,
      uniqueId: 'd53ec0af-86df-4951-97f1-32ef5fcc7dc0',
      type: 'word',
      displayName: '区分値',
      isKey: true,
    },
    '課.係.勤怠管理区分.表示名称': {
      kind: 'value',
      parent: '課.係.勤怠管理区分',
      isOutsideTree: true,
      uniqueId: '062655d6-b3bb-422b-9c9f-1543c83cba6e',
      type: 'word',
      displayName: '表示名称',
    },
  },
} as const satisfies Util.AggregateMetadata.Entry

//#endregion メタデータ

//#endregion 関数
