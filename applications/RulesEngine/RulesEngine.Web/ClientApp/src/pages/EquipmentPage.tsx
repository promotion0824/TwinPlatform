import { Box, Card, CardContent, Grid, Skeleton, Stack, Tab, Tabs, Tooltip, Typography, useTheme } from '@mui/material';
import { ChangeEvent, lazy, Suspense, useState } from 'react';
import { useQuery } from 'react-query';
import { useParams } from 'react-router-dom';
import CapabilitiesTrends from '../components/chart/CapabilitiesTrends';
import SingleTrend from '../components/chart/SingleTrend';
import CopyToClipboardButton from '../components/CopyToClipboard';
import CapabilitiesGrid from '../components/grids/CapabilitesGrid';
import CommandsGrid from '../components/grids/CommandsGrid';
import InsightsGrid from '../components/grids/InsightsGrid';
import PropertiesGrid, { IPropertyDto } from '../components/grids/PropertiesGrid';
import RelatedEntitiesGrid from '../components/grids/RelatedEntitiesGrid';
import RuleInstanceGrid from '../components/grids/RuleInstancesGrid';
import IconForModel from '../components/icons/IconForModel';
import { DateFormatter, ModelFormatter2 } from '../components/LinkFormatters';
import StyledLink from '../components/styled/StyledLink';
import TabPanel from '../components/tabs/TabPanel';
import { TimeSeriesStatusFormatterStatus } from '../components/TimeSeriesStatusFormatter';
import TwinLocations from '../components/TwinLocations';
import useApi from '../hooks/useApi';
import { BatchRequestDto, EquipmentDto } from '../Rules';
import ChipList from '../components/ChipList';
import FlexTitle from '../components/FlexPageTitle';

const TwinGraph = lazy(() => import('../components/graphs/TwinGraph'));

/**
 * Displays all the rules that pertain to a given item of equipment (later a sub-graph)
 * */

/**
 * Displays Skill instances for a single twin
 * @param param0
 * @returns
 */
const RuleInstancesForTwin = ({ id }: { id: string }) => {
  const apiclient = useApi();

  const gridQuery = {
    invokeQuery: (request: BatchRequestDto) => {
      return apiclient.equipmentRuleInstances(id, request);
    },
    downloadCsv: (request: BatchRequestDto) => {
      return apiclient.exportEquipmentRuleInstances(id, request);
    },
    key: id!,
    pageId: 'Equipment'
  };
  return (<RuleInstanceGrid query={gridQuery} />);
}

/**
 * Displays a grid of entities related to this twin id
 * @param param0
 * @returns
 */
const RelatedEntitiesForTwin = ({ equipment }: { equipment: EquipmentDto }) => {
  return (
    <RelatedEntitiesGrid query={{
      equipment: equipment, related: equipment.relatedEntities, inverseRelated: equipment.inverseRelatedEntities, key: equipment?.equipmentId!, pageId: 'Equipment'
    }} />
  );
}

/**
 * Displays a grid of capabilities related to this twin id
 * @param param0
 * @returns
 */
const CapabilitiesForTwin = ({ equipment }: { equipment: EquipmentDto }) => {
  return (<CapabilitiesGrid query={{ equipment: equipment, capabilities: equipment.capabilities, key: equipment?.equipmentId!, pageId: 'Equipment' }} />);
}


interface IKeyValue {
  key: string,
  path: string,
  value: string | undefined
}

/**
 * Converts an object to a string recursively if necessary
 * @param item
 * @returns
 */
const dumpObject = (path: string, key: string, item: any): IKeyValue[] => {
  path = path !== "" ? `${path}.${key}` : key;

  if (typeof (item) === 'string') return [{ key: key, path: path, value: item }];
  if (typeof (item) === 'number' || typeof (item) === 'boolean') return [{ key: key, path: path, value: item.toString() }];
  if (item === null) return [{ key: key, path: path, value: undefined }];

  const keys = Object.keys(item);
  const values: IKeyValue[] = [];

  keys.forEach(v => dumpObject(path, v, Reflect.get(item, v))
    .forEach(s => values.push(s)));

  return values;
};

/**
 * Displays a grid of Id values for this twin id
 * @param param0
 * @returns
 */
