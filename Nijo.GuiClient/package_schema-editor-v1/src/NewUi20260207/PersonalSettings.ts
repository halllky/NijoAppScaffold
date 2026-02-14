/** ユーザー自身にだけ適用される設定 */
export type PersonalSettings = {
  /** グリッドの操作説明ボタンを非表示にする */
  hideGridButtons?: boolean
  /** 保存時に自動でコード生成を実行する */
  autoGenerateCode?: boolean
  /** ルート集約編集ペインの配置方向 */
  aggPaneOrientation?: 'horizontal' | 'vertical'
  /** ルート集約の詳細を表示するか */
  openDetails?: boolean
}
