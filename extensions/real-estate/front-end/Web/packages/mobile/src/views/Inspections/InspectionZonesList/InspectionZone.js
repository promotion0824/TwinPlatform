import { useParams } from 'react-router'
import cx from 'classnames'
import {
  CircularProgressBar,
  Icon,
  Link,
  Spacing,
  Text,
} from '@willow/mobile-ui'
import styles from './InspectionZone.css'
import InspectionStatus from '../inspectionStatus'

export default function InspectionZone({
  id,
  name,
  statistics: {
    completedCheckCount,
    workableCheckCount,
    workableCheckSummaryStatus,
  } = {},
}) {
  const params = useParams()

  const renderInpectionZoneStatusText = () => {
    const statusClassName = cx(styles.text, {
      [styles.overdue]: workableCheckSummaryStatus === InspectionStatus.Overdue,
      [styles.completed]:
        workableCheckSummaryStatus === InspectionStatus.Completed,
    })

    return (
      <Text className={statusClassName}>
        {InspectionStatus[workableCheckSummaryStatus]}
      </Text>
    )
  }

  return (
    <Link
      to={`/sites/${params.siteId}/inspectionZones/${id}/inspections`}
      className={styles.link}
      data-segment="Mobile Inspection Zone Clicked"
      data-segment-props={JSON.stringify({
        name,
      })}
    >
      <Spacing
        horizontal
        type="content"
        size="medium"
        align="middle"
        className={styles.zone}
      >
        <CircularProgressBar
          currentStep={completedCheckCount}
          maxSteps={workableCheckCount}
          barClassName={
            workableCheckSummaryStatus === InspectionStatus.Overdue
              ? styles.redBar
              : null
          }
        />
        <Spacing className={styles.info}>
          <Text className={styles.name}>{name}</Text>
          {renderInpectionZoneStatusText()}
        </Spacing>
        <Icon icon="chevron" className={styles.icon} />
      </Spacing>
    </Link>
  )
}
