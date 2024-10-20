import { InsightDto, InsightStatus } from '../../Rules';
import LaunchIcon from '@mui/icons-material/Launch';
import StyledAnchor from '../styled/StyledAnchor';

const open = { text: "Open", value: InsightStatus._0 };
const ignored = { text: "Ignored", value: InsightStatus._1 };
const inProgress = { text: "In Progress", value: InsightStatus._2 };
const resolved = { text: "Resolved", value: InsightStatus._3 };
const _new = { text: "New", value: InsightStatus._4 };
const deleted = { text: "Deleted", value: InsightStatus._5 };

const InlineItem = (params: { status: any, value: InsightStatus }) => {
  const status = params.status;
  const value = params.value;
  return ((value == status.value) ? <span>{status.text}</span> : <></>);
}

export function GetInsightStatusFilter() {
  return [
    { label: open.text, value: open.value },
    { label: ignored.text, value: ignored.value },
    { label: inProgress.text, value: inProgress.value },
    { label: resolved.text, value: resolved.value },
    { label: _new.text, value: _new.value },
    { label: deleted.text, value: deleted.value },
  ];
}

export const InsightStatusFormatter = (insight: InsightDto) => {
  //rules engine default status is open, dont clutter dots with open insights that aren't enabled to sync
  if (insight.commandEnabled !== true && insight.status == open.value) {
    return <></>;
  }
  const status = insight.status!;

  return (
    <>
      <InlineItem status={open} value={status} />
      <InlineItem status={ignored} value={status} />
      <InlineItem status={inProgress} value={status} />
      <InlineItem status={resolved} value={status} />
      <InlineItem status={_new} value={status} />
      <InlineItem status={deleted} value={status} />
    </>
  );
}

export const InsightStatusFormatterWithLink = (insight: InsightDto) => {

  if (insight.commandInsightId != '00000000-0000-0000-0000-000000000000') {
    
    let url = `${location.origin}/sites/${insight.siteId}/insights/${insight.commandInsightId}?detail=1&insightTab=occurrences`;

    if (insight.insightUrl) {
      url = insight.insightUrl;
    }

    return (
      <StyledAnchor target="_blank" title="Open Insight in Command" href={url}>
        {InsightStatusFormatter(insight)}<LaunchIcon sx={{ fontSize: 15 }} />
      </StyledAnchor>
    )
  }

  return InsightStatusFormatter(insight);
}
