import {
  Menu as MantineMenu,
  MenuTargetProps as MantineMenuTargetProps,
} from '@mantine/core'
import { mergeRefs } from '@mantine/hooks'
import { forwardRef } from 'react'
import { useHiddenElementForAccessibility } from '../../utils'

export interface MenuTargetProps extends MantineMenuTargetProps {
  children: MantineMenuTargetProps['children']
}

/**
 * `Menu.Target` is the container for the interaction point or trigger for the
 * menu.
 * It requires an element or a component as a single child that could attach a ref.
 * Using of strings, fragments, numbers and multiple elements/components are not
 * supported and will throw error.
 */
export const MenuTarget = forwardRef<HTMLDivElement, MenuTargetProps>(
  ({ ...restProps }, ref) => {
    const { targetRef, hiddenComponent } = useHiddenElementForAccessibility()

    return (
      <>
        <MantineMenu.Target {...restProps} ref={mergeRefs(ref, targetRef)} />
        {hiddenComponent}
      </>
    )
  }
)
