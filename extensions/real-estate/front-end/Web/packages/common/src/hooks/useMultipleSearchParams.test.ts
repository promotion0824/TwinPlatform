import { renderHook } from '@testing-library/react'
import { act } from 'react-dom/test-utils'
import { useLocation, useHistory } from 'react-router'

import useMultipleSearchParams from './useMultipleSearchParams'

jest.mock('react-router')
const mockPush = jest.fn()
const mockReplace = jest.fn()

const mockedUseLocation = useLocation as jest.Mock<
  Partial<ReturnType<typeof useLocation>>
>
const mockedUseHistory = useHistory as jest.Mock<
  Partial<ReturnType<typeof useHistory>>
>

mockedUseLocation.mockImplementation(() => ({
  search: '?a=aaa&b=bbb&c=c-one&c=c-two',
  state: 'my-state',
}))
mockedUseHistory.mockImplementation(() => ({
  push: mockPush,
  replace: mockReplace,
}))

describe('useMultipleSearchParams', () => {
  test('sets multiple parameters', () => {
    const { result } = renderHook(() => useMultipleSearchParams(['a', 'b']))

    act(() => {
      result.current[1]({ a: 'newA', b: 'newB' })
    })

    expect(mockPush).toBeCalledTimes(1)
    expect(mockPush).toBeCalledWith('?a=newA&b=newB&c=c-one&c=c-two', undefined)
  })
})