const IdsForTwin = ({ equipment }: { equipment: EquipmentDto }) => {
  const theme = useTheme();

  const [value, setValue] = useState(0);
  const handleChange = (_event: ChangeEvent<{}>, newValue: number) => {
    setValue(newValue);
  };

  const SearchLink = (props: { searchInput: string | undefined }) =>
    props.searchInput ?
      (<StyledLink to={"/search/?query=" + encodeURIComponent(props.searchInput)}>
        <Typography color={theme.palette.primary.light}>{props.searchInput}</Typography>
      </StyledLink >)
      : (<>-</>);

  const contents = equipment.contents ?? {};
  const keys = Object.keys(contents).filter(k => k != 'TagString');

  function getModelId(key: string) {
    let property = equipment.properties!.find(v => v.propertyName == key);

    if (!property) {
      property = equipment.properties!.find(v => key.startsWith(`${v.propertyName}.`));
    }

    return property?.modelId ?? equipment.modelId!;
  }

  const properties: IPropertyDto[] = [];

  keys.sort().forEach(key => dumpObject("", key, Reflect.get(contents, key)).forEach(k => {
    properties.push({
      propertyName: k.path,
      propertyValue: k.value,
      propertyLookupKey: k.key,
      modelId: getModelId(k.path)
    });
  }));

  equipment.properties!.forEach((v) => {
    if (!properties.find(p => p.propertyName == v.propertyName)) {
      properties.push({
        propertyName: v.propertyName!,
        propertyLookupKey: v.propertyKey!,
        propertyValue: undefined,
        modelId: v.modelId
      });
    }
  });

  return (equipment) ?
    <>
      <Tabs value={value} onChange={handleChange} aria-label="properties tabs">
        <Tab label="Twin Properties" />
        <Tab label="System Properties" />
      </Tabs>

      <TabPanel value={value} index={0}>
        {value === 0 &&
          <PropertiesGrid showValue={true} showModelId={true} properties={properties} pageId={"Equipment"} />}
      </TabPanel>

      <TabPanel value={value} index={1}>
        {value === 1 &&
          <Card variant="outlined">
            <CardContent>
              <Stack spacing={2}>
                <Box flexGrow={1}>
                  <Grid container spacing={2}>
                    <Tooltip title="This is the unique identifier of any twin inside the ADT instance (it matches ADT property dtId)" placement="left-start">
                      <Grid item xs={2}>
                        <Typography variant="body1">Id:</Typography>
                      </Grid>
                    </Tooltip>
                    <Grid item xs={10}>
                      <Typography variant="body1">{equipment.equipmentId}</Typography>
                    </Grid>

                    <Tooltip title="A guid that links telemetry data to capability (sensor) twins. Not all telemetry data uses this, some uses externalId + connectorId to identify the twin." placement="left-start">
                      <Grid item xs={2}>
                        <Typography variant="body1">TrendId:</Typography>
                      </Grid>
                    </Tooltip>
                    <Grid item xs={10}>
                      <SearchLink searchInput={equipment.trendId} />
                    </Grid>

                    <Tooltip title="A guid used by Command. Primary key? Needed for posting insights." placement="left-start">
                      <Grid item xs={2}>
                        <Typography variant="body1">UniqueId:</Typography>
                      </Grid>
                    </Tooltip>
                    <Grid item xs={10}>
                      <Typography variant="body1">{equipment.equipmentUniqueId}</Typography>
                    </Grid>

                    <Tooltip title="A string value used to match data from the customer side to twins on our side. In mining, for example it is used inside pi-connector (app that transfers data from customer side to ADX)." placement="left-start">
                      <Grid item xs={2}>
                        <Typography variant="body1">ExternalId:</Typography>
                      </Grid>
                    </Tooltip>
                    <Grid item xs={10}>
                      <Typography variant="body1"><SearchLink searchInput={equipment.externalId} /></Typography>
                    </Grid>

                    <Tooltip title="A guid representing the connector that is sending telemetry data. In conjunction with ExternalId this forms a unique id for some telemetry that does not use TrendId as the key." placement="left-start">
                      <Grid item xs={2}>
                        <Typography variant="body1">ConnectorId:</Typography>
                      </Grid>
                    </Tooltip>
                    <Grid item xs={10}>
                      <Typography variant="body1"><SearchLink searchInput={equipment.connectorId} /></Typography>
                    </Grid>

                    <Tooltip title="A guid representing a site. Needed for posting to the insight api. SiteId is a confused concept representing both a customer and/or a building." placement="left-start">
                      <Grid item xs={2}>
                        <Typography variant="body1">SiteId:</Typography>
                      </Grid>
                    </Tooltip>
                    <Grid item xs={10}>
                      <Typography variant="body1">{equipment.siteId}</Typography>
                    </Grid>

                    {equipment.timezone &&
                      <>
                        <Tooltip title="Timezone is only present on buildings." placement="left-start">
                          <Grid item xs={2}>
                            <Typography variant="body1">Timezone:</Typography>
                          </Grid>
                        </Tooltip>
                        <Grid item xs={10}>
                          <Typography variant="body1">{equipment.timezone}</Typography>
                        </Grid>
                      </>
                    }
                  </Grid>
                </Box>
              </Stack>
            </CardContent>
          </Card>}
      </TabPanel>
    </>
    :
    <Skeleton width="98%" height={138} sx={{ margin: '0 auto' }} />
}

