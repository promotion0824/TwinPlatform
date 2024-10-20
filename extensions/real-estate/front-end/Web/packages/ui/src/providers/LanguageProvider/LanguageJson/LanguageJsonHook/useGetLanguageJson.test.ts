import { renderHook, waitFor } from '@testing-library/react'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import useGetLanguageJson from './useGetLanguageJson'
import * as languageJsonService from '../LanguageJsonService/LanguageJsonService'

const baseUrl = 'https://willowinc.com/languagejson'
const responseData: languageJsonService.LanguageJsonResponse = {
  translation: {
    headers: {
      acknowledged: 'Acknowledged',
    },
    labels: {
      addComment: 'Add comment',
    },
  },
  countryList: ['Afghanistan'],
}

describe('useGetLanguageJson', () => {
  test('should return error if getLanguageJson fails', async () => {
    jest
      .spyOn(languageJsonService, 'getLanguageJson')
      .mockRejectedValue(new Error('fetch error'))

    const { result } = renderHook(
      () =>
        useGetLanguageJson({
          baseUrl,
          language: 'en',
        }),
      {
        wrapper: BaseWrapper,
      }
    )

    await waitFor(() => {
      expect(result.current.error).toBeDefined()
      expect(result.current.data).not.toBeDefined()
    })
  })

  test('should provide data when baseUrl is defined upon triggering hook', async () => {
    jest
      .spyOn(languageJsonService, 'getLanguageJson')
      .mockResolvedValue(responseData)

    const { result } = renderHook(
      () =>
        useGetLanguageJson({
          baseUrl,
          language: 'en',
        }),
      {
        wrapper: BaseWrapper,
      }
    )

    await waitFor(() => {
      expect(result.current.data).toBeDefined()
      expect(result.current.error).toBeNull()
    })
  })
})
