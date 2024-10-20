/* eslint-disable no-continue */
import { ProviderRequiredError } from '@willow/common'
import { TwinResponse } from '@willow/common/twins/types'
import { getModelInfo } from '@willow/common/twins/utils'
import {
  Model,
  ModelInfo,
  getField,
  isNestedField,
} from '@willow/common/twins/view/models'
import {
  ModelOfInterest,
  useModelsOfInterest,
} from '@willow/common/twins/view/modelsOfInterest'
import {
  Json,
  JsonDict,
  addEmptyFields,
  removeEmptyFields,
} from '@willow/common/twins/view/twinModel'
import isPlainObject from '@willow/common/utils/isPlainObject'
import { api, reduceQueryStatuses, useAnalytics, useSnackbar } from '@willow/ui'
import {
  DurationState,
  toSimplifiedIsoDuration,
} from '@willow/ui/components/DurationInput/DurationInput'
import { useConsole } from '@willow/ui/providers/ConsoleProvider/ConsoleContext'
import _ from 'lodash'
import React, {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useReducer,
} from 'react'
import {
  Control,
  DeepRequired,
  FieldErrorsImpl,
  FieldValues,
  UseFormGetValues,
  UseFormRegister,
  useForm,
} from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { UseQueryResult, useQuery, useQueryClient } from 'react-query'
import useOntology from '../../../../hooks/useOntologyInPlatform'
import mapValuesDeep from '../../../../utils/mapValuesDeep'
import useTwinHistory from './VersionHistory/useTwinHistory'
import merge from './merge'

export type Warranty = Partial<{
  startDate: string
  endDate: string
  provider: string
}>

type ContextType = {
  status: UseQueryResult['status']
  twin?: TwinResponse
  warranty?: Warranty
  canEdit: boolean
  modelInfo?: ModelInfo
  hiddenFields?: string[]
  readOnlyFields?: string[]
  expectedFields: string[]
  missingSensors: string[]
  conflictedFields?: JsonDict
  isEditing: boolean
  isExpanded: boolean
  isSaving: boolean

  // Operations
  beginEditing: () => void
  toggleExpanded: () => void
  submit: (event: any) => Promise<void>
  cancel: () => void

  // React-hook-form data
  control: Control<FieldValues, object>
  values: FieldValues
  dirtyFields: { [field: string]: any }
  register: UseFormRegister<FieldValues>
  getValues: UseFormGetValues<FieldValues>
  errors: FieldErrorsImpl<DeepRequired<FieldValues>>
  modelsOfInterest?: ModelOfInterest[]

  versionHistory: ReturnType<typeof useTwinHistory>
}

export const TwinEditorContext = createContext<ContextType | undefined>(
  undefined
)

export function useTwinEditor() {
  const context = useContext(TwinEditorContext)
  if (context == null) {
    throw new ProviderRequiredError('TwinEditor')
  }
  return context
}

type MergeResult = {
  remoteTwin: JsonDict
  merged: JsonDict
  conflictedFields: JsonDict
}

type State = {
  twin?: TwinResponse
  twinLoadError?: any
  isEditing: boolean
  isExpanded: boolean
  isSaving: boolean
  canEdit: boolean
  mergeResult?: MergeResult
}

type Action =
  | {
      type: 'twinLoadSuccess'
      twin: TwinResponse
      canEdit: boolean
    }
  | {
      type: 'twinLoadError'
      error: any
    }
  | {
      type: 'edit'
    }
  | {
      type: 'cancel'
    }
  | {
      type: 'setExpanded'
      expanded: boolean
    }
  | {
      type: 'save'
    }
  | {
      type: 'saveSuccess'
    }
  | {
      type: 'saveError'
    }
  | {
      type: 'saveEditConflict'
      mergeResult: MergeResult
    }

function reducer(state: State, action: Action): State {
  switch (action.type) {
    case 'twinLoadSuccess':
      return {
        ...state,
        twin: action.twin,
        canEdit: action.canEdit,
        isEditing: false,
        isExpanded: false,
        mergeResult: undefined,
        isSaving: false,
      }
    case 'twinLoadError':
      return {
        ...state,
        twinLoadError: action.error,
        isEditing: false,
        isExpanded: false,
        mergeResult: undefined,
        isSaving: false,
      }
    case 'edit':
      return {
        ...state,
        isEditing: true,
      }
    case 'cancel':
      return {
        ...state,
        isEditing: false,
        isExpanded: false,
        mergeResult: undefined,
      }
    case 'setExpanded':
      return {
        ...state,
        isExpanded: action.expanded,
      }
    case 'save':
      return {
        ...state,
        isSaving: true,
      }
    case 'saveSuccess':
      return {
        ...state,
        isEditing: false,
        isExpanded: false,
        mergeResult: undefined,
        isSaving: false,
      }
    case 'saveError':
      return {
        ...state,
        isSaving: false,
      }
    case 'saveEditConflict':
      return {
        ...state,
        isSaving: false,
        mergeResult: action.mergeResult,
      }
    default:
      return state
  }
}

