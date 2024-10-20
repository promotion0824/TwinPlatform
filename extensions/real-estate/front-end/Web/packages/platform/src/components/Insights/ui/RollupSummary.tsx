import styled from 'styled-components'
import { ReactNode } from 'react'
import { TextWithTooltip } from '@willow/common/insights/component'
import _ from 'lodash'
import { useTranslation } from 'react-i18next'

/**
 * A horizontal banner above insight table containing
 * some cards to show summarized information of the table
 *
 * reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/76233
 */
export default function RollupSummary({
  isLoading,
  rollupData,
}: {
  isLoading: boolean
  rollupData: Array<{
    header: string
    value?: string
    isVisible?: boolean
    children?: ReactNode
    tooltipText?: string
    tooltipWidth?: string
  }>
}) {
  return (
    <RollupSummaryContainer data-testid="rollupSummary">
      <CardsContainer>
        {rollupData.map(
          ({ value, header, isVisible, children, tooltipText, tooltipWidth }) =>
            isVisible ? (
              <CardContainer key={header}>
                <Card
                  data-tooltip={tooltipText ?? undefined}
                  data-tooltip-position="top"
                  data-tooltip-width={tooltipWidth ?? undefined}
                  data-tooltip-time={500}
                >
                  <CardHeader>{header}</CardHeader>
                  {isLoading ? null : (
                    <>
                      {value && <StyledText text={value} isTitleCase={false} />}
                      {children}
                    </>
                  )}
                </Card>
              </CardContainer>
            ) : null
        )}
      </CardsContainer>
      <ImpactMetricsDisclaimer />
    </RollupSummaryContainer>
  )
}

/**
 * This component returns the disclaimer text used in Impact metrics section
 */
export const ImpactMetricsDisclaimer = () => {
  const { t } = useTranslation()
  return (
    <FormattedText>
      {`* ${_.upperFirst(t('plainText.disclaimerImpactMetrics'))}`}
    </FormattedText>
  )
}

const FlexColumn = styled.div({
  display: 'flex',
  flexDirection: 'column',
})

const CardsContainer = styled.div({
  display: 'flex',
  gap: '36px',
  height: '110px',
  flexWrap: 'wrap',
})

const CardContainer = styled(FlexColumn)({
  padding: '12px 0',
})

const Card = styled(FlexColumn)({
  height: '85px',
  borderRadius: '8px',
  padding: '16.5px 32px',
  background: '#2B2B2B',
  gap: '2px',
  whiteSpace: 'nowrap',
  overflow: 'hidden',
  maxWidth: '250px',

  '> a': {
    margin: '0 auto',
    height: 'auto',
  },
})

const CardHeader = styled(FlexColumn)({
  color: '#C6C6C6',
  font: '600 12px/20px Poppins',
  height: '20px',
  textAlign: 'center',
})

const StyledText = styled(TextWithTooltip)(({ theme }) => ({
  color: theme.color.neutral.fg.default,
  ...theme.font.display.sm.light,
  width: '100%',
  textAlign: 'center',
}))

const FormattedText = styled.div({
  fontStyle: 'italic',
  fontSize: '10px',
  color: 'var(--lighter)',
})

const RollupSummaryContainer = styled.div({
  padding: '0 17px 10px',
})
