import axios from 'axios'

type TranslationItem = {
  [key: string]: string
}
interface Translation {
  [key: string]: TranslationItem
}

export const languages = ['en', 'fr'] as const
export type Language = typeof languages[number]

export type LanguageJsonResponse = {
  countryList: Array<string>
  translation: Translation
} | null

export function getLanguageJson(
  baseUrl: string,
  language: Language
): Promise<LanguageJsonResponse> {
  const langRegex = /^[a-z]{2}\b/g
  // Firefox will output 1st time user's browser language preference in form of "xx-XX", we need to ensure it is "xx" (ISO 639-1 standard)
  let parsedLang: Language = 'en'
  switch (language.match(langRegex)?.[0]) {
    case 'fr':
      parsedLang = 'fr'
      break
  }

  const finalUrl: string = `${baseUrl}/${parsedLang}.json`

  return axios.get(finalUrl).then(({ data }) => data)
}
