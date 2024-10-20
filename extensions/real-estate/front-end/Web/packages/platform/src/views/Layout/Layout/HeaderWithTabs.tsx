import { styled, css } from 'twin.macro'
import { Portal } from '@willow/ui'

import { useLayout } from './LayoutContext'

const SectionsContainer = styled.div<{ $hasTabs: boolean }>(
  ({ theme, $hasTabs }) =>
    css`
      width: 100%;
      height: fit-content;
      background-color: ${theme.color.neutral.bg.base.default};
      padding-left: ${theme.spacing.s16};
      padding-right: ${theme.spacing.s16};
      padding-top: ${theme.spacing.s16};
      display: flex;
      align-items: center;
      justify-content: space-between;
      flex-direction: column;

      /* same as Tabs underline border */
      ${$hasTabs &&
      css`
        border-bottom: 1px solid ${theme.color.neutral.border.default};
      `}
    `
)

const RowContainer = styled.div(
  ({ theme }) => css`
    width: 100%;
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: ${theme.spacing.s8};
  `
)

const TabsContainer = styled.div`
  margin-bottom: -1px; // so that the borders on container will be connected to Tabs.List border
  flex-shrink: 0; // not shrink the tabs, will wait for wrapped tabs design
`

const ControlsOnTabsContainer = styled.div`
  flex-grow: 1;
  align-items: center;
  display: flex;
  justify-content: end;
`

/**
 * The header section that accepts list of Tabs.Tab as left section,
 * and customisable content as right section that will be placed at the end of
 * the tabs.
 * Renders inside the headerPanelRef of the LayoutContext using a Portal.
 */
export default function HeaderWithTabs({
  tabs,
  controlsOnTabs,
  titleRow,
  className,
  ...rest
}: {
  titleRow?: React.ReactNode
  tabs?: React.ReactNode
  controlsOnTabs?: React.ReactNode
  className?: string
}) {
  const layout = useLayout()

  if (!layout?.headerPanelRef) {
    return null
  }

  return (
    <Portal target={layout?.headerPanelRef}>
      <SectionsContainer
        className={className}
        {...rest}
        $hasTabs={Boolean(tabs)}
      >
        <RowContainer>{titleRow}</RowContainer>
        {(tabs || controlsOnTabs) && (
          <RowContainer>
            {tabs && <TabsContainer>{tabs}</TabsContainer>}
            {controlsOnTabs && (
              <ControlsOnTabsContainer
                css={{
                  // when tabs are not provided
                  // the height is tabs normal height 44px - 1px border
                  minHeight: 43,
                }}
              >
                {controlsOnTabs}
              </ControlsOnTabsContainer>
            )}
          </RowContainer>
        )}
      </SectionsContainer>
    </Portal>
  )
}
