import React from "react"
import * as ReactHookForm from "react-hook-form"
import { PersonalSettings } from "./PersonalSettings"

/**
 * ユーザー自身にだけ適用される設定。
 * 実体はローカルストレージに保存される。
 */
export const usePersonalSettings = () => {

  const [forceUpdate, setForceUpdate] = React.useState(-1)
  const personalSettings = React.useMemo((): PersonalSettings => {
    try {
      const settings = localStorage.getItem(LOCAL_STORAGE_KEY)
      if (!settings) return {}
      return JSON.parse(settings)
    } catch (e) {
      return {}
    }
  }, [forceUpdate])

  const save = React.useCallback(<TPath extends ReactHookForm.Path<PersonalSettings>>(
    path: TPath,
    value: ReactHookForm.PathValue<PersonalSettings, TPath>
  ) => {
    const clone = window.structuredClone(personalSettings)
    ReactHookForm.set(clone, path, value)
    localStorage.setItem(LOCAL_STORAGE_KEY, JSON.stringify(clone))
    setForceUpdate(prev => prev * -1)
  }, [personalSettings])

  // ブラウザの他のタブで設定が更新された場合にこちらにも適用する
  React.useEffect(() => {
    const handleStorageChange = (e: StorageEvent) => {
      if (e.key === LOCAL_STORAGE_KEY) {
        setForceUpdate(prev => prev * -1)
      }
    }
    window.addEventListener('storage', handleStorageChange)
    return () => {
      window.removeEventListener('storage', handleStorageChange)
    }
  }, [])

  return {
    personalSettings,
    save,
  }
}

const LOCAL_STORAGE_KEY = 'typedDocument:personalSettings'
