import { useState, useMemo } from 'react';

import { DataGridProProps, GridSearchIcon, GridToolbar } from '@mui/x-data-grid-pro';

import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import CircularProgress, { CircularProgressProps } from '@mui/material/CircularProgress';
import FormControl from '@mui/material/FormControl';
import IconButton from '@mui/material/IconButton';
import InputAdornment from '@mui/material/InputAdornment';
import InputLabel from '@mui/material/InputLabel';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import MenuItem from '@mui/material/MenuItem';
import Select from '@mui/material/Select';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';
import useMediaQuery from '@mui/material/useMediaQuery';

import { DataGrid } from '@willowinc/ui';
import { useQuery, useQueryClient } from 'react-query';
import useApi from '../../hooks/useApi';
import { DocumentSearchMode, DocumentSearchResult, ScoredDocumentChunk } from '../../services/Clients';
import { AuthHandler } from '../../components/AuthHandler';
import { AppPermissions } from '../../AppPermissions';

const DocumentsSearch = () => {
  const [searchTerm, setSearchTerm] = useState('');
  const [paginationModel, setPaginationModel] = useState({
    pageSize: 50,
    page: 0,
  });
  const api = useApi();
  const [searchMode, setSearchMode] = useState<DocumentSearchMode>(DocumentSearchMode.Keyword);
  const queryKey = 'documentSearch';
  const client = useQueryClient();
  const {
    data = [],
    isLoading,
    isFetching,
    refetch,
  } = useQuery(
    queryKey,
    async () => {
      if (searchTerm == null || searchTerm === '') return [];
      const start = performance.now();
      var rawResult = await api.document(
        searchTerm,
        searchMode,
        paginationModel.page * paginationModel.pageSize,
        paginationModel.pageSize
      );
      const results = processResult<DocumentSearchResult>(rawResult.results);
      const end = performance.now();
      const totalChunks = results.reduce(function (accumulator, currentValue) {
        if (currentValue.chunks === undefined) return 0;
        return accumulator + currentValue.chunks.length;
      }, 0);

      const maxScore =
        results.reduce((max, obj) => ((obj.score || 1) > (max.score || 1) ? obj : max), results[0]).score ?? 1;
      // Preprocess the score in to %
      const setScore = (res: any) => {
        res.score = Number(res.score === undefined ? 0 : ((res.score / maxScore) * 100).toFixed(2));
      };
      results.forEach(function (res) {
        setScore(res);
        res.chunks?.forEach(setScore);
      });

      SetSummary(
        `Total Chunks: ${totalChunks}.\n Total Documents: ${results.length}.\n Total Execution time: ${
          end - start
        } milliseconds. MaxScore: ${maxScore}`
      );
      return results;
    },
    {
      onError: undefined,
    }
  );
  const isSmallScreen = useMediaQuery('(max-width: 600px)');
  const isMediumScreen = useMediaQuery('(max-width: 900px)');
  const docSearchColumns: any = useMemo(
    () => [
      { field: 'title', headerName: 'Document Name', width: isSmallScreen ? 200 : isMediumScreen ? 300 : 400 },
      {
        field: 'score',
        headerName: 'Score (%)',
        width: 100,
        valueGetter: (params: any) => params.row.score,
        renderCell: ({ value }: { value: string }) => <CircularProgressWithLabel value={Number(value)} />,
      },
      {
        field: 'chunks',
        headerName: 'Data',
        width: 800,
        valueGetter: (params: any) => params.row.chunks[0].chunk || '',
      },
      { field: 'lastModified', headerName: 'Last Modified', width: 200 },
    ],
    []
  );
  const [summary, SetSummary] = useState<string>('');

  // Function to manually trigger refetch on button click
  const handleSearch = () => {
    refetch();
  };

  const handleCancelSearch = () => {
    client.cancelQueries(queryKey);
  };

  const handleEnterKeyPress = (event: any) => {
    if (isFetching) return;
    if (event.key === 'Enter') {
      handleSearch();
    }
  };

  function processResult<T>(data: { [key: string]: T } | undefined): T[] {
    const res: T[] = [];
    if (data) {
      Object.keys(data).forEach((key) => {
        res.push(data[key]);
      });
    }
    return res;
  }
  //c.score === undefined ? 0 : ((c.score)/dynamicMaxScore)
  const getDetailPanelContent: DataGridProProps['getDetailPanelContent'] = ({ row }) => (
    <Box sx={{ width: '100%' }}>
      <List>
        {row.chunks != null ? (
          row.chunks.map((c: ScoredDocumentChunk) => (
            <ListItem disablePadding>
              <ListItemIcon style={{ paddingLeft: '20px' }}>
                <CircularProgressWithLabel value={c.score || 0}></CircularProgressWithLabel>
              </ListItemIcon>
              <ListItemButton>
                <ListItemText primary={c.chunk} />
              </ListItemButton>
            </ListItem>
          ))
        ) : (
          <ListItem disablePadding>
            <ListItemButton>
              <ListItemText primary={'Missing Content.'} />
            </ListItemButton>
          </ListItem>
        )}
      </List>
    </Box>
  );
  const getDetailPanelHeight: DataGridProProps['getDetailPanelHeight'] = () => 'auto';

  return (
    <AuthHandler requiredPermissions={[AppPermissions.CanSearchDocuments]} noAccessAlert>
      <div>
        <div
          style={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            height: '6vh',
            width: '100%',
            backgroundColor: '#242424',
            marginTop: 20,
          }}
        >
          <TextField
            label="Search"
            variant="outlined"
            value={searchTerm}
            size="medium"
            onChange={(e) => setSearchTerm(e.target.value)}
            style={{ width: '65vw' }}
            disabled={isFetching}
            onKeyDown={handleEnterKeyPress}
            InputProps={{
              endAdornment: (
                <InputAdornment position="end">
                  {isFetching ? (
                    <CircularProgress disableShrink size="2rem" />
                  ) : (
                    <IconButton title="Search" onClick={handleSearch}>
                      <GridSearchIcon />
                    </IconButton>
                  )}
                </InputAdornment>
              ),
            }}
          />
          <FormControl variant="filled">
            <InputLabel id="search-type-select-label">Search Mode</InputLabel>
            <Select
              labelId="search-type-select-label"
              id="search-type-select"
              value={searchMode}
              label="Search Mode"
              onChange={(evt) => setSearchMode(evt.target.value as DocumentSearchMode)}
              style={{ width: '12vw' }}
            >
              {Object.values(DocumentSearchMode).map((mode) => (
                <MenuItem key={mode} value={mode}>
                  {mode}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
          {isFetching ? (
            <Button
              variant="outlined"
              color="error"
              onClick={handleCancelSearch}
              style={{ width: '12vw', height: '100%' }}
            >
              Cancel
            </Button>
          ) : (
            <Button variant="contained" onClick={handleSearch} style={{ width: '12vw', height: '100%' }}>
              Search
            </Button>
          )}
        </div>
        <div style={{ height: '75vh', width: '100%', backgroundColor: '#242424', marginTop: 20 }}>
          <DataGrid
            rows={data}
            columns={docSearchColumns}
            paginationModel={paginationModel}
            onPaginationModelChange={setPaginationModel}
            pageSizeOptions={[50, 100, 500]}
            slots={{ toolbar: GridToolbar }}
            loading={isLoading}
            getDetailPanelHeight={getDetailPanelHeight}
            getDetailPanelContent={getDetailPanelContent}
          />
        </div>
        <Typography>{summary}</Typography>
      </div>
    </AuthHandler>
  );
};

export default DocumentsSearch;

function CircularProgressWithLabel(props: CircularProgressProps & { value: number }) {
  return (
    <Box sx={{ position: 'relative', display: 'inline-flex' }}>
      <CircularProgress variant="determinate" {...props} />
      <Box
        sx={{
          top: 0,
          left: 0,
          bottom: 0,
          right: 0,
          position: 'absolute',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
        }}
      >
        <Typography variant="caption" component="div" color="text.secondary">{`${props.value}`}</Typography>
      </Box>
    </Box>
  );
}
