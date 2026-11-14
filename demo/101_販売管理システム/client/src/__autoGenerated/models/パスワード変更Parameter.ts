
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : DataStructuresSection.cs
// ※ このファイルを生成したメソッド: Render

import type * as Util from ".."
import * as EnumDefs from "../enum-defs"

//#region 型定義
//#region 構造体定義
/** パスワード変更Parameterの構造体 */
export type Structure = {
  現在のパスワード: string | undefined
  新しいパスワード: string | undefined
  新しいパスワード_確認用: string | undefined
}
//#endregion 構造体定義

//#region 構造体新規作成用関数
/** パスワード変更Parameterの構造体の新しいインスタンスを作成します。 */
export function createNewStructure(): Structure {
  return {
    現在のパスワード: undefined,
    新しいパスワード: undefined,
    新しいパスワード_確認用: undefined,
  }
}
//#endregion 構造体新規作成用関数

//#region 画面表示用データ
/** パスワード変更Parameterの画面表示用データ。 */
export type DisplayData = {
  /**
   * 画面初期表示から保存までの間でこのインスタンスを識別する番号。
   * GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
   * エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
   */
  instanceId: string
  /**
   * 本人であることを確かめるために入力させる。席を離れた端末などで、本人以外がパスワードを変えられないようにする
   */
  現在のパスワード?: string | null
  新しいパスワード?: string | null
  新しいパスワード_確認用?: string | null

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
/** パスワード変更Parameterの画面表示用データの新しいインスタンスを作成します。 */
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
    現在のパスワード: '',
    新しいパスワード: '',
    新しいパスワード_確認用: '',
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

  // パスワード変更Parameter の等価比較
  if (areEqualパスワード変更Parameter(left, right, compareFunction)) {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = false;
  } else {
    if (option?.setRightObjectWillBeChanged) right.willBeChanged = true;
    areEqual = false;
  }

  return areEqual;

  // ------ 値比較関数 --------

  function areEqualパスワード変更Parameter(left: DisplayData, right: DisplayData, equals: Exclude<Util.DisplayDataDeepEqualOption["compareFunction"], undefined>): boolean {
    if (!equals('Word', left.現在のパスワード, right.現在のパスワード)) return false;
    if (!equals('Word', left.新しいパスワード, right.新しいパスワード)) return false;
    if (!equals('Word', left.新しいパスワード_確認用, right.新しいパスワード_確認用)) return false;
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
    uniqueId: 'da5e2ba7-864b-4d23-852b-1a4c6f525f98',
    displayName: 'パスワード変更Parameter',
    members: ['現在のパスワード', '新しいパスワード', '新しいパスワード_確認用'],
  },
  displayData: {
    '現在のパスワード': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'ac02ea1f-ae03-4796-8e9c-b6387f70f214',
      type: 'word',
      displayName: '現在のパスワード',
      comment: '本人であることを確かめるために入力させる。席を離れた端末などで、本人以外がパスワードを変えられないようにする',
      customMaxLength: 50,
      masking: true,
    },
    '新しいパスワード': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: '68322c5b-2976-453b-8e70-a968a946dedf',
      type: 'word',
      displayName: '新しいパスワード',
      customMaxLength: 50,
      masking: true,
    },
    '新しいパスワード_確認用': {
      kind: 'value',
      parent: null,
      isOutsideTree: false,
      uniqueId: 'ab05211a-883a-4737-8cba-a10e8345df92',
      type: 'word',
      displayName: '新しいパスワード_確認用',
      customMaxLength: 50,
      masking: true,
    },
  },
} as const satisfies Util.AggregateMetadata.Entry

//#endregion メタデータ

//#endregion 関数
