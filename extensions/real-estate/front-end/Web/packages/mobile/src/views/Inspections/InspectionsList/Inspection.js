import { useParams } from 'react-router'
import cx from 'classnames'
import { Icon, Link, Spacing, Text } from '@willow/mobile-ui'
import InspectionCheckStatus from '../common/InspectionCheckStatus'
import styles from './Inspection.css'
import InspectionStatus from '../inspectionStatus'

export default function Inspection({
  assetName,
  id,
  name,
  checkRecordSummaryStatus,
}) {
  const params = useParams()

  const renderInpectionStatusText = () => {
    const statusClassName = cx(styles.text, {
      [styles.overdue]: checkRecordSummaryStatus === InspectionStatus.Overdue,
      [styles.completed]:
        checkRecordSummaryStatus === InspectionStatus.Completed,
    })
    return (
      <Text className={statusClassName}>
        {InspectionStatus[checkRecordSummaryStatus]}
      </Text>
    )
  }

  return (
    <Link
      to={`/sites/${params.siteId}/inspectionZones/${params.inspectionZoneId}/inspections/${id}`}
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
        <InspectionCheckStatus
          checked={checkRecordSummaryStatus === InspectionStatus.Completed}
        />
        <Spacing className={styles.info}>
          <Text className={styles.name}>
            {name}
            {assetName && <span> - {assetName}</span>}
          </Text>
          {renderInpectionStatusText()}
        </Spacing>
        <Icon icon="chevron" className={styles.icon} />
      </Spacing>
    </Link>
  )
}
