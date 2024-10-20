import { Box, Grid } from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import useLocalStorage from "../hooks/uselocalstorage";
import { DataGrid, GridColumnVisibilityModel, GridFilterModel, GridPaginationModel, GridSortModel } from "@mui/x-data-grid";
import { IBatchRequestDto, mapFilterSpecifications, mapSortSpecifications, numberOperators, stringOperators } from "../hooks/gridfunctions";
import { useParams } from "react-router-dom";

const AssignmentSingle = () => {

  const fetchMapping = async (id: string) => {
    const res = await fetch("/api/mapping/get-mapping/" + encodeURIComponent(id), {
      method: "GET"
    });
    return res.json();
  };

  const props = useParams();

  const {
    isLoading,
    data,
    isFetching,
    isError
  } = useQuery(['single', props.id],
    async () => fetchMapping(props.id ?? ""), { keepPreviousData: true })

  if (!props.id) return (<p>Not found</p>);

  console.log('Page', props.id, data);

  if (isLoading || !data) return (<div>Loading...</div>);
  if (isError || !data) return (<div>No such mapping...</div>);


  return (<>

    <h2>Score {data.score.toPrecision(3)}</h2>

    <Grid container rowSpacing={1} columnSpacing={{ xs: 1, sm: 2, md: 3 }}>
      <Grid item xs={6}>
        <h2>Mapped</h2>
      </Grid>
      <Grid item xs={6}>
        <h2>Willow</h2>
      </Grid>
      <Grid item xs={6}>
        {data.source.id}
      </Grid>
      <Grid item xs={6}>
        {data.destination.id}
      </Grid>
      <Grid item xs={6}>
        <b>{data.source.displayName.en}</b>
      </Grid>
      <Grid item xs={6}>
        <b>{data.destination.displayName.en}</b>
      </Grid>
      <Grid item xs={6}>
        {data.source.ancestors.map((x: any) => (<p key={x}>{x}</p>))}
      </Grid>
      <Grid item xs={6}>
        {data.destination.ancestors.map((x: any) => (<p key={x}>{x}</p>))}
      </Grid>
    </Grid>
  </>);



}

export default AssignmentSingle;
