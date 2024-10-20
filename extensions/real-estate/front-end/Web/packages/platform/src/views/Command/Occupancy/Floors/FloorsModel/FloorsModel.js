import { useMemo } from 'react'
import { ErrorBoundary, Flex, NotFound } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import FloorInformation from './FloorInformation/FloorInformation'
import FloorsCanvas from './FloorsCanvas/FloorsCanvas'
import ResetViewButton from './ResetViewButton/ResetViewButton'
import getFloors from './getFloors'
import styles from './FloorsModel.css'

export default function FloorsComponent({ floors }) {
  const { t } = useTranslation()
  const nextFloors = useMemo(() => getFloors(floors), [floors])

  return (
    <ErrorBoundary>
      <div className={styles.floorsModel}>
        <Flex position="absolute" className={styles.floors}>
          {nextFloors.length === 0 && (
            <NotFound>{t('plainText.noFloorsFound')}</NotFound>
          )}
          {nextFloors.length > 0 && (
            <>
              <FloorsCanvas floors={nextFloors} />
              {nextFloors.map((floor) => (
                <FloorInformation key={floor.id} floor={floor} />
              ))}
              <ResetViewButton />
            </>
          )}
        </Flex>
      </div>
    </ErrorBoundary>
  )
}
