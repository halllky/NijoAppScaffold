import * as RHF from "react-hook-form"
import type { FormBinding } from "./FormBinding"

export type FieldLabelProps<TValues extends RHF.FieldValues> = {
  /** ラベルを付ける項目。表示名・ヘルプテキスト・メッセージはこの項目のものが使われる */
  name: RHF.Path<TValues>
  /**
   * ラベルと children を縦に並べるなら true。未指定の場合は横並び。
   * 縦並びの場合はエラーメッセージがラベルの下に、横並びの場合は children の下に表示される。
   */
  vertical?: boolean
  /**
   * FieldGroup の中で、横幅いっぱいに表示するかどうか。未指定の場合は1列分の幅。
   * FieldGroup の外に置いた場合は効果が無い。
   */
  wide?: boolean
  /**
   * 必須マークを付けるかどうか。未指定の場合は付けない。
   * 基準はデータ構造上必須か否かではなく、ユーザーが入力する必要があるかどうか。
   * 例えばプログラム上で自動的に採番される項目は、データ構造上必須でもユーザーは入力する必要がない。
   */
  isRequired?: boolean
  /** ラベル文字列の右側に追加で表示する内容。行追加ボタンなど */
  afterLabel?: React.ReactNode
  /**
   * ラベルを付ける対象。入力コンポーネントに限らず、グリッドなど何を置いてもよい。
   * name と同じ項目の入力コンポーネントがあれば、ラベルのクリックでそこにフォーカスが移る。
   */
  children?: React.ReactNode
}

/**
 * 特定の項目のラベル。以下を表示する。
 *
 * - 表示名（メタデータの表示用名称）
 * - 必須マーク
 * - ヘルプテキスト（メタデータのコメント。アイコンにマウスを乗せると表示される）
 * - この項目とその子孫に対するメッセージ。ただし、より内側に表示領域（別の FieldLabel やグリッドのセル）がある項目のものは除く。
 *   クライアント側エラー（react-hook-form の検証エラー）とサーバー側メッセージの両方。複数ある場合はすべて表示する。
 *
 * ここに表示したメッセージは RootErrors には表示されない。
 *
 * 外枠は label 要素ではなく、表示名の部分だけが label 要素になる。
 * label 要素の中には操作可能な要素を1つしか置けないが、children にはグリッドなど複数の操作可能な要素を置けるようにするため。
 * ラベルと入力欄の対応付けは htmlFor と id で行う。
 *
 * 項目の配置（何列に並べるか、ラベルの幅をそろえるか）はこのコンポーネントでは決めない。FieldGroup が決める。
 */
export function FieldLabel<TValues extends RHF.FieldValues>(props: FieldLabelProps<TValues> & {
  binding: FormBinding<TValues>
}): React.ReactNode {
  throw new Error('not implemented')
}
