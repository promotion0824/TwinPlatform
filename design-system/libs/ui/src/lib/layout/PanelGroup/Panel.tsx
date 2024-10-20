import { Children, HTMLProps, ReactElement, useMemo, useState } from 'react'
import {
  Panel as ResizablePanel,
  PanelProps as ResizablePanelProps,
} from 'react-resizable-panels'
import styled, { css } from 'styled-components'

import { Tabs } from '../../navigation/Tabs'
import CollapsedPanel from './CollapsedPanel'
import { PanelContext } from './PanelContext'
import { PanelFooter } from './PanelFooter'
import {
  PanelGroupContextProps,
  usePanelGroupContext,
} from './PanelGroupContext'
import { default as PanelHeader, type PanelHeaderProps } from './PanelHeader'
import ResizeHandle from './ResizeHandle'
import { ADD_PANEL_ID, REMOVE_PANEL_ID, getSerializationKey } from './utils'
import { Box } from '../../misc/Box'
import { WillowStyleProps, useStylesAndProps } from '../../utils'

export interface PanelProps
  extends WillowStyleProps,
    Omit<HTMLProps<HTMLDivElement>, 'as' | 'onResize' | 'ref' | 'title'> {
  /**
   * Managed by `PanelGroup` - The id of the panel, generated with lodash's uniqueId().
   */
  id?: string
  /**
   * Managed by `PanelGroup` - Whether this panel contains `PanelGroup`.
   * For grouped `Panel`, we omit the theme background for this Panel.
   */
  grouped?: boolean
  /**
   * For *resizable panel* - The minimum allowable size of the panel (in percent) of
   * the panel group. Must be between 1-100.
   * @default 10
   */
  minSize?: number
  /**
   * For *resizable panel* - The maximum allowable size of the panel (in percent) of
   * the panel group. Must be between 1-100.
   * @default 100
   */
  maxSize?: number
  /**
   * The initial size of the panel.
   *
   * For *fixed sized panel* - This can be either a number (in px) or a width/height
   * in string (eg. 25%, 100px, 100rem).
   *
   * For *resizable panel* - This is the size of panel (in percent) and must be
   * numeric between 1-100. If all panels within the group has a `defaultSize` set,
   * the sum of all panels' `defaultSize` must not be more than 100. **Note**: if there
   * is a `defaultSize` set to any panel within the group, please ensure that there is
   * at least one panel that is not collapsible and does not have a defaultSize set.
   * For more details, please refer to known issues in *PanelGroup's Docs*.
   */
  defaultSize?: number | string
  /**
   * Whether the panel is collapsible.
   */
  collapsible?: boolean
  /** If provided, a footer will be displayed containing this content. */
  footer?: React.ReactNode
  /** Optional controls to be displayed on the right side of the header. */
  headerControls?: React.ReactNode
  /**
   * Hide the header border on title variants.
   * Primarily useful if you want a collapse button but no other header content.
   * @default false
   */
  hideHeaderBorder?: PanelHeaderProps['hideBorder']
  /**
   * Callback handler when panel is collapsed or expanded.
   */
  onCollapse?: (collapsed: boolean) => void
  /** Called when the panel is resized. */
  onResize?: ResizablePanelProps['onResize']
  /**
   * To display tabs in the panel, provide a Tabs component and all relevant children
   * (List, Tab, and Panel) to this property.
   * Note that currently only the "outline" variant is supported, and will be set automatically.
   */
  tabs?: ReactElement
  /** Content to be displayed in the panel's header. */
  title?: React.ReactNode
}

const panelCss = (
  gapSize: PanelGroupContextProps['gapSize'],
  isVertical = false
) =>
  css(({ theme }) => {
    const margin = gapSize === 'medium' ? theme.spacing.s12 : theme.spacing.s4

    return {
      position: 'relative',
      boxSizing: 'border-box',

      '&:not(:first-child)': {
        marginLeft: !isVertical ? margin : undefined,
        marginTop: isVertical ? margin : undefined,
      },

      '[role=separator][aria-controls] + &': {
        marginLeft: !isVertical ? 'unset' : undefined,
        marginTop: isVertical ? 'unset' : undefined,
      },
    }
  })

const themedCss = css(({ theme }) => ({
  background: theme.color.neutral.bg.panel.default,
  border: `1px solid ${theme.color.neutral.border.default}`,
  borderRadius: theme.radius.r2,
}))

const cssFixedPanel = (isVertical: boolean, defaultSize?: number | string) =>
  css({
    flex: '1',
    overflow: 'hidden',
    ...(isVertical
      ? { minHeight: defaultSize, maxHeight: defaultSize }
      : { minWidth: defaultSize, maxWidth: defaultSize }),
  })

const PanelContentContainer = styled.div({
  display: 'flex',
  flexDirection: 'column',
  height: '100%',
  width: '100%',
})

const StyledTabs = styled(Tabs)({
  display: 'flex',
  flexDirection: 'column',
  height: '100%',

  '[role=tab]': {
    borderRadius: 0,
    borderTop: 'none',
  },

  '[role=tab]:first-of-type': {
    borderLeft: 'none',
  },

  '[role=tablist]': {
    flexWrap: 'nowrap',
  },

  '[role=tabpanel]': {
    height: '100%',
    overflowY: 'auto',
  },
})

/**
 * `Panel` is either fixed (by default) or resizable, and the panel may be collapsible.
 *
 * @see For ResizablePanel docs, please see https://github.com/bvaughn/react-resizable-panels/tree/main/packages/react-resizable-panels#panel
 */
