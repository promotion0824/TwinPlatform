import { useState, useEffect } from 'react'
import { Button, Fetch, Flex, Tabs, Tab, Modal } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import cx from 'classnames'
import InspectionChecks from './InspectionChecks'
import styles from '../../../views/Command/Inspections/InspectionHistory/InspectionHistory.css'

export default function InspectionModal({
  siteId,
  inspectionId,
  onClose,
  showNavigationButtons,
  onPreviousItem,
  onNextItem,
  times,
}) {
  const { t } = useTranslation()
  const [inspection, setInspection] = useState(null)
  const [selectedCheck, setCheck] = useState(null)
  const [state, setState] = useState({ times })

  const [isGraphActive, setIsGraphActive] = useState(true)
  const isGraphDisabled =
    selectedCheck &&
    (selectedCheck.type.toLowerCase() === 'list' ||
      selectedCheck.type.toLowerCase() === 'date')

  // Disable graph view button if inspection check's type is either list or date.
  useEffect(() => {
    setIsGraphActive(!isGraphDisabled)
  }, [isGraphDisabled])

  // Select first inspection's check tab when inspection modal is opened.
  // Used to determine if the inspection check's graph view is disabled.
  useEffect(() => {
    setCheck(inspection?.checks?.[0] || null)
  }, [inspection])

  const handleTimesChange = (nextTimes) => {
    setState({
      times: nextTimes.length === 0 ? times : nextTimes,
    })
  }
  const handleTabChange = (check) => {
    setCheck(check)
  }

  return (
    <Modal
      header={t('plainText.inspection')}
      size="large"
      onClose={onClose}
      showNavigationButtons={showNavigationButtons}
      onPreviousItem={onPreviousItem}
      onNextItem={onNextItem}
    >
      <Fetch
        name="inspection"
        url={[`/api/sites/${siteId}/inspections/${inspectionId}`]}
        onResponse={(response) => setInspection(response[0])}
      >
        {inspection && (
          <Flex fill="header">
            <Tabs $borderWidth="0">
              <Tab header={inspection.name}>
                <InspectionChecks
                  inspection={inspection}
                  times={state.times}
                  onTimesChange={handleTimesChange}
                  siteId={siteId}
                  onTabChange={handleTabChange}
                  isGraphActive={isGraphActive}
                  selectedCheckId={selectedCheck?.id}
                />
              </Tab>
              {
                // Toggle buttons for graph/table view
              }
              <Flex horizontal align="right">
                <Button
                  icon="graph"
                  height="large"
                  disabled={isGraphDisabled}
                  onClick={() => setIsGraphActive(true)}
                  className={cx(styles.icon, {
                    [styles.activeIcon]: isGraphActive,
                    [styles.disabled]: isGraphDisabled,
                  })}
                />

                <Button
                  icon="details"
                  height="large"
                  onClick={() => setIsGraphActive(false)}
                  className={cx(styles.icon, {
                    [styles.activeIcon]: !isGraphActive,
                  })}
                />
              </Flex>
            </Tabs>
          </Flex>
        )}
      </Fetch>
    </Modal>
  )
}
