import { some, toPairs } from 'lodash'
import { useTranslation } from 'react-i18next'
import styled, { css } from 'styled-components'

import { titleCase } from '@willow/common'
import { Modal, Stack } from '@willowinc/ui'
import { WidgetId } from '../../../../store/buildingHomeSlice'
import { DraggableItem } from '../DraggableColumnLayout'
import WidgetConfigCard from './WidgetConfigCard'
import WIDGET_CARD_MAP from './widgetCardMap'

interface AddWidgetModalProps {
  opened: boolean
  close: () => void
  widgets: DraggableItem[]
  onWidgetAdd: (widgetId: WidgetId) => void
  widgetMap: typeof WIDGET_CARD_MAP
}

const AddWidgetModal = ({
  opened,
  close,
  widgets,
  onWidgetAdd,
  widgetMap,
}: AddWidgetModalProps) => {
  const {
    t,
    i18n: { language },
  } = useTranslation()

  const modalContent = (
    <Stack p="s16">
      <WidgetConfigCardGrid>
        {toPairs(widgetMap).map(
          ([widgetId, { useTitle, description, imageSrc }]) => (
            <WidgetConfigCard
              key={widgetId}
              widgetId={widgetId}
              getTitle={useTitle}
              description={description}
              imageSrc={imageSrc}
              added={some(widgets, { id: widgetId })}
              onAdd={() => {
                onWidgetAdd(widgetId as WidgetId)
              }}
            />
          )
        )}
      </WidgetConfigCardGrid>
    </Stack>
  )

  const commonModalProps = {
    opened,
    onClose: close,
    header: titleCase({
      language,
      text: `${t('plainText.add')} ${t('plainText.widget')}`,
    }),
    centered: true,
  }

  return (
    <>
      {/* implemented as following because useResizeObserver won't work if breakpoints unit is not px */}
      <Modal
        {...commonModalProps}
        // Only display this modal with size fullScreen when screen size is mobile.
        css={css(({ theme }) => ({
          [`@media (width > ${theme.breakpoints.mobile})`]: {
            display: 'none',
          },
        }))}
        size="fullScreen"
      >
        {modalContent}
      </Modal>
      <Modal
        {...commonModalProps}
        // Only display this modal with size auto when screen size is bigger than mobile.
        css={css(({ theme }) => ({
          [`@media (width <= ${theme.breakpoints.mobile})`]: {
            display: 'none',
          },
        }))}
        size="auto"
      >
        {modalContent}
      </Modal>
    </>
  )
}

const WidgetConfigCardGrid = styled.div(({ theme }) => ({
  display: 'grid',
  gap: theme.spacing.s12,
  gridTemplateColumns: 'repeat(2, minmax(0, 1fr))',
  width: '100%',

  [`@media (width >= ${theme.breakpoints.monitor})`]: {
    gridTemplateColumns: 'repeat(3, minmax(0, 1fr))',
  },

  [`@media (width <= ${theme.breakpoints.mobile})`]: {
    gridTemplateColumns: 'repeat(1, minmax(0, 1fr))',
  },
}))

export default AddWidgetModal
