import { PageTitle } from './InspectionsProvider'
import getInspectionsPath from './getInspectionsPath'

export const getInspectionsPageTitle = ({
  title,
  siteId,
}: {
  title: string
  siteId?: string
}): PageTitle => ({
  title,
  href: getInspectionsPath(siteId),
})

export const getZonesPageTitle = ({
  title,
  siteId,
}: {
  siteId: string
  title: string
}): PageTitle => ({
  title,
  href: getInspectionsPath(siteId, { pageName: 'zones' }),
})

export const getZonePageTitle = ({
  siteId,
  zoneId,
  zoneName,
}: {
  siteId: string
  zoneId: string
  zoneName: string
}): PageTitle => ({
  title: zoneName,
  href: getInspectionsPath(siteId, {
    pageName: 'zones',
    pageItemId: zoneId,
  }),
})
