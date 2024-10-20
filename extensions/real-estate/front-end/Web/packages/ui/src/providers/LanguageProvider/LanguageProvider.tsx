import React, { useEffect } from 'react'
import { useUser } from '../UserProvider/UserProvider'
import { LanguageContext, LanguageContextType } from './LanguageContext'
import {
  Language,
  languages,
} from './LanguageJson/LanguageJsonService/LanguageJsonService'
import countryList from '../../../../platform/src/public/translations/countryList.json'

export { useLanguage } from './LanguageContext'

export function LanguageProvider({ i18n, children }) {
  const user: { language?: Language } = useUser()
  const currentLang: Language =
    user.language != null
      ? user.language
      : languages.find(
          (language) => language === window.localStorage.getItem('i18nextLng')
        ) || 'en'

  const languageLookup = {
    en: 'English',
    fr: 'French',
  }

  useEffect(() => {
    const handleLanguageChange = async (lang: Language) => {
      await i18n?.changeLanguage(lang)
    }
    if (i18n?.language !== currentLang) {
      handleLanguageChange(currentLang)
    }

    if (document.documentElement.lang !== currentLang) {
      // update lang attribute in html tag
      document.documentElement.lang = currentLang
    }
  }, [i18n, currentLang, i18n?.language])

  const context: LanguageContextType = {
    language: currentLang,
    languageLookup,

    // list of country names always in English.
    countryList,
  }

  return (
    <LanguageContext.Provider value={context}>
      {children}
    </LanguageContext.Provider>
  )
}
