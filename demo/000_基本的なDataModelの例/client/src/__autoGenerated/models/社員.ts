
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : DataStructuresSection.cs
// ※ このファイルを生成したメソッド: Render

import type * as Util from ".."
import * as EnumDefs from "../enum-defs"
import * as 汎用マスタ from "./汎用マスタ"
import * as 部署 from "./部署"

//#region 型定義
/** 社員の検索時の検索条件の型。 */
export type SearchCondition = Util.SearchConditionBaseType<SearchConditionFilter, SortableMember>

/** 社員の検索時の検索条件の絞り込み条件の型。 */
export type SearchConditionFilter = {
  社員ID?: { from?: string | null; to?: string | null }
  氏名?: string | null
  所属部署: 部署.SearchConditionFilter
  契約種別: 汎用マスタ.SearchConditionFilter
}

/** 社員のメンバーのうちソート可能なものを表すリテラル型 */
export type SortableMember
  = '社員.社員ID'
  | '社員.氏名'
  | '社員.所属部署.部署.部署ID'
  | '社員.所属部署.部署.部署名'
  | '社員.所属部署.部署.事業所.事業所.事業所ID'
  | '社員.所属部署.部署.事業所.事業所.事業所名'
  | '社員.契約種別.汎用マスタ.区分値'
  | '社員.契約種別.汎用マスタ.表示名称'

/** 社員のメンバーのうちソート可能なものを文字列で返します。 */
export function getSortableMember(): SortableMember[] {
  return [
    '社員.社員ID',
    '社員.氏名',
    '社員.所属部署.部署.部署ID',
    '社員.所属部署.部署.部署名',
    '社員.所属部署.部署.事業所.事業所.事業所ID',
    '社員.所属部署.部署.事業所.事業所.事業所名',
    '社員.契約種別.汎用マスタ.区分値',
    '社員.契約種別.汎用マスタ.表示名称',
  ]
}