function getInitialState() {
  return {
    isEditing: false,
    isExpanded: false,
    isSaving: false,
    canEdit: false,
  }
}

/**
 * Provider for the twin editor.
 *
 * - Handles initial retrieval of the twin (by the specified site ID and twin
 *   ID).
 * - Uses react-hook-form to support edits to the data. The react-hook-form data
 *   is separate from the loaded twin, so we can revert without refetching the
 *   original data. The current form data is sent in the `values` property. The
 *   `control` property should be used to hook up widgets to the form
 *   (see https://react-hook-form.com/api/useform/control/#main )
 * - Provides functions for beginning editing, saving, cancelling, supporting
 *   "show more" and "show less".
 */
export function TwinEditorProvider({
  children,
  siteId,
  twinId,
}: {
  children: JSX.Element
  siteId?: string
  twinId: string
}) {
  const snackbar = useSnackbar()
  const queryClient = useQueryClient()
  const translation = useTranslation()
  const { t } = translation
  const analytics = useAnalytics()

  const [
    {
      twin,
      twinLoadError,
      canEdit,
      isEditing,
      isExpanded,
      isSaving,
      mergeResult,
    },
    dispatch,
  ] = useReducer(reducer, getInitialState())

  // We use react-hook-form to manage the current state of the form. As the
  // user makes edits on the form, the form values will diverge from `twin`. On
  // a successful save, `twin` will be updated and we will reload the form to
  // that value.
  const {
    handleSubmit,
    getValues,
    reset,
    control,
    formState: { dirtyFields, errors },
    register,
  } = useForm({
    mode: 'onBlur',
  })

  const ontologyQuery = useOntology()
  const modelsOfInterestQuery = useModelsOfInterest()
  const logger = useConsole()

  const modelId = twin?.metadata?.modelId

  const restrictedFieldsQuery = useQuery(
    ['restricted-fields'],
    async () => {
      const response = await api.get(
        `/sites/${twin?.siteID}/twins/restrictedFields`
      )
      return response.data
    },
    {
      enabled: twin?.siteID != null,
    }
  )

  const dataQualityValidationQuery = useQuery(
    ['validation-fields'],
    async () => {
      const response = await api.get(
        `sites/${twin?.siteID}/twins/${twinId}/dataquality/validations`
      )
      return response.data
    },
    {
      enabled: twin?.siteID != null,
    }
  )

  // When we have the models and the twin data, get the right model out of the
  // lookup
  const modelQuery = useQuery(
    ['models', modelId],
    () => {
      if (
        twin != null &&
        modelId != null &&
        ontologyQuery.data != null &&
        modelsOfInterestQuery.data?.items != null
      ) {
        const ontology = ontologyQuery.data
        const model = ontology.getModelById(modelId)
        return getModelInfo(
          model,
          ontology,
          modelsOfInterestQuery.data.items,
          translation
        )
      }
    },
    {
      enabled:
        twin != null &&
        modelId != null &&
        ontologyQuery.data != null &&
        modelsOfInterestQuery.data?.items != null,
    }
  )

  const values = getValues()

  const getTwin = useCallback(
    (options = {}) =>
      queryClient.fetchQuery(
        [
          'twin',
          siteId,
          twinId,
          +new Date(), // cache bust
        ],
        async () => {
          const response = await api.get(
            siteId
              ? `/v2/sites/${siteId}/twins/${twinId}`
              : `/v2/twins/${twinId}`
          )
          return response.data
        },
        {
          // We assume that the twin object in this query is recreated on a
          // successful save, but with React Query's structural sharing, this does
          // not happen if we saved without changing the twin.
          structuralSharing: false,
          ...options,
        }
      ),
    [queryClient, siteId, twinId]
  )

  const loadTwin = useCallback(async () => {
    let data
    try {
      data = await getTwin()
    } catch (e) {
      logger?.error(e)
      dispatch({
        type: 'twinLoadError',
        error: e,
      })
      return
    }

    // Use `populateDeletedFields` to make sure any fields that we emptied will
    // be removed.
    reset(populateDeletedFields(data.twin, getValues()))

    dispatch({
      type: 'twinLoadSuccess',
      twin: data.twin,
      canEdit: data.permissions?.edit,
    })
  }, [getTwin, getValues, reset, logger])

  const save = useCallback(
    async (newTwin: JsonDict) => {
      if (modelQuery.data == null) {
        throw new Error('Tried to save a twin without model data')
      }

      try {
        dispatch({ type: 'save' })

        const response = await saveTwin({
          existingTwin: mergeResult?.remoteTwin ?? twin ?? {},
          newTwin,
          expandedModel: modelQuery.data.expandedModel,
          ignoreFields: restrictedFieldsQuery.data.readOnlyFields,
        })

        // We need to reload the twin to get the new etag, otherwise if we edit
        // and save again we will get a 412 Precondition Failed.
        loadTwin()

        // Fetch the latest version history that've just been added.
        await queryClient.invalidateQueries([
          'twin-version-history',
          twin?.siteID,
          twinId,
        ])

        return response.data
      } catch (e) {
        if (e.response?.status === 412) {
          // If we received a 412 Precondition Failed, it means someone edited
          // the twin after we opened it, and we need to merge.
          //
          // We combine the local and remote changes into a merged result and
          // show that merged result in the form. We also keep the unmerged
          // remote version, because that's what we will generate a patch
          // against when we save.
          const response = await getTwin()
          const remoteTwin = response.twin
          const newMergeResult = merge(
            twin ?? {},
            { ...twin, ...newTwin },
            remoteTwin
          )
          dispatch({
            type: 'saveEditConflict',
            mergeResult: {
              conflictedFields: newMergeResult.conflictedFields,
              merged: newMergeResult.result,
              remoteTwin,
            },
          })
          reset(
            isExpanded
              ? addEmptyFields(
                  newMergeResult.result,
                  modelQuery.data.expandedModel
                )
              : newMergeResult.result
          )
          snackbar.show(t('plainText.editNotSavedDueToConflict'))
        } else {
          console.error(e)
          dispatch({ type: 'saveError' })
          snackbar.show(t('plainText.tryAgain'))
        }
        return undefined
      }
    },
    [
      twin,
      mergeResult,
      getTwin,
      loadTwin,
      isExpanded,
      reset,
      restrictedFieldsQuery?.data?.readOnlyFields,
      modelQuery?.data?.expandedModel,
      snackbar,
      t,
    ]
  )

  const toggleExpanded = useCallback(() => {
    if (twin == null || modelQuery.data == null) {
      // We should never get here if we don't have the twin or the model
      return
    }

    let newValue: JsonDict | undefined
    if (isExpanded) {
      newValue = removeEmptyFields(getValues(), twin)
    } else {
      newValue = addEmptyFields(getValues(), modelQuery.data.expandedModel)
    }
    reset(newValue, { keepDirty: true, keepErrors: true })
    dispatch({ type: 'setExpanded', expanded: !isExpanded })
  }, [twin, reset, modelQuery?.data?.expandedModel, getValues, isExpanded])

  const beginEditing = useCallback(() => dispatch({ type: 'edit' }), [])

  const submit = useCallback(
    (event) => {
      const submitter = handleSubmit(async (newTwin: JsonDict) => {
        if (twin == null || modelQuery.data == null) {
          // We should never get here if we don't have the twin or the model
          return
        }

        analytics.track('Twin Information Edited', {
          twin: {
            id: twin.id,
            siteID: twin.siteID,
          },
          property_names: Object.keys(newTwin).filter(
            (k) => !_.isEqual(newTwin[k], twin[k])
          ),
        })
        await save(newTwin)
      })
      return submitter(event)
    },
    [analytics, twin, handleSubmit, save]
  )

  const cancel = useCallback(() => {
    dispatch({ type: 'cancel' })
    reset(twin)
    if (mergeResult != null) {
      loadTwin()
    }
  }, [mergeResult, reset, twin, loadTwin])

  useEffect(() => {
    loadTwin()
  }, [loadTwin])

  useEffect(() => {
    if (twin != null) {
      analytics.track('Twin Viewed', { twin })
    }
  }, [twin?.name])

  let status
  if (twinLoadError?.response?.status === 404) {
    status = 'not_found'
  } else if (twinLoadError?.response?.status === 403) {
    status = 'no_permission'
  } else {
    status = reduceQueryStatuses([
      ontologyQuery.status,
      modelQuery.status,
      restrictedFieldsQuery.status,
      modelsOfInterestQuery.status,
      twin != null ? 'success' : twinLoadError != null ? 'error' : 'loading',
    ])
  }

  // extract Warranty from customProperties in twin
  let warranty: Warranty | undefined
  if (
    isPlainObject(twin?.customProperties) &&
    isPlainObject(twin?.customProperties.Warranty)
  ) {
    warranty = twin?.customProperties.Warranty
  }

  const versionHistory = useTwinHistory({
    siteId: twin?.siteID,
    twinId,
  })

  const context: ContextType = {
    status,

    /**
     * The twin as it was retrieved from the server. Edits the user makes to
     * the form will not appear here until they are successfully saved.
     */
    twin: status === 'success' ? twin : undefined,
    warranty,
    canEdit,
    modelInfo: modelQuery.data,
    hiddenFields: restrictedFieldsQuery.data?.hiddenFields,
    readOnlyFields: restrictedFieldsQuery.data?.readOnlyFields,
    expectedFields: dataQualityValidationQuery.data?.missingProperties ?? [],
    missingSensors: dataQualityValidationQuery.data?.missingSensors ?? [],
    conflictedFields: mergeResult?.conflictedFields,
    isEditing,
    isExpanded,
    isSaving,

    // Operations
    beginEditing,
    toggleExpanded,
    submit,
    cancel,

    // React-hook-form data
    control,
    values,
    dirtyFields,
    register,
    getValues,
    errors,

    modelsOfInterest: modelsOfInterestQuery.data?.items,

    versionHistory,
  }

  return (
    <TwinEditorContext.Provider value={context}>
      {children}
    </TwinEditorContext.Provider>
  )
}

