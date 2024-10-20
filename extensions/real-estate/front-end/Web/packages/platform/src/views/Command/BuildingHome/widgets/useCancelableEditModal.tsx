import { useCallback, useRef } from 'react'

import { useDisclosure } from '@willowinc/ui'
import {
  useBuildingHomeSlice,
  WidgetConfig,
  WidgetId,
} from '../../../../store/buildingHomeSlice'

export type UseEditModal = <T extends WidgetId>(
  widgetId: T
) => {
  /** Return the specified widget feature configuration */
  widgetConfig: WidgetConfig<T>
  /**
   * Set the widget feature configurations. Will replace the whole
   * widget configuration with the new one.
   */
  setWidgetConfig: (features: WidgetConfig<T>) => void
  /** Whether the edit modal is opened */
  editModalOpened: boolean
  /**
   * Open the edit modal.
   * Will preserve the initial configurations before editing.
   */
  onOpenEditModal: () => void
  /** Close the edit modal. */
  onCloseEditModal: () => void
  /**
   * Cancel the configuration changes and reset to the initial
   * configurations.
   */
  onCancelEdit: () => void
  /** Save the configuration changes */
  onSaveEdit: () => void
}

/**
 * Hook to manage the edit modal status and cancelable change status for a widget.
 */
const useCancelableEditModal: UseEditModal = (widgetId) => {
  const { getFeatureSettingsByWidgetId, setFeatureSettings, saveAllConfigs } =
    useBuildingHomeSlice()

  const widgetConfig = getFeatureSettingsByWidgetId(widgetId)
  const initialWidgetConfig =
    useRef<WidgetConfig<typeof widgetId>>(widgetConfig)
  const setWidgetConfig = useCallback(
    (features: WidgetConfig<typeof widgetId>) => {
      setFeatureSettings({
        [widgetId]: features,
      })
    },
    [setFeatureSettings, widgetId]
  )

  const [editModalOpened, { close: onCloseEditModal, open: openEditModal }] =
    useDisclosure()

  const onOpenEditModal = useCallback(() => {
    openEditModal()
    // Persist configurations before editing
    initialWidgetConfig.current = widgetConfig
  }, [openEditModal, widgetConfig])

  const onCancelEdit = useCallback(() => {
    if (initialWidgetConfig.current === widgetConfig) {
      // skip update if no change
      return
    }
    // Reset to previous configurations
    setFeatureSettings({
      [widgetId]: initialWidgetConfig.current,
    })
  }, [setFeatureSettings, widgetConfig, widgetId])

  const onSaveEdit = useCallback(() => {
    if (initialWidgetConfig.current !== widgetConfig) {
      // required so that if user clicks Cancel again, it will not revert the changes
      initialWidgetConfig.current = widgetConfig
    }

    // will save current config to backend, no matter it is default or changed
    saveAllConfigs()
  }, [saveAllConfigs, widgetConfig])

  return {
    widgetConfig,
    setWidgetConfig,
    editModalOpened,
    onOpenEditModal,
    onCloseEditModal,
    onCancelEdit,
    onSaveEdit,
  }
}

export default useCancelableEditModal
