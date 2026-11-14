
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : DataStructuresSection.cs
// ※ このファイルを生成したメソッド: Render

import type * as Util from ".."
import * as EnumDefs from "../enum-defs"
import * as 商品 from "./商品"
import * as 従業員Ref from "./従業員Ref"

//#region 型定義
//#region 構造体定義
/** 売上詳細の構造体 */
export type Structure = {
  売上SEQ: string | undefined
  Version: string | undefined
  売上日時: string | undefined
  担当者: 従業員Ref.RefTarget
  備考: string | undefined
  合計金額: string | undefined
  売上詳細の売上明細: Structure_売上詳細の売上明細[]
}
/** 売上明細の構造体 */
export type Structure_売上詳細の売上明細 = {
  明細ID: string | undefined
  商品: 商品.RefTarget
  区分: EnumDefs.売上明細区分 | undefined
  売上数量: string | undefined
  売上総額_税込_自動計算: string | undefined
  売上総額_税込_手修正: string | undefined
}
//#endregion 構造体定義

//#region 構造体新規作成用関数
/** 売上詳細の構造体の新しいインスタンスを作成します。 */
export function createNewStructure(): Structure {
  return {
    売上SEQ: undefined,
    Version: undefined,
    売上日時: undefined,
    担当者: 従業員Ref.createNewRefTarget(),
    備考: undefined,
    合計金額: undefined,
    売上詳細の売上明細: [],
  }
}
/** 売上明細の構造体の新しいインスタンスを作成します。 */
export function createNewStructure_売上詳細の売上明細(): Structure_売上詳細の売上明細 {
  return {
    明細ID: undefined,
    商品: 商品.createNewRefTarget(),
    区分: undefined,
    売上数量: undefined,
    売上総額_税込_自動計算: undefined,
    売上総額_税込_手修正: undefined,
  }
}
//#endregion 構造体新規作成用関数

//#region 画面表示用データ
/** 売上詳細の画面表示用データ。 */
export type DisplayData = {
  /**
   * 画面初期表示から保存までの間でこのインスタンスを識別する番号。
   * GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
   * エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
   */
  instanceId: string
  /**
   * シーケンス。画面上に表示する。
   */
  売上SEQ?: string | null
  /**
   * 楽観排他用
   */
  Version?: string | null
  売上日時?: string | null
  担当者: 従業員Ref.RefTarget
  備考?: string | null
  /**
   * DBには保存しないが画面上での表示に使用
   */
  合計金額?: string | null
  /** 売上明細 */
  売上詳細の売上明細: DisplayData_売上詳細の売上明細[]

  /** このデータがDBに保存済みかどうか */
  existsInDatabase: boolean
  /** このデータに更新がかかっているかどうか */
  willBeChanged: boolean
  /** このデータが更新確定時に削除されるかどうか */
  willBeDeleted: boolean
}
/** 売上明細の画面表示用データ。 */
export type DisplayData_売上詳細の売上明細 = {
  /**
   * 画面初期表示から保存までの間でこのインスタンスを識別する番号。
   * GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
   * エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
   */
  instanceId: string
  /**
   * UUID。非表示。
   */
  明細ID?: string | null
  商品: 商品.RefTarget
  /**
   * 新規登録の場合は「売上」で固定。売上修正の場合は「修正」も選べる。
   */
  区分?: EnumDefs.売上明細区分 | null
  /**
   * 取消の場合は負の数。
   */
  売上数量?: string | null
  /**
   * 取消の場合は負の数。丸め処理済みの値。自動計算。商品マスタの単価に売上数量を乗じたもの。手修正した金額が入力されている場合はそちらが優先される。
   */
  売上総額_税込_自動計算?: string | null
  /**
   * 取消の場合は負の数。人間が手で入力した金額。
   */
  売上総額_税込_手修正?: string | null

  /** このデータがDBに保存済みかどうか */
  existsInDatabase: boolean
  /** このデータに更新がかかっているかどうか */
  willBeChanged: boolean
  /** このデータが更新確定時に削除されるかどうか */
  willBeDeleted: boolean
}
//#endregion 画面表示用データ

//#endregion 型定義