/**
 * Return `newOb` but with a null value inserted for each key that exists
 * in `oldOb` but not `newOb`.
 *
 * Eg.
 * >>> populateDeletedFields({}, {"me": 123})
 * {me: null}
 *
 * >>> populateDeletedFields({x: {}}, {x: {y: 234}})
 * {x: {y: null}}
 *
 * We use this to pass to the `reset` method for React Hook Form, since that
 * method will not remove fields from the form if they are absent from the
 * object passed to it. Setting the values to null will cause them to be
 * removed.
 */
export function populateDeletedFields(
  newOb: JsonDict,
  oldOb: JsonDict
): JsonDict {
  function recurse(newO: Json, oldO: Json): Json {
    if (isPlainObject(oldO) && isPlainObject(newO)) {
      return Object.fromEntries([
        ...Object.entries(newO).map(([k, v]) => [k, recurse(v, oldOb[k])]),
        ...Object.entries(oldO)
          .filter(([k]) => !Object.keys(newO).includes(k))
          .map(([k]) => [k, null]),
      ])
    } else {
      return newO
    }
  }

  return recurse(newOb, oldOb) as JsonDict
}

/**
 * Create a JSON Patch document that can be used to update `existingTwin` to
 * `newTwin`. The JSON Patch document may contain add, remove, and replace
 * operations.
 *
 * The most targeted operations will be used. Eg. if `existingTwin` is
 * `{x: {inner: 0}}` and `newTwin` is `{x: {inner: 1}}` then the patch will be
 * `{type: "replace", path: "/x/inner", value: 1}`, not
 * `{type: "replace", path: "/x", value: {inner: 1}}` which would also be
 * technically correct.
 *
 * Null values and empty strings will both result in removes. However if a
 * value is an empty string in both `existingTwin` and `newTwin`, it will not
 * be touched.
 *
 * String values will be converted to numbers if `expandedModel` determines
 * that the field in question is a numeric type.
 *
 * `ignoreFields` is a list of JSON paths (eg. ["/prop", "/someOtherProp"]);
 * fields matching these paths will not be included.
 */
