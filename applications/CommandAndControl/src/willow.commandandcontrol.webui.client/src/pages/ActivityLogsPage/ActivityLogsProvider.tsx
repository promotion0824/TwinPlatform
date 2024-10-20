import { PropsWithChildren, createContext, useContext, useState } from "react";
import { GetActivityLogs, useGetActivityLogs } from "./hooks/useGetActivityLogs";
import { UseState } from "../../../types/UseState";
import { ActivityLogsResponseDto, FilterSpecificationDto } from "../../services/Clients";
import { useDisclosure, useGridApiRef } from "@willowinc/ui";
import { DownloadActivityLogs, useDownloadActivityLogs } from "./hooks/useDownloadActivityLogs";

type ActivityLogsContextType = {
  compact?: boolean;
  apiRef: ReturnType<typeof useGridApiRef>;
  selectedRowState: UseState<ActivityLogsResponseDto | undefined>;
  modalState: ReturnType<typeof useDisclosure>;
  getActivityLogsQuery: GetActivityLogs;
  downloadActivityLogsQuery: DownloadActivityLogs;
};

const ActivityLogsContext = createContext<ActivityLogsContextType | undefined>(undefined);

export function useActivityLogs() {
  const context = useContext(ActivityLogsContext);
  if (context == null) {
    throw new Error("useActivityLogs must be used within an ActivityLogsProvider");
  }

  return context;
}

export const ActivityLogsProvider: React.FC<PropsWithChildren<ActivityLogsProviderProps>> = ({compact, commandId, requestId, children}) => {

  const apiRef = useGridApiRef();

  const selectedRowState = useState<ActivityLogsResponseDto | undefined >();

  // used to open/close the detail modal
  const modalState = useDisclosure(false);

  const commandFilter = !!commandId ? [new FilterSpecificationDto({field: "resolvedCommandId", logicalOperator: "OR", operator: "equals", value: commandId})] : undefined;

  const requestFilter = !!requestId ? [new FilterSpecificationDto({field: "requestedCommandId", logicalOperator: "OR", operator: "equals", value: requestId})] : undefined;

  const filters = [...commandFilter || [], ...requestFilter || []]

  const getActivityLogsQuery = useGetActivityLogs(filters);

  const downloadActivityLogs = useDownloadActivityLogs([]);

  return (
    <ActivityLogsContext.Provider value={{
      compact,
      apiRef,
      selectedRowState,
      modalState,
      getActivityLogsQuery,
      downloadActivityLogsQuery: downloadActivityLogs,
    }}>
      {children}
    </ActivityLogsContext.Provider>
  );
}

export interface ActivityLogsProviderProps {
  commandId?: string;
  requestId?: string;
  compact?: boolean;
}
