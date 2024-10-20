import { LanguageContext } from './LanguageContext'

export default function LanguageStubProvider({ children }) {
  const context = {
    language: 'en',
    languageLookup: {
      en: 'English',
    },
  }

  return (
    <LanguageContext.Provider value={context}>
      {children}
    </LanguageContext.Provider>
  )
}