export function createJsonPatch({
  existingTwin,
  newTwin,
  expandedModel,
  ignoreFields,
}: {
  existingTwin: JsonDict
  newTwin: JsonDict
  expandedModel: Model
  ignoreFields: string[]
}): JsonDict[] {
  const operations: JsonDict[] = []

  function hasKey(ob: Json, key: string, { spaceOk = true } = {}) {
    return (
      typeof ob === 'object' &&
      ob != null &&
      !Array.isArray(ob) &&
      ob[key] != null &&
      (spaceOk || ob[key] !== '')
    )
  }

  // eslint-disable-next-line complexity
  function recurse(existingOb: Json, newOb: Json, path: string[]) {
    for (const [k, v] of Object.entries(newOb ?? {})) {
      const pathStr = `/${[...path, k].join('/')}`
      if (ignoreFields.includes(pathStr)) {
        continue
      }
      if (v != null && v !== '' && hasKey(existingOb, k, { spaceOk: true })) {
        const existingVal = (existingOb as JsonDict)[k]
        if (typeof v === 'object') {
          // If both new and existing objects exist, recurse.
          recurse(existingVal, v, [...path, k])
        } else if (v !== existingVal) {
          // If both new and existing non-objects exist, create a replace
          // operation if they are different.
          operations.push({
            op: 'replace',
            path: pathStr,
            value: v,
          })
        }
      } else if (v != null && v !== '') {
        // If the new object exists and is not empty, but the existing object
        // doesn't, generate an add.
        const stripped = withoutEmpties(v)
        if (stripped != null && !_.isEqual(stripped, {})) {
          operations.push({
            op: 'add',
            path: pathStr,
            value: stripped,
          })
        }
      } else if (hasKey(existingOb, k)) {
        // If the existing object existed but the new one is null, generate a remove.
        if (!(v === '' && existingOb?.[k] === '')) {
          operations.push({
            op: 'remove',
            path: pathStr,
          })
        }
      }
    }
    for (const k of Object.keys(existingOb ?? {})) {
      if (typeof newOb !== 'object' || !(k in (newOb ?? {}))) {
        // Generate removes for all the fields in `existingOb` but not in
        // `newOb`.
        operations.push({
          op: 'remove',
          path: `/${[...path, k].join('/')}`,
        })
      }
    }
  }

  const processedTwin = mapValuesDeep(
    prepareFormValues(newTwin, expandedModel),
    (value) => (_.isString(value) ? value.trim() : value)
  )

  recurse(existingTwin, processedTwin, [])
  return operations
}

