import { Notifications, NotificationsProps } from '@mantine/notifications'
import { forwardRef } from 'react'

/**
 * Fix for Storybook ArgTypes not working with Mantine's props.
 * See https://willow.atlassian.net/l/cp/40rrHNJp
 */
export interface BaseProps {
  autoClose?: NotificationsProps['autoClose']
  /**
   * Determines whether notifications container should be rendered inside Portal
   * @default true
   */
  withinPortal?: NotificationsProps['withinPortal']
  /**
   * Maximum number of notifications displayed at a time, other new notifications
   * will be added to queue
   * @default 5
   */
  limit?: NotificationsProps['limit']
}
export const BasePropsDiv = forwardRef<HTMLDivElement, BaseProps>(() => <div />)

export interface SnackbarsProps
  extends Omit<NotificationsProps, 'message' | 'color' | 'icon'>,
    BaseProps {}

/**
 * Add `Snackbars` component anywhere in your application. Note that:
 * - It is required to render Snackbars component as child in ThemeProvider at
 * root level of your application else snackbar won't be displayed.
 * - You do not need to wrap your application with Snackbars component – it is
 * not a provider, it is a regular component
 * - You should not render multiple Snackbars components – if you do that, your
 * snackbars will be duplicated
 *
 * **Example**
 *
 * ```js
 * import { Snackbars } from '@willowinc/ui'
 *
 * function Demo() {
 *   return (
 *    <ThemeProvider>
 *      <Snackbars />
 *      <App />
 *    </ThemeProvider>
 *   )
 * }
 * ```
 */
export const Snackbars = forwardRef<HTMLDivElement, SnackbarsProps>(
  ({ autoClose = 4000, withinPortal = true, limit = 5, ...restProps }, ref) => (
    <Notifications
      {...restProps}
      autoClose={autoClose}
      limit={limit}
      withinPortal={withinPortal}
      ref={ref}
    />
  )
)
