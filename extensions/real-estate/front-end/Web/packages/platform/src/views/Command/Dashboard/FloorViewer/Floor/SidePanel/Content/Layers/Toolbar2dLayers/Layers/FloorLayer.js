import { useParams } from 'react-router'
import _ from 'lodash'
import { Fetch, Flex, Icon, Message } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import { useFloor } from '../../../../../FloorContext'
import Layer from './Layer/Layer'
import styles from './FloorLayer.css'

export default function FloorLayer() {
  const params = useParams()
  const floor = useFloor()
  const { t } = useTranslation()

  return (
    <>
      {!floor.floorLayer.hasLoaded && (
        <Fetch
          url={`/api/sites/${params.siteId}/floors`}
          params={{
            hasBaseModule: false,
          }}
          loader={
            <Flex align="center" padding="medium" className={styles.loader}>
              <Icon icon="progress" />
            </Flex>
          }
          error={
            <Flex align="center" padding="medium" className={styles.error}>
              <Message icon="error">
                {t('plainText.errorLoadingFloorLayer')}
              </Message>
            </Flex>
          }
          onResponse={(floors) => {
            const floorLayer = floors.find(
              (floorSummary) => floorSummary.id === floor.floorId
            )

            let geometry = []
            try {
              geometry = JSON.parse(floorLayer.geometry)
              if (!_.isArray(geometry)) geometry = []
            } catch (err) {
              // do nothing
            }

            floor.setFloorLayerGeometry(geometry)
          }}
        />
      )}
      {floor.floorLayer.hasLoaded && (
        <Layer
          header={floor.floorLayer.name}
          selected={floor.layerGroup === floor.floorLayer}
          onClick={() => floor.selectLayerGroup(floor.floorLayer)}
        />
      )}
    </>
  )
}