/**
 * Displays the summary panel
 * @param param0
 * @returns
 */
const SummaryPanelForTwin = ({ equipment }: { equipment: EquipmentDto }) => {
  const theme = useTheme();

  return (
    <Card sx={{ backgroundColor: theme.palette.background.paper }} >
      <CardContent> {(equipment) ?
        <Box flexGrow={1}>
          <Stack spacing={2}>
            {equipment?.capabilityStatus != null && <Typography variant="body1">Status: {TimeSeriesStatusFormatterStatus(equipment.capabilityStatus)}</Typography>}
            <Typography variant="body1">ID: {equipment.equipmentId} <CopyToClipboardButton content={equipment.equipmentId!} /></Typography>
            {equipment.description && <Typography variant="body1">Description: {equipment.description}</Typography>}
            <Typography variant="body1">Type: <ModelFormatter2 modelId={equipment.modelId!} /> <CopyToClipboardButton content={equipment.modelId!} /></Typography>
            <Typography variant="body1">Last Updated: {DateFormatter(equipment.lastUpdatedOn)}</Typography>
            {equipment.tags &&
              <Stack direction="row" alignItems="center" spacing={1}>
                <Typography variant="body1">Tags: </Typography>
                <ChipList values={equipment.tags.split(',')} />
              </Stack>}
            {equipment.valueExpression &&
              <Box flexGrow={1}>
                <Grid container spacing={1} alignItems="center">
                  <Grid item sm={11}>
                    <Tooltip title={equipment.valueExpression}>
                      <Typography variant="body1" sx={{ textOverflow: 'ellipsis', whiteSpace: 'nowrap', overflow: 'hidden' }}>
                        Expression: {equipment.valueExpression}
                      </Typography>
                    </Tooltip>
                  </Grid>
                  <Grid item sm pl={0}>
                    <CopyToClipboardButton content={equipment.valueExpression} />
                  </Grid>
                </Grid>
              </Box>}
            <TwinLocations locations={equipment.locations} />
          </Stack>
        </Box>
        :
        <Skeleton width="98%" height={138} sx={{ margin: '0 auto' }} />}
      </CardContent>
    </Card>
  );
}

/**
 * Displays a grid of insights relating to a twin
 * @param param0
 * @returns
 */
const InsightsForTwin = ({ id }: { id: string }) => {

  const apiclient = useApi();

  const insightsQuery = {
    invokeQuery: (request: BatchRequestDto) => {
      return apiclient.insightsForEquipment(id, request);
    },
    downloadCsv: (request: BatchRequestDto) => {
      return apiclient.exportInsightsForEquipment(id, request);
    },
    key: id,
    pageId: 'Equipment'
  };

  return (<InsightsGrid query={insightsQuery} />);
}

const CommandsForTwin = ({ id }: { id: string }) => {

  const apiclient = useApi();

  const commandsQuery = {
    invokeQuery: (request: BatchRequestDto) => {
      return apiclient.commandsForEquipment(id, request);
    },
    downloadCsv: (request: BatchRequestDto) => {
      return apiclient.exportCommandsForEquipment(id, request);
    },
    key: id,
    pageId: 'Equipment'
  };

  return (<CommandsGrid query={commandsQuery} />);
}

