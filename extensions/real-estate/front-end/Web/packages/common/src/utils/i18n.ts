import i18n from 'i18next'
import LanguageDetector from 'i18next-browser-languagedetector'
import { initReactI18next } from 'react-i18next'
import enTranslation from '../../../platform/src/public/translations/en.json'
import frTranslation from '../../../platform/src/public/translations/fr.json'

// example taken from:
// https://dev.to/adrai/how-to-properly-internationalize-a-react-application-using-i18next-3hdb
//
// and
//
// https://github.com/i18next/i18next-http-backend/blob/master/example/node/app.js

/**
 * we load all translation json files during initialization as this will
 * guarantee translations are always up to date. However, there could be
 * a better way to do this. Explore possible options with
 * https://dev.azure.com/willowdev/Unified/_workitems/edit/92429
 */
const resources = {
  en: enTranslation,
  fr: frTranslation,
}

export function initializeI18n() {
  return (
    i18n
      .use(LanguageDetector)
      // pass the i18n instance to react-i18next.
      .use(initReactI18next)
      .init({
        fallbackLng: 'en',
        resources,
      })
  )
}

export default i18n
