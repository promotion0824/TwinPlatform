import { renderHook, waitFor } from '@testing-library/react'
import { useDebounce } from '@willow/ui'

describe('useDebounce', () => {
  const DEBOUNCE_TIME = 100
  const initHooks = (fn, time) => renderHook(() => useDebounce(fn, time))
  const shouldTriggerActionInTime = async (fn, timeout) => {
    await waitFor(() => expect(fn).toHaveBeenCalled(), {
      timeout,
    })
  }
  const shouldNotTriggerActionInTime = async (fn, timeout) => {
    await new Promise((resolve) => setTimeout(resolve, timeout))
    expect(fn).not.toHaveBeenCalled()
  }
  const shouldTriggerActionOnlyOnce = async (fn, timeout) => {
    await waitFor(() => expect(fn).toHaveBeenCalledTimes(1), {
      timeout,
    })
  }
  const initiateDebounce = ({ current }) => current()
  const initiateDebounceMultiTimes = (result, rerenderingOptions) => {
    const { rerender, numberOfTimes, fn, debounceTime } = rerenderingOptions
    ;[...Array(numberOfTimes)].forEach(() => {
      // Need more debate or opinions on rerendering as useDebounce only keeps latest callback function
      rerender(() => useDebounce(fn, debounceTime))
      initiateDebounce(result)
    })
  }

  test('should trigger callback function after given time', async () => {
    const mockFn = jest.fn()
    const { result } = initHooks(mockFn, DEBOUNCE_TIME)

    initiateDebounce(result)

    const timeAfterDebounce = DEBOUNCE_TIME + 300
    await shouldTriggerActionInTime(mockFn, timeAfterDebounce)
  })

  test('should not trigger callback function before given time', async () => {
    const mockFn = jest.fn()
    const { result } = initHooks(mockFn, DEBOUNCE_TIME)

    initiateDebounce(result)

    const timeBeforeDebounce = DEBOUNCE_TIME - 60
    await shouldNotTriggerActionInTime(mockFn, timeBeforeDebounce)
  })

  test('should trigger callback function only once when debounce function is called multiple times', async () => {
    const mockFn = jest.fn()
    const { result, rerender } = initHooks(mockFn, DEBOUNCE_TIME)

    const NUMBER_OF_DEBOUNCE_CALLS = 3
    const rerenderingOptions = {
      fn: mockFn,
      debounceTime: DEBOUNCE_TIME,
      numberOfTimes: NUMBER_OF_DEBOUNCE_CALLS,
      rerender,
    }
    initiateDebounceMultiTimes(result, rerenderingOptions)

    const timeAfterDebounce = DEBOUNCE_TIME + 600
    await shouldTriggerActionOnlyOnce(mockFn, timeAfterDebounce)
  })
})
