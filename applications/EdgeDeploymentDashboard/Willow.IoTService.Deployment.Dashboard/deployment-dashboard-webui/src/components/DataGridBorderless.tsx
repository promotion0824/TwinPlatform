import { DataGrid } from "@willowinc/ui";
import styled from "styled-components";

export const DataGridBorderless = styled(DataGrid)({
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
