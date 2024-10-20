import { VanillaButton, useAnalytics } from '@willow/ui'
import { Icon, Loader } from '@willowinc/ui'
import { useSites } from 'providers'
import styled, { css } from 'styled-components'
import { useTimeSeries } from '../TimeSeriesContext'

export default function Point({ point, asset }) {
  const analytics = useAnalytics()
  const sites = useSites()
  const timeSeries = useTimeSeries()
  const sitePointId = `${asset.siteId}_${point.entityId}`

  const timeSeriesPoint = timeSeries.points.find(
    (prevPoint) => prevPoint.sitePointId === sitePointId
  )

  const isSelected = timeSeriesPoint != null
  const isLoading = timeSeries.loadingSitePointIds.some(
    (prevPoint) => prevPoint === sitePointId
  )

  const handleClick = () => {
    const site = sites.find((layoutSite) => layoutSite.id === point.siteId)
    analytics.track(isSelected ? 'Point Deselected' : 'Point Selected', {
      name: point.name,
      Site: site,
      item_name: asset.data.name,
    })
    timeSeries.toggleSitePointId(sitePointId)
  }

  return (
    <PointerButton
      key={point.id}
      onClick={handleClick}
      data-tooltip={point.externalPointId}
      data-tooltip-position="bottom"
    >
      <ContentContainer>
        <Icon
          icon={isSelected ? 'visibility' : 'visibility_off'}
          css={{ color: !isSelected ? 'dark' : timeSeriesPoint?.color }}
        />
        <span
          css={css(({ theme }) => ({
            ...theme.font.body.md.regular,
            color: isSelected
              ? theme.color.neutral.fg.default
              : theme.color.neutral.fg.subtle,
            textAlign: 'start',
          }))}
        >
          {point.name}
        </span>
      </ContentContainer>

      {isLoading && <Loader intent="secondary" />}
    </PointerButton>
  )
}

const PointerButton = styled(VanillaButton)(
  ({ theme }) => css`
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: ${theme.spacing.s8};
    padding-left: ${theme.spacing.s32};
  `
)

const ContentContainer = styled.div(
  ({ theme }) => css`
    display: flex;
    align-items: center;
    gap: ${theme.spacing.s8};
  `
)
