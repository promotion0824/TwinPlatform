import { useQuery } from "react-query";
import useApi from "../../hooks/useApi";
import { Button, Grid, Stack, Typography } from "@mui/material";
import ParamsTile from "../ParamsTile";
import FlexTitle from "../FlexPageTitle";
import StyledLink from "../styled/StyledLink";

const TemplatePickerField = (params: { selectionChanged: (id: any, name: any) => void }) => {
  const apiclient = useApi();

  const { isLoading, data: ruleTemplates = [] } = useQuery('rule-templates', async () => {
    const rt = await apiclient.getRuleTemplates();
    return rt;
  });

  return isLoading ? (
    <div>Loading...</div>
  ) : (
    <Stack spacing={4}>
      <FlexTitle>
        <StyledLink to={"/rules"}>Skills</StyledLink>
        Templates
      </FlexTitle>
      <Grid sx={{ flexGrow: 1 }} justifyContent="center" container gap={2}>
        <Grid item md={12} lg={8}>
          <Stack spacing={4}>
            {ruleTemplates.map(rt => (
              <ParamsTile key={rt.id} style={{ padding: 15 }}>
                <Typography variant="h3">{rt.name}</Typography><br />
                <Typography variant="body1">{rt.description?.split('\n').map((x, i) => <span key={i}>{x}</span>)}</Typography><br />
                <Button variant="contained" color="primary" style={{ float: 'right' }} onClick={(_e: any) => { params.selectionChanged(rt.id, rt.name); }}>Select</Button>
              </ParamsTile>
            ))}
          </Stack>
        </Grid>
      </Grid>
    </Stack>
  );
};

export default TemplatePickerField;
