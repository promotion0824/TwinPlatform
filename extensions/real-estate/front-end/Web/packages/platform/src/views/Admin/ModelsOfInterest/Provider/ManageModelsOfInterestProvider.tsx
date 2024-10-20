/* eslint-disable @typescript-eslint/no-non-null-assertion */
import axios from 'axios'
import {
  createContext,
  useContext,
  useState,
  Dispatch,
  SetStateAction,
  useCallback,
  useEffect,
  useRef,
  MutableRefObject,
} from 'react'
import _ from 'lodash'
import { useForm, Control } from 'react-hook-form'
import { useMutation, useQueryClient, UseMutationResult } from 'react-query'
import { useTranslation } from 'react-i18next'
import { GridRowOrderChangeParams } from '@willowinc/ui'
import { getUrl, useSnackbar, useTimer, useUser } from '@willow/ui'
import { ProviderRequiredError } from '@willow/common'
import { useModelsOfInterest } from '@willow/common/twins/view/modelsOfInterest'
import {
  PartialModelOfInterest,
  FormMode,
  ExistingModelOfInterest,
} from '../types'
import usePostModelOfInterest from '../../../../hooks/Customers/usePostModelOfInterest'
import usePutModelOfInterest from '../../../../hooks/Customers/usePutModelOfInterest'
import useDeleteModelOfInterest from '../../../../hooks/Customers/useDeleteModelsOfInterest'

type ManageModelsOfInterestContext = {
  selectedModelOfInterest?: PartialModelOfInterest
  setSelectedModelOfInterest: Dispatch<
    SetStateAction<PartialModelOfInterest | undefined>
  >

  formMode: FormMode
  setFormMode: Dispatch<SetStateAction<FormMode>>

  setExistingModelOfInterest: (modelOfInterest: ExistingModelOfInterest) => void
  existingModelOfInterest?: ExistingModelOfInterest

  handleRevertChange: () => void
  shouldRevertChangeRef: MutableRefObject<boolean>

  control: Control<PartialModelOfInterest>
  submit: () => void

  postModelOfInterestMutation: UseMutationResult<PartialModelOfInterest>
  putModelOfInterestMutation: UseMutationResult
  deleteModelOfInterestMutation: UseMutationResult

  deleteModelOfInterest: () => void

  showConfirmDeleteModal: boolean
  setShowConfirmDeleteModal: (showConfirmDeleteModal: boolean) => void

  moveUp: (modelId: string) => void
  moveDown: (modelId: string) => void
  handleRowOrderChange: ({
    oldIndex,
    targetIndex,
  }: GridRowOrderChangeParams) => void
  isReordering: boolean
}

const ManageModelsOfInterest = createContext<
  ManageModelsOfInterestContext | undefined
>(undefined)

export function useManageModelsOfInterest() {
  const context = useContext(ManageModelsOfInterest)

  if (context == null) {
    throw new ProviderRequiredError('ManageModelsOfInterest')
  }

  return context
}

