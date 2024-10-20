import React, { useState } from 'react';
import { Button, Box, FormControl, Typography, Stack, TextField } from '@mui/material';
import useApi from '../../hooks/useApi';
import { StyledHeader } from '../../components/Common/StyledComponents';

const ModelsDeletionById = () => {
  const api = useApi();
  const [modelId, setModelId] = useState('');
  const [userData, setUserData] = useState('');

  const submitDeleteRequest = () => {};

  return (
    <>
      <Typography variant="h1">Model ID:</Typography>
      <FormControl fullWidth required sx={{ minWidth: 120, maxWidth: '30%' }}>
        <TextField
          fullWidth
          id="filled-basic"
          label="Model Id"
          variant="filled"
          onChange={(event) => setModelId(event.target.value)}
        />
      </FormControl>

      <Box sx={{ m: 5 }}> </Box>
      <Typography variant="h1">Deletion reason (optional):</Typography>
      <FormControl
        fullWidth
        required
        sx={{
          minWidth: 120,
          maxWidth: '50%',
        }}
      >
        <TextField
          fullWidth
          label="Comment"
          variant="filled"
          value={userData}
          onChange={(event) => setUserData(event.target.value)}
        />
      </FormControl>
      <Box sx={{ m: 5 }}> </Box>
      <Button sx={{ maxWidth: '90%' }} onClick={submitDeleteRequest} variant="contained" size="large">
        Delete
      </Button>
    </>
  );
};

const DeleteModelsByIdPage = () => {
  return (
    <div style={{ width: '100%' }}>
      <Stack direction="column" justifyContent="flex-start" alignItems="flex-start" spacing={2}>
        <StyledHeader variant="h2">Delete models</StyledHeader>
        <Box sx={{ m: 5 }}> </Box>
        <ModelsDeletionById />
      </Stack>
    </div>
  );
};

export { DeleteModelsByIdPage };
