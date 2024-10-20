import reduceQueryStatuses from '../reduceQueryStatuses'

const errorStatus = 'error' as const
const successStatus = 'success' as const
const loadingStatus = 'loading' as const
const idleStatus = 'idle' as const

describe('reduceQueryStatuses', () => {
  test.each([
    {
      initialStatuses: [
        errorStatus,
        successStatus,
        successStatus,
        loadingStatus,
      ],
      finalStatus: errorStatus,
    },
    {
      initialStatuses: [successStatus, successStatus, idleStatus, errorStatus],
      finalStatus: errorStatus,
    },
    {
      initialStatuses: [
        loadingStatus,
        loadingStatus,
        loadingStatus,
        errorStatus,
      ],
      finalStatus: errorStatus,
    },
  ])(
    'when statuses contain error status, return error as final status',
    ({ initialStatuses, finalStatus }) => {
      expect(reduceQueryStatuses(initialStatuses)).toBe(finalStatus)
    }
  )

  test.each([
    {
      initialStatuses: [successStatus, successStatus, loadingStatus],
      finalStatus: loadingStatus,
    },
    {
      initialStatuses: [loadingStatus, loadingStatus, loadingStatus],
      finalStatus: loadingStatus,
    },
    {
      initialStatuses: [idleStatus, successStatus, idleStatus],
      finalStatus: loadingStatus,
    },
  ])(
    'when statuses contain no error status but loading or idle status, return loading as final status',
    ({ initialStatuses, finalStatus }) => {
      expect(reduceQueryStatuses(initialStatuses)).toBe(finalStatus)
    }
  )

  test('return success as final status when status contains no idle/loading/error status', () => {
    expect(reduceQueryStatuses([successStatus, successStatus])).toBe(
      successStatus
    )
  })
})
