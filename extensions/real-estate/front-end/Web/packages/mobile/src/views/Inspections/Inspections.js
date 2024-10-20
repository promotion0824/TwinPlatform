/* eslint-disable react/no-children-prop */ // to allow replacement of `children` to `element` on v6 upgrade
import { Fragment } from 'react'
import { Switch, Route, useParams } from 'react-router'
import { Portal, useFeatureFlag } from '@willow/mobile-ui'
import InspectionsList from './InspectionsList/InspectionsList'
import { useInspectionRecords } from './InspectionChecksList/InspectionRecordsContext'
import WillBeSynced from './WillBeSynced'
import InspectionZonesList from './InspectionZonesList/InspectionZonesList'
import InspectionChecksListPage from './InspectionChecksList/InspectionChecksListPage'
import InspectionChecksListOld from './InspectionChecksList/InspectionChecksListOld'

export default function Inspections() {
  const featureFlags = useFeatureFlag()

  if (!featureFlags.isLoaded) {
    return null
  }

  if (featureFlags.hasFeatureToggle('inspectionsOfflineMode')) {
    return <InspectionsNew />
  } else {
    return <InspectionsOld />
  }
}

export function InspectionsNew() {
  const { siteId } = useParams()
  const inspectionRecords = useInspectionRecords()

  return (
    <Fragment key={siteId}>
      <Switch>
        <Route
          path="/sites/:siteId/inspectionZones"
          exact
          children={<InspectionZonesList />}
        />
        <Route
          path="/sites/:siteId/inspectionZones/:inspectionZoneId/inspections"
          exact
          children={<InspectionsList />}
        />
        <Route
          path="/sites/:siteId/inspectionZones/:inspectionZoneId/inspections/:inspectionId"
          exact
          children={<InspectionChecksListPage />}
        />
      </Switch>
      {inspectionRecords.unsynced.displayMessage && (
        <Portal>
          <WillBeSynced onClick={() => inspectionRecords.dismissSyncLater()} />
        </Portal>
      )}
    </Fragment>
  )
}

function InspectionsOld() {
  return (
    <Switch>
      <Route
        path="/sites/:siteId/inspectionZones"
        exact
        children={<InspectionZonesList />}
      />
      <Route
        path="/sites/:siteId/inspectionZones/:inspectionZoneId/inspections"
        exact
        children={<InspectionsList />}
      />
      <Route
        path="/sites/:siteId/inspectionZones/:inspectionZoneId/inspections/:inspectionId"
        exact
        children={<InspectionChecksListOld />}
      />
    </Switch>
  )
}