//#region 関数
//#region 画面表示用データ新規作成用関数
/** 売上詳細の画面表示用データの新しいインスタンスを作成します。 */
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
    売上SEQ: '',
    Version: '',
    売上日時: '',
    担当者: 従業員Ref.createNewRefTarget(),
    備考: '',
    合計金額: '',
    existsInDatabase: false,
    willBeChanged: true,
    willBeDeleted: false,
    売上詳細の売上明細: [],
  }
}
/** 売上明細の画面表示用データの新しいインスタンスを作成します。 */
export function createNewDisplayData_売上詳細の売上明細(): DisplayData_売上詳細の売上明細 {
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
    明細ID: '',
    商品: 商品.createNewRefTarget(),
    区分: null,
    売上数量: '',
    売上総額_税込_自動計算: '',
    売上総額_税込_手修正: '',
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

  // 売上詳細 の等価比較
  if (areEqual売上詳細(left, right, compareFunction)) {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = false;
  } else {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = true;
    areEqual = false;
  }
  
  // 売上明細 の等価比較
  // 以下いずれかに該当すれば変更ありと判定
  // * 配列の長さが変わっている
  // * 要素の並び替えが発生している（instanceId 基準）
  // * 配列の要素を instanceId で突合しそれぞれの値を比較
  const leftIds = left.売上詳細の売上明細?.map(x => x.instanceId) ?? [];
  const rightIds = right.売上詳細の売上明細?.map(x => x.instanceId) ?? [];
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
  
  const leftDict = new Map(left.売上詳細の売上明細?.map(x => [x.instanceId, x]) ?? []);
  const rightDict = new Map(right.売上詳細の売上明細?.map(x => [x.instanceId, x]) ?? []);
  for (const instanceId of Array.from(new Set([...leftIds, ...rightIds]))) {
    const x = leftDict.get(instanceId);
    const y = rightDict.get(instanceId);
    if (!x && y && option?.setRightObjectWillBeChanged) y.willBeChanged = true;
    if (!x || !y) continue;
    if (areEqual売上詳細の売上明細(x, y, compareFunction)) {
      if (option?.setRightObjectWillBeChanged) y.willBeChanged = false;
    } else {
      if (option?.setRightObjectWillBeChanged) y.willBeChanged = true;
      areEqual = false;
    }
  }

  return areEqual;

  // ------ 値比較関数 --------

  function areEqual売上詳細(left: DisplayData, right: DisplayData, equals: Exclude<Util.DisplayDataDeepEqualOption["compareFunction"], undefined>): boolean {
    if (!equals('Integer', left.売上SEQ, right.売上SEQ)) return false;
    if (!equals('Integer', left.Version, right.Version)) return false;
    if (!equals('DateTime', left.売上日時, right.売上日時)) return false;
    if (!equals('Word', left.担当者?.従業員番号, right.担当者?.従業員番号)) return false;
    if (!equals('Word', left.担当者?.氏名, right.担当者?.氏名)) return false;
    if (!equals('Description', left.備考, right.備考)) return false;
    if (!equals('Integer', left.合計金額, right.合計金額)) return false;
    if ((left.willBeDeleted ?? false) !== (right.willBeDeleted ?? false)) return false;
    return true;
  }

  function areEqual売上詳細の売上明細(left: DisplayData_売上詳細の売上明細, right: DisplayData_売上詳細の売上明細, equals: Exclude<Util.DisplayDataDeepEqualOption["compareFunction"], undefined>): boolean {
    if (!equals('Word', left.明細ID, right.明細ID)) return false;
    if (!equals('Sequence', left.商品?.商品SEQ, right.商品?.商品SEQ)) return false;
    if (!equals('Word', left.商品?.外部システム側ID, right.商品?.外部システム側ID)) return false;
    if (!equals('Word', left.商品?.商品名, right.商品?.商品名)) return false;
    if (!equals('Decimal', left.商品?.売値単価_税抜, right.商品?.売値単価_税抜)) return false;
    if (!equals('Enum', left.商品?.消費税区分, right.商品?.消費税区分, '消費税区分')) return false;
    if (!equals('Enum', left.区分, right.区分, '売上明細区分')) return false;
    if (!equals('Integer', left.売上数量, right.売上数量)) return false;
    if (!equals('Integer', left.売上総額_税込_自動計算, right.売上総額_税込_自動計算)) return false;
    if (!equals('Integer', left.売上総額_税込_手修正, right.売上総額_税込_手修正)) return false;
    if ((left.willBeDeleted ?? false) !== (right.willBeDeleted ?? false)) return false;
    return true;
  }
}
//#endregion ディープイコール関数

