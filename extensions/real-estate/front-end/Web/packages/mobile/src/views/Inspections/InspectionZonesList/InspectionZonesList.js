import { useParams } from 'react-router'
import { Fetch, Spacing, NotFound } from '@willow/mobile-ui'
import { Tab as TicketTab } from '@willow/common/ticketStatus'
import List from 'components/List/List'
import SitesSelect from 'components/SitesSelect/SitesSelect'
import { LayoutHeader } from 'views/Layout/Layout'
import { useLayout, useAnimationTransition, useInspections } from 'providers'
import InspectionZone from './InspectionZone'
import styles from '../Inspections.css'

export default function InspectionZonesList() {
  const params = useParams()
  const { setShowBackButton } = useLayout()
  const { isExiting } = useAnimationTransition()
  const { setInspectionZones } = useInspections()

  setShowBackButton(false)

  const handleResponse = (response) => {
    setInspectionZones(response)
  }

  return (
    <>
      {!isExiting && (
        <LayoutHeader className={styles.headerRoot} type="content" width="100%">
          <div className={styles.header}>
            <div className={styles.siteWrap}>
              <SitesSelect
                to={(site) => {
                  const { isInspectionEnabled } = site.features
                  return isInspectionEnabled
                    ? `/sites/${site.id}/inspectionZones`
                    : `/tickets/sites/${site.id}/${TicketTab.open}`
                }}
              />
            </div>
          </div>
        </LayoutHeader>
      )}
      <Fetch
        url={`/api/sites/${params.siteId}/inspectionZones`}
        cache
        onResponse={handleResponse}
      >
        {(zones) => (
          <Spacing type="content">
            {zones.length > 0 && (
              <List
                stretchColumn
                activeIndex={-1}
                data={zones.sort((a, b) => {
                  if (a.name.toLowerCase() < b.name.toLowerCase()) {
                    return -1
                  }
                  if (a.name.toLowerCase() > b.name.toLowerCase()) {
                    return 1
                  }
                  return 0
                })}
                ListItem={InspectionZone}
              />
            )}
            {zones.length === 0 && (
              <Spacing>
                <NotFound>No inspection zones found</NotFound>
              </Spacing>
            )}
          </Spacing>
        )}
      </Fetch>
    </>
  )
}
