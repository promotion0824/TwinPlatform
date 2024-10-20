import { flatten } from 'lodash'
import { useTranslation } from 'react-i18next'

import {
  MoreButtonDropdown,
  MoreButtonDropdownOption,
  MoreButtonDropdownOptionDivider,
} from '@willow/ui'
import { Button, Group, Icon, useDisclosure } from '@willowinc/ui'
import WarningModal from '../../../../components/LocationHome/WarningModal/WarningModal'
import { useBuildingHomeSlice } from '../../../../store/buildingHomeSlice'
import AddWidgetModal from './AddWidgetModal'
import WIDGET_CARD_MAP from './widgetCardMap'

const WidgetLayoutControls = () => {
  const { t } = useTranslation()
  const [
    addWidgetModalOpened,
    { open: openAddWidgetModal, close: closeAddWidgetModal },
  ] = useDisclosure(false)
  const [
    widgetResetWarningModalOpened,
    { open: openWidgetResetWarningModal, close: closeWidgetResetWarningModal },
  ] = useDisclosure(false)

  const {
    layout,
    addWidget,
    isEditingMode,
    setIsEditingMode,
    resetAllConfigs,
  } = useBuildingHomeSlice()

  const addWidgetButtonProps = {
    prefix: <Icon icon="add" />,
    onClick: openAddWidgetModal,
    children: t('labels.addWidget'),
  }

  return (
    <>
      {!isEditingMode && (
        <MoreButtonDropdown targetButtonIcon="settings">
          <MoreButtonDropdownOption
            {...addWidgetButtonProps}
            css={{
              textTransform: 'capitalize',
            }}
          />

          <MoreButtonDropdownOption
            prefix={<Icon icon="edit" />}
            onClick={() => {
              setIsEditingMode(true)
            }}
            css={{
              textTransform: 'capitalize',
            }}
          >
            {t('plainText.edit')} {t('plainText.widget')}
          </MoreButtonDropdownOption>
          <MoreButtonDropdownOptionDivider />
          <MoreButtonDropdownOption
            intent="negative"
            prefix={<Icon icon="reset_settings" />}
            onClick={openWidgetResetWarningModal}
            css={{
              textTransform: 'capitalize',
            }}
          >
            {t('plainText.resetHomeToDefault')}
          </MoreButtonDropdownOption>
        </MoreButtonDropdown>
      )}
      {isEditingMode && (
        <Group>
          <Button
            {...addWidgetButtonProps}
            kind="secondary"
            css={{
              textTransform: 'capitalize',
            }}
          />
          <Button
            onClick={() => {
              setIsEditingMode(false)
            }}
            css={{
              textTransform: 'capitalize',
            }}
          >
            {t('labels.exitEditMode')}
          </Button>
        </Group>
      )}
      <AddWidgetModal
        opened={addWidgetModalOpened}
        close={closeAddWidgetModal}
        widgetMap={WIDGET_CARD_MAP}
        onWidgetAdd={(widgetId) => {
          addWidget(widgetId, WIDGET_CARD_MAP)
        }}
        widgets={flatten(layout)}
      />
      <WarningModal
        opened={widgetResetWarningModalOpened}
        onClose={closeWidgetResetWarningModal}
        onWarningConfirm={resetAllConfigs}
        confirmationButtonLabel={t('plainText.reset')}
      >
        {t('messages.resetWidgetSettingsWarning')}
      </WarningModal>
    </>
  )
}

export default WidgetLayoutControls
