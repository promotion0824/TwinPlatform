import {
  Focus,
  ModalType,
  MyWorkgroup,
  NotificationTrigger,
  NotificationType,
  ProviderRequiredError,
  Segment,
  Skill,
  Twin,
  Workgroup,
} from '@willow/common'
import { InsightType } from '@willow/common/insights/insights/types'
import { Ontology } from '@willow/common/twins/view/models'
import { LocationNode } from '@willow/ui/components/ScopeSelector/ScopeSelector'
import { createContext, useContext } from 'react'
import { QueryStatus } from 'react-query'

export interface NotificationState {
  notification: NotificationType
  activeModal?: ModalType
  activeFocus?: Focus
  workgroups: Workgroup[]
  selectedCategories?: InsightType[]
  selectedSkills: Skill[]
  tempSelectedWorkgroupIds: string[]
  tempSelectedSkills: Skill[]
  tempSelectedNodes: LocationNode[]
  tempSelectedLocationIds: string[]
  tempSelectedModelIds: string[]
  selectedPriorities?: string[]
  selectedTwins: Twin[]
  selectedTwinCategoryIds: string[]
  allowNotificationTurnOff: boolean
  selectedNodes: LocationNode[]
  selectedLocationIds: string[]
  segment: Segment
  notificationSettingList: NotificationTrigger[]
  categories: Array<{ key: number; value: string }>
  notificationTriggerId?: string
  skills: Skill[]
  myWorkgroups: MyWorkgroup[]
  queryStatus: QueryStatus
  ontology?: Ontology
  isViewOnlyUser: boolean
  isReadToSubmit: boolean
}

export interface NotificationSettingsContextType extends NotificationState {
  onModalChange: (activeModal?: ModalType) => void
  onFocusChange: (activeFocus?: string) => void
  updateWorkgroup: (selectedIds: string[]) => void
  onCategoriesChange: (selectedCategories: InsightType[]) => void
  onSkillsChange: (selectedSkills: Skill[]) => void
  /** Function that helps to create temporary copy of selected workgroupIds when Workgroup Modal is open */
  onTempWorkgroupsChange: (selectedWorkgroupIds: string[]) => void
  /** Function that helps to create temporary copy of selected skills when Skill Modal is open */
  onTempSkillsChange: (selectedSkills: Skill[]) => void
  /** Function used to store temporary copy of selected nodes when Location Selector Modal is open */
  onTempNodesChange: (selectedNodes: LocationNode[]) => void
  /** Function used to store temporary copy of selected nodeIds when Location Selector Modal is open */
  onTempLocationIdsChange: (selectedLocationIds: string[]) => void
  /** Function used to store temporary copy of  selected modelIds when Twin Category Modal is open */
  onTempSelectedModelIds: (modelIds: string[]) => void
  onPrioritiesChange: (selectedPriorities: string[]) => void
  onTwinsChange: (selectedTwins: Twin[]) => void
  onTwinsCategoryIdChange: (selectedTwinCategoryIds: string[]) => void
  onAllowNotificationTurnOff: (allowNotificationTurnOff: boolean) => void
  onSelectedNodesChange: (selectedNodes: LocationNode[]) => void
  onSelectedLocationIdsChange: (selectedLocationIds: string[]) => void
  onSaveModal: () => void
  onCreateNotification: () => void
  onEditNotification: (notificationId: string) => void
  onResetNotificationTrigger: () => void
  onToggleTriggers: ({
    baseUrl,
    params,
  }: {
    baseUrl: string
    params: {
      isEnabled?: boolean
      isEnabledForUser?: boolean
      source?: string
    }
  }) => void
  onSegmentChange: (segment: Segment) => void
  initializeNotificationSettings: (notifications: NotificationTrigger[]) => void
  onDeleteNotificationSettingsTrigger: (
    notification: NotificationTrigger
  ) => void
}

export const NotificationSettingsContext = createContext<
  NotificationSettingsContextType | undefined
>(undefined)

export function useNotificationSettingsContext() {
  const context = useContext(NotificationSettingsContext)
  if (context == null) {
    throw new ProviderRequiredError('NotificationSettingsProvider')
  }
  return context
}
