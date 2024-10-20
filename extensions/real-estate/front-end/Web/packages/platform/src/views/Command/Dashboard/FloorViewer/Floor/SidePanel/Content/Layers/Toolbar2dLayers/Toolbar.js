import { useState } from 'react'
import { Flex, NotFound, CollapsablePanelSection } from '@willow/ui'
import { IconButton, Panel } from '@willowinc/ui'
import _ from 'lodash'
import { useTranslation } from 'react-i18next'
import { css } from 'styled-components'
import { titleCase } from '@willow/common'
import { useFloor } from '../../../../FloorContext'
import PanelFooter from '../PanelFooter/PanelFooter'
import VisibleLayersStatus from '../VisibleLayersStatus/VisibleLayersStatus'
import FloorLayer from './Layers/FloorLayer'
import ImageLayer from './Layers/ImageLayer'
import ZoneLayer from './Layers/ZoneLayer'
import DeleteImageModal from './DeleteImageModal/DeleteImageModal'
import FloorImageModal from './FloorImageModal/FloorImageModal'
import styles from './Toolbar.css'

// eslint-disable-next-line complexity
export default function Toolbar(props) {
  const { isHidden } = props
  const floor = useFloor()
  const {
    t,
    i18n: { language },
  } = useTranslation()

  const [showFloorImageModal, setShowFloorImageModal] = useState(false)
  const [deleteImage, setDeleteImage] = useState()

  const layerGroups = floor.layerGroups.filter(
    (layerGroup) => layerGroup.id !== 'floor_layer'
  )
  const filteredLayerGroups = layerGroups.filter(
    (layerGroup) => !floor.isReadOnly || layerGroup.name !== 'Assets layer'
  )

  let { modules2D } = floor
  const useNewSortingFeature = !!floor.disciplinesSortOrder?.sortOrder2d
  if (useNewSortingFeature) {
    modules2D = _(floor.modules2D)
      .orderBy((layer) =>
        floor.disciplinesSortOrder?.sortOrder2d?.indexOf(layer.moduleTypeId)
      )
      .value()
  }

  return (
    <>
      <Panel
        title={titleCase({
          text: t('plainText.viewControls'),
          language,
        })}
        collapsible
        defaultSize={30}
        id="2d-layers-outer-panel"
        css={css`
          display: ${isHidden ? 'none' : 'block'};
        `}
      >
        <Flex size="medium" padding="0">
          <CollapsablePanelSection
            name="levelImages"
            header={t('headers.levelImages')}
            adornment={
              floor.isReadOnly ? (
                <VisibleLayersStatus
                  number={floor.modules2D.reduce(
                    (count, image) => count + image.isVisible,
                    0
                  )}
                />
              ) : null
            }
          >
            {modules2D.map((image) => (
              <ImageLayer
                key={image.id}
                image={image}
                onDeleteClick={() => setDeleteImage(image)}
              />
            ))}
            {!floor.isReadOnly && (
              <PanelFooter>
                <IconButton
                  icon="add"
                  kind="secondary"
                  background="transparent"
                  onClick={() => setShowFloorImageModal(true)}
                />
              </PanelFooter>
            )}
          </CollapsablePanelSection>
          <CollapsablePanelSection
            name="zoneLayers"
            header={t('headers.zoneLayers')}
            adornment={
              floor.isReadOnly ? (
                <VisibleLayersStatus
                  number={filteredLayerGroups.reduce(
                    (count, layerGroup) =>
                      count + floor.isLayerGroupVisible(layerGroup),
                    0
                  )}
                />
              ) : null
            }
          >
            {filteredLayerGroups.map((layerGroup) => (
              <ZoneLayer key={layerGroup.localId} layerGroup={layerGroup} />
            ))}
            {!floor.isReadOnly && <FloorLayer />}
            {layerGroups.length === 0 && floor.isReadOnly && (
              <NotFound className={styles.notFound}>
                {t('plainText.noZoneLayers')}
              </NotFound>
            )}
            {!floor.isReadOnly && (
              <PanelFooter>
                <IconButton
                  icon="add"
                  kind="secondary"
                  background="transparent"
                  onClick={floor.addLayerGroup}
                  data-segment="Add Layer"
                />
              </PanelFooter>
            )}
          </CollapsablePanelSection>
        </Flex>
      </Panel>
      {showFloorImageModal && (
        <FloorImageModal onClose={() => setShowFloorImageModal(false)} />
      )}
      {deleteImage != null && (
        <DeleteImageModal
          image={deleteImage}
          onClose={() => setDeleteImage()}
        />
      )}
    </>
  )
}
