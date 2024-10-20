/* eslint-disable complexity */
import {
  ALL_CATEGORIES,
  CreateNotification,
  Focus,
  ModalType,
  MyWorkgroup,
  NotificationTrigger,
  NotificationType,
  PriorityIds,
  Segment,
  Skill,
  Twin,
  Workgroup,
  priorities,
  titleCase,
  useGetMyWorkgroups,
  useGetNotificationSettingCategories,
  useSaveNotificationSetting,
  useUpdateTriggers,
} from '@willow/common'
import useMultipleSearchParams from '@willow/common/hooks/useMultipleSearchParams'
import { InsightType } from '@willow/common/insights/insights/types'
import {
  ALL_LOCATIONS,
  api,
  reduceQueryStatuses,
  useScopeSelector,
  useUser,
} from '@willow/ui'
import { LocationNode } from '@willow/ui/components/ScopeSelector/ScopeSelector'
import { flattenTree } from '@willow/ui/components/ScopeSelector/scopeSelectorUtils'
import { Button, useSnackbar } from '@willowinc/ui'
import _ from 'lodash'
import { useCallback, useEffect, useReducer } from 'react'
import { useTranslation } from 'react-i18next'
import { useMutation, useQuery, useQueryClient } from 'react-query'
import { useHistory, useParams } from 'react-router'
import useOntology from '../../../hooks/useOntologyInPlatform'
import useGetWorkgroupSelectors from '../../../hooks/WorkGroups/useGetWorkGroupSelectors'
import routes from '../../../routes'
import {
  NotificationSettingsContext,
  NotificationState,
} from './NotificationSettingsContext'

const getSelectedSiteIds = (locationIds = [], scopeLookup) => {
  const selectedNodes = locationIds.map(
    (locationId) => scopeLookup?.[locationId]
  )
  return [
    ...new Set(
      flattenTree(selectedNodes ?? []).map((item) => item?.twin.siteId)
    ),
  ].filter((item) => !!item) as string[]
}

export enum NotificationActionType {
  'onModalChange',
  'onFocusChange',
  'updateWorkgroup',
  'initializeWorkgroup',
  'initializeMyWorkgroups',
  'onCategoriesChange',
  'onSkillsChange',
  'onPrioritiesChange',
  'onTwinsChange',
  'onTwinsCategoryIdChange',
  'onAllowNotificationTurnOff',
  'onTempWorkgroupsChange',
  'onTempSkillsChange',
  'onTempNodesChange',
  'onTempLocationIdsChange',
  'onSelectedNodesChange',
  'onSelectedLocationIdsChange',
  'onTempSelectedModelIds',
  'onSaveModal',
  'onSegmentChange',
  'onDeleteNotificationSettingsTrigger',
  'onAddNotificationSettingsList',
  'initializeNotificationSettings',
  'onEditNotification',
  'initializeSkills',
  'initializeCategories',
  'onResetNotificationTrigger',
}

