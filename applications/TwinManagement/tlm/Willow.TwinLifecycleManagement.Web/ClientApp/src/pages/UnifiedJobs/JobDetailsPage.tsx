import PageLayout from '../../components/PageLayout';
import { useJobs } from './JobsProvider';
import { Panel, Group, DataGrid, Tabs, Loader, Link, GridToolbar } from '@willowinc/ui';
import styled from '@emotion/styled';
import { AsyncJobStatusChip } from '../../components/AsyncJobStatusChip';
import useGetJob from './hooks/useGetJob';
import { useParams } from 'react-router-dom';
import { AsyncJobStatus, JobsEntry, JobsEntryDetail } from '../../services/Clients';
import { useMemo, useEffect } from 'react';
import useLoader from '../../hooks/useLoader';
import useHandleJobDetailsTabsQueryParams from './hooks/useHandleJobDetailsPageTabState';

export default function JobDetailsPage() {
  const { jobId: jobIdParam } = useParams();
  const { selectedJobState } = useJobs();

  const {
    data: jobData,
    isLoading,
    isSuccess,
    isFetching,
  } = useGetJob(jobIdParam!, {
    enabled: !!jobIdParam, refetchInterval: (currJob) => {

      if (!!currJob && (currJob.status === AsyncJobStatus.Processing || currJob.status === AsyncJobStatus.Queued))
        return 3 * 1000;
      else
        return false;
    }
  });

  const [showLoader, hideLoader, inProgress] = useLoader();
  useEffect(() => {
    if (isFetching && !inProgress) {
      showLoader();
    } else {
      if (!isFetching && inProgress) hideLoader();
    }
    return () => {
      if (inProgress) hideLoader();
    };
  }, [isFetching, showLoader, hideLoader, inProgress]);

  const job = isSuccess ? jobData : selectedJobState[0];

  const { jobId = '', status } = job || {};
  return (
    <PageLayout>
      <PageLayout.Header
        pageTitleItems={[
          { title: 'Jobs', href: '../jobs' },
          { title: jobId, suffix: status && <AsyncJobStatusChip value={status} />, isLoading: isLoading },
        ]}
      ></PageLayout.Header>
      <PageLayout.MainContent>
        {isLoading ? (
          <Container>
            <Loader variant="dots" size="lg" />
          </Container>
        ) : (
          job && <JobDetailsContent job={job!} />
        )}
      </PageLayout.MainContent>
    </PageLayout>
  );
}

const Container = styled('div')({
  width: '100%',
  height: '100%',
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
});

function JobDetailsContent({ job }: { job: JobsEntry }) {
  const jobDetailsTabState = useHandleJobDetailsTabsQueryParams();
  const { jobsEntryDetail, fields } = useMemo(() => parseJob(job), [job]);
  const { inputsJson, customData, errorsJson, outputsJson } = jobsEntryDetail || {};

  let errorsSection = errorsJson;

  let outputSection = outputsJson;

  return (
    <NoBorderPanelTab
      id="job-details"
      tabs={
        <Tabs
          defaultValue="details"
          value={jobDetailsTabState[0]}
          onTabChange={(value: string | null) => jobDetailsTabState[1](value as (typeof jobDetailsTabState)[0])}
        >
          <Tabs.List>
            <Tabs.Tab value="details">Details</Tabs.Tab>
            {inputsJson && <Tabs.Tab value="inputs">Inputs</Tabs.Tab>}
            {customData && <Tabs.Tab value="customdata">Custom Data</Tabs.Tab>}
            {outputSection && <Tabs.Tab value="output">Output</Tabs.Tab>}
            {errorsSection && <Tabs.Tab value="errors">Errors</Tabs.Tab>}
          </Tabs.List>

          <Tabs.Panel
            style={{
              margin: '1rem',
            }}
            value="details"
          >
            <div>
              {fields.map(({ heading, content }) => (
                <Details key={heading} heading={heading} content={content} />
              ))}
            </div>
          </Tabs.Panel>

          <Tabs.Panel
            style={{
              margin: '1rem',
            }}
            value="inputs"
          >
            {inputsJson && <SectionJsonContent content={inputsJson} />}
          </Tabs.Panel>

          <Tabs.Panel
            style={{
              margin: '1rem',
            }}
            value="customdata"
          >
            {customData && <SectionJsonContent content={customData} />}
          </Tabs.Panel>

          <Tabs.Panel
            style={{
              margin: '1rem',
            }}
            value="output"
          >
            {outputSection && <SectionGridOrJsonContent content={outputSection} />}
          </Tabs.Panel>

          <Tabs.Panel
            style={{
              margin: '1rem',
            }}
            value="errors"
          >
            {errorsSection && <SectionGridOrJsonContent content={errorsSection} />}
          </Tabs.Panel>
        </Tabs>
      }
    />
  );
}

const NoBorderPanelTab = styled(Panel)({ border: 'none !important' });

