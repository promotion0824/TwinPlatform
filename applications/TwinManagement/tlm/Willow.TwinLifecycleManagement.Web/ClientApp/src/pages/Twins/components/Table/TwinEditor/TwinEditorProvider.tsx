import { useCallback, useEffect, useReducer, createContext, useContext, useState } from 'react';
import {
  Control,
  DeepRequired,
  FieldErrorsImpl,
  FieldValues,
  useForm,
  UseFormGetValues,
  UseFormReset,
  UseFormSetValue,
} from 'react-hook-form';
import { BasicDigitalTwin, SourceType } from '../../../../../services/Clients';
import { useQueryClient, useQuery } from 'react-query';
import { Alert, AlertProps, Snackbar } from '@mui/material';
import usePutTwin from '../../../hooks/usePutTwin';
import useOntology, { Model } from '../../../../../hooks/useOntology/useOntology';
import { filterUndefined, addEmptyFields, prepareTwinForPut, prepareTwinForReactHookForm } from './utils';
import useGetTwinById from '../../../hooks/useGetTwinById';
import { useGridApiRef } from '@mui/x-data-grid-pro';

type State = {
  twin?: any;
  isEditing: boolean;
  isSaving: boolean;
  isTwinSaved: boolean;
};

type Action =
  | {
      type: 'twinLoadSuccess';
      twin: any;
    }
  | {
      type: 'edit';
    }
  | {
      type: 'cancel';
    }
  | {
      type: 'setExpanded';
      expanded: boolean;
    }
  | {
      type: 'save';
    }
  | {
      type: 'saveSuccess';
    }
  | {
      type: 'saveError';
    };

function reducer(state: State, action: Action): State {
  switch (action.type) {
    case 'twinLoadSuccess':
      return {
        ...state,
        twin: action.twin,
      };

    case 'edit':
      return {
        ...state,
        isEditing: true,
      };
    case 'cancel':
      return {
        ...state,
        isEditing: false,
      };
    case 'save':
      return {
        ...state,
        isSaving: true,
      };
    case 'saveSuccess':
      return {
        ...state,
        isEditing: false,
        isSaving: false,
        isTwinSaved: true,
      };

    case 'saveError':
      return {
        ...state,
        isSaving: false,
      };

    default:
      return state;
  }
}

function getInitialState() {
  return {
    isEditing: false,
    isExpanded: false,
    isSaving: false,
    canEdit: false,
    isTwinSaved: false,
  };
}

