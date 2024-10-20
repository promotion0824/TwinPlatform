import { CalculatedPointDto, CapabilityDto, CommandDto, InsightDto, RelatedEntityDto, RuleInstanceDto, RuleInstanceStatus, RuleParameterBoundDto } from '../Rules';
import IconForModel from './icons/IconForModel';
import { FaCheckCircle, FaCircle, FaCheck } from 'react-icons/fa';
import { Box, Stack, Tooltip } from '@mui/material';
import StyledLink from './styled/StyledLink';
import { RuleInstanceStatusLookup } from './Lookups';
import moment from 'moment';

export const stripPrefix = (model: string) => model
  .replace('dtmi:digitaltwins:rec_3_3:asset:', '')
  .replace('dtmi:digitaltwins:rec_3_3:building:', '')
  .replace('dtmi:com:willowinc:mining:', '')
  .replace('dtmi:com:willowinc:airport:', '')
  .replace('dtmi:com:willowinc:', '')
  .replace(';1', '');


export const LinkFormatter = (a: InsightDto) => (<StyledLink to={"/equipment/" + encodeURIComponent(a.equipmentId!)}> {a.equipmentId} </StyledLink >);
export const TwinLinkFormatterById = (id: string, text: string) => (<StyledLink to={"/equipment/" + encodeURIComponent(id)}> {text} </StyledLink >);
export const InsightLinkFormatter = (a: InsightDto) => (<StyledLink to={"/insight/" + encodeURIComponent(a.id!)}> {a.text} </StyledLink >);
export const InsightLinkFormatterById = (id: string, text: string) => (<StyledLink to={"/insight/" + encodeURIComponent(id)}> {text} </StyledLink >);
export const RuleInstanceLinkFormatter = (a: InsightDto) => (<StyledLink to={"/ruleinstance/" + encodeURIComponent(a.ruleInstanceId!)}> {a.ruleName} </StyledLink>);
export const RuleInstanceLinkFormatterById = (id: string, text: string) => (<StyledLink to={"/ruleinstance/" + encodeURIComponent(id)}> {text} </StyledLink>);
export const InsightLinkFormatterOnRuleName = (a: InsightDto) => (<StyledLink to={"/insight/" + encodeURIComponent(a.id!)}> {a.ruleName} </StyledLink>);
export const InsightLinkFormatterOnEquipment = (a: InsightDto) => (<StyledLink to={"/insight/" + encodeURIComponent(a.id!)}> {a.equipmentId ?? a.text} </StyledLink >);
export const CommandLinkFormatter = (a: CommandDto) => (<StyledLink to={"/command/" + encodeURIComponent(a.id!)}> {a.commandName} </StyledLink>);
export const CommandSyncFormatter = (commandEnabled?: boolean) => (<span style={{ fontSize: 14, color: (commandEnabled ? "green" : "grey") }}> {commandEnabled ? <FaCheck /> : <FaCircle />}</span>);
export const InsightFaultyFormatter = (a: InsightDto) => (!a.isFaulty ? (<Tooltip title="Not Faulty"><span style={{ fontSize: 14, color: "grey" }}> <FaCircle /></span></Tooltip>) : (<Tooltip title="Faulty"><span style={{ fontSize: 14, color: "red" }}> <FaCircle /></span></Tooltip>));
export const InsightValidFormatter = (a: InsightDto) => (!a.isValid ? (<Tooltip title="Invalid"><span style={{ fontSize: 14, color: "orange" }}> <FaCircle /></span></Tooltip>) : (<Tooltip title="Valid"><span style={{ fontSize: 14, color: "green" }}> <FaCircle /></span></Tooltip>));
export const IsValidTriggerFormatter = (a: boolean) => (!a ? (<Tooltip title="Never Triggered"><span style={{ fontSize: 14, color: "orange" }}> <FaCircle /></span></Tooltip>) : (<Tooltip title="Active"><span style={{ fontSize: 14, color: "green" }}> <FaCircle /></span></Tooltip>));
export const IsTriggeredFormatter = (a: boolean) => (!a ? (<Tooltip title="Not Triggering"><span style={{ fontSize: 14, color: "green" }}> <FaCircle /></span></Tooltip>) : (<Tooltip title="Triggering"><span style={{ fontSize: 14, color: "red" }}> <FaCircle /></span></Tooltip>));
export const IsTriggeredTextFormatter = (a: boolean) => (!a ? (<Tooltip title="Not Triggering"><>Not Triggering</></Tooltip>) : (<Tooltip title="Triggering"><>Triggering</></Tooltip>));
export const CapbilityValidFormatter = (a: CalculatedPointDto) => (!a.valid ? (<Tooltip title="Invalid"><span style={{ fontSize: 14, color: "orange" }}> <FaCircle /></span></Tooltip>) : (<Tooltip title="Valid"><span style={{ fontSize: 14, color: "green" }}> <FaCircle /></span></Tooltip>));
export const InsightLastFaultedFormatter = (a: InsightDto) => (a.lastFaultedDate?.format('ddd, MM/DD HH:mm:ss') ?? '-');
export const DateFormatter = (a?: moment) => (a?.format('ddd, MM/DD HH:mm:ss') ?? '-');
export const RuleInstanceLink = (a: RuleInstanceDto) => (<StyledLink to={"/ruleinstance/" + encodeURIComponent(a.id!)}> {a.equipmentId} </StyledLink>);
export const RuleLinkFormatter = (ruleId: string, ruleName: string, reload?: boolean) => (<StyledLink reloadDocument={reload ?? false} to={"/rule/" + encodeURIComponent(ruleId)}> {ruleName} </StyledLink>);
export const GlobalLinkFormatter = (globalId: string, globalName: string, reload?: boolean) => (<StyledLink reloadDocument={reload ?? false} to={"/global/" + encodeURIComponent(globalId)}> {globalName} </StyledLink>);
export const ValidFormatter = (r: RuleInstanceDto) => ValidFormatterStatus(r.status!);
export const ValidFormatterBoundParamater = (r: RuleParameterBoundDto) => ValidFormatterStatus(r.status!);
export const ValidFormatterStatus = (s: RuleInstanceStatus) => (
  <Tooltip title={RuleInstanceStatusLookup.getStatusString(s)}>
    <Box sx={{ flex: 1 }}>
      <Stack spacing={0.5} direction='row'>
        {(s & RuleInstanceStatus._1 || (s as number) === 0) ? <FaCheckCircle color="green" /> : <></>}
        {(s & RuleInstanceStatus._2) ? <FaCircle color="red" /> : <></>}
        {(s & RuleInstanceStatus._4) ? <FaCircle color="grey" /> : <></>}
        {(s & RuleInstanceStatus._8) ? <FaCircle color="orange" /> : <></>}
        {(s & RuleInstanceStatus._16) ? <FaCircle color="blue" /> : <></>}
        {(s & RuleInstanceStatus._32) ? <FaCircle color="pink" /> : <></>}
      </Stack>
    </Box>
  </Tooltip>);
