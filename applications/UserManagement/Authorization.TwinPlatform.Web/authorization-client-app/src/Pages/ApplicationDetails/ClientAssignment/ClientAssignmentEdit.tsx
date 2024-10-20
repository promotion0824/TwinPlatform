import { yupResolver } from '@hookform/resolvers/yup';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import { AxiosError, AxiosResponse } from 'axios';
import { useEffect, useState } from 'react';
import { SubmitHandler, useForm } from 'react-hook-form';
import { Button, Group as LayoutGroup, Modal } from '@willowinc/ui';
import { useLoading } from '../../../Hooks/useLoading';
import { useCustomSnackbar } from '../../../Hooks/useCustomSnackbar';
import { ClientAssignmentModel } from '../../../types/ClientAssignmentModel';
import { PermissionModel } from '../../../types/PermissionModel';
import { ApplicationClientModel } from '../../../types/ApplicationClientModel';
import FormAutoComplete from '../../../Components/FormComponents/FormAutoComplete';
import { ApplicationApiClient, AssignmentClient } from '../../../Services/AuthClient';
import { ApplicationModel } from '../../../types/ApplicationModel';
import { FilterOptions } from '../../../types/FilterOptions';
import CheckboxList from '../../../Components/CheckboxList';
import { ValidateExpressionModel } from '../../../types/ValidateExpressionModel';

export default function ClientAssignmentEdit({ refreshData, editModel, application, applicationPermissions }: { refreshData: () => void, editModel:ClientAssignmentModel, application: ApplicationModel, applicationPermissions: PermissionModel[] }) {
  const [open, setOpen] = useState<boolean>(false);
  const loader = useLoading();
  const { enqueueSnackbar } = useCustomSnackbar();
  const { register, handleSubmit, formState: { errors }, reset, control, setValue, setError } = useForm<ClientAssignmentModel>({ resolver: yupResolver(ClientAssignmentModel.validationSchema), defaultValues: editModel });

  useEffect(() => {
    reset(editModel);
  }, [editModel, reset]);

  const [clientDataSource, setClientDataSource] = useState<{ data: ApplicationClientModel[] }>({ data: [] });
  const OnSearchClient = async (searchText: string) => {
    setClientDataSource({ data: await ApplicationApiClient.GetApplicationClients(application.name, new FilterOptions(searchText, 0, 100)) });
  };

  const EditClientAssignmentRecord: SubmitHandler<ClientAssignmentModel> = async (data: ClientAssignmentModel) => {
    try {
      loader(true, 'Editing Client Assignment');
      // Validate expression
      if (!!data.condition) {
        const validateModel = new ValidateExpressionModel(data.condition);
        const validationResponse = await AssignmentClient.ValidateExpression(validateModel.expression);
        if (validationResponse.data.length > 0) {
          setError('condition', { type: 'validate', message: validationResponse.data.join(',') });
          return;
        }
      }

      setOpen(false);

      AssignmentClient.EditClientAssignment(data).then((d: AxiosResponse<any, any>) => {
        //Reset Form Values
        reset(new ClientAssignmentModel());
        //refreshTableData
        refreshData();
        // Hide Add Client Assignment Dialog and resetData
        enqueueSnackbar('Client assignment updated successfully', { variant: 'success' });
      }).catch((error: AxiosError) => {
        enqueueSnackbar('Error while updating client assignment', { variant: 'error' }, error);
      })
    } catch (e) {
      console.error(e);
    }
    finally {
      loader(false);
    }
  }

  return (
    <>
      <Button color='primary' variant="contained" onClick={() => { setOpen(true) }}> Edit Assignment
      </Button>
      <Modal opened={open} onClose={() => { setOpen(false) }} withCloseButton={false} size="lg" header="Edit Assignment">
        <DialogContent>
          <DialogContentText>
            Please edit the client and the permissions and click submit to update the assignment or cancel to close the dialog
          </DialogContentText>
          <form onSubmit={handleSubmit(EditClientAssignmentRecord)}>
            <Stack>

              <FormAutoComplete<ClientAssignmentModel, ApplicationClientModel>
                control={control}
                label="Client"
                errors={errors}
                fieldName='applicationClient'
                options={clientDataSource.data}
                OnUpdateInput={OnSearchClient}
                getOptionLabel={(option) => {
                  return option.name;
                }}
                isOptionEqToValue={(option, value) => { return option.id === value.id }}
              />

              <TextField
                margin="dense"
                id="expression"
                label="Expression"
                type="text"
                fullWidth
                variant="outlined"
                error={errors.expression ? true : false}
                helperText={errors.expression?.message as string}
                multiline
                {...register('expression')}
              />

              <TextField
                margin="dense"
                id="condition"
                label="Condition"
                type="text"
                fullWidth
                variant="outlined"
                error={errors.condition ? true : false}
                helperText={errors.condition?.message as string}
                multiline
                {...register('condition')}
              />

              <CheckboxList<PermissionModel>
                options={applicationPermissions}
                initialOptions={editModel?.permissions}
                onSelectionChange={(x) => setValue('permissions', x)}
                getOptionLabel={(x) => x.name}
                getOptionValue={(x) => x.id}
                title={"Permissions"}
                helperText={errors.permissions?.message as string}
              />

            </Stack>
            <LayoutGroup justify='flex-end' mt="s24">
              <Button type='reset' kind="secondary" onClick={() => setOpen(false)}>Cancel</Button>
              <Button type='submit' kind='primary' variant="contained">Submit</Button>
            </LayoutGroup>
          </form>
        </DialogContent>
      </Modal>
    </>
  );
}
