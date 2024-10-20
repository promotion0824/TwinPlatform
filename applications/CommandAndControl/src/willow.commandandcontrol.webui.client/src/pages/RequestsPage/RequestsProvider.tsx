import { useGridApiRef } from "@mui/x-data-grid-pro";
import { useQueryClient } from "@tanstack/react-query";
import { createContext, useContext, useEffect, useState } from "react";
import { TwinId } from "../../../types/TwinId";
import { UseState } from "../../../types/UseState";
import { useAppContext } from "../../providers/AppContextProvider";
import { useSnackbar } from "../../providers/SnackbarProvider/SnackbarProvider";
import { IConflictingCommandsResponseDto, RequestedCommandStatus, UpdateRequestedCommandsStatusDto } from "../../services/Clients";
import { AppName } from "../../utils/appName";
import useGetRequestedCommands, { IGetRequestedCommands } from "./hooks/useGetRequestedCommands";
import usePostRequestedCommandsStatus from "./hooks/usePostRequestedCommandsStatus";
import useGetRequestedCommandsByTwinId from "./hooks/useGetRequestedCommandsByTwinId";

type RequestsContextType = {
  apiRef: ReturnType<typeof useGridApiRef>;
  reviewRequestTableApiRef: ReturnType<typeof useGridApiRef>;
  selectedRowState: UseState<IConflictingCommandsResponseDto | undefined>;
  requestDetailsLoading: boolean;
  getNewRequestedCommandsQuery: IGetRequestedCommands;
  requestedApproveRejectState: UseState<Record<string, RequestedCommandStatus>>;
  handleReviewRequestResolve: () => Promise<void>;
  handleReviewRequestCancel: () => void;
  statusUpdating: boolean;
};

const RequestsContext = createContext<RequestsContextType | undefined>(undefined);

export function useRequests() {
  const context = useContext(RequestsContext);
  if (context == null) {
    throw new Error("useCommands must be used within a CommandsProvider");
  }

  return context;
}

export type RequestsType = "requests" | "closedRequests" | string;

export default function RequestsProvider({
  selectedId,
  children,
}: {
  selectedId?: TwinId;
  children: React.ReactNode;
}) {
  const queryClient = useQueryClient();

  const snackbar = useSnackbar();
  const apiRef = useGridApiRef(); // This is the apiRef for the requests table
  const reviewRequestTableApiRef = useGridApiRef();
  const { breadCrumbsState } = useAppContext();

  const selectedRowState = useState<IConflictingCommandsResponseDto | undefined>();

  const { data, isLoading: requestDetailsLoading } = useGetRequestedCommandsByTwinId(selectedId, {
    // Fetch ResolveCommandById only if on command details page and selectedRowState is not set. i.e. user navigates directly to command details page via url
    enabled:
      !!selectedId &&
      !selectedRowState[0] &&
      location.pathname.split("/").length > 2,
  });

  // set the state for review request table's approve switch
  const requestedApproveRejectState = useState<Record<string, RequestedCommandStatus>>({});

  const getNewRequestedCommandsQuery: IGetRequestedCommands = useGetRequestedCommands("newRequests");

  // // handle case when user navigates directly to command details page
  // // - fetch data for selected command
  useEffect(() => {
    if (selectedId && data) {
      selectedRowState[1](data);
    }

    return (() => {
      selectedRowState[1](undefined);
      requestedApproveRejectState[1]({});
    })
  }, [selectedId, data]);


  // Update breadcrumbs with current page: commands and command details
  useEffect(() => {
    if (selectedId) {
      breadCrumbsState[1](() => [
        { text: AppName, to: "/" },
        { text: "Requests", to: "/requests" },
        {
          text: selectedRowState[0]?.twinId ?? "",
        },
      ]);
    } else {
      breadCrumbsState[1]([{ text: AppName, to: "/" }]);
    }
  }, [selectedId, selectedRowState[0]]);

  const { mutateAsync, isPending: statusUpdating } = usePostRequestedCommandsStatus({
    onSuccess: async () => {
      snackbar.show("Request resolved");
      // refetch the requested commands query
      await queryClient.invalidateQueries({ queryKey: ["requestedCommandsCount"] });
      await queryClient.invalidateQueries({ queryKey: ["requestedCommands"] });
      await queryClient.invalidateQueries({ queryKey: ["Statistics"] });
      await queryClient.refetchQueries({ queryKey: ["RequestedCommandsByTwinId", selectedId] });

      requestedApproveRejectState[1]({});
    }
  });

  function handleReviewRequestCancel() {
    requestedApproveRejectState[1]({});
  }

  async function handleReviewRequestResolve() {
    const approved = Object.entries(requestedApproveRejectState[0])
      .filter((r: [string, RequestedCommandStatus]) => r[1] === RequestedCommandStatus.Approved)
      .map((r) => r[0]);

    const rejected = Object.entries(requestedApproveRejectState[0])
      .filter((r: [string, RequestedCommandStatus]) => r[1] === RequestedCommandStatus.Rejected)
      .map((r) => r[0]);

    await mutateAsync({
      updateRequestedCommandsStatus: new UpdateRequestedCommandsStatusDto({
        approveIds: approved,
        rejectIds: rejected,
      }),
    });
  };

  return (
    <RequestsContext.Provider
      value={{
        apiRef,
        reviewRequestTableApiRef,
        selectedRowState,
        requestDetailsLoading,
        getNewRequestedCommandsQuery,
        requestedApproveRejectState,
        handleReviewRequestResolve,
        handleReviewRequestCancel,
        statusUpdating,
      }}
    >
      {children}
    </RequestsContext.Provider>
  );
}
