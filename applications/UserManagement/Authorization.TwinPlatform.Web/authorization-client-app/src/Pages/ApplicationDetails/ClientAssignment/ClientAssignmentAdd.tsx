import { yupResolver } from '@hookform/resolvers/yup';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import { AxiosError, AxiosResponse } from 'axios';
import { useState } from 'react';
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

export default function ClientAssignmentAdd({ refreshData, application, applicationPermissions }: { refreshData: () => void, application: ApplicationModel, applicationPermissions: PermissionModel[]}) {
  const [open, setOpen] = useState<boolean>(false);
  const loader = useLoading();
  const { enqueueSnackbar } = useCustomSnackbar();
  const { register, handleSubmit, formState: { errors }, reset, control, setValue, setError } = useForm<ClientAssignmentModel>({ resolver: yupResolver(ClientAssignmentModel.validationSchema), defaultValues: new ClientAssignmentModel() });

  const [clientDataSource, setClientDataSource] = useState<{ data: ApplicationClientModel[] }>({ data: [] });
  const OnSearchClient = async (searchText: string) => {
    setClientDataSource({ data: await ApplicationApiClient.GetApplicationClients(application.name, new FilterOptions(searchText, 0, 100)) });
  };

  const AddClientAssignmentRecord: SubmitHandler<ClientAssignmentModel> = async (data: ClientAssignmentModel) => {
    try {
      loader(true, 'Adding Client Assignment');
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

      // set the permissions

      AssignmentClient.AddClientAssignment(data).then((d: AxiosResponse<any, any>) => {
        //Reset Form Values
        reset(new ClientAssignmentModel());
        //refreshTableData
        refreshData();
        // Hide Add Client Assignment Dialog and resetData
        enqueueSnackbar('Client assignment created successfully', { variant: 'success' });
      }).catch((error: AxiosError) => {
        enqueueSnackbar('Error while creating client assignment', { variant: 'error' }, error);
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
      <Button color='primary' variant="contained" onClick={() => { setOpen(true) }}> Add Assignment
      </Button>
      <Modal opened={open} onClose={() => { setOpen(false) }} withCloseButton={false} size="lg" header="Add Assignment">
        <DialogContent>
          <DialogContentText>
            Please select the client and the permissions and click Add to create assignment or cancel to close the dialog
          </DialogContentText>
          <form onSubmit={handleSubmit(AddClientAssignmentRecord)}>
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
                initialOptions={[]}
                onSelectionChange={(x) => setValue('permissions',x)}
                getOptionLabel={(x) => x.name}
                getOptionValue={(x) => x.id}
                title={"Permissions"}
                helperText={errors.permissions?.message as string}
              />

            </Stack>
            <LayoutGroup justify='flex-end' mt="s24">
              <Button type='reset' kind="secondary" onClick={() => setOpen(false)}>Cancel</Button>
              <Button type='submit' kind='primary' variant="contained">Add</Button>
            </LayoutGroup>
          </form>
        </DialogContent>
      </Modal>
    </>
  );
}
