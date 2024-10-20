/* eslint-disable @typescript-eslint/no-non-null-assertion */
import { renderHook, RenderHookResult, waitFor } from '@testing-library/react'
import { ReactQueryStubProvider } from '@willow/common'
import { rest } from 'msw'
import { setupServer } from 'msw/node'
import { Settings } from 'luxon'
import useGetTimeZones, {
  useTimeZoneInfo,
  getTimeZone,
  TimeZoneInfo,
} from './useGetTimeZones'

const timeZones = {
  UTC: {
    id: 'UTC',
    displayName: '(UTC) Coordinated Universal Time',
    offset: '+00:00',
    regionTimeZone: {
      '001': 'Etc/GMT',
      GL: 'America/Danmarkshavn',
      ZZ: 'Etc/GMT Etc/UTC',
    },
  },
  'Qyzylorda Standard Time': {
    id: 'Qyzylorda Standard Time',
    displayName: '(UTC+05:00) Qyzylorda',
    offset: '+05:00',
  },
  'Singapore Standard Time': {
    id: 'Singapore Standard Time',
    displayName: '(UTC+08:00) Kuala Lumpur, Singapore',
    offset: '+08:00',
    regionTimeZone: {
      '001': 'Asia/Singapore',
      BN: 'Asia/Brunei',
      ID: 'Asia/Makassar',
      MY: 'Asia/Kuala_Lumpur Asia/Kuching',
      PH: 'Asia/Manila',
      SG: 'Asia/Singapore',
      ZZ: 'Etc/GMT-8',
    },
  },
  'E. Australia Standard Time': {
    id: 'E. Australia Standard Time',
    displayName: '(UTC+10:00) Brisbane',
    offset: '+10:00',
    regionTimeZone: {
      '001': 'Australia/Brisbane',
      AU: 'Australia/Brisbane Australia/Lindeman',
    },
  },
  'AUS Eastern Standard Time': {
    id: 'AUS Eastern Standard Time',
    displayName: '(UTC+10:00) Canberra, Melbourne, Sydney',
    offset: '+10:00',
    regionTimeZone: {
      '001': 'Australia/Sydney',
      AU: 'Australia/Sydney Australia/Melbourne',
    },
  },
}

const server = setupServer(
  rest.get('/api/timezones', (_req, res, ctx) =>
    res(ctx.json(Object.values(timeZones)))
  )
)

beforeAll(() => server.listen())
afterEach(() => {
  server.restoreHandlers()
  Settings.defaultZone = 'system'
})
afterAll(() => server.close())

describe('useGetTimeZones', () => {
  test('Gets the time zone list', async () => {
    const { result } = renderHook(() => useGetTimeZones(), {
      wrapper: ReactQueryStubProvider,
    })

    expect(result.current.isLoading).toBeTrue()

    await waitFor(() => expect(result.current.isLoading).toBeFalse())

    expect(result.current.isSuccess).toBeTrue()
    expect(result.current.data).toStrictEqual(Object.values(timeZones))
  })

  describe('useTimeZoneInfo', () => {
    const renderUseTimeZoneInfo = (
      timeZoneId?: string
    ): RenderHookResult<TimeZoneInfo | null, string> => {
      const renderHookResult = renderHook(() => useTimeZoneInfo(timeZoneId), {
        wrapper: ReactQueryStubProvider,
      })

      expect(renderHookResult.result.current).toBeNull()

      return renderHookResult
    }

    test('Should return the time zone info based on zone id', async () => {
      const { result } = renderUseTimeZoneInfo('AUS Eastern Standard Time')

      await waitFor(() =>
        expect(result.current).toStrictEqual(
          timeZones['AUS Eastern Standard Time']
        )
      )
    })

    test.each([
      {
        defaultZone: 'Asia/Kuala_Lumpur',
        expectedZoneId: 'Singapore Standard Time',
      },
      {
        defaultZone: 'Asia/Kuching',
        expectedZoneId: 'Singapore Standard Time',
      },
      {
        defaultZone: 'UTC',
        expectedZoneId: 'UTC',
      },
      {
        defaultZone: 'Etc/UTC',
        expectedZoneId: 'UTC',
      },
    ])(
      'Should return the time zone info of "$expectedZoneId" when system zone name is $defaultZone',
      async ({
        defaultZone,
        expectedZoneId,
      }: {
        defaultZone: string
        expectedZoneId: keyof typeof timeZones
      }) => {
        Settings.defaultZone = defaultZone
        const { result } = renderUseTimeZoneInfo()

        await waitFor(() => {
          expect(result.current).toStrictEqual(timeZones[expectedZoneId])
        })
      }
    )

    test('Should return null when no id is specified and system is in area not in the time zone list', async () => {
      Settings.defaultZone = 'Australia/Perth'

      const { result } = renderUseTimeZoneInfo()

      await waitFor(() => {
        expect(result.current).toBeNull()
      })
    })
  })

  describe('getTimeZone', () => {
    test('Should return time zone as specified', () => {
      expect(
        getTimeZone({
          id: 'Line Islands Standard Time',
          displayName: '(UTC+14:00) Kiritimati Island',
          offset: '+14:00',
          regionTimeZone: {
            '001': 'Pacific/Kiritimati',
            KI: 'Pacific/Kiritimati',
            ZZ: 'Etc/GMT-14',
          },
        })
      ).toBe('Pacific/Kiritimati')
    })

    test('Should return time zone based on UTC offset', () => {
      expect(
        getTimeZone({
          id: 'Qyzylorda Standard Time',
          displayName: '(UTC+05:00) Qyzylorda',
          offset: '+05:00',
        })
      ).toBe('UTC+05:00')
    })
  })
})
