import { useQuery, UseQueryOptions, UseQueryResult } from 'react-query'
import {
  getLanguageJson,
  Language,
  LanguageJsonResponse,
} from '../LanguageJsonService/LanguageJsonService'

export default function useGetLanguageJson(props: {
  baseUrl: string
  language: Language
  options?: UseQueryOptions
}): UseQueryResult<LanguageJsonResponse, Error> {
  const { baseUrl, language, options } = props
  return useQuery(
    [baseUrl, language],
    () => getLanguageJson(baseUrl, language),
    options
  ) as UseQueryResult<LanguageJsonResponse, Error>
}
