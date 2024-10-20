import _ from 'lodash'
import { useAnalytics, Select, Option } from '@willow/mobile-ui'
import { useLayout } from 'providers'
import styles from './SitesSelect.css'

export default function SitesSelect({ to }) {
  const analytics = useAnalytics()
  const layout = useLayout()

  function handleSiteClick(site) {
    layout.selectSite(site)

    analytics.track('Building Switched')
  }

  const sortedSites =
    layout.sites?.sort((a, b) => {
      if (a.name.toLowerCase() < b.name.toLowerCase()) {
        return -1
      }
      if (a.name.toLowerCase() > b.name.toLowerCase()) {
        return 1
      }
      return 0
    }) || []

  return (
    <Select
      value={layout.site}
      header={(site) => site.name}
      labelClassName={styles.siteSelectLabel}
      iconClassName={styles.sitesIcon}
      className={styles.sitesSelect}
    >
      {sortedSites.map((site) => {
        let nextTo = _.isFunction(to) ? to(site) : to
        if (to == null) {
          nextTo = layout.getSiteUrl(site.id)
        }

        return (
          <Option
            key={site.id}
            value={site}
            to={nextTo}
            onClick={() => handleSiteClick(site)}
          >
            {site.name}
          </Option>
        )
      })}
    </Select>
  )
}
