import axios from 'axios'
/* eslint-disable-next-line */
import {
  getAuthenticatedReport,
  AuthenticatedReport,
} from './AuthWidgetService'

const responseData: AuthenticatedReport = {
  token: 'myToken',
  url: 'authenticate_url',
  expiration: '2022-01-03T03:41:03.000Z',
}

describe('Authenticate Widget Service', () => {
  test('should return expected data', async () => {
    jest
      .spyOn(axios, 'get')
      .mockResolvedValue(Promise.resolve({ data: responseData }))
    const response = await getAuthenticatedReport('url_to_auth_a_report')
    expect(response).toMatchObject(responseData)
  })

  test('should return error when error occurs', async () => {
    jest.spyOn(axios, 'get').mockRejectedValue(new Error('fetch error'))
    await expect(
      getAuthenticatedReport('url_to_auth_a_report')
    ).rejects.toThrowError('fetch error')
  })
})
