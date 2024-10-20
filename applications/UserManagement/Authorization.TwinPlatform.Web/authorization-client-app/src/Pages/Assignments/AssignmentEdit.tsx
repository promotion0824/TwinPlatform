import { yupResolver } from '@hookform/resolvers/yup';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import { useEffect, useState } from 'react';
import { SubmitHandler, useForm } from 'react-hook-form';
import FormAutoComplete from '../../Components/FormComponents/FormAutoComplete';
import { useCustomSnackbar } from '../../Hooks/useCustomSnackbar';
import { useLoading } from '../../Hooks/useLoading';
import { AssignmentClient, GroupClient, RoleClient, TwinsClient, UserClient } from '../../Services/AuthClient';
import { AssignmentModel, AssignmentType } from '../../types/AssignmentModel';
import { FilterOptions } from '../../types/FilterOptions';
import { GroupModel, Group } from '../../types/GroupModel';
import { GroupRoleAssignmentModel } from '../../types/GroupRoleAssignmentModel';
import { RoleFieldNames, RoleModel } from '../../types/RoleModel';
import { UserFieldNames, UserModel, UserType } from '../../types/UserModel';
import { UserRoleAssignmentModel } from '../../types/UserRoleAssignmentModel';
import { ValidateExpressionModel } from '../../types/ValidateExpressionModel';
import { FormControlLabel, Grid, Switch } from '@mui/material';
import FormSelectTree from '../../Components/FormComponents/FormSelectTree';
import { ILocationTwinModel } from '../../types/SelectTreeModel';
import { Button, ButtonGroup, Group as LayoutGroup, Modal } from '@willowinc/ui';
import { BatchRequestDto, FilterSpecificationDto } from '../../types/BatchRequestDto';

