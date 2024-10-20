import { renderHook } from '@testing-library/react'
import { useEffectOnceMounted } from '@willow/common'

describe('useEffectOnceMounted', () => {
  it('should not run on the first render', () => {
    const effect = jest.fn()
    renderHook(() => useEffectOnceMounted(effect))
    expect(effect).not.toHaveBeenCalled()
  })

  it('should run after the first render', () => {
    const effect = jest.fn()
    const { rerender } = renderHook(() => useEffectOnceMounted(effect))
    rerender()
    expect(effect).toHaveBeenCalledTimes(1)
  })
})
