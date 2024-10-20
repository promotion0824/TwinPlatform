import { Card, DataGrid, DataGridProps, GridValidRowModel } from "@willowinc/ui";
import styled from "styled-components";

export const CustomDataGrid = (props: DataGridProps<GridValidRowModel>) => {
  return (
    <Card background="panel" style={{ flexGrow: 1, overflow: 'hidden', display: 'inline-grid' }}>
      <StyledDataGrid {...props} />
    </Card>
  );
}


const StyledDataGrid = styled(DataGrid)({
  "&&&": {
    border: "0px",
  },
  "&.MuiDataGrid-root .MuiDataGrid-cell:focus-within": {
    outline: "none !important",
  },
  "&.MuiDataGrid-root .MuiDataGrid-columnHeader:focus": {
    outline: "none !important",
  },
  "&.MuiDataGrid-root .MuiDataGrid-columnHeader:focus-within": {
    outline: "none !important",
  },
});
