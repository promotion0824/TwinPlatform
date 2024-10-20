import { createContext, useContext } from 'react'
import { Language } from './LanguageJson/LanguageJsonService/LanguageJsonService'

export interface LanguageContextType {
  language: Language
  languageLookup: { [Property in Language]: string }
  countryList: string[]
}

export const LanguageContext = createContext<LanguageContextType | undefined>(
  undefined
)

export function useLanguage() {
  const context = useContext(LanguageContext)
  if (context == null) {
    throw new Error('useLanguage needs a LanguageContext provider')
  }
  return context
}
