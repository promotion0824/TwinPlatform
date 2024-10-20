import { useState } from "react";
import { ApiException, SortSpecificationDto, FilterSpecificationDto, ActivityLogsRequestDto } from "../../../services/Clients";
import useApi from "../../../hooks/useApi";
import { useAppContext } from "../../../providers/AppContextProvider";
import { UseState } from "../../../../types/UseState";
import { toFlatArray } from "../../../utils/toArray";
import { useSiteFilter } from "../../../hooks/useSiteFilter";
import { ActivityLogExportFormat } from "../../../../types/ActivityLogExportFormat";

export interface DownloadActivityLogs {
  query: (type?: ActivityLogExportFormat) => Promise<void>;
  sortState: UseState<SortSpecificationDto[]>;
}

export const useDownloadActivityLogs = (
  defaultFilters: FilterSpecificationDto[] = [],
): DownloadActivityLogs => {
  const api = useApi();
  const { activityLogsFilters } = useAppContext();
  const siteFilter = useSiteFilter();

  const sortState = useState<SortSpecificationDto[]>([new SortSpecificationDto({ field: "timestamp", sort: "desc" })]);

  const body = new ActivityLogsRequestDto();

  const filterSpecifications = [
    ...toFlatArray(activityLogsFilters[0]),
    ...defaultFilters,
    siteFilter,
  ].filter((x) => x) as FilterSpecificationDto[];

  body.filterSpecifications = filterSpecifications;
  body.sortSpecifications = sortState[0];

  const query = async (type: ActivityLogExportFormat = "csv") => {
    try {
      const fileResponse = await api.exportActivityLogs(type, body);
      const url = window.URL.createObjectURL(new Blob([fileResponse.data]));
      const link = document.createElement('a');
      link.href = url;
      const contentDisposition = !!fileResponse.headers && fileResponse.headers['content-disposition'] as string;
      let fileName = "ActivityLogs." + type;
      if (contentDisposition) {
        const fileNameMatch = contentDisposition.match(/filename=(.+);/);
        if (fileNameMatch?.length === 2)
          fileName = fileNameMatch[1];
      }
      link.setAttribute('download', fileName);
      document.body.appendChild(link);
      link.click();
      link.remove();
    }
    catch (error) {
      console.error(error);
    }
  };

  return {
    query,
    sortState,
  };
}
