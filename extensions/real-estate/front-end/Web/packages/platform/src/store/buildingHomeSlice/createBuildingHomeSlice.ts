import { Lens, LensContext, Setter } from '@dhmk/zustand-lens'
import invariant from 'tiny-invariant'
import {
  addWidgetToShorterColumn,
  removeWidgetFromColumns,
} from './actionUtils'
import {
  defaultWidgetLayout,
  defaultWidgetSettings,
  WidgetSettings,
} from './defaultWidgetConfig'
import { WidgetLayout } from './types'
import { WidgetId } from './widgetId'

export type WidgetConfig<T extends WidgetId> =
  T extends keyof typeof defaultWidgetSettings ? WidgetSettings[T] : undefined

interface States {
  /** `twinId` for current building */
  currentTwinId?: string
  layout: WidgetLayout
  featureSettings: WidgetSettings
  isEditingMode: boolean
  isLoading: boolean
  isError: boolean
  /** Selected date range for all applicable widgets */
  selectedDateRange: [Date, Date]
}

interface Actions {
  /** Get latest data from endpoint */
  initialize: (twinId: string) => void
  /** Set selected date range, which will be shared for all buildings */
  setSelectedDateRange: (dateRange: [Date, Date]) => void
  /** Set layout state and save to server for current building */
  saveLayout: (layout: WidgetLayout) => void
  /**
   * Add a widget to the layout of current building,
   * and save the changes to server
   */
  addWidget: (
    widgetId: WidgetId,
    widgetMap: Record<string, { defaultHeight: number }>
  ) => void
  /**
   * Remove a widget from the layout of current building,
   * and save the changes to server
   */
  removeWidget: (widgetId: WidgetId) => void
  /** Set editing mode for current building */
  setIsEditingMode: (isEditingMode: boolean) => void
  /** Get widget feature settings by widgetId for current building */
  getFeatureSettingsByWidgetId: <T extends WidgetId>(
    widgetId: T
  ) => WidgetConfig<T>
  /**
   * Will update the feature settings for the provided widgetId(s), and
   * merge with the existing settings for current building.
   * This action will only update the UI without saving the changes to server.
   */
  setFeatureSettings: (config: Partial<WidgetSettings>) => void
  /** Save both layout and settings of current building to server */
  saveAllConfigs: () => void
  /** Reset both layout and settings locally and in server for current building */
  resetAllConfigs: () => void
}

export interface BuildingHomeSlice extends States, Actions {}

const aMonthAgo = new Date()
aMonthAgo.setMonth(aMonthAgo.getMonth() - 1)

const defaultState: States = {
  layout: defaultWidgetLayout,
  isEditingMode: false,
  featureSettings: defaultWidgetSettings,
  isLoading: false,
  isError: false,
  // default date range is a month ago to now, will change later when we
  // have the date range picker
  selectedDateRange: [aMonthAgo, new Date()],
}

const createBuildingHomeSlice: Lens<
  BuildingHomeSlice,
  unknown,
  Setter<BuildingHomeSlice>,
  LensContext<BuildingHomeSlice, unknown>
> = (set, get) => ({
  ...defaultState,
  initialize: (twinId: string) => {
    set((state) => {
      state.currentTwinId = twinId
      state.isLoading = true

      // reset states from other twinId
      state.isError = false
      state.isEditingMode = false
    })
    // be careful if you want to add a check of whether layout or settings exists,
    // as it might be stale data for another twinId.
    // As discussed with AndrewM, we might consider store data by twinId in the future.
    const { layout = defaultWidgetLayout, settings = defaultWidgetSettings } =
      {} // TODO: await getAsyncData(twinId)

    // mock up time for data loading
    setTimeout(() => {
      set((state) => {
        state.isLoading = false
        state.layout = layout
        state.featureSettings = settings
      })
    }, 2000)
  },
  saveLayout: (layout) => {
    set({ layout })

    get().saveAllConfigs()
  },
  addWidget: (widgetId, widgetMap) => {
    const { layout, saveAllConfigs } = get()
    set({
      layout: addWidgetToShorterColumn({
        widgetColumns: layout,
        widgetIdToAdd: widgetId,
        widgetMap,
      }),
    })

    saveAllConfigs()
  },
  removeWidget: (widgetId) => {
    const { layout, saveAllConfigs } = get()
    set({
      layout: removeWidgetFromColumns({
        widgetColumns: layout,
        widgetIdToRemove: widgetId,
      }),
    })

    saveAllConfigs()
  },
  setIsEditingMode: (isEditingMode) => set({ isEditingMode }),
  getFeatureSettingsByWidgetId: (widgetId) => {
    const { featureSettings } = get()

    return featureSettings[widgetId as WidgetId]
  },
  saveAllConfigs: () => {
    const twinId = get().currentTwinId
    invariant(twinId, 'twinId is required to save configs')

    // TODO: async call to save the config
  },
  setFeatureSettings: (config) =>
    set((state) => {
      state.featureSettings = {
        ...state.featureSettings,
        ...config,
      }
    }),
  resetAllConfigs: () => {
    const twinId = get().currentTwinId
    invariant(twinId, 'twinId is required to reset configs')

    set((state) => {
      state.layout = defaultWidgetLayout
      state.featureSettings = defaultWidgetSettings
    })

    // TODO: async call to update layout and settings to server
  },
  setSelectedDateRange: (dateRange) => set({ selectedDateRange: dateRange }),
})

export default createBuildingHomeSlice
