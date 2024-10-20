import tw, { styled } from 'twin.macro'
import { DatePicker } from '@willow/ui'
import { Icon } from '@willowinc/ui'
import { InsightDetail, Container } from '@willow/common/insights/component'
import { useTranslation } from 'react-i18next'
import { Insight } from '@willow/common/insights/insights/types'
import { useSites } from '../../../../../providers'

export default function Dates({ insight }: { insight: Insight }) {
  const { t } = useTranslation()
  const sites = useSites()
  const timeZone = sites.find((s) => s.id === insight.siteId)?.timeZone

  return (
    <Container>
      <InsightDetail
        headerIcon={<StyledIcon icon="calendar_today" />}
        headerText={t('plainText.dates')}
      >
        <InnerContainer>
          {[
            {
              label: t('labels.createdDate'),
              value: insight.createdDate,
            },
            {
              label: t('labels.updatedDate'),
              value: insight.updatedDate,
            },
          ].map(({ label, value }) => (
            <DatePickerContainer key={label}>
              <DatePicker
                tw="w-[214px]"
                label={label}
                type="date-time"
                value={value}
                timezone={timeZone}
                readOnly
              />
            </DatePickerContainer>
          ))}
        </InnerContainer>
      </InsightDetail>
    </Container>
  )
}

const InnerContainer = styled.div(({ theme }) => ({
  display: 'flex',
  flexDirection: 'column',
  gap: theme.spacing.s16,
}))

const StyledIcon = styled(Icon)(({ theme }) => ({
  '&&&': {
    fontSize: theme.spacing.s24,
  },
}))

const DatePickerContainer = styled.div({
  flexDirection: 'column',
  flex: '1 1 0%',

  '& > div': {
    width: '100%',
  },
})
