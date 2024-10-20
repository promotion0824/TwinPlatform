import { siteAdminUserRole } from '@willow/common'
import {
  Error,
  MoreButtonDropdown,
  MoreButtonDropdownOption,
  NotFound,
  Pill,
  api,
  useDateTime,
  useScopeSelector,
} from '@willow/ui'
import { DataGrid, GridColDef, Icon, IconName } from '@willowinc/ui'
import { useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useQuery, useQueryClient } from 'react-query'
import { useHistory } from 'react-router'
import { InspectionZone } from '../../../../services/Inspections/InspectionsServices'
import ArchiveZoneModal from '../InspectionModal/ArchiveZoneModal'
import getInspectionsPath from '../getInspectionsPath'
import makeScopedInspectionsPath from '../makeScopedInspectionsPath'

const ZonesDataGrid = ({
  siteId,
  onZoneClick,
  userRole = 'user',
}: {
  siteId?: string
  onZoneClick: (zone: InspectionZone) => void
  userRole?: string
}) => {
  const dateTime = useDateTime()
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const history = useHistory()
  const { isScopeSelectorEnabled, location } = useScopeSelector()
  const queryClient = useQueryClient()
  const [zoneToArchive, setZoneToArchive] = useState<
    InspectionZone | undefined
  >()

  const inspectionZonesQuery = useQuery<InspectionZone[]>(
    ['inspectionZones', siteId],

    async () => {
      const response = await api.get(`/sites/${siteId}/inspectionZones`)
      return response.data
    },
    {
      select: (inspectionZones) =>
        inspectionZones.map((zone) => ({
          ...zone,
          lastUpdated: zone.statistics.lastCheckSubmittedDate
            ? dateTime(zone.statistics.lastCheckSubmittedDate).format(
                'ago',
                undefined,
                language
              )
            : '-',
        })),
    }
  )

  const columns = useMemo<GridColDef[]>(
    () => [
      {
        field: 'name',
        headerName: t('plainText.zone'),
        flex: 1,
      },
      {
        field: 'inspectionCount',
        headerName: t('headers.inspections'),
        width: 120,
        renderCell: ({ row: zone }: { row: InspectionZone }) => (
          <Pill>{zone.inspectionCount}</Pill>
        ),
      },
      {
        field: 'lastUpdated',
        headerName: t('labels.lastUpdated'),
        width: 130,
      },
      ...(userRole === siteAdminUserRole
        ? [
            {
              field: 'actions',
              width: 50,
              headerName: '',
              sortable: false,
              renderCell: ({ row: zone }: { row: InspectionZone }) => (
                <MoreButtonDropdown
                  targetButtonProps={{
                    background: 'transparent',
                  }}
                >
                  {[
                    ['arrow_forward', onZoneClick, 'plainText.viewZone'],
                    ['delete', setZoneToArchive, 'headers.archiveZone'],
                  ].map(
                    ([icon, onClick, text]: [
                      IconName,
                      (zone: InspectionZone) => void,
                      string
                    ]) => (
                      <MoreButtonDropdownOption
                        key={icon}
                        onClick={(e) => {
                          e.preventDefault()
                          e.stopPropagation()
                          onClick(zone)
                        }}
                        prefix={<Icon icon={icon} />}
                      >
                        {t(text)}
                      </MoreButtonDropdownOption>
                    )
                  )}
                </MoreButtonDropdown>
              ),
            },
          ]
        : []),
    ],
    [onZoneClick, t, userRole]
  )

  return inspectionZonesQuery.isError ? (
    <Error>{t('plainText.errorOccurred')}</Error>
  ) : (
    <>
      <DataGrid
        columns={columns}
        rows={inspectionZonesQuery.data ?? []}
        loading={inspectionZonesQuery.isLoading}
        onRowClick={({ row: zone }) => {
          history.push(
            isScopeSelectorEnabled
              ? makeScopedInspectionsPath(location?.twin?.id, {
                  pageName: 'zones',
                  pageItemId: `zone/${zone.id}`,
                })
              : getInspectionsPath(siteId, {
                  pageName: 'zones',
                  pageItemId: zone.id,
                })
          )
        }}
        css={`
          border: none;
        `}
        slots={{
          noRowsOverlay: () => (
            <NotFound>{t('plainText.noZonesFound')}</NotFound>
          ),
        }}
      />
      {zoneToArchive != null && (
        <ArchiveZoneModal
          siteId={siteId}
          zone={zoneToArchive}
          onClose={(response) => {
            setZoneToArchive(undefined)

            if (response === 'submitted') {
              // invalidate query
              queryClient.invalidateQueries(['inspectionZones', siteId])
            }
          }}
        />
      )}
    </>
  )
}

export default ZonesDataGrid