export default function ManageModelsOfInterestProvider({
  children,
}: {
  children: JSX.Element
}) {
  const { t } = useTranslation()
  const snackbar = useSnackbar()
  const timer = useTimer()
  const queryClient = useQueryClient()
  const user = useUser()
  const customerId = user?.customer?.id

  const modelsOfInterestQuery = useModelsOfInterest()

  const [selectedModelOfInterest, setSelectedModelOfInterest] = useState<
    PartialModelOfInterest | undefined
  >(undefined)

  const [existingModelOfInterest, setExistingModelOfInterest] =
    useState<ExistingModelOfInterest>()

  // Determine if form is for adding new MOI or editing existing MOI. formMode is null when form is not opened.
  const [formMode, setFormMode] = useState(null)

  const [showConfirmDeleteModal, setShowConfirmDeleteModal] = useState(false)

  const { handleSubmit, reset, control, getValues } =
    useForm<PartialModelOfInterest>()

  // After successful POST/PUT/DELETE requests
  function handleHasSucceeded() {
    // refresh models of interest table
    queryClient.invalidateQueries(['modelsOfInterest'])

    // close modals
    if (showConfirmDeleteModal) {
      setShowConfirmDeleteModal(false)
    }
    setSelectedModelOfInterest(undefined)
  }

  // Update useForm's field values with latest input values.
  useEffect(() => {
    if (formMode) reset(selectedModelOfInterest)
  }, [selectedModelOfInterest, formMode, reset])

  const postModelOfInterestMutation = usePostModelOfInterest({
    customerId,
    options: {
      onError: (e) => {
        snackbar.show(e.message)
      },
      onSuccess: () => {
        // Refresh models of interest table after successful POST request
        // Close modal
        handleHasSucceeded()

        snackbar.show(t('plainText.newMOIAdded'), {
          isToast: true,
          closeButtonLabel: t('plainText.dismiss'),
        })
      },
    },
  })

  const reorderMutation = useMutation(
    ({ id, addend }: { id: string; addend: number }) => {
      const currentIndex = _.findIndex(
        modelsOfInterestQuery!.data!.items,
        (m) => m.id === id
      )

      return reorder({
        customerId,
        modelOfInterestId: id,
        newIndex: currentIndex + addend,
        etag: modelsOfInterestQuery!.data!.etag,
      })
    },
    {
      onSuccess: () => {
        queryClient.invalidateQueries(['modelsOfInterest'])
      },
    }
  )

  const { mutate: postModelOfInterest, reset: resetPostModelsOfInterest } =
    postModelOfInterestMutation

  const putModelOfInterestMutation = usePutModelOfInterest({
    customerId,
    options: {
      onError: (e) => {
        snackbar.show(e.message)
      },
      onSuccess: () => {
        // Refresh models of interest table after successful PUT request
        // Close modal
        handleHasSucceeded()
      },
    },
  })

  const { mutate: putModelOfInterest, reset: resetPutModelsOfInterest } =
    putModelOfInterestMutation

  const deleteModelOfInterestMutation = useDeleteModelOfInterest({
    customerId,
    options: {
      onError: (e) => {
        snackbar.show(e.message)
      },
      onSuccess: () => {
        // Refresh models of interest table after successful DELETE request
        // Close modals
        handleHasSucceeded()
      },
    },
  })
  const { reset: resetDeleteModelsOfInterest } = deleteModelOfInterestMutation

  // Explicitly reset mutation state when POST/PUT/DELETE request is successful or failed.
  // Mutation state is used in the modal save button's loading, successful, and error state.
  useEffect(() => {
    const resetMutationState = async () => {
      if (
        postModelOfInterestMutation.isError ||
        postModelOfInterestMutation.isSuccess ||
        putModelOfInterestMutation.isError ||
        putModelOfInterestMutation.isSuccess ||
        deleteModelOfInterestMutation.isError ||
        deleteModelOfInterestMutation.isSuccess
      ) {
        await timer.sleep(1000)
        if (formMode === 'add') resetPostModelsOfInterest()
        if (formMode === 'edit') {
          resetPutModelsOfInterest()
          resetDeleteModelsOfInterest()
        }
      }
    }
    resetMutationState()
  }, [
    postModelOfInterestMutation.isError,
    postModelOfInterestMutation.isSuccess,
    timer,
    resetPostModelsOfInterest,
    putModelOfInterestMutation.isError,
    putModelOfInterestMutation.isSuccess,
    resetPutModelsOfInterest,
    resetDeleteModelsOfInterest,
    deleteModelOfInterestMutation.isError,
    deleteModelOfInterestMutation.isSuccess,
    formMode,
  ])

  const submit = () => {
    const submitter = handleSubmit(() => {
      saveModelOfInterest()
    })
    return submitter()
  }

  const saveModelOfInterest = () => {
    const { modelId, color, text } = getValues()
    if (formMode === 'add') {
      postModelOfInterest({
        modelOfInterest: { modelId, color, text },
        etag: modelsOfInterestQuery!.data!.etag,
      })
    }
    if (formMode === 'edit') {
      putModelOfInterest({
        modelOfInterest: {
          ...existingModelOfInterest,
          color,
          text,
          modelId,
        } as ExistingModelOfInterest,
        etag: modelsOfInterestQuery!.data!.etag,
      })
    }
  }

  const deleteModelOfInterest = () => {
    deleteModelOfInterestMutation.mutate({
      id: existingModelOfInterest!.id,
      etag: modelsOfInterestQuery!.data!.etag,
    })
  }

  // Reset edit form's values
  // Reset edit form with original data
  const shouldRevertChangeRef = useRef(false) // this is a flag that prevents "Maximum update depth exceeded" warning.
  const handleRevertChange = () => {
    setSelectedModelOfInterest(existingModelOfInterest)
    shouldRevertChangeRef.current = true
  }

  const moveUp = useCallback(
    (id: string) => {
      reorderMutation.mutate({ id, addend: -1 })
    },
    [reorderMutation]
  )

  const moveDown = useCallback(
    (id: string) => {
      reorderMutation.mutate({ id, addend: +1 })
    },
    [reorderMutation]
  )

  const handleRowOrderChange = useCallback(
    ({ row, oldIndex, targetIndex }: GridRowOrderChangeParams) => {
      reorderMutation.mutate({ id: row.id, addend: targetIndex - oldIndex })
    },
    [reorderMutation]
  )

  return (
    <ManageModelsOfInterest.Provider
      value={{
        formMode,
        setFormMode,

        selectedModelOfInterest,
        setSelectedModelOfInterest,

        // React-hook-form data
        control,
        submit,

        postModelOfInterestMutation,
        putModelOfInterestMutation,
        deleteModelOfInterestMutation,
        deleteModelOfInterest,

        showConfirmDeleteModal,
        setShowConfirmDeleteModal,

        existingModelOfInterest,
        setExistingModelOfInterest,
        handleRevertChange,
        shouldRevertChangeRef,

        moveUp,
        moveDown,
        handleRowOrderChange,
        isReordering: reorderMutation.isLoading,
      }}
    >
      {children}
    </ManageModelsOfInterest.Provider>
  )
}

/**
 * Calls the server to reorder the models of interest such that the model of
 * interest with id `modelOfInterestId` will have index `newIndex`.
 */
async function reorder({
  customerId,
  modelOfInterestId,
  newIndex,
  etag,
}: {
  customerId: string
  modelOfInterestId: string
  newIndex: number
  etag: string
}) {
  return axios.put(
    getUrl(
      `/api/customers/${customerId}/modelsOfInterest/${modelOfInterestId}/reorder`
    ),
    {
      index: newIndex,
    },
    {
      headers: {
        'If-Match': etag,
      },
    }
  )
}
