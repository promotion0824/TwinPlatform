import { yupResolver } from '@hookform/resolvers/yup';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import Stack from '@mui/material/Stack';
import { useState } from 'react';
import { SubmitHandler, useForm } from 'react-hook-form';
import FormAutoComplete from '../../Components/FormComponents/FormAutoComplete';
import { useCustomSnackbar } from '../../Hooks/useCustomSnackbar';
import { useLoading } from '../../Hooks/useLoading';
import { GroupClient, UserClient } from '../../Services/AuthClient';
import { FilterOptions } from '../../types/FilterOptions';
import { GroupModel } from '../../types/GroupModel';
import { GroupUserModel, GroupUserType } from '../../types/GroupUserModel';
import { UserModel } from '../../types/UserModel';
import { Button, Group, Modal } from '@willowinc/ui';
import { BatchRequestDto, FilterSpecificationDto } from '../../types/BatchRequestDto';

export default function AssignUserToGroup({ groupModel, refreshData }: { groupModel: GroupModel, refreshData: () => void }) {
  const [open, setOpen] = useState<boolean>(false);
  const loader = useLoading();
  const { enqueueSnackbar } = useCustomSnackbar();
  const { handleSubmit, formState: { errors }, reset, control } = useForm<GroupUserType>({ resolver: yupResolver(GroupUserModel.validationSchema), defaultValues: new GroupUserModel() });
  const [usersDataSource, setUsersDataSource] = useState<{ data: UserModel[] }>({ data: [] });

  const AddGroupUserRecordAsync: SubmitHandler<GroupUserType> = async (data: GroupUserType) => {
    data.groupId = groupModel.id;
    try {
      loader(true, 'Assigning user to group');
      // Hide Add Group User Dialog and resetData
      setOpen(false);
      await GroupClient.AddUserToGroup(data);

      //Reset Form Values
      reset(new GroupUserModel());
      //refreshTableData
      refreshData();
      enqueueSnackbar('User assigned to group successfully', { variant: 'success' });

    } catch (e: any) {
      enqueueSnackbar('Error while assigning user to group', { variant: 'error' }, e);
    }
    finally {
      loader(false);
    }
  }

  const OnSearchUser = async (searchText: string) => {
    const batchRequest = new BatchRequestDto();
    if (!!searchText) {
      batchRequest.filterSpecifications.push(new FilterSpecificationDto("lastName", "contains", searchText, "OR"));
      batchRequest.filterSpecifications.push(new FilterSpecificationDto("firstName", "contains", searchText, "OR"));
      batchRequest.filterSpecifications.push(new FilterSpecificationDto("email", "contains", searchText, "OR"));
    }
    let users = await UserClient.GetUsersByGroup(groupModel.id, batchRequest, true);
    setUsersDataSource({ data: users.items });
  };

  return (
    <>
      <Button color='primary' variant="contained" onClick={() => { setOpen(true) }}> Assign
      </Button>
      <Modal opened={open} onClose={() => { setOpen(false) }} withCloseButton={false} size="lg" header="Assign User">
        <DialogContent>
          <DialogContentText>
            Please select a User and click add to assign it to the {groupModel.name} group or click cancel to close the dialog
          </DialogContentText>
          <form onSubmit={handleSubmit(AddGroupUserRecordAsync)}>
            <Stack spacing={2}>

              <FormAutoComplete<GroupUserModel, UserModel>
                control={control}
                label="User"
                errors={errors}
                fieldName='user'
                options={usersDataSource.data}
                OnUpdateInput={OnSearchUser}
                getOptionLabel={(option) => {
                  return UserModel.GetAutocompleteLabel(option);
                }}
                isOptionEqToValue={(option, value) => { return option.id === value.id }}
              />

            </Stack>
            <Group justify='flex-end' mt="s24">
              <Button type='reset' kind="secondary" onClick={() => setOpen(false)}>Cancel</Button>
              <Button type='submit' kind='primary'>Add</Button>
            </Group>
          </form>
        </DialogContent>
      </Modal>
    </>
  );
}
