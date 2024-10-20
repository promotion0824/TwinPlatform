import { useQuery, UseQueryOptions, UseQueryResult } from 'react-query';
import { JobsResponse, ApiException, AsyncJobStatus, FilterSpecificationDto, BatchRequestDto, SortSpecificationDto } from '../../../services/Clients';
import useApi from '../../../hooks/useApi';
import { Dispatch, SetStateAction, useState, useMemo } from 'react';
import useUserInfo from '../../../hooks/useUserInfo';

export interface IGetJobs {
  unifiedJobsQuery: UseQueryResult<JobsResponse, ApiException>;
  selectedStatusesState: [AsyncJobStatus[], Dispatch<SetStateAction<AsyncJobStatus[]>>];
  selectedDateState: [string | null, Dispatch<SetStateAction<string | null>>];
  selectedJobTypesState: [string[], Dispatch<SetStateAction<string[]>>];
  paginationState: [{ pageSize: number; page: number }, Dispatch<SetStateAction<{ pageSize: number; page: number }>>];
  justMyJobsCheckboxState: [boolean, Dispatch<SetStateAction<boolean>>];
  hideSytemJobs: [boolean, Dispatch<SetStateAction<boolean>>];
}

export default function useGetUnifiedJobs(options?: UseQueryOptions<JobsResponse, ApiException>): IGetJobs {
  const api = useApi();
  const { userEmail } = useUserInfo();

  const selectedStatusesState = useState<AsyncJobStatus[]>([]);
  const selectedDateState = useState<string | null>(null);
  const selectedJobTypesState = useState<string[]>([]);
  // eslint-disable-next-line react-hooks/exhaustive-deps
  const { startDate, endDate } = useMemo(() => calculateDates(selectedDateState[0]), [selectedDateState[0]]);
  const justMyJobsCheckboxState = useState<boolean>(false);
  const hideSytemJobs = useState<boolean>(true);
  const paginationState = useState<{ pageSize: number; page: number }>({
    pageSize: 100, // JobsTable available page sizes is [100, 250, 1000]
    page: 0,
  });

  const { page, pageSize } = paginationState[0];


  const AddFilterToRequest = (request: BatchRequestDto, field: string, operator: string, value: any) => {
    if (value === null || value === undefined)
      return;
    const filterSpec = new FilterSpecificationDto();
    filterSpec.field = field;
    filterSpec.operator = operator;
    filterSpec.value = value;
    request.filterSpecifications ??= [];
    request.filterSpecifications.push(filterSpec);
  }

  const body = new BatchRequestDto();

  const sort = new SortSpecificationDto();
  sort.field = "timeCreated";
  sort.sort = "desc";

  body.sortSpecifications = [sort];

  // Add Jobs Types Filter
  if (!!selectedJobTypesState[0] && selectedJobTypesState[0].length > 0) {
    AddFilterToRequest(body, "jobType", "in", selectedJobTypesState[0]);
  }

  // Add Statuses Filter
  if (!!selectedJobTypesState[0] && selectedStatusesState[0].length > 0) {
    AddFilterToRequest(body, "status", "in", selectedStatusesState[0]);
  }

  // Add Start Date Filter
  AddFilterToRequest(body, "timeCreated", ">", startDate);
  // Add End Date Filter
  AddFilterToRequest(body, "timeCreated", "<", endDate);
  // Add IsDeleted Filter
  AddFilterToRequest(body, "isDeleted", "=", false);
  // Add User Filter
  if (justMyJobsCheckboxState[0]) {
    AddFilterToRequest(body, "userId", "=", userEmail);
  }

  if (hideSytemJobs[0]) {
    AddFilterToRequest(body, "userId", "!=", "System");
  }


  body.pageSize = pageSize;
  body.page = page;

  const unifiedJobsQuery = useQuery<JobsResponse, ApiException>(
    [
      'getUnifiedJobs',
      selectedStatusesState[0],
      selectedDateState[0],
      selectedJobTypesState[0],
      page,
      pageSize,
      justMyJobsCheckboxState[0],
      hideSytemJobs
    ],
    () => api.listJobs(false, true, body),
    {
      refetchInterval: 1000 * 10, // refetch every 10 seconds
      cacheTime: 0,
    }
  );

  return {
    selectedStatusesState,
    unifiedJobsQuery,
    selectedDateState,
    selectedJobTypesState,
    paginationState,
    justMyJobsCheckboxState,
    hideSytemJobs
  };
}

const calculateDates = (selectedValue: string | null) => {
  const endDate = new Date(); // Current date
  let startDate = new Date();

  switch (selectedValue) {
    case '1':
      startDate.setDate(endDate.getDate() - 1); // Last 24 Hours
      break;
    case '7':
      startDate.setDate(endDate.getDate() - 7); // Last 7 Days
      break;
    case '30':
      startDate.setDate(endDate.getDate() - 30); // Last 30 Days
      break;
    case '365':
      startDate.setFullYear(endDate.getFullYear() - 1); // Last Year
      break;
    case '730':
      startDate.setFullYear(endDate.getFullYear() - 2); // Last Two Years
      break;
    default:
      return { startDate: undefined, endDate: undefined };
  }

  return { startDate, endDate };
};
