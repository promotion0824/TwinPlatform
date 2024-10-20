import { useEffect } from 'react'
import { useLatest } from '@willow/common'

function shouldIgnore(element) {
  if (element == null) {
    return false
  }

  if (element.classList?.contains('ignore-onclickoutside')) {
    return true
  }

  return shouldIgnore(element.parentNode)
}

export default function OnClickOutside({
  targetRef,
  targetRefs,
  children,
  onClickOutside = () => {},
}) {
  const latestOnClickOutside = useLatest(onClickOutside)

  useEffect(() => {
    function handlePointerDown(e) {
      const LEFT_BUTTON = 0
      if (e.button === LEFT_BUTTON || e.button == null) {
        let targets = [targetRef.current]
        if (targetRefs != null) {
          const nextTargets = targetRefs().map((target) => target.current)
          const lastOpenedTarget = nextTargets.slice(-1)[0]
          const isLastOpenedTarget = lastOpenedTarget === targetRef.current
          if (!isLastOpenedTarget) {
            targets = nextTargets.filter(
              (target) =>
                target.contains(lastOpenedTarget) ||
                target === targetRef.current
            )
          }
        }
        targets = targets.filter((target) => target != null)

        const contains = targets.some((target) => target.contains(e.target))

        if (!contains && !shouldIgnore(e.target)) {
          latestOnClickOutside(e)
        }
      }
    }

    document.addEventListener('pointerdown', handlePointerDown)

    return () => {
      document.removeEventListener('pointerdown', handlePointerDown)
    }
  }, [targetRef, targetRefs])

  return children ?? null
}
