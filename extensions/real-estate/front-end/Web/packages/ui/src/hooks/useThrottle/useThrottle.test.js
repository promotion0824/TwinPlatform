import { renderHook } from '@testing-library/react'
import { useThrottle } from '@willow/ui'

describe('useThrottle', () => {
  const THROTTLE_TIME = 1000

  const initHooks = (fn, time) => renderHook(() => useThrottle(fn, time))
  const initialThrottle = ({ current }) => {
    current()
  }

  test('should trigger callback function at most 1 time within throttle time', async () => {
    const mockFn = jest.fn()
    const { result } = initHooks(mockFn, THROTTLE_TIME)

    initialThrottle(result)
    initialThrottle(result)
    initialThrottle(result)

    expect(mockFn).toBeCalledTimes(1)
  })

  test('callback function can be triggered 2nd time after intial throttle time expires', async () => {
    const mockFn = jest.fn()
    const { result } = initHooks(mockFn, THROTTLE_TIME)

    initialThrottle(result)
    await new Promise((resolve) => {
      setTimeout(() => {
        resolve(initialThrottle(result))
      }, THROTTLE_TIME + 100)
    })

    expect(mockFn).toBeCalledTimes(2)
  })
})