/**
 * Takes the current form values and transforms them to values suitable for
 * saving. Currently this means:
 * - Converting numeric fields from strings back to numbers
 * - Converting our internal duration type to an ISO duration string
 */
export function prepareFormValues(ob: Json, model: Model): Json {
  function recurse(o: Json, path: string[]): Json {
    if (path.length === 0 || isNestedField(model, path)) {
      if (isPlainObject(o)) {
        return _.mapValues(o, (v, k) => recurse(v, [...path, k]))
      } else if (o != null) {
        throw new Error('Nested field had primitive type')
      } else {
        return o
      }
    } else {
      let field
      try {
        field = getField(model, path)
      } catch (e) {
        // If we couldn't find the field, return the object as is. Might be
        // "id" or "etag" etc.
        return o
      }
      const { schema } = field

      if (
        typeof schema === 'string' &&
        typeof o === 'string' &&
        ['integer', 'long', 'float', 'double'].includes(schema)
      ) {
        if (o !== '') {
          return parseFloat(o)
        } else {
          return null
        }
      } else if (
        typeof schema === 'string' &&
        schema === 'duration' &&
        isPlainObject(o)
      ) {
        return toSimplifiedIsoDuration(o as DurationState)
      } else {
        return o
      }
    }
  }

  return recurse(ob, [])
}

/**
 * Return `ob` but without any null values, empty strings, or objects
 * that don't contain any non-null non-empty-string values.
 */
export function withoutEmpties(ob: Json): Json {
  if (typeof ob === 'object' && ob != null && !Array.isArray(ob)) {
    return _.pickBy(
      _.mapValues(ob, withoutEmpties),
      (v) => v != null && v !== '' && !_.isEqual(v, {})
    )
  } else {
    return ob
  }
}

function saveTwin({
  existingTwin,
  newTwin,
  expandedModel,
  ignoreFields,
}: {
  existingTwin: JsonDict
  newTwin: JsonDict
  expandedModel: Model
  ignoreFields: string[]
}) {
  return api.patch(
    `/sites/${newTwin.siteID}/twins/${newTwin.id}`,
    createJsonPatch({
      existingTwin,
      newTwin,
      expandedModel,
      ignoreFields: [
        ...ignoreFields,
        // Some fields we know we don't want to send. Possibly we might want
        // to get all of these added to the list of hidden fields in the
        // "restricted fields" work.
        // (https://dev.azure.com/willowdev/Unified/_workitems/edit/54008)
        '/etag',
        '/metadata',
      ],
    }),
    {
      headers: {
        'If-Match': newTwin.etag as string,
      },
    }
  )
}
