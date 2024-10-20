import { useParams } from 'react-router'
import {
  BackButton,
  Fetch,
  Flex,
  NotFound,
  Panel,
  PowerBIReport,
  Text,
} from '@willow/ui'
import { useTranslation } from 'react-i18next'
import LayoutHeaderPanel from 'views/Layout/Layout/LayoutHeaderPanel'
import FloorSelector from './FloorSelector/FloorSelector'
import styles from './OccupancyFloor.css'

export default function OccupancyFloor() {
  const params = useParams()
  const { t } = useTranslation()

  return (
    <Fetch url="/api/pilot/occupancy?hasBaseModule=true">
      {(floors) => {
        const floor = floors.find(
          (nextFloor) => nextFloor.floorId === params.floorId
        )

        return (
          <>
            <LayoutHeaderPanel>
              <BackButton to={`/sites/${params.siteId}/occupancy`} />
              {floor != null && (
                <Flex horizontal align="middle" padding="0 large">
                  <Text type="h2">{floor.floorName}</Text>
                </Flex>
              )}
            </LayoutHeaderPanel>
            <Flex horizontal fill="content" size="small">
              <FloorSelector
                floors={floors.map((nextFloor) => ({
                  ...nextFloor,
                  id: nextFloor.floorId,
                  name: nextFloor.floorName,
                  code: nextFloor.floorCode,
                  people: Math.max(nextFloor.runningTotal ?? 0, 0),
                  peopleLimit: nextFloor.floorLimit,
                }))}
              />
              <Flex
                key={floor?.floorId}
                horizontal
                size="small"
                padding="small 0"
              >
                {floor != null ? (
                  <Panel fill="header" className={styles.left}>
                    <PowerBIReport
                      groupId="5fdc7cac-0c8b-4f5d-bf20-09da0d68026f"
                      reportId={floor.reportId}
                    />
                  </Panel>
                ) : (
                  <NotFound>{t('plainText.noReport')}</NotFound>
                )}
                <Panel fill="header" className={styles.right}>
                  <PowerBIReport
                    groupId="5fdc7cac-0c8b-4f5d-bf20-09da0d68026f"
                    reportId="988841e1-9a62-48f5-90d0-fe52f2baf6b9"
                    embedUrl={(report) =>
                      `${report.url}&filter=Floor/FloorId eq '${params.floorId}'`
                    }
                  />
                </Panel>
              </Flex>
            </Flex>
          </>
        )
      }}
    </Fetch>
  )
}
