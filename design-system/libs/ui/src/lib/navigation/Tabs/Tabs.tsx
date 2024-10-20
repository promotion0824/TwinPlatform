import {
  Tabs as MantineTabs,
  TabsProps as MantineTabsProps,
} from '@mantine/core'
import {
  forwardRef,
  isValidElement,
  useEffect,
  useImperativeHandle,
  useRef,
  useState,
} from 'react'
import styled, { css } from 'styled-components'
import { ForwardRefWithStaticComponents, rem } from '../../utils'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'
import { COLLAPSIBLE_TABS_BUTTON, List } from './List'
import { Panel } from './Panel'
import { Tab } from './Tab'
import { TabWithWidth, TabsContext } from './TabsContext'
import useCollapsibleList from './useCollapsibleTabs'

/**
 * Props that will be maintained by us.
 * Separate it due to a bug in Storybook ArgTypes.
 */
export interface TabsBaseProps {
  /** Tabs content. Includes Tabs.List and Tabs.Panel. */
  children?: MantineTabsProps['children']
  /** Default value for uncontrolled component */
  defaultValue?: MantineTabsProps['defaultValue']
  /** Value for controlled component */
  value?: MantineTabsProps['value']
  /** Callback for controlled component */
  onTabChange?: MantineTabsProps['onChange']

  /**
   * Tabs variant
   * @default 'default'
   */
  variant?: 'default' | 'outline' | 'pills'
  /**
   * Tabs orientation
   * 'vertical' is not supported yet.
   * @default 'horizontal'
   */
  orientation?: 'horizontal' // | 'vertical'
}

export interface TabsProps
  extends WillowStyleProps,
    Omit<
      MantineTabsProps,
      keyof WillowStyleProps | 'children' | 'orientation' | 'variant'
    >,
    TabsBaseProps {}

type TabsComponent = ForwardRefWithStaticComponents<
  TabsProps,
  {
    List: typeof List
    Tab: typeof Tab
    Panel: typeof Panel
  }
>

function initializeTabWidthsArray(
  children: React.ReactNode,
  existingTabWidths: TabWithWidth[]
) {
  const tabList: React.ReactNode =
    // Tabs component with only Tabs.List
    !Array.isArray(children)
      ? children
      : // PanelGroup containing Tabs component
      children.length === 3
      ? // Tabs component with Tabs.List and Tabs.Panels
        children[0].props.children
      : children[0]

  if (!isValidElement(tabList)) return []

  return tabList.props.children
    .map((child: React.ReactNode) =>
      isValidElement(child) ? child.props.value : undefined
    )
    .filter((child: React.ReactNode | undefined) => child)
    .map((value: string) => {
      const width = existingTabWidths.find((tab) => tab.value === value)?.width
      return { value, width }
    })
}

/**
 * `Tabs` is a set of tab elements and their associated tab panels,
 * which could be used to switch between different views.
 */