export default function TwinEditorProvider({
  twinData,
  twinId,
  children,
  apiRef,
}: {
  twinData: any;
  twinId?: string;
  children: JSX.Element;
  apiRef?: ReturnType<typeof useGridApiRef>;
}) {
  const [snackbar, setSnackbar] = useState<Pick<AlertProps, 'children' | 'severity'> | null>(null);

  const handleCloseSnackbar = () => setSnackbar(null);

  const queryClient = useQueryClient();
  const [{ twin, isEditing, isSaving, isTwinSaved }, dispatch] = useReducer(reducer, getInitialState());

  const {
    handleSubmit,
    getValues,
    reset,
    control,
    setValue,
    formState: { dirtyFields, errors },
  } = useForm({
    mode: 'onSubmit',
  });

  const modelId = twinData?.$metadata?.$model;

  const ontologyQuery = useOntology();

  // When we have the models and the twin data, get the right model out of the
  // lookup
  const modelQuery = useQuery(
    ['models-ontology', modelId],
    () => {
      const ontology = ontologyQuery.data;
      return { id: modelId, expandedModel: ontology.getExpandedModel(modelId!) };
    },
    {
      enabled: twin !== null && modelId !== null && modelId !== '' && ontologyQuery.data != null,
    }
  );

  const values = getValues();

  let { data: twinById, isSuccess, isFetching } = useGetTwinById(twinId!, undefined, true, SourceType.Adx);

  useEffect(() => {
    const loadTwin = async () => {
      let t = twinData;
      let newTwin = modelQuery?.data?.expandedModel
        ? addEmptyFields(structuredClone(t), modelQuery?.data?.expandedModel)
        : structuredClone(t);

      newTwin = prepareTwinForReactHookForm(newTwin);

      dispatch({
        type: 'twinLoadSuccess',
        twin: newTwin,
      });

      reset(newTwin);
    };

    loadTwin();
  }, [isSuccess, isFetching, getValues, reset, twinById, twinData, modelQuery?.data?.expandedModel, isTwinSaved]);

  const enableEditMode = useCallback(() => {
    dispatch({ type: 'edit' });
    reset(twin);
  }, [reset, twin]);

  const cancel = useCallback(() => {
    dispatch({ type: 'cancel' });
    reset(twin);
  }, [reset, twin]);

  const READONLY_FIELDS = ['$dtId', 'lastUpdateTime', 'uniqueID', 'mappedIds', '$metadata.$model'];
  const IGNORE_FIELDS = ['$etag', '$lastUpdateTime', '$metadata'];

  const { saveTwin } = usePutTwin();

  const save = useCallback(
    async (newTwin: BasicDigitalTwin) => {
      try {
        dispatch({ type: 'save' });
        const response = await saveTwin({
          newTwin: prepareTwinForPut(filterUndefined(newTwin), modelQuery?.data?.expandedModel) as BasicDigitalTwin,
        });
        await new Promise((resolve) => setTimeout(resolve, 2000)); //bandaid fix for latency delay for twin update, delay 2 second before refresh

        // update data grid's row with updated twin
        const savedTwinId = newTwin.$dtId;
        const getSavedRow = await apiRef?.current?.getRow(savedTwinId!);
        await apiRef?.current.updateRows([{ ...getSavedRow, twin: response }]);

        await queryClient.invalidateQueries(['twinById']);

        dispatch({ type: 'saveSuccess' });
      } catch (e) {
        console.error(e);
        setSnackbar({
          children: 'There was an error trying to save your twin. Please try again.',
          severity: 'error',
        });
        dispatch({ type: 'saveError' });
      }
      return undefined;
    }, // eslint-disable-next-line react-hooks/exhaustive-deps
    [queryClient, saveTwin]
  );

  const submit = useCallback(
    (event: any) => {
      const submitter = handleSubmit(async (newTwin: any) => {
        await save(newTwin);
      });
      return submitter(event);
    },
    [handleSubmit, save]
  );

  return (
    <TwinEditorContext.Provider
      value={{
        twin,
        expandedModel: modelQuery.data?.expandedModel,
        twinById,

        // States
        isEditing,
        isSaving,

        // Operations
        enableEditMode,
        cancel,
        submit,

        // React-hook-form data
        control,
        values,
        dirtyFields,
        getValues,
        errors,
        setReactHookFormValue: setValue,
        reset,

        readOnlyFields: READONLY_FIELDS,
        hiddenFields: IGNORE_FIELDS,
      }}
    >
      {children}

      {!!snackbar && (
        <Snackbar
          sx={{ bottom: '100px !important' }}
          open
          anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
          onClose={handleCloseSnackbar}
          autoHideDuration={6000}
        >
          <Alert {...snackbar} onClose={handleCloseSnackbar} variant="filled" />
        </Snackbar>
      )}
    </TwinEditorContext.Provider>
  );
}

type ContextType = {
  twin?: any;
  expandedModel?: Model;
  twinById?: any;

  hiddenFields: string[];
  readOnlyFields: string[];

  isEditing: boolean;
  isSaving: boolean;

  // Operations
  enableEditMode: () => void;
  submit: (event: any) => Promise<void>;
  cancel: () => void;

  // React-hook-form data
  control: Control<FieldValues, object>;
  values: FieldValues;
  dirtyFields: { [field: string]: any };
  getValues: UseFormGetValues<FieldValues>;
  errors: FieldErrorsImpl<DeepRequired<FieldValues>>;
  setReactHookFormValue: UseFormSetValue<FieldValues>;
  reset: UseFormReset<FieldValues>;
};

export const TwinEditorContext = createContext<ContextType | undefined>(undefined);

export function useTwinEditor() {
  const context = useContext(TwinEditorContext);
  if (context == null) {
    throw new Error('useTwinEditor must be used within a TwinEditorProvider');
  }
  return context;
}
