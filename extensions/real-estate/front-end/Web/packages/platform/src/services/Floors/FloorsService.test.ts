import axios from 'axios'
import { getFloors, Floors } from './FloorsService'

const siteId = '4e5fc229-ffd9-462a-882b-16b4a63b2a8a' // 1mw in uat
const floors: Floors = [
  {
    id: '4f2bd55f-8fa9-4922-84d4-93d741e58ed3',
    name: 'BLDG',
    code: 'BLDG',
    geometry: '[]',
    isSiteWide: true,
    modelReference: '7ea923ae-b760-4b76-aaaa-6355825b07fe',
  },
  {
    id: '568547a7-68b9-4419-97f4-9d269da813e0',
    name: 'L36',
    code: 'L36',
    geometry: '[]',
    isSiteWide: false,
  },
  {
    id: '6a2caed8-6ec9-4f63-9a00-f8d2c7356219',
    name: 'L35',
    code: 'L35',
    geometry: '',
    isSiteWide: false,
  },
]

describe('Floors Service', () => {
  test('should return expected data', async () => {
    jest
      .spyOn(axios, 'get')
      .mockResolvedValue(Promise.resolve({ data: floors }))
    const response = await getFloors(siteId)
    expect(response).toMatchObject(floors)
  })

  test('should return error when error occurs', async () => {
    jest.spyOn(axios, 'get').mockRejectedValue(new Error('fetch error'))
    await expect(getFloors(siteId)).rejects.toThrowError('fetch error')
  })
})