/**
 * Displays the time series data for a capability
 * @param param0
 * @returns
 */
const ChartForCapability = ({ equipment }: { equipment: EquipmentDto }) => {

  const trendId = equipment.trendId;

  // Mapped doesn't have trendIds yet but all points are PNT....
  if (trendId!?.length > 0 || equipment?.equipmentId!.startsWith('PNT')) {
    return (<SingleTrend id={equipment?.equipmentId!} timezone={equipment.timezone} />);
  }
  else if (equipment.capabilities!?.length > 0) {
    return (<CapabilitiesTrends capabilities={equipment.capabilities!} timezone={equipment.timezone} />);
  }

  return (<p>Trend data missing or not recent</p>)
}


const EquipmentPage = () => {
  const apiclient = useApi();
  const params = useParams<{ id: string, previous1Id: string | undefined, previous2Id: string }>();
  const twinIds: string[] = [params.id ?? 'all', params.previous1Id, params.previous2Id].filter(x => !!x).map(x => x as string);

  const equipmentQuery = useQuery(['equipment', params.id], async () => {
    const data = await apiclient.equipmentWithRelationships(params.id);
    return data;
  });
  const equipment = equipmentQuery.data!;

  const [value, setValue] = useState(0);
  const handleChange = (_event: ChangeEvent<{}>, newValue: number) => {
    setValue(newValue);
  };

  if (!params.id) return (<>No id provided</>);
  if (equipmentQuery.isLoading) return <>Loading...</>;

  return (
    <Stack spacing={2}>
      <FlexTitle>
        <Stack direction="row" alignItems="center" spacing={1}>
          <StyledLink to={"/equipment/all"}> Equipment</StyledLink>
        </Stack>
        <Stack direction="row" alignItems="center" spacing={1}>
          <><IconForModel modelId={equipment.modelId!} size={24} />&nbsp;{equipment.name ?? "No name"}&nbsp;{equipment.unit && <>({equipment.unit})</>}{equipment.isCalculatedPointTwin && <> - (Calculated Point)</>}&nbsp;<CopyToClipboardButton content={equipment.name ?? "No name"} /></>
        </Stack>
      </FlexTitle>
      <SummaryPanelForTwin equipment={equipment} />
      <Box flexGrow={1}>
        <Tabs value={value} onChange={handleChange} aria-label="equipment tabs">
          <Tab label="Graph" />
          <Tab label="Properties" />
          <Tab label="Insights" sx={{ display: equipment.isCalculatedPointTwin ? 'none' : 'inline-flex' }} />
          <Tab label="Commands" />
          <Tab label="Skill Instances" />
          <Tab label="Related Entities" />
          <Tab label="Capabilities" />
          <Tab label="TimeSeries" /> {/* Only for capabilities */}
        </Tabs>

        <TabPanel value={value} index={0}>
          {value === 0 && <Suspense fallback={<div>Loading...</div>}><TwinGraph twinIds={twinIds} /></Suspense>}
        </TabPanel>

        <TabPanel value={value} index={1}>
          {value === 1 && <IdsForTwin equipment={equipment} />}
        </TabPanel>

        <TabPanel value={value} index={2}>
          {value === 2 && <InsightsForTwin id={params.id} />}
        </TabPanel>

        <TabPanel value={value} index={3}>
          {value === 3 && <CommandsForTwin id={params.id} />}
        </TabPanel>

        <TabPanel value={value} index={4}>
          {value === 4 && <RuleInstancesForTwin id={params.id} />}
        </TabPanel>

        <TabPanel value={value} index={5}>
          {value === 5 && <RelatedEntitiesForTwin equipment={equipment} />}
        </TabPanel>

        <TabPanel value={value} index={6}>
          {value === 6 && <CapabilitiesForTwin equipment={equipment} />}
        </TabPanel>

        <TabPanel value={value} index={7}>
          {value === 7 && <ChartForCapability equipment={equipment} />}
        </TabPanel>
      </Box>
    </Stack>
  );
}

export default EquipmentPage;
