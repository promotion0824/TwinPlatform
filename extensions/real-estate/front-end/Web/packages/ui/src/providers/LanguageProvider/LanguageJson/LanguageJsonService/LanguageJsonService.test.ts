import axios from 'axios'
import { getLanguageJson, LanguageJsonResponse } from './LanguageJsonService'

const definedBaseUrl = 'https://www.getMeLangJson'
const responseData: LanguageJsonResponse = {
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

describe('LanguageJsonService', () => {
  test('should return error when error occurs', async () => {
    jest.spyOn(axios, 'get').mockRejectedValue(new Error('fetch error'))
    await expect(getLanguageJson(definedBaseUrl, 'en')).rejects.toThrowError(
      'fetch error'
    )
  })

  test('should return expected data', async () => {
    jest
      .spyOn(axios, 'get')
      .mockResolvedValue(Promise.resolve({ data: responseData }))
    const response = await getLanguageJson(definedBaseUrl, 'en')
    expect(response).toMatchObject(responseData)
  })
})
