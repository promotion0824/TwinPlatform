import { useInterval } from '@willow/common'
import { api, useUser } from '@willow/ui'
import { create } from 'react-test-renderer'
import AuthSilentRenew from './AuthSilentRenew'

const minutes = 60 * 1000
const SILENT_RENEW_CHECK_INTERVAL = 5 * minutes

jest.mock('@willow/common')

beforeEach(() => jest.resetAllMocks())

describe('AuthSilentRenew Tests', () => {
  test('AuthSilentRenew renders without issues', async () => {
    const testRenderer = await create(
      <AuthSilentRenew api={api} app="platform" useUser={useUser} />
    )
    expect(testRenderer.root).not.toBe(null)
  })

  test('AuthSilentRenew sets up a new 5 min interval', async () => {
    await create(<AuthSilentRenew api={api} app="platform" useUser={useUser} />)

    expect(useInterval).toBeCalledWith(
      expect.any(Function),
      SILENT_RENEW_CHECK_INTERVAL
    )
  })
})
