import { getSerializationKey } from './serialization'

describe('getSerializationKey', () => {
  it('should return minSize if order is 0', () => {
    expect(getSerializationKey(0, 1)).toBe('1')
  })

  it('should return minSize if order is null', () => {
    expect(getSerializationKey(null, 1)).toBe('1')
  })

  it('should return minSize if order is undefined', () => {
    expect(getSerializationKey(undefined, 1)).toBe('1')
  })

  it('should return correct key with colon with order and minSize', () => {
    expect(getSerializationKey(1, 1)).toBe('1:1')
  })
})
