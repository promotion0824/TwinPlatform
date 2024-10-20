import { useMemo, useState, useEffect } from 'react';
import { Box, SxProps, styled } from '@mui/material';
import { DataGridPro, useGridApiRef, GridColumnVisibilityModel, gridClasses } from '@mui/x-data-grid-pro';
import { usePersistentGridState } from '../../../../hooks/usePersistentGridState';
import { BasicRelationship } from '../../../../services/Clients';

export type TwinRelationshipsType = 'incoming' | 'outgoing';

export default function TwinRelationships({
  relationshipsData,
  type,
}: {
  relationshipsData: BasicRelationship[];
  type: TwinRelationshipsType;
}) {
  // include properties in relationships data, which is concatenated into a string
  let data = relationshipsData.map((relationship) => {
    return { ...relationship, properties: parseProperties(relationship) };
  });

  return (
    <RelationshipsContainer>
      <RelationshipsTable
        data={data}
        sx={relationshipsData.length === 0 ? { height: '162px' } : {}} // bandaid solution for displaying empty table properly
        type={type}
      />
    </RelationshipsContainer>
  );
}

function parseProperties(relationship: BasicRelationship) {
  // get properties from relationships data
  const { $relationshipId, $targetId, $sourceId, $relationshipName, $etag, ...properties } = relationship;
  const propertiesData = properties;

  return Object.entries(propertiesData)
    .map(([key, value]) => `${key}: ${typeof value === 'object' ? JSON.stringify(value) : value}`)
    .join(', ');
}

const RelationshipsContainer = styled('div')({
  padding: '1rem',
  paddingTop: 0,
  paddingLeft: '0.5rem',
  marginTop: '1rem',
});

type Data = (BasicRelationship | { properties: string })[];
/**
 * Table for displaying twin's incoming or outgoing relationships
 */
function RelationshipsTable({ data, sx, type }: { data: Data; sx?: SxProps; type: TwinRelationshipsType }) {
  const apiRef = useGridApiRef();

  const { savedState } = usePersistentGridState(apiRef, `${type}relationships`);

  const columns: any = useMemo(
    () => [
      {
        field: 'relationshipName',
        headerName: 'Relationship Name',
        flex: 1,
        valueGetter: (params: any) => params.row?.$relationshipName || '',
        renderCell: ({ value }: { value: string }) => <StyledBox title={value}>{value}</StyledBox>,
      },
      {
        field: 'properties',
        headerName: 'Properties',
        flex: 1,
        valueGetter: (params: any) => params.row?.properties || '',
        renderCell: ({ value }: { value: string }) => <StyledBox title={value}>{value}</StyledBox>,
      },
      {
        field: 'targetId',
        headerName: 'Target Id',
        flex: 1,
        valueGetter: (params: any) => params.row?.$targetId || '(not set)',
        renderCell: ({ value }: { value: string }) => <StyledBox title={value}>{value}</StyledBox>,
      },
      {
        field: 'sourceId',
        headerName: 'Source Id',
        flex: 1,
        valueGetter: (params: any) => params.row?.$sourceId || '(not set)',
        renderCell: ({ value }: { value: string }) => <StyledBox title={value}>{value}</StyledBox>,
      },
      {
        field: 'relationshipId',
        headerName: 'Id',
        flex: 1,
        valueGetter: (params: any) => params.row?.$relationshipId,
        renderCell: ({ value }: { value: string }) => <StyledBox title={value}>{value}</StyledBox>,
      },
    ],
    // eslint-disable-next-line react-hooks/exhaustive-deps
    []
  );

  const [columnVisibilityModel, setColumnVisibilityModel] = useState<GridColumnVisibilityModel>({
    relationshipId: false,
    sourceId: type === 'incoming',
    targetId: type === 'outgoing',
    relationshipName: true,
  });

  const { columnVisibilityModel: columnVisibilityModelState } = savedState?.columns || {};
  const { relationshipId, sourceId, targetId, relationshipName = true } = columnVisibilityModelState || {};

  useEffect(() => {
    setColumnVisibilityModel({
      relationshipId: false || !!relationshipId,
      sourceId: type === 'incoming' || !!sourceId,
      targetId: type === 'outgoing' || !!targetId,
      relationshipName: !!relationshipName,
      properties: data.some((row) => !!row.properties) || false,
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [type, data]);

  return (
    <div style={{ width: '100%', height: '30vh' }}>
      <StyledDataGrid
        sx={{ ...sx}}
        apiRef={apiRef}
        initialState={savedState}
        rows={data}
        getRowId={(row) => {
          return row.$relationshipId!;
        }}
        columns={columns}
        columnVisibilityModel={columnVisibilityModel}
        onColumnVisibilityModelChange={(newModel) => setColumnVisibilityModel(newModel)}
      />
    </div>
  );
}

const StyledBox = styled(Box)({
  overflowWrap: 'anywhere',
  width: '100%',
  textOverflow: 'ellipsis',
  display: 'inline-block',
  overflow: 'hidden',
});

const StyledDataGrid = styled(DataGridPro)(({ theme }) => ({
  [`& .${gridClasses.row}`]: {
    backgroundColor: 'rgba(23, 23, 23, 0.38)',
  },
}));
