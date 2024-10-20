import { renderHook } from '@testing-library/react'
import { act } from 'react-dom/test-utils'
import { useLocation, useHistory } from 'react-router'
import useSingleSearchParam from './useSingleSearchParam'

jest.mock('react-router')
const mockPush = jest.fn()
const mockReplace = jest.fn()
useLocation.mockImplementation(() => ({
  search: '?a=aaa&b=bbb&c=c-one&c=c-two',
  state: 'my-state',
}))
useHistory.mockImplementation(() => ({ push: mockPush, replace: mockReplace }))

describe('useSingleSearchParam', () => {
  beforeEach(() => {
    mockPush.mockClear()
    mockReplace.mockClear()
  })

  test('returns the value of a parameter', () => {
    const { result } = renderHook(() => useSingleSearchParam('a'))
    expect(result.current[0]).toBe('aaa')
  })

  test('returns undefined for an unspecified parameter', () => {
    const { result } = renderHook(() => useSingleSearchParam('other'))
    expect(result.current[0]).toBe(undefined)
  })

  test('returns an array for a parameter that has multiple entries when asked for an array', () => {
    const { result } = renderHook(() => useSingleSearchParam('c', true))
    expect(result.current[0]).toEqual(['c-one', 'c-two'])
  })

  test('returns an empty array for an unspecified parameter when asked for an array', () => {
    const { result } = renderHook(() => useSingleSearchParam('other', true))
    expect(result.current[0]).toEqual([])
  })

  test('changes a parameter without changing others', () => {
    const { result } = renderHook(() => useSingleSearchParam('a'))

    act(() => {
      result.current[1]('new value')
    })

    expect(mockPush).toBeCalledWith(
      '?a=new+value&b=bbb&c=c-one&c=c-two',
      undefined
    )
  })

  test('changes an array parameter without changing others', () => {
    const { result } = renderHook(() => useSingleSearchParam('c', true))

    act(() => {
      result.current[1]([1, 2, 3])
    })

    expect(mockPush).toBeCalledWith('?a=aaa&b=bbb&c=1&c=2&c=3', undefined)
  })

  test('removes a parameter set to undefined without changing others', () => {
    const { result } = renderHook(() => useSingleSearchParam('a'))

    act(() => {
      result.current[1](undefined)
    })

    expect(mockPush).toBeCalledWith('?b=bbb&c=c-one&c=c-two', undefined)
  })

  test('removes a parameter set to null without changing others', () => {
    const { result } = renderHook(() => useSingleSearchParam('a'))

    act(() => {
      result.current[1](null)
    })

    expect(mockPush).toBeCalledWith('?b=bbb&c=c-one&c=c-two', undefined)
  })

  test('removes an array parameter without affecting others', () => {
    const { result } = renderHook(() => useSingleSearchParam('c', true))

    act(() => {
      result.current[1](null)
    })

    expect(mockPush).toBeCalledWith('?a=aaa&b=bbb', undefined)
  })

  test('removes an array parameter when it is empty without affecting others', () => {
    const { result } = renderHook(() => useSingleSearchParam('c', true))

    act(() => {
      result.current[1]([])
    })

    expect(mockPush).toBeCalledWith('?a=aaa&b=bbb', undefined)
  })

  test('sets a new parameter without affecting others', () => {
    const { result } = renderHook(() => useSingleSearchParam('new'))

    act(() => {
      result.current[1]('value')
    })

    expect(mockPush).toBeCalledWith(
      '?a=aaa&b=bbb&c=c-one&c=c-two&new=value',
      undefined
    )
  })

  test('sets a new array parameter without affecting others', () => {
    const { result } = renderHook(() => useSingleSearchParam('new', true))

    act(() => {
      result.current[1](['1', '2', '3'])
    })

    expect(mockPush).toBeCalledWith(
      '?a=aaa&b=bbb&c=c-one&c=c-two&new=1&new=2&new=3',
      undefined
    )
  })

  describe('With navigate options to replace history', () => {
    test('set a new parameter', () => {
      const { result } = renderHook(() =>
        useSingleSearchParam('new', false, { replace: true })
      )

      act(() => {
        result.current[1]('b')
      })

      expect(mockReplace).toBeCalledWith(
        '?a=aaa&b=bbb&c=c-one&c=c-two&new=b',
        undefined
      )
    })

    test('set a new parameter with new state', () => {
      const { result } = renderHook(() =>
        useSingleSearchParam('new', false, {
          replace: true,
          state: 'my-new-state',
        })
      )

      act(() => {
        result.current[1]('b')
      })

      expect(mockReplace).toBeCalledWith(
        '?a=aaa&b=bbb&c=c-one&c=c-two&new=b',
        'my-new-state'
      )
    })
  })
})
