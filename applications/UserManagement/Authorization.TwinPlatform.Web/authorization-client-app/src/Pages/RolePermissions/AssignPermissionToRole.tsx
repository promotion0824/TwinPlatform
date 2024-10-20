import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import { SubmitHandler, useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { useState } from 'react';
import Stack from '@mui/material/Stack';
import FormAutoComplete from '../../Components/FormComponents/FormAutoComplete';
import { RolePermissionModel, RolePermissionType } from '../../types/RolePermissionModel';
import { PermissionFieldNames, PermissionModel } from '../../types/PermissionModel';
import { RoleModel } from '../../types/RoleModel';
import { useLoading } from '../../Hooks/useLoading';
import { PermissionClient, RoleClient } from '../../Services/AuthClient';
import { FilterOptions } from '../../types/FilterOptions';
import { useCustomSnackbar } from '../../Hooks/useCustomSnackbar';
import { Button, Group, Modal } from '@willowinc/ui';
import { BatchRequestDto, FilterSpecificationDto } from '../../types/BatchRequestDto';

export default function AssignPermissionToRole({ roleModel, refreshData }: { roleModel: RoleModel, refreshData: () => void }) {
  const [open, setOpen] = useState<boolean>(false);
  const loader = useLoading();
  const { enqueueSnackbar } = useCustomSnackbar();
  const { handleSubmit, formState: { errors }, reset, control } = useForm<RolePermissionType>({ resolver: yupResolver(RolePermissionModel.validationSchema), defaultValues: new RolePermissionModel() });
  const [permissionsDataSource, setPermissionsDataSource] = useState<{ data: PermissionModel[] }>({ data: [] });

  const AddRoleAssignmentRecordAsync: SubmitHandler<RolePermissionType> = async (data: RolePermissionType) => {
    data.roleId = roleModel.id;
    try {
      loader(true, 'Adding permission to the role');
      // Hide Add Role Permission Dialog and resetData
      setOpen(false);
      await RoleClient.AddPermissionToRole(data);

      //Reset Form Values
      reset(new RolePermissionModel());
      //refreshTableData
      refreshData();
      enqueueSnackbar('Permission added to the role', { variant: 'success' });

    } catch (e: any) {
      enqueueSnackbar('Error while assigning permission to role', { variant: 'error' }, e);
    }
    finally {
      loader(false);
    }
  }

  const OnSearchPermission = async (searchText: string) => {
    const batchRequest = new BatchRequestDto();
    if (!!searchText) {
      batchRequest.filterSpecifications.push(new FilterSpecificationDto(PermissionFieldNames.name.field, "contains", searchText, "OR"));
    }
    let nonMemberPermissions = await PermissionClient.GetPermissionsByRole(roleModel.id, batchRequest, true);

    setPermissionsDataSource({ data: nonMemberPermissions.items });
  };

  return (
    <>
      <Button color='primary' variant="contained" onClick={() => { setOpen(true) }}> Assign
      </Button>
      <Modal opened={open} onClose={() => { setOpen(false) }} withCloseButton={false} size="lg" header="Assign Permission">
        <DialogContent>
          <DialogContentText>
            Please select permission and click add to assign to the {roleModel.name} role or cancel to close the dialog
          </DialogContentText>
          <form onSubmit={handleSubmit(AddRoleAssignmentRecordAsync)}>
            <Stack spacing={2}>

              <FormAutoComplete<RolePermissionModel, PermissionModel>
                control={control}
                label="Permission"
                errors={errors}
                fieldName='permission'
                options={permissionsDataSource.data.sort((a, b) => a.application.name.localeCompare(b.application.name))}
                OnUpdateInput={OnSearchPermission}
                getOptionLabel={(option) => {
                  return option.name;
                }}
                isOptionEqToValue={(option, value) => { return option.id === value.id }}
                groupBy={(option) => option.application.name}
              />

            </Stack>
            <Group justify='flex-end' mt="s24">
              <Button type='reset' kind="secondary" onClick={() => setOpen(false)}>Cancel</Button>
              <Button type='submit' kind='primary' variant="contained">Add</Button>
            </Group>
          </form>
        </DialogContent>
      </Modal>
    </>
  );
}