//#region 画面表示用データ
/** 社員の画面表示用データ。 */
export type DisplayData = {
  /**
   * 画面初期表示から保存までの間でこのインスタンスを識別する番号。
   * GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
   * エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
   */
  instanceId: string
  /**
   * 入社時に発行される一意なID
   */
  社員ID?: string | null
  氏名?: string | null
  所属部署: 部署.RefTarget
  契約種別: 汎用マスタ.RefTarget

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
//#endregion 他の集約から参照されるときの画面表示用オブジェクト

//#endregion 型定義


//#region 関数
/** 社員の検索条件クラスの空オブジェクトを作成して返します。 */
export function createNewSearchCondition(): SearchCondition {
  return {
    filter: {
      社員ID: { from: '', to: '' },
      氏名: '',
      所属部署: {
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
      契約種別: {
        区分値: '',
        表示名称: '',
      },
    },
    sort: [],
    skip: '',
    take: '',
  }
}

//#region 画面表示用データ新規作成用関数
/** 社員の画面表示用データの新しいインスタンスを作成します。 */
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
    社員ID: '',
    氏名: '',
    所属部署: 部署.createNewRefTarget(),
    契約種別: 汎用マスタ.createNewRefTarget(),
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

  // 社員 の等価比較
  if (areEqual社員(left, right, compareFunction)) {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = false;
  } else {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = true;
    areEqual = false;
  }

  return areEqual;

  // ------ 値比較関数 --------

  function areEqual社員(left: DisplayData, right: DisplayData, equals: Exclude<Util.DisplayDataDeepEqualOption["compareFunction"], undefined>): boolean {
    if (!equals('Integer', left.社員ID, right.社員ID)) return false;
    if (!equals('Word', left.氏名, right.氏名)) return false;
    if (!equals('Integer', left.所属部署?.部署ID, right.所属部署?.部署ID)) return false;
    if (!equals('Word', left.所属部署?.部署名, right.所属部署?.部署名)) return false;
    if (!equals('Word', left.所属部署?.事業所?.事業所ID, right.所属部署?.事業所?.事業所ID)) return false;
    if (!equals('Word', left.所属部署?.事業所?.事業所名, right.所属部署?.事業所?.事業所名)) return false;
    if (!equals('Word', left.契約種別?.区分値, right.契約種別?.区分値)) return false;
    if (!equals('Word', left.契約種別?.表示名称, right.契約種別?.表示名称)) return false;
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
    isStored: true,
    isQueryable: true,
    uniqueId: '1956b3f6-a6e5-493c-b87a-e86933c7b45f',
    displayName: '社員',
    members: ['社員ID', '氏名', '所属部署', '契約種別'],
  },
  displayData: {
    '社員ID': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '84baff83-2968-4325-b03a-e1097ddf746d',
      type: 'int',
      displayName: '社員ID',
      comment: '入社時に発行される一意なID',
      isKey: true,
    },
    '氏名': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '4f668126-74ad-41db-8696-13267e93d59d',
      type: 'word',
      displayName: '氏名',
      isNotNull: true,
    },
    '所属部署': {
      kind: 'ref',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'a2e46961-8e91-45c9-b329-ab9ea55921f9',
      refTo: 'ref-to:部署',
      displayName: '所属部署',
      members: ['所属部署.部署ID', '所属部署.部署名', '所属部署.事業所'],
    },
    '所属部署.部署ID': {
      kind: 'value',
      parent: '所属部署',
      isOutsideTree: true,
      uniqueId: '2d52d3fd-0e35-4371-8ebf-7c70d8a2e1c6',
      type: 'int',
      displayName: '部署ID',
      isKey: true,
    },
    '所属部署.部署名': {
      kind: 'value',
      parent: '所属部署',
      isOutsideTree: true,
      uniqueId: '910a581f-4337-423f-958b-c9975a2551e2',
      type: 'word',
      displayName: '部署名',
      isNotNull: true,
    },
    '所属部署.事業所': {
      kind: 'ref',
      parent: '所属部署',
      isOutsideTree: true,
      uniqueId: 'ce9290d5-a160-4e13-82db-bb3df15e6bc3',
      refTo: 'ref-to:事業所',
      displayName: '事業所',
      members: ['所属部署.事業所.事業所ID', '所属部署.事業所.事業所名'],
    },
    '所属部署.事業所.事業所ID': {
      kind: 'value',
      parent: '所属部署.事業所',
      isOutsideTree: true,
      uniqueId: 'c185f4b5-ed5a-43ba-a418-effca9791119',
      type: 'word',
      displayName: '事業所ID',
      comment: 'ビュー内部でハードコードする。',
      isKey: true,
    },
    '所属部署.事業所.事業所名': {
      kind: 'value',
      parent: '所属部署.事業所',
      isOutsideTree: true,
      uniqueId: '25b6c40f-24a7-4c6e-9df5-385149e739b7',
      type: 'word',
      displayName: '事業所名',
      comment: 'ビュー内部でハードコードする。',
    },
    '契約種別': {
      kind: 'ref',
      parent: null,
      isOutsideTree: false,
      uniqueId: '6a19d6bf-64d3-4edc-ba43-762ab2edada8',
      refTo: 'ref-to:汎用マスタ',
      displayName: '契約種別',
      category: '契約区分',
      members: ['契約種別.区分値', '契約種別.表示名称'],
    },
    '契約種別.区分値': {
      kind: 'value',
      parent: '契約種別',
      isOutsideTree: true,
      uniqueId: 'd53ec0af-86df-4951-97f1-32ef5fcc7dc0',
      type: 'word',
      displayName: '区分値',
      isKey: true,
    },
    '契約種別.表示名称': {
      kind: 'value',
      parent: '契約種別',
      isOutsideTree: true,
      uniqueId: '062655d6-b3bb-422b-9c9f-1543c83cba6e',
      type: 'word',
      displayName: '表示名称',
    },
  },
  searchCondition: {
    '社員ID': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '84baff83-2968-4325-b03a-e1097ddf746d',
      type: 'int',
      displayName: '社員ID',
      comment: '入社時に発行される一意なID',
      isKey: true,
    },
    '氏名': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '4f668126-74ad-41db-8696-13267e93d59d',
      type: 'word',
      displayName: '氏名',
      isNotNull: true,
    },
    '所属部署': {
      kind: 'ref',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'a2e46961-8e91-45c9-b329-ab9ea55921f9',
      refTo: 'ref-to:部署',
      displayName: '所属部署',
      members: ['所属部署.部署ID', '所属部署.部署名', '所属部署.事業所', '所属部署.課'],
    },
    '所属部署.部署ID': {
      kind: 'value',
      parent: '所属部署',
      isOutsideTree: true,
      uniqueId: '2d52d3fd-0e35-4371-8ebf-7c70d8a2e1c6',
      type: 'int',
      displayName: '部署ID',
      isKey: true,
    },
    '所属部署.部署名': {
      kind: 'value',
      parent: '所属部署',
      isOutsideTree: true,
      uniqueId: '910a581f-4337-423f-958b-c9975a2551e2',
      type: 'word',
      displayName: '部署名',
      isNotNull: true,
    },
    '所属部署.事業所': {
      kind: 'ref',
      parent: '所属部署',
      isOutsideTree: true,
      uniqueId: 'ce9290d5-a160-4e13-82db-bb3df15e6bc3',
      refTo: 'ref-to:事業所',
      displayName: '事業所',
      members: ['所属部署.事業所.事業所ID', '所属部署.事業所.事業所名'],
    },
    '所属部署.事業所.事業所ID': {
      kind: 'value',
      parent: '所属部署.事業所',
      isOutsideTree: true,
      uniqueId: 'c185f4b5-ed5a-43ba-a418-effca9791119',
      type: 'word',
      displayName: '事業所ID',
      comment: 'ビュー内部でハードコードする。',
      isKey: true,
    },
    '所属部署.事業所.事業所名': {
      kind: 'value',
      parent: '所属部署.事業所',
      isOutsideTree: true,
      uniqueId: '25b6c40f-24a7-4c6e-9df5-385149e739b7',
      type: 'word',
      displayName: '事業所名',
      comment: 'ビュー内部でハードコードする。',
    },
    '所属部署.課': {
      kind: 'child',
      parent: '所属部署',
      isOutsideTree: true,
      uniqueId: '51d08ec1-ddd8-4822-bf2b-91c458ee7e62',
      displayName: '課',
      uniqueConstraints: '1f6febf0-adfe-4377-8749-9eb7223ef14c;',
      members: ['所属部署.課.コード', '所属部署.課.旧システムコード', '所属部署.課.課名称', '所属部署.課.係'],
    },
    '所属部署.課.コード': {
      kind: 'value',
      parent: '所属部署.課',
      isOutsideTree: true,
      uniqueId: '1e4a365b-7b39-46da-8be0-de2d7bb75241',
      type: 'word',
      displayName: 'コード',
      isKey: true,
    },
    '所属部署.課.旧システムコード': {
      kind: 'ref',
      parent: '所属部署.課',
      isOutsideTree: true,
      uniqueId: '1f6febf0-adfe-4377-8749-9eb7223ef14c',
      refTo: 'ref-to:旧システム部署情報',
      displayName: '旧システムコード',
      comment: '非主キーの外部参照のユニーク制約の例',
      members: ['所属部署.課.旧システムコード.旧システムコード', '所属部署.課.旧システムコード.名称'],
    },
    '所属部署.課.旧システムコード.旧システムコード': {
      kind: 'value',
      parent: '所属部署.課.旧システムコード',
      isOutsideTree: true,
      uniqueId: '78f8403b-9d15-424e-91c3-f31bd0aeb562',
      type: 'word',
      displayName: '旧システムコード',
      isKey: true,
    },
    '所属部署.課.旧システムコード.名称': {
      kind: 'value',
      parent: '所属部署.課.旧システムコード',
      isOutsideTree: true,
      uniqueId: '1376a08d-3312-46c6-b51b-d4e70a74908a',
      type: 'word',
      displayName: '名称',
      isNotNull: true,
    },
    '所属部署.課.課名称': {
      kind: 'value',
      parent: '所属部署.課',
      isOutsideTree: true,
      uniqueId: 'f8667bbb-c921-46ec-869b-b989649634d5',
      type: 'word',
      displayName: '課名称',
      isNotNull: true,
    },
    '所属部署.課.係': {
      kind: 'child',
      parent: '所属部署.課',
      isOutsideTree: true,
      uniqueId: '3d203063-3cf5-4acd-ba63-ecbe5701782d',
      displayName: '係',
      members: ['所属部署.課.係.連番', '所属部署.課.係.係名称', '所属部署.課.係.勤怠管理区分'],
    },
    '所属部署.課.係.連番': {
      kind: 'value',
      parent: '所属部署.課.係',
      isOutsideTree: true,
      uniqueId: '72f8ff2c-a014-428b-8a74-1b50d033cd94',
      type: 'int',
      displayName: '連番',
      isKey: true,
    },
    '所属部署.課.係.係名称': {
      kind: 'value',
      parent: '所属部署.課.係',
      isOutsideTree: true,
      uniqueId: '61e72ff4-9992-4a83-a546-0fb4c93ef3fa',
      type: 'word',
      displayName: '係名称',
      isNotNull: true,
    },
    '所属部署.課.係.勤怠管理区分': {
      kind: 'ref',
      parent: '所属部署.課.係',
      isOutsideTree: true,
      uniqueId: '1c9be8ce-0d9d-41c0-a3b4-d56aac06b0f2',
      refTo: 'ref-to:汎用マスタ',
      displayName: '勤怠管理区分',
      category: '勤怠管理区分',
      members: ['所属部署.課.係.勤怠管理区分.区分値', '所属部署.課.係.勤怠管理区分.表示名称'],
    },
    '所属部署.課.係.勤怠管理区分.区分値': {
      kind: 'value',
      parent: '所属部署.課.係.勤怠管理区分',
      isOutsideTree: true,
      uniqueId: 'd53ec0af-86df-4951-97f1-32ef5fcc7dc0',
      type: 'word',
      displayName: '区分値',
      isKey: true,
    },
    '所属部署.課.係.勤怠管理区分.表示名称': {
      kind: 'value',
      parent: '所属部署.課.係.勤怠管理区分',
      isOutsideTree: true,
      uniqueId: '062655d6-b3bb-422b-9c9f-1543c83cba6e',
      type: 'word',
      displayName: '表示名称',
    },
    '契約種別': {
      kind: 'ref',
      parent: null,
      isOutsideTree: false,
      uniqueId: '6a19d6bf-64d3-4edc-ba43-762ab2edada8',
      refTo: 'ref-to:汎用マスタ',
      displayName: '契約種別',
      category: '契約区分',
      members: ['契約種別.区分値', '契約種別.表示名称'],
    },
    '契約種別.区分値': {
      kind: 'value',
      parent: '契約種別',
      isOutsideTree: true,
      uniqueId: 'd53ec0af-86df-4951-97f1-32ef5fcc7dc0',
      type: 'word',
      displayName: '区分値',
      isKey: true,
    },
    '契約種別.表示名称': {
      kind: 'value',
      parent: '契約種別',
      isOutsideTree: true,
      uniqueId: '062655d6-b3bb-422b-9c9f-1543c83cba6e',
      type: 'word',
      displayName: '表示名称',
    },
  },
} as const satisfies Util.AggregateMetadata.Entry

//#endregion メタデータ

//#endregion 関数