export const ValidFormatterStatusSimple = (s: RuleInstanceStatus) => (
  <span>
    {(s & RuleInstanceStatus._1 || (s as number) === 0) ? <FaCheckCircle color="green" /> : <FaCircle color="red" />}
  </span>);
export const ModelIdFormatter = (modelId: string) => (<StyledLink to={"/model/" + modelId}>{stripPrefix(modelId ?? '')}</StyledLink >);
export const RelatedEntityFormatter = (re: RelatedEntityDto) => (<StyledLink to={"/equipment/" + encodeURIComponent(re.id!)}> <IconForModel modelId={re.modelId!} size={14} /> {re.name ?? re.id}</StyledLink >);

export const WithTooltipFormatter = (props: { id: string }) => (<Tooltip title={props.id}><span>{props.id}</span></Tooltip>);
export const LinkFormatter2 = (a: CapabilityDto) => (<StyledLink to={"/equipment/" + encodeURIComponent(a.id!)}> {a.name}</StyledLink >);

export const ModelFormatter = (a: string) => (<><IconForModel modelId={a} size={14} />&nbsp; {a} </>);
export const ModelFormatter2 = (props: { modelId: string }) =>
(<StyledLink to={"/model/" + props.modelId}>
  <IconForModel modelId={props.modelId} size={14} />&nbsp;
  <Tooltip title={props.modelId ?? 'missing id'} enterDelay={1000}>
    <span>{stripPrefix(props.modelId || '')}</span>
  </Tooltip>
</StyledLink>);
export const YesNoFormatter = (a: boolean) => (a ? <>Yes</> : <>No</>);
export const TrueFormatter = (value: boolean) => (<span style={{ fontSize: 14, color: "grey" }}> {value ? <FaCheck /> : <></>}</span>);
