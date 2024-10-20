import { getValueControls } from './getValueControls'

describe('getValueControls', () => {
  it('should return user controlled value when controlledValue is provided', () => {
    const result = getValueControls({
      externalValue: 'controlled',
      defaultValue: undefined,
      internalValue: 'internal',
    })
    expect(result).toEqual({ defaultValue: undefined, value: 'controlled' })
  })

  it('should return user controlled value and default value when both provided', () => {
    const result = getValueControls({
      externalValue: 'controlled',
      defaultValue: 'default',
      internalValue: 'internal',
    })
    expect(result).toEqual({ defaultValue: 'default', value: 'controlled' })
  })

  it('should return internal controlled value when no defaultValue and no controlledValue is provided', () => {
    const result = getValueControls({
      internalValue: 'internal',
      defaultValue: undefined,
      externalValue: undefined,
    })
    expect(result).toEqual({ value: 'internal' })
  })

  it('should return internal controlled value when user has made a selection but not controlled by user', () => {
    const result = getValueControls({
      defaultValue: 'default',
      internalValue: 'internal',
      externalValue: undefined,
    })
    expect(result).toEqual({ value: 'internal' })
  })

  it('should return defaultValue when user is not controlled and no selection is made', () => {
    const result = getValueControls({
      defaultValue: 'default',
      internalValue: '',
      externalValue: undefined,
    })
    expect(result).toEqual({ value: 'default' })
  })
})
