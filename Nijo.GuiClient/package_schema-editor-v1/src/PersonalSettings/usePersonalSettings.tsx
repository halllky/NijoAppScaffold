import React from "react"
import * as ReactHookForm from "react-hook-form"
import { PersonalSettings } from "../PersonalSettings/PersonalSettings"

const LOCAL_STORAGE_KEY = 'typedDocument:personalSettings'

type PersonalSettingsContextType = {
  personalSettings: PersonalSettings
  save: <TPath extends ReactHookForm.Path<PersonalSettings>>(
    path: TPath,
    value: ReactHookForm.PathValue<PersonalSettings, TPath>
  ) => void
}

const PersonalSettingsContext = React.createContext<PersonalSettingsContextType | null>(null)

/**
 * ユーザー自身にだけ適用される設定のプロバイダ。
 * 実体はローカルストレージに保存される。
 */
export const PersonalSettingsProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [personalSettings, setPersonalSettings] = React.useState<PersonalSettings>(() => {
    try {
      const settings = localStorage.getItem(LOCAL_STORAGE_KEY)
      if (!settings) return {}
      return JSON.parse(settings)
    } catch (e) {
      return {}
    }
  })

  const save = React.useCallback(<TPath extends ReactHookForm.Path<PersonalSettings>>(
    path: TPath,
    value: ReactHookForm.PathValue<PersonalSettings, TPath>
  ) => {
    const clone = window.structuredClone(personalSettings)
    ReactHookForm.set(clone, path, value)
    localStorage.setItem(LOCAL_STORAGE_KEY, JSON.stringify(clone))
    setPersonalSettings(clone)
  }, [personalSettings])

  // ブラウザの他のタブで設定が更新された場合にこちらにも適用する
  React.useEffect(() => {
    const handleStorageChange = (e: StorageEvent) => {
      if (e.key === LOCAL_STORAGE_KEY) {
        try {
          const settings = e.newValue ? JSON.parse(e.newValue) : {}
          setPersonalSettings(settings)
        } catch (error) {
          setPersonalSettings({})
        }
      }
    }
    window.addEventListener('storage', handleStorageChange)
    return () => {
      window.removeEventListener('storage', handleStorageChange)
    }
  }, [])

  const value = React.useMemo(() => ({
    personalSettings,
    save,
  }), [personalSettings, save])

  return (
    <PersonalSettingsContext.Provider value={value}>
      {children}
    </PersonalSettingsContext.Provider>
  )
}

/**
 * ユーザー自身にだけ適用される設定。
 * 実体はローカルストレージに保存される。
 */
export const usePersonalSettings = () => {
  const context = React.useContext(PersonalSettingsContext)
  if (!context) {
    throw new Error('usePersonalSettings must be used within PersonalSettingsProvider')
  }
  return context
}
