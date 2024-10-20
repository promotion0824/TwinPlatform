import { yupResolver } from '@hookform/resolvers/yup';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import { AxiosError, AxiosResponse } from 'axios';
import { useState } from 'react';
import { SubmitHandler, useForm } from 'react-hook-form';
import { useCustomSnackbar } from '../../Hooks/useCustomSnackbar';
import { useLoading } from '../../Hooks/useLoading';
import { GroupClient, GroupTypeClient } from '../../Services/AuthClient';
import { GroupModel, Group } from '../../types/GroupModel';
import { GroupTypeModel } from '../../types/GroupTypeModel';
import FormAutoComplete from '../../Components/FormComponents/FormAutoComplete';
import { Button, Group as LayoutGroup, Modal } from '@willowinc/ui';

export default function GroupAdd({ refreshData }: { refreshData: () => void }) {
  const [open, setOpen] = useState<boolean>(false);
  const loader = useLoading();
  const { enqueueSnackbar } = useCustomSnackbar();
  const { register, handleSubmit, formState: { errors }, reset, control } = useForm<Group>({ resolver: yupResolver(GroupModel.validationSchema), defaultValues: new GroupModel() });
  const [groupTypeDataSource, setGroupTypeDataSource] = useState<{ data: GroupTypeModel[] }>({ data: [] });

  const OnSearchGroupType = async (searchText: string) => {
    if (groupTypeDataSource.data.length > 0) {
      return;
    }
    let groupTypes = await GroupTypeClient.GetAllGroupTypes();
    setGroupTypeDataSource({ data: groupTypes });
  };
  const AddGroupRecordAsync: SubmitHandler<Group> = async (data: Group) => {
    try {
      loader(true, 'Adding Group');
      setOpen(false);

      // Set Ids
      data.groupTypeId = data.groupType!.id;

      GroupClient.AddGroup(data).then((d: AxiosResponse<any, any>) => {
        //Reset Form Values
        reset(new GroupModel());
        //refreshTableData
        refreshData();
        // Hide Add Group Dialog and resetData
        enqueueSnackbar('Group created successfully', { variant: 'success' });
      }).catch((error: AxiosError) => {
        enqueueSnackbar('Error while creating the group.', { variant: 'error' }, error);
      }).finally(() => {
        loader(false);
      });
    } catch (e) {
      console.error(e);
    }
  }

  return (
    <>
      <Button color='primary' variant="contained" onClick={() => { setOpen(true) }}> Add Group
      </Button>
      <Modal opened={open} onClose={() => { setOpen(false) }} withCloseButton={false} size="lg" header="Add Group">
        <DialogContent>
          <DialogContentText>
            Please provide a group name and click Add to create a group or cancel to close the dialog
          </DialogContentText>
          <form onSubmit={handleSubmit(AddGroupRecordAsync)}>
            <Stack spacing={2}>

              <TextField
                autoFocus
                margin="dense"
                id="name"
                label="Group Name"
                type="text"
                fullWidth
                variant="outlined"
                error={errors.name ? true : false}
                helperText={errors.name?.message as string}
                {...register('name')}
              />

              <FormAutoComplete<GroupModel, GroupTypeModel>
                control={control}
                label="Type"
                errors={errors}
                fieldName='groupType'
                options={groupTypeDataSource.data}
                OnUpdateInput={OnSearchGroupType}
                getOptionLabel={(option) => {
                  return option.name;
                }}
                isOptionEqToValue={(option, value) => { return option.id === value.id }}
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