//#region メタデータ

/** この集約のメタデータ */
export const metadata = {
  root: {
    kind: 'root',
    isStored: false,
    isQueryable: false,
    uniqueId: '2352e942-65b1-47fe-ac3b-0c49bd4e295f',
    displayName: '売上詳細',
    comment: '売上詳細画面のデータ構造。\n@[売上](f6a1b2c3-d4e5-4f2a-9b3c-4d5e6f7a8b9c) DataModel と異なり、入荷明細の引当に関する情報を持っていないのが重要。これはつまり、売上詳細画面を操作するユーザーにとってその商品の在庫引当がどの入荷に対して行われるかは入力項目ではなく、引当はシステム内部で自動的に判定されることを意味する。',
    members: ['売上SEQ', 'Version', '売上日時', '担当者', '備考', '合計金額', '売上詳細の売上明細'],
  },
  displayData: {
    '売上SEQ': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '1067685c-ca3e-424d-82ca-2b8045464fb5',
      type: 'int',
      displayName: '売上SEQ',
      comment: 'シーケンス。画面上に表示する。',
    },
    'Version': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '7f1b8f4e-3a55-4f0e-9d3c-5a1c2e8b6d17',
      type: 'int',
      displayName: 'Version',
      comment: '楽観排他用',
    },
    '売上日時': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '5fe83afc-073e-490f-932d-5eefad3b0c85',
      type: 'datetime',
      displayName: '売上日時',
    },
    '担当者': {
      kind: 'ref',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'c2758cdd-4867-41b6-94db-208ca2559858',
      refTo: 'ref-to:従業員Ref',
      displayName: '担当者',
      refToObject: 'RefTarget',
      members: ['担当者.従業員番号', '担当者.氏名'],
    },
    '担当者.従業員番号': {
      kind: 'value',
      parent: '担当者',
      isOutsideTree: true,
      uniqueId: '91f94ec3-0cff-4468-84df-dbba8091f1c8',
      type: 'word',
      displayName: '従業員番号',
      stringSearchBehavior: 'Exact',
    },
    '担当者.氏名': {
      kind: 'value',
      parent: '担当者',
      isOutsideTree: true,
      uniqueId: '5fd0ce8d-d1fc-41cd-9a56-96db91fc7e8b',
      type: 'word',
      displayName: '氏名',
    },
    '備考': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'b66f5118-fff3-441d-842a-c14a596b44ab',
      type: 'description',
      displayName: '備考',
    },
    '合計金額': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'e49f5737-d260-40cf-99e7-404772e10153',
      type: 'int',
      displayName: '合計金額',
      comment: 'DBには保存しないが画面上での表示に使用',
      isCurrency: true,
    },
    '売上詳細の売上明細': {
      kind: 'children',
      parent: null,
      isOutsideTree: false,
      uniqueId: '98b7b6c4-9e74-4330-9bc8-e8594811c0a6',
      displayName: '売上詳細の売上明細',
      comment: '引当のロジックの都合上、一度更新確定した明細は更新や削除ができない。取消の場合は別で取消の明細を追加していく。',
      members: ['売上詳細の売上明細.明細ID', '売上詳細の売上明細.商品', '売上詳細の売上明細.区分', '売上詳細の売上明細.売上数量', '売上詳細の売上明細.売上総額_税込_自動計算', '売上詳細の売上明細.売上総額_税込_手修正'],
    },
    '売上詳細の売上明細.明細ID': {
      kind: 'value',
      parent: '売上詳細の売上明細',
      isOutsideTree: false,
      uniqueId: 'fe54c1ee-d63b-400c-a950-05adc927f28d',
      type: 'word',
      displayName: '明細ID',
      comment: 'UUID。非表示。',
    },
    '売上詳細の売上明細.商品': {
      kind: 'ref',
      parent: '売上詳細の売上明細',
      isOutsideTree: false,
      uniqueId: '398db3b4-a77e-45a2-881e-cad75af1bb46',
      refTo: 'ref-to:商品',
      displayName: '商品',
      refToObject: 'RefTarget',
      members: ['売上詳細の売上明細.商品.商品SEQ', '売上詳細の売上明細.商品.外部システム側ID', '売上詳細の売上明細.商品.商品名', '売上詳細の売上明細.商品.売値単価_税抜', '売上詳細の売上明細.商品.消費税区分'],
    },
    '売上詳細の売上明細.商品.商品SEQ': {
      kind: 'value',
      parent: '売上詳細の売上明細.商品',
      isOutsideTree: true,
      uniqueId: 'b2c3d4e5-f6a1-4b2c-9d3e-4f5a6b7c8d9e',
      type: 'sequence',
      displayName: '商品SEQ',
      comment: 'このシステムにおける識別番号。非表示',
      isKey: true,
      sequenceName: 'ITEM_SEQ',
    },
    '売上詳細の売上明細.商品.外部システム側ID': {
      kind: 'value',
      parent: '売上詳細の売上明細.商品',
      isOutsideTree: true,
      uniqueId: 'f5f225fa-eddd-4cca-ae58-b408e1b34100',
      type: 'word',
      comment: '外部システム側の識別番号',
      displayName: '商品コード',
      isNotNull: true,
      stringSearchBehavior: 'Exact',
    },
    '売上詳細の売上明細.商品.商品名': {
      kind: 'value',
      parent: '売上詳細の売上明細.商品',
      isOutsideTree: true,
      uniqueId: 'c3d4e5f6-a1b2-4c3d-0e4f-5a6b7c8d9e0f',
      type: 'word',
      displayName: '商品名',
      isNotNull: true,
    },
    '売上詳細の売上明細.商品.売値単価_税抜': {
      kind: 'value',
      parent: '売上詳細の売上明細.商品',
      isOutsideTree: true,
      uniqueId: 'd4e5f6a1-b2c3-4d4e-1f5a-6b7c8d9e0f1a',
      type: 'decimal',
      displayName: '売値単価_税抜',
      comment: '必須でないのは、キャンペーン用販促商品など価格が存在しない商品があるため。',
      isCurrency: true,
      notNegative: true,
    },
    '売上詳細の売上明細.商品.消費税区分': {
      kind: 'value',
      parent: '売上詳細の売上明細.商品',
      isOutsideTree: true,
      uniqueId: '1fd1192c-06a3-4aee-b88e-96700d545049',
      enumType: '消費税区分',
      displayName: '消費税区分',
    },
    '売上詳細の売上明細.区分': {
      kind: 'value',
      parent: '売上詳細の売上明細',
      isOutsideTree: false,
      uniqueId: '8c35dcac-1c74-4535-b2c0-fc5f50a37fdd',
      enumType: '売上明細区分',
      displayName: '区分',
      comment: '新規登録の場合は「売上」で固定。売上修正の場合は「修正」も選べる。',
    },
    '売上詳細の売上明細.売上数量': {
      kind: 'value',
      parent: '売上詳細の売上明細',
      isOutsideTree: false,
      uniqueId: '88781893-d022-4594-bdda-5eedfc3a3eb4',
      type: 'int',
      displayName: '売上数量',
      comment: '取消の場合は負の数。',
      isQuantity: true,
    },
    '売上詳細の売上明細.売上総額_税込_自動計算': {
      kind: 'value',
      parent: '売上詳細の売上明細',
      isOutsideTree: false,
      uniqueId: 'aa938911-99c2-4e98-b6d1-ed07a16eb255',
      type: 'int',
      displayName: '売上総額_税込_自動計算',
      comment: '取消の場合は負の数。丸め処理済みの値。自動計算。商品マスタの単価に売上数量を乗じたもの。手修正した金額が入力されている場合はそちらが優先される。',
      isCurrency: true,
    },
    '売上詳細の売上明細.売上総額_税込_手修正': {
      kind: 'value',
      parent: '売上詳細の売上明細',
      isOutsideTree: false,
      uniqueId: '77fbe568-1f31-4320-80b1-b4572d5543c7',
      type: 'int',
      displayName: '売上総額_税込_手修正',
      comment: '取消の場合は負の数。人間が手で入力した金額。',
      isCurrency: true,
    },
  },
} as const satisfies Util.AggregateMetadata.Entry

//#endregion メタデータ

//#endregion 関数
