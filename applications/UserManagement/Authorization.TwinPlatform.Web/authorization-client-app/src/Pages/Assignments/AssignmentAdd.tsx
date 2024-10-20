import TextField from '@mui/material/TextField';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import { SubmitHandler, useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { useEffect, useState } from 'react';
import Stack from '@mui/material/Stack';
import { UserFieldNames, UserModel, UserType } from '../../types/UserModel';
import FormAutoComplete from '../../Components/FormComponents/FormAutoComplete';
import { RoleFieldNames, RoleModel } from '../../types/RoleModel';
import { AssignmentModel } from '../../types/AssignmentModel';
import { useLoading } from '../../Hooks/useLoading';
import { AssignmentClient, GroupClient, RoleClient, TwinsClient, UserClient } from '../../Services/AuthClient';
import { FilterOptions } from '../../types/FilterOptions';
import { GroupModel, Group } from '../../types/GroupModel';
import { UserRoleAssignmentModel } from '../../types/UserRoleAssignmentModel';
import { GroupRoleAssignmentModel } from '../../types/GroupRoleAssignmentModel';
import { useCustomSnackbar } from '../../Hooks/useCustomSnackbar';
import { ValidateExpressionModel } from '../../types/ValidateExpressionModel';
import { ILocationTwinModel } from '../../types/SelectTreeModel';
import FormSelectTree from '../../Components/FormComponents/FormSelectTree';
import { FormControlLabel, Grid, Switch } from '@mui/material';
import { Button, ButtonGroup, Group as LayoutGroup, Modal } from '@willowinc/ui';
import { BatchRequestDto, FilterSpecificationDto } from '../../types/BatchRequestDto';

export default function AssignmentAdd({ refreshData, locations }: { refreshData: () => void, locations: ILocationTwinModel[] }) {
  const [open, setOpen] = useState<boolean>(false);
  const loader = useLoading();
  const { enqueueSnackbar } = useCustomSnackbar();
  const { register, handleSubmit, formState: { errors }, setError, reset, control, setValue } = useForm<AssignmentModel>({ resolver: yupResolver(AssignmentModel.validationSchema), defaultValues: new AssignmentModel() });
  const [showRawEditor, SetShowRawEditor] = useState<boolean>(false);
  const [userAndGroupSource, setUserAndGroupSource] = useState<{ data: (UserModel | GroupModel)[] }>({ data: [] });
  const [rolesDataSource, setRolesDataSource] = useState<{ data: RoleModel[] }>({ data: [] });

  const AddUserAssignmentRecordAsync: SubmitHandler<AssignmentModel> = async (data: AssignmentModel) => {
    try {
      loader(true, 'Adding Assignment');

      // Validate expression
      if (!!data.condition) {
        const validateModel = new ValidateExpressionModel(data.condition);
        const validationResponse = await AssignmentClient.ValidateExpression(validateModel.expression);
        if (validationResponse.data.length > 0) {
          setError('condition', { type: 'validate', message: validationResponse.data.join(',') });
          return;
        }
      }

      // Hide Add Group Dialog and resetData
      setOpen(false);

      if (data.userOrGroup instanceof UserModel) {
        let payload = UserRoleAssignmentModel.MapModel(data);
        payload.user = data.userOrGroup;
        await AssignmentClient.AddUserAssignment(payload);
      } else if (data.userOrGroup instanceof GroupModel) {
        let payload = GroupRoleAssignmentModel.MapModel(data);
        payload.group = data.userOrGroup;
        await AssignmentClient.AddGroupAssignment(payload);
      }

      //Reset Form Values
      reset(new AssignmentModel());
      //refreshTableData
      refreshData();

      enqueueSnackbar('Assignment added successfully', { variant: 'success' });

    } catch (e: any) {
      enqueueSnackbar('Error while creating assignment.', { variant: 'error' }, e);
    }
    finally {
      loader(false);
    }
  }

  const OnSearchUserOrGroup = async (searchText: string) => {
    const userFilter = new FilterSpecificationDto(UserFieldNames.firstName.field, "contains", searchText, "AND");
    const users = await UserClient.GetAllUsers(new BatchRequestDto([userFilter]));

    const groupFilter = new FilterSpecificationDto("name", "contains", searchText, "AND");
    const groups = await GroupClient.GetAllGroups(new BatchRequestDto([groupFilter]));

    const userOrGroups: (UserModel | GroupModel)[] = [...users.items.map(m => UserModel.MapModel(m)), ...groups.items.map(m => GroupModel.MapModel(m))];
    setUserAndGroupSource({ data: userOrGroups });
  };

  const OnSearchRole = async (searchText: string) => {
    const roleFilter = new FilterSpecificationDto(RoleFieldNames.name.field, "contains", searchText, "AND");
    const roles = await RoleClient.GetAllRoles(new BatchRequestDto([roleFilter]));
    setRolesDataSource({ data: roles.items });
  };

  return (
    <>
      <Button color='primary' variant="contained" onClick={() => { setOpen(true) }}> Add Assignment
      </Button>
      <Modal opened={open} onClose={() => { setOpen(false) }} withCloseButton={false} size="lg" header="Add Assignment">
        <DialogContent>
          <DialogContentText>
            Please provide assignment details and click Add to create assignment or cancel to close the dialog
          </DialogContentText>
          <form onSubmit={handleSubmit(AddUserAssignmentRecordAsync)}>
            <Stack spacing={2}>
              <FormAutoComplete<AssignmentModel, UserType | Group>
                control={control}
                label="User or Group"
                errors={errors}
                fieldName='userOrGroup'
                options={userAndGroupSource.data}
                OnUpdateInput={OnSearchUserOrGroup}
                getOptionLabel={(option) => {
                  if (option instanceof UserModel)
                    return UserModel.GetAutocompleteLabel(option as UserModel);
                  else if (option instanceof GroupModel)
                    return option.name;
                  return option.id;
                }}
                isOptionEqToValue={(option, value) => { return option.id === value.id }}
              />

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
                defaultRawValue=""
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
                <Button type='reset' kind="secondary" onClick={() => setOpen(false)}>Cancel</Button>
                <Button type='submit' kind='primary' variant="contained">Add</Button>
              </ButtonGroup>
            </LayoutGroup>
          </form>
        </DialogContent>
      </Modal>
    </>
  );
}
