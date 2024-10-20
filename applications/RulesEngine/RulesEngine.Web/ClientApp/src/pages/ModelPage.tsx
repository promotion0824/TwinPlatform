import { Box, Card, CardContent, Stack, Tab, Tabs, Typography, useTheme } from '@mui/material';
import { useState } from 'react';
import { useQuery } from 'react-query';
import { useParams } from 'react-router-dom';
import CopyToClipboardButton from '../components/CopyToClipboard';
import FlexTitle from '../components/FlexPageTitle';
import InheritanceGraph from '../components/graphs/InheritanceGraph';
import ModelGraph from '../components/graphs/ModelGraph';
import InsightsGrid from '../components/grids/InsightsGrid';
import PropertiesGrid, { IPropertyDto } from '../components/grids/PropertiesGrid';
import RuleInstancesGrid from '../components/grids/RuleInstancesGrid';
import RulesGrid from '../components/grids/RulesGrid';
import TwinsByModelGrid from '../components/grids/TwinsByModelGrid';
import IconForModel from '../components/icons/IconForModel';
import StyledLink from '../components/styled/StyledLink';
import TabPanel from '../components/tabs/TabPanel';
import useApi from '../hooks/useApi';
import { BatchRequestDto } from '../Rules';

const ModelPage = () => {

  const theme = useTheme();
  const params = useParams<{ id: string }>();

  const apiclient = useApi();

  var singleModelQuery = useQuery(["model", params.id], async (_x) => {
    const singleModel = await apiclient.model(params.id);
    return singleModel;
  });

  const insightsQuery = {
    invokeQuery: (request: BatchRequestDto) => {
      return apiclient.insightsForModel(params.id, request);
    },
    downloadCsv: (request: BatchRequestDto) => {
      return apiclient.exportInsightsForModel(params.id, request);
    },
    key: params.id ?? "",
    pageId: 'Model'
  };

  const rulesQuery = {
    invokeQuery: (request: BatchRequestDto) => {
      return apiclient.rulesForModel(params.id, request);
    },
    downloadCsv: (request: BatchRequestDto) => {
      return apiclient.exportRulesForModel(params.id, request);
    },
    key: params.id ?? "",
    pageId: 'Model'
  };

  const ruleInstancesQuery = {
    invokeQuery: (request: BatchRequestDto) => {
      return apiclient.ruleInstancesForModel(params.id, request);
    },
    downloadCsv: (request: BatchRequestDto) => {
      return apiclient.exportRuleInstancesForModel(params.id, request);
    },
    key: params.id ?? "",
    pageId: 'Model'
  };
  // TABS
  const [value, setValue] = useState(0);

  const handleChange = (_event: React.ChangeEvent<{}>, newValue: number) => {
    setValue(newValue);
  };

  if (singleModelQuery.isFetched && params.id) {

    const data = singleModelQuery.data;

    const properties = data!.properties!.map(k => {
      return {
        propertyName: k.propertyName,
        propertyType: k.propertyType,
        modelId: k.modelId
      } as IPropertyDto
    });

    return (
      <Stack spacing={2}>
        <FlexTitle>
          <Stack direction="row" alignItems="center" spacing={1}>
            <StyledLink to={"/models"}>Models</StyledLink>
          </Stack>
          <Stack direction="row" alignItems="center" spacing={1}>
            <><IconForModel modelId={data?.id!} size={24} />&nbsp;{data?.languageDisplayNames!['en'] ?? "No name"}&nbsp;<CopyToClipboardButton content={data?.languageDisplayNames!['en'] ?? "No name"} /></>
          </Stack>
        </FlexTitle>
        <Card sx={{ backgroundColor: theme.palette.background.paper }} >
          <CardContent>
            <Box flexGrow={1}>
              <Stack spacing={2}>
                <Typography variant="body1">ID: {data?.id} <CopyToClipboardButton content={data?.id!} /></Typography>
                {data && data.languageDescriptions && <Typography variant="body1">Description: {(data.languageDescriptions['en'] ?? "")}</Typography>}
                {data?.decommissioned && <Typography variant="body1">Decomissioned</Typography>}
              </Stack>
            </Box>
          </CardContent>
        </Card>

        <Box flexGrow={1}>
          <Tabs value={value} onChange={handleChange} aria-label="simple tabs example">
            <Tab label="Graph" />
            <Tab label="Inheritance" />
            <Tab label="Properties" />
            <Tab label="Insights" />
            <Tab label="Skills" />
            <Tab label="Skill Instances" />
            <Tab label="Entities" />
          </Tabs>
          <TabPanel value={value} index={0}>
            <ModelGraph modelId={params.id} />
          </TabPanel>

          <TabPanel value={value} index={1}>
            <InheritanceGraph modelId={params.id} />
          </TabPanel>

          <TabPanel value={value} index={2}>
            <PropertiesGrid showType={true} showModelId={true} properties={properties} pageId={"Model"} />
          </TabPanel>

          <TabPanel value={value} index={3}>
            <InsightsGrid query={insightsQuery} />
          </TabPanel>

          <TabPanel value={value} index={4}>
            <RulesGrid query={rulesQuery} />
          </TabPanel>

          <TabPanel value={value} index={5}>
            <RuleInstancesGrid query={ruleInstancesQuery} />
          </TabPanel>

          <TabPanel value={value} index={6}>
            <TwinsByModelGrid props={{ modelId: params.id, pageId: 'Model' }} />
          </TabPanel>
        </Box>
      </Stack>
    );
  }
  else if (params.id) {
    return <div>Loading...{params.id}</div>
  } else {
    return <Typography variant="caption">No model</Typography>
  }
}

export default ModelPage;
