import '@testing-library/jest-dom'
import { render, screen } from '@testing-library/react'
import i18n from 'i18next'
import { initReactI18next, useTranslation } from 'react-i18next'
import _ from 'lodash'
import { act } from 'react-dom/test-utils'
import { useState, useContext } from 'react'
import userEvent from '@testing-library/user-event'
import { cookie } from '@willow/ui'
import { FeatureFlagContext } from '../FeatureFlagProvider/FeatureFlagContext'
import { UserContext } from '../UserProvider/UserContext'
import { ConfigContext } from '../ConfigProvider/ConfigContext'
import { LanguageProvider } from '..'
import { useLanguage } from './LanguageContext'
import {
  ReactQueryProvider,
  queryCache,
} from '@willow/common/providers/ReactQueryProvider/ReactQueryProvider'
import * as languageJsonService from './LanguageJson/LanguageJsonService/LanguageJsonService.ts'
import enTranslation from '../../../../platform/src/public/translations/en.json'
import frTranslation from '../../../../platform/src/public/translations/fr.json'

const useUser = () => useContext(UserContext)

function UserProvider({ children }) {
  const [user, setUser] = useState({
    language: 'en',
  })
  const value = {
    ...user,
    saveLanguage: (lang) => {
      setUser((prevUser) => ({
        ...prevUser,
        language: lang,
      }))
    },
  }

  return <UserContext.Provider value={value}>{children}</UserContext.Provider>
}

function ConfigProvider({ children }) {
  return <ConfigContext.Provider>{children}</ConfigContext.Provider>
}

function FeatureFlagProvider({ value, children }) {
  return (
    <FeatureFlagContext.Provider value={value}>
      {children}
    </FeatureFlagContext.Provider>
  )
}

/*
LanguageSelect uses Select and Option @willow.ui components which have many side effects
testing with Select and Option will be quite time consuming and complex

opted to test with a mocked LanguageSelect Component which is functionally same.
The tests below cover cases from initial loading of English Json and display text in English
to user selects French Option, fetch French Json and ultimately display text in French.
*/
function MockLanguageSelect() {
  const { language, languageLookup } = useLanguage()
  const { t } = useTranslation()
  const user = useUser()

  return (
    <>
      <div>{`Current Language: ${t('interpolation.plainText', {
        key: _.camelCase(languageLookup[language]),
      })}`}</div>
      <select
        data-testid="mockLangSelect"
        value={languageLookup[language]}
        onChange={(e) => {
          user.saveLanguage(e.target.value)
        }}
      >
        {Object.entries(languageLookup).map(([lang, languageFullName]) => (
          <option key={lang} value={lang}>
            {t('interpolation.plainText', {
              key: _.camelCase(languageFullName),
            })}
          </option>
        ))}
      </select>
    </>
  )
}

const initializeI18n = async (
  canUpdateLanguage = true,
  defaultResource = {}
) => {
  i18n
    .use(initReactI18next)
    .init({
      fallbackLng: 'en',
      resources: defaultResource,
    })
    .then(() => {
      render(
        <ConfigProvider>
          <UserProvider>
            <ReactQueryProvider>
              <FeatureFlagProvider
                value={{
                  hasFeatureToggle() {
                    return canUpdateLanguage
                  },
                }}
              >
                <LanguageProvider i18n={i18n}>
                  <MockLanguageSelect />
                </LanguageProvider>
              </FeatureFlagProvider>
            </ReactQueryProvider>
          </UserProvider>
        </ConfigProvider>
      )
    })
}

afterEach(() => queryCache.clear())

describe('LanguageProvider tests', () => {
  test('expect to have English Json loaded and display text in English and repeat the process for French when French Option is selected', async () => {
    // mock initial English Json file fetching
    jest.spyOn(cookie, 'get').mockReturnValue('au')
    const englishLangJsonSpy = jest
      .spyOn(languageJsonService, 'getLanguageJson')
      .mockResolvedValue({
        translation: {
          plainText: {
            english: 'English',
            french: 'French',
          },
          interpolation: {
            plainText: '$t(plainText.{{key}})',
          },
        },
      })

    await initializeI18n(true, {
      en: enTranslation,
      fr: frTranslation,
    })

    const currentLangEnglishNode = await screen.findByText(
      'Current Language: English'
    )
    expect(currentLangEnglishNode).toBeInTheDocument()

    const englishOption = await screen.findByText('English')
    expect(englishOption).toBeInTheDocument()

    const frenchOption = await screen.findByText('French')
    expect(frenchOption).toBeInTheDocument()

    // clear mock for English Json fetch and set up mock for French Json fetching
    englishLangJsonSpy.mockClear()

    jest.spyOn(languageJsonService, 'getLanguageJson').mockResolvedValue({
      translation: {
        plainText: {
          english: 'Anglais',
          french: 'Français',
        },
        interpolation: {
          plainText: '$t(plainText.{{key}})',
        },
      },
    })

    act(() => {
      userEvent.selectOptions(screen.getByTestId('mockLangSelect'), ['fr'])
    })

    const currentLangFrenchNode = await screen.findByText(
      'Current Language: Français'
    )
    expect(currentLangFrenchNode).toBeInTheDocument()

    const englishOptionAfterSelectFrench = await screen.findByText('Anglais')
    expect(englishOptionAfterSelectFrench).toBeInTheDocument()

    const frenchOptionAfterSelectFrench = await screen.findByText('Français')
    expect(frenchOptionAfterSelectFrench).toBeInTheDocument()
  })

  test('disabled useGetLanguageJson with idle status as translation is already loaded will not prevent user from switching language', async () => {
    await initializeI18n(true, {
      en: enTranslation,
      fr: frTranslation,
    })

    const currentLangEnglishNode = await screen.findByText(
      'Current Language: English'
    )
    expect(currentLangEnglishNode).toBeInTheDocument()

    act(() => {
      userEvent.selectOptions(screen.getByTestId('mockLangSelect'), ['fr'])
    })

    const currentLangFrenchNode = await screen.findByText(
      'Current Language: Français'
    )
    expect(currentLangFrenchNode).toBeInTheDocument()
  })
})