export type NotificationAction =
  | {
      type: NotificationActionType.onModalChange
      activeModal?: ModalType
    }
  | {
      type: NotificationActionType.onFocusChange
      activeFocus?: Focus
    }
  | {
      type: NotificationActionType.updateWorkgroup
      selectedIds: string[]
    }
  | {
      type: NotificationActionType.initializeWorkgroup
      workgroups: Workgroup[]
    }
  | {
      type: NotificationActionType.onCategoriesChange
      selectedCategories: InsightType[]
    }
  | {
      type: NotificationActionType.onSkillsChange
      selectedSkills: Skill[]
    }
  | {
      type: NotificationActionType.onPrioritiesChange
      selectedPriorities: string[]
    }
  | {
      type: NotificationActionType.onTwinsChange
      selectedTwins: Twin[]
    }
  | {
      type: NotificationActionType.onTwinsCategoryIdChange
      selectedTwinCategoryIds: string[]
    }
  | {
      type: NotificationActionType.onAllowNotificationTurnOff
      allowNotificationTurnOff: boolean
    }
  | {
      type: NotificationActionType.onTempWorkgroupsChange
      tempSelectedWorkgroupIds: string[]
    }
  | {
      type: NotificationActionType.onTempSkillsChange
      tempSelectedSkills: Skill[]
    }
  | {
      type: NotificationActionType.onTempNodesChange
      tempSelectedNodes: LocationNode[]
    }
  | {
      type: NotificationActionType.onTempLocationIdsChange
      tempSelectedLocationIds: string[]
    }
  | {
      type: NotificationActionType.onSelectedNodesChange
      selectedNodes: LocationNode[]
    }
  | {
      type: NotificationActionType.onSelectedLocationIdsChange
      selectedLocationIds: string[]
    }
  | {
      type: NotificationActionType.onTempSelectedModelIds
      tempSelectedModelIds: string[]
    }
  | {
      type: NotificationActionType.onSaveModal
    }
  | {
      type: NotificationActionType.onSegmentChange
      segment: Segment
    }
  | {
      type: NotificationActionType.initializeNotificationSettings
      notificationSettingList: NotificationTrigger[]
    }
  | {
      type: NotificationActionType.onDeleteNotificationSettingsTrigger
      deletedNotifications: NotificationTrigger
    }
  | {
      type: NotificationActionType.onAddNotificationSettingsList
      addNotifications: NotificationTrigger
    }
  | {
      type: NotificationActionType.onEditNotification
      trigger: NotificationTrigger
      scopeLookup: Record<string, LocationNode>
      isAdmin: boolean
    }
  | {
      type: NotificationActionType.initializeSkills
      skills: Skill[]
    }
  | {
      type: NotificationActionType.initializeMyWorkgroups
      myWorkgroups: MyWorkgroup[]
    }
  | {
      type: NotificationActionType.initializeCategories
      categories: {
        key: number
        value: string
      }[]
    }
  | {
      type: NotificationActionType.onResetNotificationTrigger
    }

const defaultState: NotificationState = {
  notification: NotificationType.settings,
  workgroups: [],
  myWorkgroups: [],
  selectedCategories: [],
  selectedSkills: [],
  tempSelectedWorkgroupIds: [],
  tempSelectedSkills: [],
  tempSelectedNodes: [],
  tempSelectedLocationIds: [],
  tempSelectedModelIds: [],
  isViewOnlyUser: false,

  // priorities are displayed by default in Low, Medium, High, Critical order
  selectedPriorities: priorities.map(({ name }) => name).reverse(),
  selectedTwins: [],
  selectedTwinCategoryIds: [],
  selectedNodes: [],
  selectedLocationIds: [],
  allowNotificationTurnOff: true,
  segment: Segment.workgroup,
  notificationSettingList: [],
  skills: [],
  categories: [],
  queryStatus: 'idle',
  isReadToSubmit: false,
}

