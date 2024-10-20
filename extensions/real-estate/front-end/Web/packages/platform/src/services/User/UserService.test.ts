import axios from 'axios'
import UserService from './UserService'

const formData = {
  firstName: 'Jolli',
  lastName: 'Bee',
  email: 'infor@Jollibee.com',
  contactNumber: '604-265-7353',
  company: 'Jollibee',
  useB2C: true,
  isCustomerAdmin: true,
  portfolios: [
    {
      portfolioId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
      portfolioName: 'JB portfolio',
      role: 'string',
      sites: [
        {
          siteId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
          siteName: 'site1',
          role: '',
        },
      ],
    },
  ],
}
const headers = { language: 'en' }

describe('postUserService', () => {
  test('should return expected response', async () => {
    const postResponseData = {
      ...formData,
      id: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
      initials: 'JB',
      createdDate: '2022-08-11T20:27:13.871Z',
      status: 'deleted',
      canEdit: true,
      portfolios: [
        {
          portfolioId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
          portfolioName: 'JB portfolio',
          role: 'string',
          sites: [
            {
              siteId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
              siteName: 'site1',
              role: '',
              logoUrl: 'https://logo.com',
              logoOriginalSizeUrl: 'https://logo.com',
            },
          ],
        },
      ],
    }

    jest.spyOn(axios, 'post').mockResolvedValue({ data: postResponseData })

    const response = await UserService('1', formData, headers)

    expect(response).toMatchObject(postResponseData)
  })

  test('should return error when exception error happens', async () => {
    const errorMessage = 'fetch error'
    jest.spyOn(axios, 'post').mockRejectedValue(new Error(errorMessage))

    await expect(UserService('2', formData, headers)).rejects.toThrowError(
      errorMessage
    )
  })
})