export default function AssignmentEdit({ refreshData, getEditModel, locations }: { refreshData: () => void, getEditModel: () => AssignmentModel, locations: ILocationTwinModel[] }) {
  const [open, setOpen] = useState<boolean>(false);
  const loader = useLoading();
  const { enqueueSnackbar } = useCustomSnackbar();
  const { register, handleSubmit, formState: { errors }, setError, reset, control, setValue, getValues } = useForm<AssignmentType>({ resolver: yupResolver(AssignmentModel.validationSchema), defaultValues: getEditModel() });
  const [showRawEditor, SetShowRawEditor] = useState<boolean>(false);
  const [userSource, setUserSource] = useState<{ data: (UserModel)[] }>({ data: [] });
  const [groupSource, setGroupSource] = useState<{ data: (GroupModel)[] }>({ data: [] });
  const [rolesDataSource, setRolesDataSource] = useState<{ data: RoleModel[] }>({ data: [] });

  const EditUserAssignmentRecordAsync: SubmitHandler<AssignmentType> = async (data: AssignmentType) => {
    try {
      loader(true, 'Updating Assignment');

      // Validate expression
      if (!!data.condition) {
        const validateModel = new ValidateExpressionModel(data.condition);
        const validationResponse = await AssignmentClient.ValidateExpression(validateModel.expression);
        if (validationResponse.data.length > 0) {
          setError('condition', { type: 'validate', message: validationResponse.data.join(',') });
          return;
        }
      }

      // Hide Edit Assignment Dialog and resetData
      setOpen(false);

      if (data.type === 'U') {
        const payload = UserRoleAssignmentModel.MapModel(data);
        payload.user = data.userOrGroup as UserModel;
        await AssignmentClient.EditUserAssignment(payload);
      } else if (data.type === 'G') {
        const payload = GroupRoleAssignmentModel.MapModel(data);
        payload.group = data.userOrGroup as GroupModel;
        await AssignmentClient.EditGroupAssignment(payload);
      }
      //Reset Form Values
      reset();
      //refreshTableData
      refreshData();
      enqueueSnackbar('Assignment updated successfully.', { variant: 'success' });

    } catch (e: any) {
      enqueueSnackbar('Error while updating assignment.', { variant: 'error' }, e);
      reset();
    }
    finally {
      loader(false);
    }
  }

  const OnSearchUser = async (searchText: string) => {
    const userFilter = new FilterSpecificationDto(UserFieldNames.firstName.field, "contains", searchText, "AND");
    const users = await UserClient.GetAllUsers(new BatchRequestDto([userFilter]));
    setUserSource({ data: users.items });
  };

  const OnSearchGroup = async (searchText: string) => {
    const groupFilter = new FilterSpecificationDto("name", "contains", searchText, "AND");
    const groups = await GroupClient.GetAllGroups(new BatchRequestDto([groupFilter]));
    setGroupSource({ data: groups.items });
  };

  const OnSearchRole = async (searchText: string) => {
    const roleFilter = new FilterSpecificationDto(RoleFieldNames.name.field, "contains", searchText, "AND");
    const roles = await RoleClient.GetAllRoles(new BatchRequestDto([roleFilter]));
    setRolesDataSource({ data: roles.items });  };

  return (
    <>
      <Button color='primary' variant="contained" onClick={() => { setOpen(true) }}> Edit Assignment
      </Button>
      <Modal opened={open} onClose={() => { setOpen(false) }} withCloseButton={false} size="lg" header="Edit Assignment">
        <DialogContent>
          <DialogContentText>
            Please update assignment details and click Submit to update assignment or cancel to close the dialog
          </DialogContentText>
          <form onSubmit={handleSubmit(EditUserAssignmentRecordAsync)}>
            <Stack spacing={2}>

              {
                getEditModel().type === 'U' ?
                  <FormAutoComplete<AssignmentModel, UserType>
                    control={control}
                    label="User"
                    errors={errors}
                    fieldName='userOrGroup'
                    options={userSource.data}
                    OnUpdateInput={OnSearchUser}
                    getOptionLabel={(option) => {
                      return option.firstName + ' ' + option.lastName
                    }}
                    isOptionEqToValue={(option, value) => { return option.id === value.id }}
                  />
                  :
                  <FormAutoComplete<AssignmentModel, Group>
                    control={control}
                    label="Group"
                    errors={errors}
                    fieldName='userOrGroup'
                    options={groupSource.data}
                    OnUpdateInput={OnSearchGroup}
                    getOptionLabel={(option) => {
                      return option.name;

                    }}
                    isOptionEqToValue={(option, value) => { return option.id === value.id }}
                  />
              }

              <FormAutoComplete<AssignmentModel, RoleModel>
                control={control}
                label="Role"
                errors={errors}
                fieldName='role'
                options={rolesDataSource.data}
                OnUpdateInput={OnSearchRole}
                getOptionLabel={(option) => {
                  return option.name;
                }}
                isOptionEqToValue={(option, value) => { return option.id === value.id }}
              />

              <FormSelectTree
                selectLabel="Location"
                rawLabel="Expression"
                options={locations}
                onChange={(val) => setValue('expression', val)}
                showRawEditor={showRawEditor}
                defaultRawValue={getValues('expression')}
                onError={(errorModel) => {
                  if (errorModel.expParseFailure) {
                    SetShowRawEditor(true);
                  }
                }}
              />

              <TextField
                autoFocus
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
            </Stack>
            <LayoutGroup justify='space-between' mt="s24">
              <FormControlLabel
                control={
                  <Switch
                    checked={showRawEditor}
                    onChange={() => SetShowRawEditor(!showRawEditor)}
                  />
                }
                label={showRawEditor ? 'Expression' : 'Location Twins'}
              />
              <ButtonGroup>
                <Button type='reset' kind='secondary' onClick={() => setOpen(false)}>Cancel</Button>
                <Button type='submit' kind='primary'>Submit</Button>
              </ButtonGroup>
            </LayoutGroup>
          </form>
        </DialogContent>
      </Modal>
    </>
  );
}
