import { titleCase } from '@willow/common'
import { getContainmentHelper, invariant, useScopeSelector } from '@willow/ui'
import { Group, Stack } from '@willowinc/ui'
import { forwardRef } from 'react'
import { useTranslation } from 'react-i18next'
import { useHistory } from 'react-router'
import styled from 'styled-components'
import CountsTile from '../../../../../components/LocationHome/CountsTile/CountsTile'
import { useSite } from '../../../../../providers'
import routes from '../../../../../routes'
import { WidgetId } from '../../../../../store/buildingHomeSlice'
import { DraggableContent } from '../../DraggableColumnLayout/types'
import BuildingHomeWidgetCard from '../BuildingHomeWidgetCard'
import DailyNewTicketsChartTile from './DailyNewTicketsChartTile'
import TopActiveTicketCategoriesChartTile from './TopActiveTicketCategoriesChartTile'

const { ContainmentWrapper, getContainerQuery } = getContainmentHelper()

const Container = styled.div(({ theme }) => {
  const containerQuery = getContainerQuery(
    `max-width: ${theme.breakpoints.mobile}`
  )

  return {
    '.hide-on-mobile': { display: 'inherit' },
    '.show-on-mobile': { display: 'none' },

    [containerQuery]: {
      '.hide-on-mobile': { display: 'none' },
      '.show-on-mobile': { display: 'inherit' },
    },
  }
})

const TicketsWidget: DraggableContent<WidgetId> = forwardRef(
  ({ canDrag, id = WidgetId.Tickets, ...props }, ref) => {
    const history = useHistory()
    const { scopeId } = useScopeSelector()
    const site = useSite()
    const {
      i18n: { language },
      t,
    } = useTranslation()

    invariant(
      site.ticketStatsByStatus,
      'No ticket stats were returned for this site.'
    )

    const countsTile = (
      <CountsTile
        breakpoint={0}
        data={[
          {
            icon: { name: 'release_alert' },
            intent: 'negative',
            label: titleCase({
              language,
              text: t('plainText.overdue'),
            }),
            onClick: () =>
              // TODO: Add parameter(s) once additional ticket filtering has been added.
              history.push(`${routes.tickets_scope__scopeId(scopeId)}`),
            value: site.ticketStats.overdueCount,
          },
          {
            icon: { filled: false, name: 'circle' },
            intent: 'secondary',
            label: titleCase({ language, text: t('plainText.open') }),
            onClick: () =>
              history.push(
                `${routes.tickets_scope__scopeId(scopeId)}?tab=Open`
              ),
            value: site.ticketStatsByStatus.openCount,
          },
        ]}
      />
    )

    return (
      <BuildingHomeWidgetCard
        {...props}
        id={id}
        isDraggingMode={canDrag}
        navigationButtonContent={t('interpolation.goTo', {
          value: titleCase({ language, text: t('headers.tickets') }),
        })}
        navigationButtonLink={routes.tickets_scope__scopeId(scopeId)}
        ref={ref}
        title={titleCase({ language, text: t('headers.tickets') })}
      >
        <ContainmentWrapper style={{ containerType: 'inline-size' }}>
          <Container>
            <Group align="flex-start" gap="s12" wrap="nowrap">
              <Stack gap="s12" w="100%">
                {countsTile}

                <div className="show-on-mobile">
                  <TopActiveTicketCategoriesChartTile />
                </div>

                <DailyNewTicketsChartTile />
              </Stack>

              <Stack className="hide-on-mobile" w="100%">
                <TopActiveTicketCategoriesChartTile />
              </Stack>
            </Group>
          </Container>
        </ContainmentWrapper>
      </BuildingHomeWidgetCard>
    )
  }
)

export default TicketsWidget
