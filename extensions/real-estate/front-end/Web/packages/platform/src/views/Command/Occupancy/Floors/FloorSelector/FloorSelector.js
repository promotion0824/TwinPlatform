import { useParams } from 'react-router'
import { Flex, TabBackButton } from '@willow/ui'
import { useSite } from 'providers'
import { useFloors } from '../FloorsContext'
import Pill from '../../Pill/Pill'
import Scroll from './Scroll'
import styles from './FloorSelector.css'

export default function FloorSelector({ floors }) {
  const floorsContext = useFloors()
  const params = useParams()
  const site = useSite()

  const shouldHideDashboard =
    site.features.isTicketingDisabled &&
    site.features.isInsightsDisabled &&
    site.features.is2DViewerDisabled &&
    floors.length > 0

  return (
    <Flex fill="content">
      <div>
        {params.floorId != null && !shouldHideDashboard && (
          <Flex padding="medium 0 0">
            <TabBackButton
              to={
                floorsContext.isReadOnly ? `/sites/${params.siteId}` : '/admin'
              }
              className={styles.backButton}
            />
          </Flex>
        )}
      </div>
      <Scroll>
        <Flex align="center" padding="tiny" className={styles.floorSelector}>
          <Flex size="tiny">
            {floors.map((floor) => {
              let color
              if (floorsContext.isReadOnly) {
                if (floor.insightsHighestPriority === 1) color = 'red'
                if (floor.insightsHighestPriority === 2) color = 'orange'
                if (floor.insightsHighestPriority === 3) color = 'yellow'
              }

              const isSelected = floor.id === params.floorId
              const isHovering = floor.id === floorsContext.hoverFloorId

              return (
                <Pill
                  key={floor.id}
                  color={color}
                  selected={isSelected}
                  hovering={isHovering}
                  ripple={false}
                  to={`/sites/${params.siteId}/occupancy/floors/${floor.id}`}
                  onPointerEnter={() => floorsContext.setHoverFloor(floor)}
                  onPointerLeave={() => floorsContext.setHoverFloor()}
                  onFocus={() => floorsContext.setHoverFloor(floor)}
                  onBlur={() => floorsContext.setHoverFloor()}
                >
                  {floor.code}
                </Pill>
              )
            })}
            {floors.length === 0 && (
              <Flex align="center" padding="small">
                -
              </Flex>
            )}
          </Flex>
        </Flex>
      </Scroll>
    </Flex>
  )
}