function SectionJsonContent({ content }: { content: string }) {
  let prettyJsonString;
  try {
    // Parse JSON string to JavaScript object
    const jsonObj = JSON.parse(content!);

    // Convert the JavaScript object back to a formatted JSON string
    prettyJsonString = JSON.stringify(jsonObj, null, 3);
  } catch {
    prettyJsonString = content;
  }

  return <CodeBlock>{prettyJsonString}</CodeBlock>;
}

function SectionGridOrJsonContent({ content }: { content: string }) {
  let gridContent: { [key: string]: string } | null = null;
  try {
    const parsedContent = JSON.parse(content);

    const isDictType =
      Object.entries(parsedContent).length > 0 && Object.entries(parsedContent).every((e) => typeof e[1] == 'string');
    // set grid content if json is dict type { [key:string]:string }
    if (isDictType) {
      gridContent = parsedContent;
    }
  } catch (e) {
    // suppress parse exception
  }

  if (!gridContent) {
    return SectionJsonContent({ content });
  } else {
    return <GridContent content={gridContent} />;
  }
}

function GridContent({ content }: { content: { [key: string]: string } }) {
  const columns: any = useMemo(
    () => [
      {
        field: 'id',
        headerName: '',
        flex: 0.25,
      },
      {
        field: 'value',
        headerName: '',
        flex: 1,
        renderCell: (params: any) => (
          <div title={params.value}>
            <LinkifyText text={params.value} />
          </div>
        ),
      },
    ],
    []
  );

  const rows = useMemo(() => Object.entries(content).map(([key, value]) => ({ id: key, value })), [content]);

  return (
    <StyledDataGrid
      key="job-details-grid"
      rows={rows}
      columns={columns}
      slots={{ toolbar: GridToolbar }}
      getRowHeight={() => 'auto'}
    />
  );
}

const StyledDataGrid = styled(DataGrid)({
  '.MuiDataGrid-cell--textLeft': { alignItems: 'start' },
  'p > p:first-child, p:first-of-type': {
    margin: 0,
  },
});

function LinkifyText({ text }: { text: string }) {
  // Regular expression to match URLs
  const urlRegex = /(https?:\/\/[^\s]+)/g;

  // Splitting the text by line breaks first, then by URLs within each line
  const lines = text.split('\n').map((line, lineIndex) => (
    <p key={lineIndex}>
      {line.split(urlRegex).map((part, index) =>
        urlRegex.test(part) ? (
          <Link title={part} key={index} href={part} target="_blank" rel="noopener noreferrer">
            {part}
          </Link>
        ) : (
          part
        )
      )}
    </p>
  ));

  return <p>{lines}</p>;
}

function parseJob(jobsEntry: JobsEntry): {
  jobsEntryDetail: JobsEntryDetail;
  fields: { heading: string; content: any }[];
} {
  const SORTED_FIELDS_ORDER = [
    'userId',
    'jobType',
    'jobSubtype',
    'parentJobId',
    'sourceResourceUri',
    'targetResourceUri',
    'timeCreated',
    'timeLastUpdated',
    'processingStartTime',
    'processingEndTime',
    'progressCurrentCount',
    'progressTotalCount',
    'progressStatusMessage',
    'userMessage',
    'isDeleted',
    'isExternal',
  ];

  // Always show these fields even if they're undefined, show "-" instead
  const REQUIRID_FIELDS = [
    'userId',
    'jobType',
    'jobSubtype',
    'timeCreated',
    'timeLastUpdated',
    'processingEndTime',
    'processingStartTime',
  ];

  const { jobsEntryDetail = {} as JobsEntryDetail } = jobsEntry;

  const result: { heading: string; content: any }[] = [];

  SORTED_FIELDS_ORDER.forEach((key) => {
    let value = jobsEntry[key as keyof JobsEntry]?.toString();

    if (value !== undefined && value !== '') {
      result.push({
        heading: key,
        content: value,
      });
    } else {
      if (REQUIRID_FIELDS.includes(key)) {
        result.push({
          heading: key,
          content: '-',
        });
      }
    }
  });

  return { jobsEntryDetail, fields: result };
}

const CodeBlock = styled('code')({
  display: 'block',
  padding: '1rem',
  color: 'unset !important',
  backgroundColor: 'black',
  borderRadius: '4px',
  overflow: 'auto',
  whiteSpace: 'pre-wrap',
  fontSize: '1em',
});

const Details = ({ heading, content }: { heading: string; content?: React.ReactNode }) => (
  <DetailContainer>
    <HeaderSection>
      <KeyField w="240px" align="start" gap="s4">
        {heading}
      </KeyField>
    </HeaderSection>
    <div>{content}</div>
  </DetailContainer>
);

const DetailContainer = styled.div({
  display: 'flex',
  width: '100%',
  paddingBottom: '0.5rem',
  gap: '0.25rem',
  color: 'rgb(198, 198, 198)',
  fontFamily: 'Poppins, Arial, sans-serif',
  fontWeight: 400,
  fontSize: '0.75rem',
  lineHeight: '1.25rem',

  justifyContent: 'start',
});

const HeaderSection = styled.div({
  color: 'rgb(145, 145, 145)',
});

const KeyField = styled(Group)({ color: 'rgb(145, 145, 145)' });
