import {
  TabsTab as MantineTab,
  TabsTabProps as MantineTabProps,
} from '@mantine/core'
import { useElementSize } from '@mantine/hooks'
import { debounce } from 'lodash'
import { forwardRef, useImperativeHandle, useLayoutEffect } from 'react'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'
import { useTabsContext } from './TabsContext'

export interface TabProps
  extends WillowStyleProps,
    Omit<MantineTabProps, keyof WillowStyleProps | 'prefix'>,
    // The MantineTabProps is not the final type applied to MantineTabs.Tab,
    // this ref is part of the final type.
    React.RefAttributes<HTMLButtonElement> {
  value: MantineTabProps['value']
  /** The customizable content displays before label. */
  prefix?: MantineTabProps['leftSection']
  /** The customizable content displays after label. */
  suffix?: MantineTabProps['rightSection']
  children?: MantineTabProps['children']
  /**
   * Disable a tab.
   * @default false
   */
  disabled?: boolean
}

/**
 * `Tabs.Tab` is an element in the tab list that serves as a label for one of
 * the tab panels and can be activated to display that panel.
 *
 * @see TODO: add link to storybook
 */
export const Tab = forwardRef<HTMLButtonElement, TabProps>(
  ({ prefix, suffix, ...restProps }, externalRef) => {
    const { setTabWidths, tabWidths } = useTabsContext()
    const { ref, width } = useElementSize<HTMLButtonElement>()
    const stringifiedTabWidths = JSON.stringify(tabWidths)

    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    useImperativeHandle(externalRef, () => ref.current!, [ref])

    // The initial value returned from offsetWidth has been found to often not be the final,
    // correct value. By adding Mantine's useElementSize's width to the dependency array
    // we can catch the correct value when it changes. This happens very quickly, so a small
    // debounce is sufficient to prevent the first one from firing unnecessarily. If the first
    // one did go through and some tabs were hidden because of it, the hidden tabs wouldn't
    // have their second width update occur, which is the other reason the debounce is required.
    useLayoutEffect(() => {
      if (!ref.current || typeof restProps.children !== 'string') return
      const index = tabWidths.findIndex((tab) => tab.value === restProps.value)
      if (index === -1) return

      const updateTabWidth = debounce(() => {
        // Some of this logic is duplicated from useLayoutEffect above, in case the tab
        // has been removed from the DOM during the debounce time.
        if (!ref.current || typeof restProps.children !== 'string') return
        const index = tabWidths.findIndex(
          (tab) => tab.value === restProps.value
        )
        if (index === -1) return
        const newTabWidths = [...tabWidths]
        newTabWidths[index].width = ref.current.offsetWidth
        setTabWidths(newTabWidths)
      }, 0)

      updateTabWidth()

      // Excluding setTabWidths/tabWidths from the dependency array to prevent an infinite loop.
      // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [
      prefix,
      ref,
      restProps.children,
      restProps.value,
      stringifiedTabWidths,
      suffix,
      width,
    ])

    return (
      <MantineTab
        {...restProps}
        {...useWillowStyleProps(restProps)}
        ref={ref}
        leftSection={prefix}
        rightSection={suffix}
        data-title={restProps.children}
      />
    )
  }
)