const Tabs: TabsComponent = forwardRef<HTMLDivElement, TabsProps>(
  (
    {
      defaultValue,
      onTabChange,
      variant = 'default',
      orientation = 'horizontal',
      value,
      ...restProps
    },
    externalRef
  ) => {
    const ref = useRef<HTMLDivElement>(null)
    const [selectedTab, setSelectedTab] = useState(value ?? defaultValue)
    const [tabWidths, setTabWidths] = useState<TabWithWidth[]>([])
    const [visibleCount, setVisibleCount] = useState<number>(Infinity)

    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    useImperativeHandle(externalRef, () => ref.current!, [ref])

    const nextVisibleCount = useCollapsibleList({
      gap: variant === 'default' ? 12 : variant === 'outline' ? 0 : 4,
      ref,
      tabWidths,
    })

    if (nextVisibleCount !== visibleCount) {
      setVisibleCount(nextVisibleCount)
    }

    useEffect(() => {
      setTabWidths(initializeTabWidthsArray(restProps.children, tabWidths))
      // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [restProps.children])

    useEffect(() => {
      if (value) setSelectedTab(value)
    }, [value])

    return (
      <TabsContext.Provider
        value={{
          selectedTab,
          setSelectedTab: (nextTab: string | null) => {
            setSelectedTab(nextTab)
            onTabChange?.(nextTab)
          },
          setTabWidths,
          setVisibleCount,
          tabWidths,
          visibleCount,
        }}
      >
        <StyledTabs
          {...restProps}
          {...useWillowStyleProps(restProps)}
          onChange={(value) => {
            if (value === COLLAPSIBLE_TABS_BUTTON) return
            onTabChange?.(value)
            setSelectedTab(value)
          }}
          ref={ref}
          variant={variant}
          orientation={orientation}
          value={selectedTab}
        />
      </TabsContext.Provider>
    )
  }
  // No good fix for this yet, follows what Mantine does.
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
) as any

Tabs.List = List
Tabs.Tab = Tab
Tabs.Panel = Panel

export { Tabs }

const StyledTabs = styled(MantineTabs)(
  ({ theme, variant }) => css`
    width: 100%;

    &,
    *,
    *::before,
    *::after {
      box-sizing: border-box;
    }

    .mantine-Tabs-list {
      flex-wrap: nowrap;

      &::before {
        border-width: ${rem(1)};
        border-top: unset;
        border-color: ${theme.color.neutral.border.default};
      }

      ${variant === 'default' &&
      css`
        gap: ${theme.spacing.s12};
      `};

      ${variant === 'pills' &&
      css`
        gap: ${theme.spacing.s4};
      `};
    }

    .mantine-Tabs-tab {
      ${theme.font.body.md.regular};
      opacity: 1; // override Mantine's opacity

      border-top-right-radius: ${theme.radius.r4};
      border-top-left-radius: ${theme.radius.r4};

      [data-position='left']:not(:only-child) {
        --_tab-section-margin-right: ${theme.spacing.s8};
      }

      [data-position='right']:not(:only-child) {
        --_tab-section-margin-left: ${theme.spacing.s8};
      }

      .mantine-Tabs-tabSection {
        &[data-position='left'] {
          margin-right: ${theme.spacing.s8};
        }

        &[data-position='right'] {
          margin-left: ${theme.spacing.s8};
        }
      }

      ${variant === 'default' &&
      css`
        border-width: ${rem(2)};
        height: 44px;
        color: ${theme.color.neutral.fg.muted};
        background-color: transparent;
        margin-bottom: 0;
        border-top: none;
        border-left: none;
        border-right: none;
        padding: ${theme.spacing.s12} 0;

        // override Mantine's active style
        &[data-active='true'] {
          color: ${theme.color.neutral.fg.default};
          border-color: ${theme.color.intent.primary.bg.bold.default};
        }

        // override Mantine's hover style
        &:hover {
          color: ${theme.color.neutral.fg.default};

          &:disabled,
          &:not([data-active]) {
            border-color: transparent;
          }
        }
      `};

      ${variant === 'outline' &&
      css`
        color: ${theme.color.neutral.fg.muted};
        padding: ${theme.spacing.s12};
        margin-bottom: 0;
        height: ${rem(44)};

        // override Mantine's active style
        &[data-active] {
          ${theme.font.body.md.regular};
          color: ${theme.color.neutral.fg.default};
          background-color: ${theme.color.neutral.bg.panel.default};
          border-color: ${theme.color.neutral.border.default};
          border-bottom: none;

          &::before {
            background-color: transparent;
          }
        }

        &:hover {
          &:not(:disabled):not([data-active]) {
            color: ${theme.color.neutral.fg.default};
          }
        }

        &::after {
          content: none;
        }
      `};

      ${variant === 'pills' &&
      css`
        color: ${theme.color.neutral.fg.default};
        padding: ${theme.spacing.s4} ${theme.spacing.s8};
        border: none;

        // override Mantine's active style
        &[data-active] {
          color: ${theme.color.neutral.fg.highlight};
          background-color: ${theme.color.intent.primary.bg.bold.default};
        }

        [data-position='left']:not(:only-child) {
          --_tab-section-margin-right: ${theme.spacing.s4};
        }

        [data-position='right']:not(:only-child) {
          --_tab-section-margin-left: ${theme.spacing.s4};
        }

        &:hover {
          &:not(:disabled):not([data-active]) {
            background-color: ${theme.color.intent.secondary.bg.subtle.hovered};
            color: ${theme.color.neutral.fg.default};
          }
        }

        .mantine-Tabs-tabSection {
          &[data-position='left'] {
            margin-right: ${theme.spacing.s4};
          }
          &[data-position='right'] {
            margin-left: ${theme.spacing.s4};
          }
        }
      `};

      &:focus,
      &:focus-visible {
        outline: none;
      }

      &:disabled {
        color: ${theme.color.state.disabled.fg};
      }
    }
  `
)

/**
 * Fix for Storybook ArgTypes not working with Mantine's props.
 * See https://willow.atlassian.net/l/cp/40rrHNJp
 */
export const ComponentForStorybookArgTypes = forwardRef<
  HTMLDivElement,
  TabsBaseProps
>(() => <div />)
