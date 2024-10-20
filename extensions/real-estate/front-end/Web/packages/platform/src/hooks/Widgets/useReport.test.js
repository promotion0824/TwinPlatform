import { useState } from 'react'
import _, { get } from 'lodash'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import { UserContext } from '@willow/ui/providers/UserProvider/UserContext'
import { renderHook, waitFor } from '@testing-library/react'
import { queryCache } from '@willow/common'
import * as widgetsService from '../../services/Widgets/WidgetsService'
import useReport from './useReport'

const siteId = '4e5fc229-ffd9-462a-882b-16b4a63b2a8a' // 1mw in uat

function UserProvider({ children, options = {} }) {
  const [user, setUser] = useState({})
  const value = {
    ...user,
    options,
    saveOptions: (k, v) => {
      setUser((prevUser) => ({
        ...prevUser,
        [k]: v,
      }))
    },
    clearOptions: (k) => {
      setUser((prevUser) => ({ ..._.omit(prevUser, k) }))
    },
  }

  return <UserContext.Provider value={value}>{children}</UserContext.Provider>
}

const getWrapper = (options) =>
  function ({ children }) {
    return (
      <BaseWrapper>
        <UserProvider options={options}>{children}</UserProvider>
      </BaseWrapper>
    )
  }

const responseData = {
  widgets: [
    {
      id: '02f6698e-a5dc-48c6-a581-22f8bfc3b381',
      metadata: {
        groupId: '3d25fa85-03c4-4d5b-89c8-3e678aecfa1c',
        reportId: '608bcd67-faae-4fb2-b5e8-9ae16607d45c',
        name: 'Setpoint Compliance',
      },
      type: 'powerBIReport',
      position: '0',
    },
    {
      id: 'f0e08428-72d3-450f-94c7-4c4f1e2d8f24',
      metadata: {
        embedPath:
          'https://app.sigmacomputing.com/embed/1-fGcv1VE0doQ0d2F5S6cgq',
        name: 'Overall',
      },
      type: 'sigmaReport',
      position: '3',
    },
    {
      id: '5cde889f-1ec5-44d6-b9c3-a3129a7c5915',
      metadata: {
        embedPath:
          'https://app.sigmacomputing.com/embed/1-3PFTdZyQINgmQ7dchdhRyU',
        name: 'Building',
      },
      type: 'sigmaReport',
      position: '4',
    },
    {
      id: '4e103fba-3fce-4619-93db-b47bd4431b93',
      metadata: {
        embedPath:
          'https://app.sigmacomputing.com/embed/1-3VzjPp0XPisJTrOBEsPzmi',
        name: 'Occupancy',
      },
      type: 'sigmaReport',
      position: '2',
    },
    {
      id: 'b6c26e0b-c513-44d8-9d57-de6caf64a3e3',
      metadata: {
        groupId: '3d25fa85-03c4-4d5b-89c8-3e678aecfa1c',
        reportId: '5530cea9-4281-4ef0-94b1-c65b97c79805',
        name: 'Aggregated Metering',
      },
      type: 'powerBIReport',
      position: '1',
    },
  ],
}

afterEach(() => queryCache.clear())

describe('useReport', () => {
  test('should provide error when exception error happens', async () => {
    jest
      .spyOn(widgetsService, 'getWidgets')
      .mockRejectedValue(new Error('fetch error'))

    const { result } = renderHook(() => useReport('/api/sites', siteId), {
      wrapper: getWrapper(),
      initialProps: { options: { test: 'test' } },
    })

    await waitFor(() => {
      expect(result.current.isLoading).toBeFalse()
      expect(result.current.data).not.toBeDefined()
      expect(result.current.isError).toBe(true)
      expect(result.current.selectedReport).not.toBeDefined()
    })
  })

  test('hook should provide data at index 0 from widgets as selectedReport when user has no option', async () => {
    jest.spyOn(widgetsService, 'getWidgets').mockResolvedValue(responseData)

    const { result } = renderHook(() => useReport('/api/sites', siteId), {
      wrapper: getWrapper(),
    })

    await waitFor(() => {
      expect(result.current.isLoading).toBeFalse()
      expect(result.current.selectedReport).toMatchObject(
        responseData.widgets[0]
      )
    })
  })

  test('hook should provide user preferred report as selectedReport when user has option', async () => {
    jest.spyOn(widgetsService, 'getWidgets').mockResolvedValue(responseData)

    const { result } = renderHook(() => useReport('/api/sites', siteId), {
      wrapper: getWrapper({
        [`reportId-${siteId}`]: 'b6c26e0b-c513-44d8-9d57-de6caf64a3e3',
      }),
    })

    await waitFor(() => {
      expect(result.current.isLoading).toBeFalse()
      expect(result.current.selectedReport).toMatchObject({
        id: 'b6c26e0b-c513-44d8-9d57-de6caf64a3e3',
        metadata: {
          groupId: '3d25fa85-03c4-4d5b-89c8-3e678aecfa1c',
          reportId: '5530cea9-4281-4ef0-94b1-c65b97c79805',
          name: 'Aggregated Metering',
        },
        type: 'powerBIReport',
        position: '1',
      })
    })
  })
})
