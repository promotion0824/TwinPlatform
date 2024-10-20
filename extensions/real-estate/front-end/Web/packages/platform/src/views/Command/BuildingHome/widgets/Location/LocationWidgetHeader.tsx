import { titleCase } from '@willow/common'
import { PagedSiteResult, Site } from '@willow/common/site/site/types'
import {
  MoreButtonDropdown,
  MoreButtonDropdownOption,
  useScopeSelector,
} from '@willow/ui'
import { Group, Icon, Stack } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import { useHistory } from 'react-router'
import styled from 'styled-components'
import routes from '../../../../../routes'
import WeatherBadge from '../../../../Portfolio/LocationCard/WeatherBadge'

const BackgroundImage = styled.div<{
  $imageUrl: string
  $isPlaceholder?: boolean
}>(({ $imageUrl, $isPlaceholder = false, theme }) => ({
  backgroundColor: theme.color.neutral.bg.panel.default,
  backgroundImage: `url(${$imageUrl})`,
  backgroundPosition: 'center',
  backgroundRepeat: 'no-repeat',
  backgroundSize: $isPlaceholder ? '200px auto' : 'cover',
  borderRadius: `${theme.radius.r4} ${theme.radius.r4} 0 0`,
  height: '200px',
  minWidth: '200px',
}))

const BuildingType = styled.div(({ theme }) => ({
  ...theme.font.body.lg.semibold,
  color: theme.color.neutral.fg.default,
  display: 'flex',
  gap: theme.spacing.s4,
}))

const Container = styled(Stack)(({ theme }) => ({
  borderBottom: `1px solid ${theme.color.neutral.border.default}`,
}))

const Gradient = styled.div(({ theme }) => ({
  background:
    'linear-gradient(180deg, rgba(0, 0, 0, 0.80) 0%, rgba(0, 0, 0, 0.00) 25%, rgba(0, 0, 0, 0.00) 75%, rgba(0, 0, 0, 0.80) 100%)',
  borderRadius: theme.radius.r4,
  height: '100%',
}))

function LocationWidgetHeaderOverlay({
  isEditingMode,
  onEditClick,
  site,
}: {
  isEditingMode: boolean
  onEditClick: () => void
  site: Site | PagedSiteResult
}) {
  const history = useHistory()
  const { location } = useScopeSelector()

  const {
    i18n: { language },
    t,
  } = useTranslation()

  const goToTwin = () => {
    history.push(routes.portfolio_twins_view__twinId(location?.twin.id))
  }

  return (
    <Container
      h="100%"
      justify="space-between"
      pt="s12"
      pb="s12"
      pl="s16"
      pr="s16"
    >
      <Group justify="space-between" w="100%">
        <BuildingType>
          <Icon icon="apartment" />
          <div>{site.type}</div>
        </BuildingType>

        {!isEditingMode && (
          <MoreButtonDropdown targetButtonProps={{ background: 'transparent' }}>
            <MoreButtonDropdownOption
              onClick={goToTwin}
              prefix={<Icon icon="open_in_new" />}
            >
              {titleCase({ language, text: t('plainText.goToTwin') })}
            </MoreButtonDropdownOption>
            <MoreButtonDropdownOption
              onClick={onEditClick}
              prefix={<Icon icon="edit" />}
            >
              {titleCase({ language, text: t('plainText.edit') })}
            </MoreButtonDropdownOption>
          </MoreButtonDropdown>
        )}
      </Group>

      {site.weather && <WeatherBadge siteWeather={site.weather} />}
    </Container>
  )
}

export default function LocationWidgetHeader({
  isEditingMode,
  onEditClick,
  site,
}: {
  isEditingMode: boolean
  onEditClick: () => void
  site: Site | PagedSiteResult
}) {
  return site.logoUrl ? (
    <BackgroundImage $imageUrl={site.logoUrl}>
      <Gradient>
        <LocationWidgetHeaderOverlay
          isEditingMode={isEditingMode}
          onEditClick={onEditClick}
          site={site}
        />
      </Gradient>
    </BackgroundImage>
  ) : (
    <BackgroundImage
      $imageUrl="/public/location-card-placeholder.png"
      $isPlaceholder
    >
      <LocationWidgetHeaderOverlay
        isEditingMode={isEditingMode}
        onEditClick={onEditClick}
        site={site}
      />
    </BackgroundImage>
  )
}