const notificationReducer = (
  state: NotificationState,
  action: NotificationAction
) => {
  switch (action.type) {
    case NotificationActionType.onFocusChange:
      return {
        ...state,
        activeFocus: action.activeFocus,
        selectedSkills: [],
        selectedCategories: [],
        selectedTwins: [],
      }
    case NotificationActionType.onModalChange:
      return {
        ...state,
        activeModal: action.activeModal,
        tempSelectedWorkgroupIds: state.workgroups
          .filter((item) => item.selected)
          .map((workgroup) => workgroup.id),
        tempSelectedNodes: state.selectedNodes,
        tempSelectedSkills: state.selectedSkills,
        tempSelectedLocationIds: state.selectedLocationIds,
        tempSelectedModelIds: state.selectedTwinCategoryIds,
      }
    case NotificationActionType.updateWorkgroup:
      return {
        ...state,
        tempSelectedWorkgroupIds: action.selectedIds,
        workgroups: state.workgroups.map((workgroup) => ({
          ...workgroup,
          selected: !!action.selectedIds.includes(workgroup.id),
        })),
      }
    case NotificationActionType.initializeWorkgroup:
      return {
        ...state,
        workgroups: action.workgroups,
      }
    case NotificationActionType.initializeMyWorkgroups:
      return {
        ...state,
        myWorkgroups: action.myWorkgroups,
      }
    case NotificationActionType.onCategoriesChange:
      return {
        ...state,
        selectedCategories: action.selectedCategories,
      }
    case NotificationActionType.onSkillsChange:
      return {
        ...state,
        tempSelectedSkills: action.selectedSkills,
        selectedSkills: action.selectedSkills,
      }
    case NotificationActionType.onPrioritiesChange:
      return {
        ...state,
        selectedPriorities: action.selectedPriorities,
      }
    case NotificationActionType.onTwinsChange:
      return {
        ...state,
        selectedTwins: action.selectedTwins,
      }
    case NotificationActionType.onTwinsCategoryIdChange:
      return {
        ...state,
        selectedTwinCategoryIds: action.selectedTwinCategoryIds,
        tempSelectedModelIds: action.selectedTwinCategoryIds,
      }
    case NotificationActionType.onAllowNotificationTurnOff:
      return {
        ...state,
        allowNotificationTurnOff: action.allowNotificationTurnOff,
      }
    case NotificationActionType.onTempWorkgroupsChange:
      return {
        ...state,
        tempSelectedWorkgroupIds: action.tempSelectedWorkgroupIds,
      }
    case NotificationActionType.onTempSkillsChange:
      return {
        ...state,
        tempSelectedSkills: action.tempSelectedSkills,
      }
    case NotificationActionType.onTempNodesChange:
      return {
        ...state,
        tempSelectedNodes: action.tempSelectedNodes,
      }
    case NotificationActionType.onTempLocationIdsChange:
      return {
        ...state,
        tempSelectedLocationIds: action.tempSelectedLocationIds,
      }
    case NotificationActionType.onSelectedNodesChange:
      return {
        ...state,
        selectedNodes: action.selectedNodes,
        tempSelectedNodes: action.selectedNodes,
      }
    case NotificationActionType.onSelectedLocationIdsChange:
      return {
        ...state,
        selectedLocationIds: action.selectedLocationIds,
        tempSelectedLocationIds: action.selectedLocationIds,
      }
    case NotificationActionType.onTempSelectedModelIds:
      return {
        ...state,
        tempSelectedModelIds: action.tempSelectedModelIds,
      }
    case NotificationActionType.onSaveModal:
      return {
        ...state,

        selectedNodes:
          state.activeModal &&
          [ModalType.location, ModalType.locationReport].includes(
            state.activeModal
          )
            ? state.tempSelectedNodes
            : state.selectedNodes,
        selectedLocationIds:
          state.activeModal &&
          [ModalType.location, ModalType.locationReport].includes(
            state.activeModal
          )
            ? state.tempSelectedLocationIds
            : state.selectedLocationIds,

        workgroups:
          state.activeModal === ModalType.workgroup
            ? state.workgroups.map((workgroup) => ({
                ...workgroup,
                selected: !!state.tempSelectedWorkgroupIds.includes(
                  workgroup.id
                ),
              }))
            : state.workgroups,

        activeModal: undefined,
      }

    case NotificationActionType.onSegmentChange:
      return {
        ...state,
        segment: action.segment,
      }
    case NotificationActionType.initializeNotificationSettings:
      return {
        ...state,
        notificationSettingList: action.notificationSettingList,
      }
    case NotificationActionType.onDeleteNotificationSettingsTrigger: {
      const itemIndex = state.notificationSettingList.findIndex(
        (item) => item.id === action.deletedNotifications.id
      )

      const updatedNotificationList =
        itemIndex !== -1
          ? [
              ...state.notificationSettingList.slice(0, itemIndex),
              ...state.notificationSettingList.slice(itemIndex + 1),
            ]
          : state.notificationSettingList

      return {
        ...state,
        notificationSettingList: updatedNotificationList,
      }
    }
    case NotificationActionType.onAddNotificationSettingsList:
      return {
        ...state,
        notificationSettingList: [action.addNotifications].concat(
          state.notificationSettingList
        ),
      }
    case NotificationActionType.onEditNotification: {
      const locations = action.trigger.locations ?? []
      const isAlLocationsSelected = locations.length === 0

      return {
        ...state,
        notificationTriggerId: action.trigger.id,
        selectedLocationIds: isAlLocationsSelected
          ? [ALL_LOCATIONS as string]
          : locations,
        selectedNodes: action.scopeLookup
          ? isAlLocationsSelected
            ? [
                {
                  isAllItemsNode: true,
                  twin: {
                    id: ALL_LOCATIONS,
                    name: ALL_LOCATIONS,
                    siteId: '',
                    metadata: {
                      modelId: ALL_LOCATIONS,
                    },
                    userId: '',
                  },
                },
              ]
            : locations.map((locationId) => action.scopeLookup[locationId])
          : [],
        selectedTwins: (action.trigger.twins ?? []).map((twin) => ({
          id: twin.twinId,
          name: twin.twinName,
          // TODO: Needs to reverted back. This is a temporary fix
          // Link: https://dev.azure.com/willowdev/Unified/_workitems/edit/135964
          modelId: twin.modelId || 'dtmi:com:willowinc:Asset;1',
        })),
        selectedSkills: (state.skills ?? []).filter((skill) =>
          action.trigger.skillIds?.includes(skill.id)
        ),
        selectedCategories: (state.categories ?? [])
          .filter((category) =>
            action.trigger.skillCategoryIds?.includes(category.key)
          )
          .map(({ value }) => value),
        workgroups: state.workgroups.map((workgroup) => ({
          ...workgroup,
          selected: !!action.trigger.workgroupIds?.includes(workgroup.id),
        })),
        selectedPriorities: priorities
          .filter((priority) =>
            action.trigger.priorityIds?.includes(priority.id)
          )
          .map(({ name }) => name)
          .reverse(),
        allowNotificationTurnOff: action.trigger.canUserDisableNotification,
        // If `twinCategoryIds` from API is empty, that means it is `All Categories` for UI purposes
        selectedTwinCategoryIds:
          action.trigger.focus === Focus.twinCategory
            ? action.trigger.twinCategoryIds?.length
              ? action.trigger.twinCategoryIds ?? []
              : [ALL_CATEGORIES]
            : [],
        segment: action.trigger.type,
        activeFocus: action.trigger.focus,
        // If segment is workgroup and user is non-admin, they only have "view" access
        isViewOnlyUser:
          !action.isAdmin && action.trigger.type === Segment.workgroup,
      }
    }
    case NotificationActionType.initializeSkills:
      return {
        ...state,
        skills: action.skills,
      }
    case NotificationActionType.initializeCategories:
      return {
        ...state,
        categories: action.categories,
      }
    case NotificationActionType.onResetNotificationTrigger:
      return {
        ...state,
        notificationTriggerId: undefined,
        selectedLocationIds: [],
        selectedNodes: [],
        selectedTwins: [],
        selectedSkills: [],
        selectedTwinCategoryIds: [],
        workgroups: state.workgroups.map((workgroup) => ({
          ...workgroup,
          selected: false,
        })),
        selectedPriorities: [],
        activeFocus: undefined,
      }
    default:
      return state
  }
}

