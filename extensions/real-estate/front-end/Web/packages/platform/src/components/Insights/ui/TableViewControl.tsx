import _ from 'lodash'
import styled from 'styled-components'
import { useTranslation } from 'react-i18next'
import { RadioButton, Checkbox, useUser } from '@willow/ui'
import { titleCase } from '@willow/common'
import { InsightMetric } from '@willow/common/insights/costImpacts/types'
import { forwardRef } from 'react'
import {
  Analytics,
  EventBody,
  InsightTableControls,
} from '@willow/common/insights/insights/types'
import { Icon, Popover } from '@willowinc/ui'

/**
 * A widget to be shown when user click on the "cog"
 * button on insights table; it contains radio buttons
 * to switch between different impact views (cost / energy)
 * and checkboxes to show/hide different summary information
 *
 * reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/76233
 */
export default forwardRef<
  HTMLDivElement,
  {
    rollupControls: Array<{ text: string; control: InsightTableControls }>
    selectedImpactView: string
    onChange: (obj: { [key: string]: string | string[] }) => void
    isCardView?: boolean
    controls?: {
      [key: string]: string | string[]
    }
    showTotalImpact: boolean
    onShowTotalImpact: () => void
    analytics?: Analytics
    eventBody: EventBody
    nextIncludedRollups: InsightTableControls[]
    setNextIncludedRollups?: (
      nextIncludedRollups: InsightTableControls[]
    ) => void
  }
>(function TableViewControl(
  {
    controls,
    rollupControls,
    selectedImpactView,
    onChange,
    onShowTotalImpact,
    showTotalImpact,
    analytics,
    eventBody,
    nextIncludedRollups,
    setNextIncludedRollups,
    isCardView = false,
  },
  ref
) {
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const user = useUser()

  const handleImpactViewChange = (metric: string) => {
    onChange({ impactView: metric })
    user.saveOptions('insightsImpactView', metric)
    setNextIncludedRollups?.(nextIncludedRollups)
    analytics?.track('insightTabViewControls', {
      ...eventBody,
      includedRollups: nextIncludedRollups,
      impactView: metric,
    })
  }

  /**
   * business logic to hide total impact to date rollup by default
   * so we accept a state from the parent component to control its visibility
   * reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/80387
   */
  const handleViewOptionClick = (
    isVisibilityControlled: boolean,
    control: InsightTableControls
  ) => {
    setNextIncludedRollups?.(_.xor(nextIncludedRollups, [control]))
    analytics?.track('insightTabViewControls', {
      ...eventBody,
      includedRollups: _.xor(nextIncludedRollups, [control]),
      impactView: controls?.impactView ?? 'cost',
    })

    if (isVisibilityControlled) {
      onShowTotalImpact()
    } else {
      onChange({
        excludedRollups: _.xor(controls?.excludedRollups, [control]),
      })
    }
  }

  return (
    <Popover>
      <Popover.Target>
        <IconContainer data-testid="insightTableViewControls">
          <StyledIcon icon="settings" />
        </IconContainer>
      </Popover.Target>
      <Popover.Dropdown>
        <TableViewControlContainer
          ref={ref}
          data-testid="tableViewControlWidget"
        >
          <ControlHeader>
            {t('interpolation.typeView', { type: t('plainText.impact') })}
          </ControlHeader>
          {Object.keys(InsightMetric).map((metric) => {
            const isSelected = metric === selectedImpactView
            return (
              <ViewOption
                key={metric}
                $isSelected={isSelected}
                onClick={() => handleImpactViewChange(metric)}
              >
                <RadioButton
                  value={metric}
                  checked={isSelected}
                  onChange={(e) => {
                    e.stopPropagation()
                  }}
                />
                {_.capitalize(t(`plainText.${metric}`))}
              </ViewOption>
            )
          })}

          {rollupControls?.length > 0 && (
            <ControlHeader>{t('plainText.rollupWidgets')}</ControlHeader>
          )}
          {rollupControls.map(({ text, control }) => {
            const isExcluded =
              controls?.excludedRollups?.includes(control) ?? false
            // Checking if it's a card view because we don't want to hide the total impact to date by default
            const isVisibilityControlled = isCardView
              ? false
              : control === InsightTableControls.showTotalImpactToDate ||
                control === InsightTableControls.showSavingsToDate
            const isViewOptionChecked = isVisibilityControlled
              ? showTotalImpact
              : !isExcluded

            return (
              <ViewOption
                key={text}
                $isSelected={isViewOptionChecked}
                onClick={() => {
                  handleViewOptionClick(isVisibilityControlled, control)
                }}
              >
                <Checkbox value={isViewOptionChecked} />
                {titleCase({ text, language })}
              </ViewOption>
            )
          })}
        </TableViewControlContainer>
      </Popover.Dropdown>
    </Popover>
  )
})

const IconContainer = styled.div({
  cursor: 'pointer',
  height: '100%',
  display: 'flex',
  flexDirection: 'column',
  justifyContent: 'center',
  '&:hover > svg': {
    fill: 'currentColor',
  },
  '& > svg': {
    height: '28px',
    width: '28px',
    fill: '#7E7E7E',
  },
})

const StyledIcon = styled(Icon)(({ theme }) => ({
  cursor: 'pointer',

  '&&&': {
    color: theme.color.neutral.fg.default,

    '&:hover': {
      color: theme.color.neutral.fg.highlight,
    },
  },
}))

const ControlHeader = styled.div({
  font: '700 10px/16px Poppins',
  color: '#A4A5A6',
  textTransform: 'uppercase',
  height: '28px',
  lineHeight: '28px',
  padding: '0 8px',
})

const ViewOption = styled.div<{ $isSelected: boolean }>(({ $isSelected }) => ({
  height: '32px',
  cursor: 'pointer',
  font: '500 12px/20px Poppins',
  color: $isSelected ? '#D9D9D9' : '#A4A5A6',
  backgroundColor: $isSelected ? '#2B2B2B' : '#252525',
  display: 'flex',
  alignItems: 'center',
  '& > input[type="radio"]': {
    margin: '0 8px',
  },
  '&:hover': {
    color: '#D9D9D9',
  },
}))

const TableViewControlContainer = styled.div(({ theme }) => ({
  width: '242px',
  backgroundColor: '#252525',
  boxShadow: '0px 10px 10px 0px #00000033',
  border: `1px solid ${theme.color.neutral.border.default}`,
  position: 'absolute',
  right: '-15px',
  zIndex: theme.zIndex.popover,
  display: 'flex',
  flexDirection: 'column',
  '& > *': {
    width: '100%',
  },
}))