const Panel = ({
  children,
  footer,
  headerControls,
  hideHeaderBorder,
  id,
  onCollapse,
  collapsible = false,
  defaultSize,
  minSize = 10,
  maxSize,
  grouped = false,
  className,
  onResize,
  tabs,
  title,
  ...restProps
}: PanelProps) => {
  const {
    gapSize,
    isVertical,
    idsRef,
    resizable,
    activePanelIds,
    dispatchActivePanelIds,
    persistPanelLayout,
  } = usePanelGroupContext()

  const stylesAndProps = useStylesAndProps(restProps)

  const panelIds = idsRef.current ?? []

  const order = id && panelIds ? panelIds.indexOf(id) : undefined
  if (order === undefined) {
    // not sure when and what if it is a problem,
    // so add this log to monitor further
    console.warn('No order will assign to Panel, id = ', id)
  }
  const panelSerializationKey =
    typeof order === 'number' ? getSerializationKey(order, minSize) : ''

  /**
   * Used to decide if a panel is collapsed initially,
   * when loading the initial status from storage.
   */
  const initiallyCollapsed =
    !panelSerializationKey || activePanelIds === null /* not initialized */
      ? false
      : !activePanelIds.includes(panelSerializationKey)

  const [isCollapsed, setCollapsed] = useState<boolean>(initiallyCollapsed)

  const panelValue = useMemo(
    () => ({
      collapsible,
      gapSize,
      id,
      onCollapse: (collapsed: boolean) => {
        if (persistPanelLayout) {
          dispatchActivePanelIds({
            type: collapsed ? REMOVE_PANEL_ID : ADD_PANEL_ID,
            id: panelSerializationKey,
          })
        }

        setCollapsed(collapsed)
        onCollapse?.(collapsed)
      },
    }),
    [
      collapsible,
      gapSize,
      id,
      persistPanelLayout,
      onCollapse,
      dispatchActivePanelIds,
      panelSerializationKey,
    ]
  )

  const { children: _, variant: __, ...tabProps } = tabs?.props ?? {}
  const tabRoot = tabs && Children.map(tabs, (child: ReactElement) => child)

  if (tabRoot && tabRoot[0].type !== Tabs) {
    throw new Error(
      'The Tabs component must be the first child of the tabs property'
    )
  }

  const tabChildren = tabRoot?.[0].props.children

  // when tabs prop is defined and user only provides 1 child,
  // tabChildren will be a single ReactNode instead of an array
  if (tabs && (!Array.isArray(tabChildren) || tabChildren.length < 2)) {
    throw new Error(
      'The Tabs component must have at least 2 children, the 1st being a list of tabs, the 2nd being the content'
    )
  }

  const panelContent = (
    <PanelContentContainer>
      {tabs ? (
        <StyledTabs variant="outline" {...tabProps}>
          <PanelHeader headerControls={headerControls} variant="tabs">
            {tabChildren[0]}
          </PanelHeader>
          {tabChildren.slice(1)}
          {footer != null && <PanelFooter>{footer}</PanelFooter>}
        </StyledTabs>
      ) : (
        <>
          {(collapsible || title) && (
            <PanelHeader
              headerControls={headerControls}
              hideBorder={hideHeaderBorder}
            >
              {title}
            </PanelHeader>
          )}
          {children}
          {footer != null && <PanelFooter>{footer}</PanelFooter>}
        </>
      )}
    </PanelContentContainer>
  )

  return (
    <PanelContext.Provider value={panelValue}>
      {isCollapsed ? (
        // The customized div for a collapsed panel was originally
        // to fix a collapse + resizing bug; Potential fix could be
        // hide the `ResizeHandle` instead of removing it from DOM.

        // Now, this customized div is required for persisting panel collapse
        // states in storage.
        //
        // The original layout information stored by react-resizable-panels
        // is an array of each panel's width ratio, which can not
        // represent collapsed status. Though react-resizable-panels uses 0 for
        // collapsed panel, we cannot use 0 in our case, because our collapsed
        // panel has a width of 44px for displaying a toggle button, value 0
        // will cause bug because in some cases the base library will try to
        // override the width we set and assign 0 to the panel . And recalculate
        // and manage PanelGroup ratio on our own is adding unnecessary complexity.
        //
        // Another solution is to trigger the recalculation in the base library,
        // but it only gets triggered by dragging ResizeHandler currently in
        // react-resizable-panels.
        <CollapsedPanel
          css={[panelCss(gapSize, isVertical), themedCss]}
          onExpand={() => panelValue.onCollapse(false)}
          isVertical={isVertical}
          title={title}
          data-panel="" // same attribute as ResizablePanel used for testing
          // CollapsedPanel will handel style props itself
          {...restProps}
        />
      ) : resizable ? (
        <>
          <ResizablePanel
            className={className}
            css={[panelCss(gapSize, isVertical), !grouped && themedCss]}
            id={id}
            order={order}
            defaultSize={defaultSize as number}
            minSize={minSize}
            maxSize={maxSize}
            onResize={onResize}
            // data-testid will be discarded by the base library
            {...stylesAndProps}
          >
            {panelContent}
          </ResizablePanel>
          <ResizeHandle />
        </>
      ) : (
        <Box
          className={className}
          css={[
            panelCss(gapSize, isVertical),
            cssFixedPanel(isVertical, defaultSize),
            !grouped && themedCss,
          ]}
          data-panel="" // same attribute as ResizablePanel
          {...restProps}
        >
          {panelContent}
        </Box>
      )}
    </PanelContext.Provider>
  )
}

export default Panel
