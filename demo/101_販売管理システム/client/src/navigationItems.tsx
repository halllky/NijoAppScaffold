export type RootNavigationItem = {
  to: string
  label: string
  icon?: React.ElementType
}

/**
 * ルートナビゲーションに表示する画面の一覧。
 * 業務画面のフォルダ直下のモジュールが export する navigationItem を、ファイル名の順に並べたもの。
 */
export const navigationItems: RootNavigationItem[] = collectNavigationItems()

function collectNavigationItems(): RootNavigationItem[] {
  const modules = import.meta.glob<{ navigationItem?: RootNavigationItem }>("./pages/*.tsx", { eager: true })
  return Object.entries(modules)
    .sort(([pathA], [pathB]) => pathA < pathB ? -1 : pathA > pathB ? 1 : 0)
    .flatMap(([, module]) => module.navigationItem ?? [])
}
