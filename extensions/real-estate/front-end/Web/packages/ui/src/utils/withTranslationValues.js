import i18n from 'i18next'
import { withTranslation, initReactI18next } from 'react-i18next'

/**
 * Util to be able to inject actual translation values into a component.
 *
 * Usage:
 *
 * const NewComponentWithInjectedTranslations = withTranslationValues({
 *    "translation.key": "english value",
 *    // ...
 * })(OriginalComponent);
 */
export default function withTranslationValues(translation) {
  const i18nInstance = i18n.createInstance()
  i18nInstance.use(initReactI18next)
  i18nInstance.init({
    resources: {
      en: {
        translation,
      },
    },
    lng: 'en',
  })

  return (Component) => {
    const WithTranslation = withTranslation('en')(Component)
    return (props) => <WithTranslation i18n={i18nInstance} {...props} />
  }
}
