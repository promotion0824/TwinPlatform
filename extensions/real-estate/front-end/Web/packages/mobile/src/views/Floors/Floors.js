import { Fragment } from 'react'
import { Switch, Route, useParams } from 'react-router'
/* eslint-disable react/no-children-prop */ // to allow replacement of `children` to `element` on v6 upgrade
import { useAnimationTransition } from 'providers'
import SitesSelect from 'components/SitesSelect/SitesSelect'
import { LayoutHeader } from 'views/Layout/Layout'
import Floor from './Floor'
import AssetDetails from './AssetDetails'
import styles from './Floors.css'

export default function Floors() {
  const { siteId } = useParams()
  const { isExiting } = useAnimationTransition()

  return (
    <Fragment key={siteId}>
      {!isExiting && (
        <LayoutHeader className={styles.headerRoot} type="content" width="100%">
          <div className={styles.header}>
            <div className={styles.siteWrap}>
              <SitesSelect to={(site) => `/sites/${site.id}/floors`} />
            </div>
          </div>
        </LayoutHeader>
      )}
      <Switch>
        <Route path="/sites/:siteId/floors" exact children={<Floor />} />
        <Route
          path="/sites/:siteId/floors/asset/:assetId"
          exact
          children={<AssetDetails />}
        />
      </Switch>
    </Fragment>
  )
}
