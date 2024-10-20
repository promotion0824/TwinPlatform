import { useState } from 'react'
import { Flex, Text } from '@willow/ui'
import { NavList } from '@willowinc/ui'
import _ from 'lodash'
import { styled } from 'twin.macro'
import { useTranslation } from 'react-i18next'
import { Widget } from '../../../services/Widgets/WidgetsService'

const ReportsFilter = ({
  report,
}: {
  report: {
    selectedReport?: Widget
    data: {
      widgets?: Widget[]
    }
    isLoading: boolean
    isError: boolean
    isSuccess: boolean
    handleReportChange: (nextReport: Widget) => void
  }
}) => {
  const { isSuccess, data, selectedReport, handleReportChange } = report
  const { t } = useTranslation()

  return (
    <Flex fill="content">
      {isSuccess && data?.widgets && data.widgets.length > 0 ? (
        <StyledListWrapper>
          {_.sortBy(data.widgets, (widget) => widget.metadata.name).map(
            (widget) => (
              <NavList.Item
                active={selectedReport?.id === widget.id}
                key={widget.id}
                label={<FormattedText>{widget?.metadata?.name}</FormattedText>}
                onClick={() => handleReportChange(widget)}
              />
            )
          )}
        </StyledListWrapper>
      ) : (
        <NoReports>{t('plainText.noReportsAvailable')}</NoReports>
      )}
    </Flex>
  )
}

export default ReportsFilter

const NoReports = styled.div(({ theme }) => ({
  fontWeight: '600',
  lineHeight: '20px',
  color: theme.color.neutral.fg.subtle,
  padding: '32px 16px',
}))

const FormattedText = styled(Text)(({ theme }) => ({
  ...theme.font.body.md.regular,
  color: theme.color.neutral.fg.default,
  whiteSpace: 'nowrap',
  display: 'block',
}))

const StyledListWrapper = styled.div(({ theme }) => ({
  display: 'flex',
  flexDirection: 'column',
  padding: `${theme.spacing.s16} 0`,
}))
