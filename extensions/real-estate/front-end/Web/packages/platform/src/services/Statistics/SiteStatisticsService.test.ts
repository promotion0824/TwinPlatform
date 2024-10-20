import axios from 'axios'
import { getSiteStats, StatsResponse } from './SiteStatisticsService'

const ERROR_MESSAGE = 'fetch error'

const siteId = '4e5fc229-ffd9-462a-882b-16b4a63b2a8a'
const floorId = '9bac10b8-4266-4035-9b83-ddb56ca6924b'

describe('Site Statistics service', () => {
  test('Querying Insights stats without floorId: should return expected data', async () => {
    const responseData: StatsResponse = {
      highCount: 9,
      lowCount: 10,
      mediumCount: 433,
      openCount: 427,
      urgentCount: 9,
    }

    jest.spyOn(axios, 'get').mockResolvedValue({ data: responseData })

    const response = await getSiteStats(siteId, 'insights')

    expect(response).toMatchObject(responseData)
  })

  test('Querying Insights stats without floorId: should return error when exception error happens', async () => {
    jest.spyOn(axios, 'get').mockRejectedValue(new Error(ERROR_MESSAGE))

    await expect(getSiteStats(siteId, 'insights')).rejects.toThrowError(
      ERROR_MESSAGE
    )
  })

  test('Querying Insights stats with floorId: should return expected data', async () => {
    const responseData: StatsResponse = {
      highCount: 3,
      lowCount: 3,
      mediumCount: 3,
      openCount: 3,
      urgentCount: 3,
    }

    jest.spyOn(axios, 'get').mockResolvedValue({ data: responseData })

    const response = await getSiteStats(siteId, 'insights', floorId)

    expect(response).toMatchObject(responseData)
  })

  test('Querying Insights stats with floorId: should return error when exception error happens', async () => {
    jest.spyOn(axios, 'get').mockRejectedValue(new Error(ERROR_MESSAGE))

    await expect(
      getSiteStats(siteId, 'insights', floorId)
    ).rejects.toThrowError(ERROR_MESSAGE)
  })

  test('Querying Tickets stats without floorId: should return expected data', async () => {
    const responseData: StatsResponse = {
      highCount: 9,
      lowCount: 10,
      mediumCount: 433,
      openCount: 427,
      urgentCount: 9,
    }

    jest.spyOn(axios, 'get').mockResolvedValue({ data: responseData })

    const response = await getSiteStats(siteId, 'tickets')

    expect(response).toMatchObject(responseData)
  })

  test('Querying Tickets stats without floorId: should return error when exception error happens', async () => {
    jest.spyOn(axios, 'get').mockRejectedValue(new Error(ERROR_MESSAGE))

    await expect(getSiteStats(siteId, 'tickets')).rejects.toThrowError(
      ERROR_MESSAGE
    )
  })

  test('Querying Tickets stats with floorId: should return expected data', async () => {
    const responseData: StatsResponse = {
      highCount: 3,
      lowCount: 3,
      mediumCount: 3,
      openCount: 3,
      urgentCount: 3,
    }

    jest.spyOn(axios, 'get').mockResolvedValue({ data: responseData })

    const response = await getSiteStats(siteId, 'tickets', floorId)

    expect(response).toMatchObject(responseData)
  })

  test('Querying Tickets stats with floorId: should return error when exception error happens', async () => {
    jest.spyOn(axios, 'get').mockRejectedValue(new Error(ERROR_MESSAGE))

    await expect(getSiteStats(siteId, 'tickets', floorId)).rejects.toThrowError(
      ERROR_MESSAGE
    )
  })
})