export default function NotificationSettingsProvider({
  children,
}: {
  children: React.ReactNode
}) {
  const { scopeLookup } = useScopeSelector()
  const [
    {
      notification,
      activeFocus,
      activeModal,
      myWorkgroups,
      workgroups,
      selectedCategories,
      selectedSkills,
      selectedPriorities,
      selectedTwins,
      selectedTwinCategoryIds,
      allowNotificationTurnOff,
      tempSelectedWorkgroupIds,
      tempSelectedSkills,
      tempSelectedLocationIds,
      tempSelectedNodes,
      tempSelectedModelIds,
      selectedNodes,
      selectedLocationIds,
      segment,
      notificationSettingList,
      notificationTriggerId,
      isViewOnlyUser,
    },
    dispatch,
  ] = useReducer(notificationReducer, defaultState)
  const { data: ontology } = useOntology()
  const queryClient = useQueryClient()
  const history = useHistory()
  const { showAdminMenu: isAdmin } = useUser()
  const [_siteIds, setSearchParams] = useMultipleSearchParams(['siteIds'])

  const categoriesQuery = useGetNotificationSettingCategories({
    onSuccess: (data) => {
      dispatch({
        type: NotificationActionType.initializeCategories,
        categories: data,
      })
    },
  })

  const workGroupQuery = useGetWorkgroupSelectors({
    onSuccess: (data) => {
      dispatch({
        type: NotificationActionType.initializeWorkgroup,
        workgroups: data,
      })
    },
  })

  useGetMyWorkgroups({
    onSettled: (data) => {
      dispatch({
        type: NotificationActionType.initializeMyWorkgroups,
        myWorkgroups: data ?? [],
      })
    },
  })

  const skillsQuery = useQuery(
    ['skills'],
    async () => {
      const { data } = await api.post('/skills', {
        contentType: 'application/json',
      })

      return { data }
    },
    {
      select: ({ data }) => ({
        items: data.items.filter(
          (item) => !!item.id && !!item.name && !!item.category
        ),
      }),
      onSuccess: ({ items }) => {
        dispatch({
          type: NotificationActionType.initializeSkills,
          skills: items,
        })
      },
    }
  )

  const deleteNotificationMutation = useMutation(
    async ({ addNotifications }: { addNotifications: NotificationTrigger }) => {
      await api.delete(`/notifications/triggers/${addNotifications.id}`)
    },
    {
      onError: (_, context) => {
        snackbar.show({
          title: t('plainText.anErrorOccurred'),
          description: t('plainText.pleaseTryAgain'),
          intent: 'negative',
        })
        dispatch({
          type: NotificationActionType.onAddNotificationSettingsList,
          addNotifications: context.addNotifications,
        })
      },
    }
  )

  const createNotificationMutation = useSaveNotificationSetting()
  const updateTriggerMutation = useUpdateTriggers()

  const { triggerId } = useParams<{ triggerId: string }>()

  const { data, isSuccess } = useQuery(
    ['getNotification', triggerId],
    async () => {
      const response = await api.get(`/notifications/triggers/${triggerId}`)

      return response.data
    },

    {
      enabled: triggerId != null && scopeLookup != null,
    }
  )

  useEffect(() => {
    if (data) {
      dispatch({
        type: NotificationActionType.onEditNotification,
        trigger: data,
        scopeLookup,
        isAdmin,
      })
      setSearchParams({
        siteIds: getSelectedSiteIds(data.locations, scopeLookup),
      })
    }
  }, [data?.id, triggerId])

  const snackbar = useSnackbar()
  const user = useUser()
  const {
    t,
    i18n: { language },
  } = useTranslation()

  const onModalChange = useCallback(
    (toggledModal?: ModalType) =>
      dispatch({
        type: NotificationActionType.onModalChange,
        activeModal: toggledModal,
      }),
    []
  )

  const updateWorkgroup = useCallback(
    (selectedIds: string[]) =>
      dispatch({
        type: NotificationActionType.updateWorkgroup,
        selectedIds,
      }),
    []
  )

  const onFocusChange = useCallback(
    (nextFocus?: Focus) =>
      dispatch({
        type: NotificationActionType.onFocusChange,
        activeFocus: nextFocus,
      }),
    []
  )

  const onCategoriesChange = useCallback(
    (updatedCategories: InsightType[]) =>
      dispatch({
        type: NotificationActionType.onCategoriesChange,
        selectedCategories: updatedCategories,
      }),
    []
  )

  const onSkillsChange = useCallback(
    (updatedSkills: Skill[]) =>
      dispatch({
        type: NotificationActionType.onSkillsChange,
        selectedSkills: updatedSkills,
      }),
    []
  )

  const onPrioritiesChange = useCallback(
    (updatedPriorities: string[]) =>
      dispatch({
        type: NotificationActionType.onPrioritiesChange,
        selectedPriorities: updatedPriorities,
      }),
    []
  )

  const onTwinsChange = useCallback(
    (updatedTwins: Twin[]) =>
      dispatch({
        type: NotificationActionType.onTwinsChange,
        selectedTwins: updatedTwins,
      }),
    []
  )

  const onTwinsCategoryIdChange = useCallback(
    (updatedTwinCategoryIds: string[]) =>
      dispatch({
        type: NotificationActionType.onTwinsCategoryIdChange,
        selectedTwinCategoryIds: updatedTwinCategoryIds,
      }),
    []
  )

  const onAllowNotificationTurnOff = useCallback(
    (turnOff: boolean) =>
      dispatch({
        type: NotificationActionType.onAllowNotificationTurnOff,
        allowNotificationTurnOff: turnOff,
      }),
    []
  )

  const onTempWorkgroupsChange = useCallback(
    (currentSelectedWorkgroupsIds: string[]) =>
      dispatch({
        type: NotificationActionType.onTempWorkgroupsChange,
        tempSelectedWorkgroupIds: currentSelectedWorkgroupsIds,
      }),
    []
  )

  const onTempSkillsChange = useCallback(
    (currentSkills: Skill[]) =>
      dispatch({
        type: NotificationActionType.onTempSkillsChange,
        tempSelectedSkills: currentSkills,
      }),
    []
  )

  const onTempNodesChange = useCallback(
    (currentNodes: LocationNode[]) =>
      dispatch({
        type: NotificationActionType.onTempNodesChange,
        tempSelectedNodes: currentNodes,
      }),

    []
  )

  const onTempLocationIdsChange = useCallback(
    (currentLocationIds: string[]) =>
      dispatch({
        type: NotificationActionType.onTempLocationIdsChange,
        tempSelectedLocationIds: currentLocationIds,
      }),
    []
  )

  const onSelectedNodesChange = useCallback(
    (currentNodes: LocationNode[]) =>
      dispatch({
        type: NotificationActionType.onSelectedNodesChange,
        selectedNodes: currentNodes,
      }),

    []
  )

  const onSelectedLocationIdsChange = useCallback(
    (currentLocationIds: string[]) =>
      dispatch({
        type: NotificationActionType.onSelectedLocationIdsChange,
        selectedLocationIds: currentLocationIds,
      }),
    []
  )

  const onSegmentChange = useCallback(
    (updatedSegment: Segment) =>
      dispatch({
        type: NotificationActionType.onSegmentChange,
        segment: updatedSegment,
      }),
    []
  )

  const onTempSelectedModelIds = useCallback(
    (currentModelIds: string[]) =>
      dispatch({
        type: NotificationActionType.onTempSelectedModelIds,
        tempSelectedModelIds: currentModelIds,
      }),
    []
  )

  const onSaveModal = useCallback(
    () => dispatch({ type: NotificationActionType.onSaveModal }),
    []
  )

  const initializeNotificationSettings = useCallback(
    (newNotificationList: NotificationTrigger[]) =>
      dispatch({
        type: NotificationActionType.initializeNotificationSettings,
        notificationSettingList: newNotificationList,
      }),
    []
  )

  const onResetNotificationTrigger = useCallback(
    () =>
      dispatch({
        type: NotificationActionType.onResetNotificationTrigger,
      }),
    []
  )

  // Adding timeout to make sure that API call is made after snackbar is closed
  // We are clearing the timeout if user clicks on undo since we don't want to make API call in that case
  const onDeleteNotificationSettingsTrigger = useCallback(
    (newDeletedNotifications: NotificationTrigger) => {
      dispatch({
        type: NotificationActionType.onDeleteNotificationSettingsTrigger,
        deletedNotifications: newDeletedNotifications,
      })
      let timeId: NodeJS.Timeout
      snackbar.show({
        id: newDeletedNotifications.id,
        title: t('plainText.notificationDeleted'),
        intent: 'positive',
        actions: (
          <Button
            kind="primary"
            background="transparent"
            onClick={() => {
              dispatch({
                type: NotificationActionType.onAddNotificationSettingsList,
                addNotifications: newDeletedNotifications,
              })
              snackbar.hide(newDeletedNotifications.id)
              clearTimeout(timeId)
            }}
          >
            {t('plainText.undo')}
          </Button>
        ),
      })

      timeId = setTimeout(() => {
        deleteNotificationMutation.mutate({
          addNotifications: newDeletedNotifications,
        })
      }, 4000)
    },
    [deleteNotificationMutation, snackbar, t]
  )

  const onCreateNotification = () => {
    const selectedLocations = selectedNodes.map((node) => node.twin.id)
    const workGroupIds = workgroups
      ?.filter((workgroup) => workgroup.selected)
      .map((workgroup) => workgroup.id)

    // Passing all the priorityIds when the focus is skill or skillCategory
    const priorityIds =
      activeFocus === Focus.skill || activeFocus === Focus.skillCategory
        ? (priorities.map((item) => item.id) as PriorityIds[])
        : ((selectedPriorities ?? []).map(
            (priority) => priorities.find((item) => item.name === priority)?.id
          ) as PriorityIds[])

    const categoryIds = (selectedCategories ?? []).map(
      (selectedCategory) =>
        (categoriesQuery.data ?? []).find(
          (item) => _.camelCase(item.value) === selectedCategory
        )?.key as number
    )

    const selectedData: CreateNotification = {
      type: segment,
      source: 'insight',
      focus: activeFocus as Focus,
      locations: selectedLocations.includes(ALL_LOCATIONS)
        ? undefined
        : selectedLocations,
      isEnabled: true,
      createdBy: user.id,
      canUserDisableNotification: allowNotificationTurnOff,
      workGroupIds,
      skillCategories: categoryIds,
      // Business Logic : If `All Categories` is selected, send an empty array
      twinCategoryIds: selectedTwinCategoryIds?.includes(ALL_CATEGORIES)
        ? []
        : selectedTwinCategoryIds ?? [],
      twins: selectedTwins.map((selectedTwin) => ({
        twinId: selectedTwin.id,
        twinName: selectedTwin.name,
        modelId: selectedTwin.modelId,
      })),
      skillIds: (selectedSkills ?? []).map((selectedSkill) => selectedSkill.id),
      priorityIds,
      channels: ['inApp'],
    }

    createNotificationMutation.mutate(
      {
        baseUrl: notificationTriggerId
          ? `/api/notifications/triggers/${notificationTriggerId}`
          : `/api/notifications/triggers`,
        formData: selectedData,
        id: notificationTriggerId,
      },
      {
        onError: () => {
          snackbar.show({
            title: t('plainText.errorOccurred'),
            intent: 'negative',
          })
        },
        onSuccess: (item) => {
          history.push(routes.admin_notification_settings)
          queryClient.invalidateQueries(['notification-trigger-list'])
          snackbar.show({
            title: t('plainText.notificationCreated'),
            intent: 'positive',
            actions: (
              <Button
                kind="secondary"
                background="transparent"
                onClick={() => onEditNotification(item.id)}
              >
                {t('plainText.edit')}
              </Button>
            ),
          })
        },
      }
    )
  }

  const onEditNotification = useCallback((currentNotificationId) => {
    history.push(
      routes.admin_notification_settings__triggerId_edit(currentNotificationId)
    )
  }, [])

  const reducedQueryStatus = reduceQueryStatuses(
    [categoriesQuery.status, workGroupQuery.status, skillsQuery.status].map(
      (query) => query
    )
  )
  const isMandatoryFieldsSelected =
    selectedNodes.length > 0 &&
    activeFocus &&
    [
      selectedCategories,
      selectedTwinCategoryIds,
      selectedSkills,
      selectedTwins,
    ].some((arr) => (arr ?? []).length > 0)
  const isReadToSubmit = !!(segment === Segment.personal
    ? isMandatoryFieldsSelected
    : isMandatoryFieldsSelected &&
      workgroups
        ?.filter((workgroup) => workgroup.selected)
        .map((workgroup) => workgroup.id).length > 0)

  const onToggleTriggers = useCallback(({ baseUrl, params }) => {
    updateTriggerMutation.mutate(
      {
        baseUrl,
        params,
      },
      {
        onSuccess: () => {
          queryClient.invalidateQueries(['notification-trigger-list'])
        },
        onError: () => {
          snackbar.show({
            title: t('plainText.errorOccurred'),
            description: titleCase({
              text: t('plainText.pleaseTryAgain'),
              language,
            }),
            intent: 'negative',
          })
        },
      }
    )
  }, [])

  const context = {
    myWorkgroups,
    workgroups,
    activeFocus,
    activeModal,
    notification,
    selectedCategories,
    selectedSkills,
    onSkillsChange,
    onCategoriesChange,
    selectedPriorities,
    selectedTwinCategoryIds,
    onTwinsCategoryIdChange,
    onPrioritiesChange,
    onAllowNotificationTurnOff,
    onCreateNotification,
    onToggleTriggers,
    onEditNotification,
    onSelectedNodesChange,
    onSegmentChange,
    selectedNodes,
    segment,
    allowNotificationTurnOff,
    selectedTwins,
    onTwinsChange,
    onModalChange,
    onFocusChange,
    updateWorkgroup,
    onTempWorkgroupsChange,
    onTempSkillsChange,
    onTempNodesChange,
    onTempLocationIdsChange,
    onTempSelectedModelIds,
    tempSelectedWorkgroupIds,
    tempSelectedSkills,
    tempSelectedLocationIds,
    tempSelectedNodes,
    tempSelectedModelIds,
    selectedLocationIds,
    onSelectedLocationIdsChange,
    onSaveModal,
    onResetNotificationTrigger,
    notificationSettingList,
    onDeleteNotificationSettingsTrigger,
    initializeNotificationSettings,
    categories: categoriesQuery.data ?? [],
    skills: skillsQuery.data?.items ?? [],
    queryStatus: reducedQueryStatus,
    ontology,
    scopeLookup,
    isViewOnlyUser,
    isReadToSubmit,
  }

  return (
    <NotificationSettingsContext.Provider value={context}>
      {children}
    </NotificationSettingsContext.Provider>
  )
}
