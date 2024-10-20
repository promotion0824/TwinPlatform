import _ from 'lodash'
import { renderHook } from '@testing-library/react'
import useSaveUserPreferences from './useSaveUserPreferences'

describe('useSaveUserPreferences', () => {
  test('user preference will be saved if it differs from existing preference', () => {
    const mockSave = jest.fn()

    const preferenceNames = ['a', 'b', 'c', 'd', 'e']
    const preferencesToSave: Array<CustomPreferenceType> = [
      'newA',
      {
        B: 'newB-1',
      },
      'sameValueC',
      ['newD'],
      undefined,
    ]
    const preferenceValues: Array<CustomPreferenceType> = [
      'oldA',
      {
        B: 'oldB-1',
      },
      'sameValueC',
      ['oldD'],
      undefined,
    ]
    const difference = _.difference(preferencesToSave, preferenceValues)

    renderHook(() =>
      useSaveUserPreferences<CustomPreferenceType>({
        preferenceNames,
        preferencesToSave,
        preferenceValues,
        save: mockSave,
      })
    )

    expect(mockSave).toBeCalledTimes(difference.length)
    for (const name of preferenceNames) {
      const index = preferenceNames.indexOf(name)
      if (preferencesToSave[index] !== preferenceValues[index]) {
        expect(mockSave).toBeCalledWith(name, preferencesToSave[index])
      }
    }
  })
})

type CustomPreferenceType =
  | string
  | { B?: string }
  | undefined
  | (string | undefined)[]
