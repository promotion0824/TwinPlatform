import { useDisclosure } from '@willowinc/ui'
import { useMemo, useId } from 'react'
import { useOnClickOutsideIds } from '../providers'

/**
 * Behaves like `useDisclosure` from `@willowinc/ui`, but is designed to work
 * for a modal that is placed on top of a `@willow/ui` Modal. If you use the
 * regular `useDisclosure`, any click will close the modal that is beneath your
 * own modal. Feel free to think of a better name for this hook!
 */
export default function useDisclosure2(
  initialState = false
): [boolean, { open: () => void; close: () => void }] {
  const [isOpen, { open, close }] = useDisclosure(initialState)
  const onClickOutsideIds = useOnClickOutsideIds()
  const id = useId()

  const functions = useMemo(
    () => ({
      open: () => {
        onClickOutsideIds.registerOnClickOutsideId(id)
        open()
      },
      close: () => {
        onClickOutsideIds.unregisterOnClickOutsideId(id)
        close()
      },
    }),
    [id, open, close]
  )

  return [isOpen, functions]
}
