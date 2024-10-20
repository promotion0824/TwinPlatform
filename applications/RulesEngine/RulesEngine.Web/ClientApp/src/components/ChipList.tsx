import { Stack, Chip } from "@mui/material";

const ChipList = (params: { values: string[] | undefined }) => {
  const labels = params.values ?? [];

  if (labels.length == 0) {
    return (<></>);
  }

  return (
    <Stack direction="row" alignItems="center" spacing={1} sx={{ flexWrap: 'wrap' }}>
    {labels.map((labelText) => (
        <Chip variant="outlined" label={labelText} size="small" sx={{ fontSize: '12px' }}/>
    ))}
    </Stack>)
}

export default ChipList
