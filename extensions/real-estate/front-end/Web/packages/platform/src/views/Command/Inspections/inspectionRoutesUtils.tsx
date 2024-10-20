import { TFunction } from 'i18next'
import { Redirect } from 'react-router'
import { UseQueryResult } from 'react-query'

import routes from '../../../routes'
import InspectionHistory from './InspectionHistory/InspectionHistory'
import ScopedInspectionHistory from './InspectionHistory/ScopedInspectionHistory'
import InspectionResults from './Inspections/Inspections'
import ScopedInspections from './Inspections/ScopedInspections'
import Usage from './Usage/Usage'
import Zone from './Zone/Zone'
import ScopedZones from './Zones/ScopedZones'
import Zones from './Zones/Zones'
import ScopedZone from './Zone/ScopedZone'
import makeScopedInspectionsPath from './makeScopedInspectionsPath'
import { Inspection } from '../../../services/Inspections/InspectionsServices'
import ScopedUsage from './Usage/ScopedUsage'

/**
 * This array is used to generate the routes for the inspections page
 * incorporating siteId.
 *
 * TODO: once scope selector feature is complete, remove this array.
 */
export const legacySiteInspectionRoutes = [
  {
    path: routes.inspections,
    routeOptions: {
      exact: true,
    },
    child: <InspectionResults />,
    controls: {
      showTabs: true,
    },
  },
  {
    path: routes.inspections__inspectionId__checkId(),
    routeOptions: {
      exact: true,
    },
    child: <InspectionHistory />,
    controls: {
      showTimePicker: true,
    },
  },
  {
    path: routes.sites__siteId_inspections(),
    routeOptions: {
      exact: true,
    },
    child: <InspectionResults />,
    controls: {
      showTabs: true,
    },
  },
  {
    path: routes.sites__siteId_inspections_usage(),
    routeOptions: {
      exact: true,
    },
    child: <Usage />,
    controls: {
      showTabs: true,
    },
  },
  {
    path: routes.sites__siteId_inspections_zones(),
    routeOptions: {
      exact: true,
    },
    child: <Zones />,
    controls: {
      showTabs: true,
      showAddZoneButton: true,
    },
  },
  {
    path: routes.sites__siteId_inspections_zones__zoneId(),
    routeOptions: {
      exact: true,
    },
    child: <Zone />,
    controls: {
      showAddInspectionButton: true,
    },
  },
  {
    path: routes.sites__siteId_inspections__inspectionId__checkId(),
    routeOptions: {
      exact: true,
    },
    child: <InspectionHistory />,
    controls: {
      showTimePicker: true,
    },
  },
]

/**
 * this utility function is used to generate the routes for the inspections page
 * incorporating scopeId, and generate routes to redirect users from legacy routes with siteId
 * to new routes with scopeId.
 */
export const makeScopedInspectionRoutes = ({
  t,
  scopeId,
  inspectionQuery,
  checkId,
  inspectionId,
  siteIdForBuildingScope,
  userRole,
  zoneId,
  zoneInspectionsQuery,
}: {
  t: TFunction
  scopeId?: string
  inspectionQuery: UseQueryResult<Inspection>
  checkId?: string
  inspectionId?: string
  siteIdForBuildingScope?: string
  userRole?: string
  zoneId?: string
  zoneInspectionsQuery: UseQueryResult<{
    id: string
    name?: string
    siteId?: string
    inspections?: Array<Inspection>
  }>
}) => [
  {
    path: routes.inspections,
    routeOptions: {
      exact: true,
    },
    child: <ScopedInspections />,
    controls: {
      showTabs: true,
      pageTitles: [
        {
          href: makeScopedInspectionsPath(),
          title: t('headers.inspections'),
        },
      ],
    },
  },
  {
    path: routes.inspections_scope__scopeId(),
    routeOptions: {
      exact: true,
    },
    child: <ScopedInspections scopeId={scopeId} />,
    controls: {
      showTabs: true,
      pageTitles: [
        {
          href: makeScopedInspectionsPath(scopeId),
          title: t('headers.inspections'),
        },
      ],
    },
  },
  {
    path: [
      routes.inspections_inspection__inspectionId_check__checkId(),
      routes.inspections_scope__scopeId_inspection__inspectionId_check__checkId(),
    ],
    routeOptions: {
      exact: true,
    },
    child: <ScopedInspectionHistory inspectionQuery={inspectionQuery} />,
    controls: {
      showTimePicker: true,
      pageTitles: [
        {
          href: makeScopedInspectionsPath(scopeId),
          title: t('headers.inspections'),
        },
        {
          title: inspectionQuery?.data?.name,
        },
      ],
    },
  },
  {
    path: routes.sites__siteId_inspections(),
    routeOptions: {
      exact: true,
    },
    child: <Redirect to={makeScopedInspectionsPath(scopeId)} />,
  },
  {
    path: [
      routes.inspections__inspectionId__checkId(),
      routes.sites__siteId_inspections__inspectionId__checkId(),
    ],
    routeOptions: {
      exact: true,
    },
    child: (
      <Redirect
        to={makeScopedInspectionsPath(scopeId, {
          inspectionId: `inspection/${inspectionId}`,
          pageName: 'check',
          pageItemId: checkId,
        })}
      />
    ),
  },
  {
    path: routes.inspections_scope__scopeId_zones(),
    routeOptions: {
      exact: true,
    },
    child: <ScopedZones siteId={siteIdForBuildingScope} userRole={userRole} />,
    controls: {
      showTabs: true,
      showAddZoneButton: true,
      pageTitles: [
        {
          title: t('headers.inspections'),
          href: makeScopedInspectionsPath(scopeId),
        },
      ],
    },
  },
  {
    path: routes.sites__siteId_inspections_zones(),
    routeOptions: {
      exact: true,
    },
    child: (
      <Redirect
        to={makeScopedInspectionsPath(scopeId, {
          pageName: 'zones',
        })}
      />
    ),
  },
  {
    path: routes.inspections_scope__scopeId_zones_zone__zoneId(),
    routeOptions: {
      exact: true,
    },
    child: (
      <ScopedZone zoneInspectionsQuery={zoneInspectionsQuery} zoneId={zoneId} />
    ),
    controls: {
      showAddInspectionButton: !zoneInspectionsQuery.isFetching,
      pageTitles: [
        {
          title: t('headers.inspections'),
          href: makeScopedInspectionsPath(scopeId),
        },
        {
          title: t('headers.zones'),
          href: makeScopedInspectionsPath(scopeId, {
            pageName: 'zones',
          }),
        },
        {
          title: zoneInspectionsQuery?.data?.name,
        },
      ],
    },
  },
  {
    path: routes.sites__siteId_inspections_zones__zoneId(),
    routeOptions: {
      exact: true,
    },
    child: (
      <Redirect
        to={makeScopedInspectionsPath(scopeId, {
          pageName: 'zones',
          pageItemId: `zone/${zoneId}`,
        })}
      />
    ),
  },
  {
    path: routes.inspections_scope__scopeId_usage(),
    routeOptions: {
      exact: true,
    },
    child: <ScopedUsage siteId={siteIdForBuildingScope} />,
    controls: {
      showTabs: true,
      pageTitles: [
        {
          title: t('headers.inspections'),
          href: makeScopedInspectionsPath(scopeId),
        },
      ],
    },
  },
  {
    path: routes.sites__siteId_inspections_usage(),
    routeOptions: {
      exact: true,
    },
    child: (
      <Redirect
        to={makeScopedInspectionsPath(scopeId, {
          pageName: 'usage',
        })}
      />
    ),
  },
]
