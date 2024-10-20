import axios from 'axios'
/* eslint-disable-next-line */
import {
  getWidgets,
  postWidget,
  putWidget,
  WidgetsResponse,
} from './WidgetsService'
import { DashboardConfigForm } from '../../components/Reports/DashboardModal/DashboardConfigForm'
import { DashboardReportCategory } from '@willow/ui'

const siteId = '4e5fc229-ffd9-462a-882b-16b4a63b2a8a' // 1mw in uat
const responseData: WidgetsResponse = {
  widgets: [
    {
      id: '02f6698e-a5dc-48c6-a581-22f8bfc3b381',
      metadata: {
        groupId: '3d25fa85-03c4-4d5b-89c8-3e678aecfa1c',
        reportId: '608bcd67-faae-4fb2-b5e8-9ae16607d45c',
        name: 'Setpoint Compliance',
      },
      type: 'powerBIReport',
    },
    {
      id: 'f0e08428-72d3-450f-94c7-4c4f1e2d8f24',
      metadata: {
        embedPath:
          'https://app.sigmacomputing.com/embed/1-fGcv1VE0doQ0d2F5S6cgq',
        name: 'Overall',
      },
      type: 'sigmaReport',
    },
    {
      id: '5cde889f-1ec5-44d6-b9c3-a3129a7c5915',
      metadata: {
        embedPath:
          'https://app.sigmacomputing.com/embed/1-3PFTdZyQINgmQ7dchdhRyU',
        name: 'Building',
      },
      type: 'sigmaReport',
    },
    {
      id: '4e103fba-3fce-4619-93db-b47bd4431b93',
      metadata: {
        embedPath:
          'https://app.sigmacomputing.com/embed/1-3VzjPp0XPisJTrOBEsPzmi',
        name: 'Occupancy',
      },
      type: 'sigmaReport',
    },
    {
      id: 'b6c26e0b-c513-44d8-9d57-de6caf64a3e3',
      metadata: {
        groupId: '3d25fa85-03c4-4d5b-89c8-3e678aecfa1c',
        reportId: '5530cea9-4281-4ef0-94b1-c65b97c79805',
        name: 'Aggregated Metering',
      },
      type: 'powerBIReport',
    },
  ],
}

const postFormData: DashboardConfigForm = {
  positions: [
    {
      siteId: 'a6b78f54-9875-47bc-9612-aa991cc464f3',
      siteName: '126 Phillip Street',
      position: 0,
    },
  ],
  metadata: {
    category: DashboardReportCategory.OPERATIONAL,
    embedLocation: 'dashboardsTab',
    embedGroup: [
      {
        name: 'report1',
        embedPath: 'https://report1.com',
        order: 0,
      },
    ],
  },
  type: 'sigmaReport',
}

const id = 'abc'
const postResponse = { ...postFormData, id }

const putFormData = { ...postFormData }
const putResponse = { ...putFormData, id }

describe('Widgets Service', () => {
  describe('getWidgets', () => {
    test('should return expected data', async () => {
      jest
        .spyOn(axios, 'get')
        .mockResolvedValue(Promise.resolve({ data: responseData }))
      const response = await getWidgets('/api/sites', siteId)
      expect(response).toMatchObject(responseData)
    })

    test('should return error when error occurs', async () => {
      jest.spyOn(axios, 'get').mockRejectedValue(new Error('fetch error'))
      await expect(getWidgets('/api/sites', siteId)).rejects.toThrowError(
        'fetch error'
      )
    })
  })

  describe('postWidget', () => {
    test('should return expected data', async () => {
      jest
        .spyOn(axios, 'post')
        .mockResolvedValue(Promise.resolve({ data: postResponse }))
      const response = await postWidget(postFormData)
      expect(response).toMatchObject(postResponse)
    })

    test('should return error when error occurs', async () => {
      jest.spyOn(axios, 'post').mockRejectedValue(new Error('fetch error'))
      await expect(postWidget(postFormData)).rejects.toThrowError('fetch error')
    })
  })

  describe('putWidget', () => {
    test('should return expected data', async () => {
      jest
        .spyOn(axios, 'put')
        .mockResolvedValue(Promise.resolve({ data: putResponse }))
      const response = await putWidget(id, putFormData)
      expect(response).toMatchObject(putResponse)
    })

    test('should return error when error occurs', async () => {
      jest.spyOn(axios, 'put').mockRejectedValue(new Error('fetch error'))
      await expect(postWidget(putFormData)).rejects.toThrowError('fetch error')
    })
  })
})
