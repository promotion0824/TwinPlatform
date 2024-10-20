import { createContext, useContext, useEffect, useState } from "react";
import { useGridApiRef } from "@mui/x-data-grid-pro";
import { useAppContext } from "../../providers/AppContextProvider";
import CommandStatus from "./components/Commands/CommandStatus";
import { useSnackbar } from "../../providers/SnackbarProvider/SnackbarProvider";
import useGetResolvedCommands, { IGetResolvedCommands } from "./hooks/useGetResolvedCommands";
import { FilterSpecificationDto, IResolvedCommandResponseDto, ResolvedCommandAction, ResolvedCommandStatus, UpdateResolvedCommandStatusDto } from "../../services/Clients";
import useGetResolvedCommandById from "./hooks/useGetResolvedCommandById";
import usePostResolvedCommandStatus from "./hooks/usePostResolvedCommandStatus";
import { useInvalidateOnSuccess } from "../../hooks/useInvalidateOnSuccess";
import { useDisclosure } from "@willowinc/ui";
import ConfirmationExecuteActionModal from "./components/Commands/ConfirmationExecuteActionModal";
import { UseState } from "../../../types/UseState";
import { AppName } from "../../utils/appName";

type CommandsContextType = {
  apiRef: ReturnType<typeof useGridApiRef>;
  selectedRowState: UseState<IResolvedCommandResponseDto | undefined>;
  handleAction: (id: string, action: ResolvedCommandAction, comment?: string) => void;
  handleVerifyAction: (id: string, action: ResolvedCommandAction) => void;
  closeModal: () => void;
  getResolvedCommandsQuery: IGetResolvedCommands;
  getResolvedPastCommandsQuery: IGetResolvedCommands;
  selectedTabState: UseState<CommandsTabType>;
};

const CommandsContext = createContext<CommandsContextType | undefined>(undefined);

export function useCommands() {
  const context = useContext(CommandsContext);
  if (context == null) {
    throw new Error("useCommands must be used within a CommandsProvider");
  }

  return context;
}

export type CommandsTabType = "commands" | "pastCommands" | string;

export default function CommandsProvider({
  selectedId,
  children,
}: {
  selectedId?: string;
  children: React.ReactNode;
}) {
  const apiRef = useGridApiRef();
  const snackbar = useSnackbar();
  const invalidateOnSuccess = useInvalidateOnSuccess();

  const { breadCrumbsState } = useAppContext();
  const selectedTabState = useState<CommandsTabType>("commands");
  const selectedRowState = useState<IResolvedCommandResponseDto | undefined>();

  const { data } = useGetResolvedCommandById(selectedId, {
    // Fetch ResolveCommandById only if on command details page and selectedRowState is not set. i.e. user navigates directly to command details page via url
    enabled:
      !!selectedId &&
      !selectedRowState[0] &&
      location.pathname.split("/").length > 2,
  });

  const getResolvedCommandsQuery = useGetResolvedCommands(
    "commands",
    [
      // Default filter to show only closed requests, i.e. status that is not pending
      new FilterSpecificationDto({
        field: "status",
        logicalOperator: "AND",
        operator: "notin",
        value: [ResolvedCommandStatus.Cancelled, ResolvedCommandStatus.Executed],
      }),
    ],
    {
      enabled: selectedTabState[0] === "commands",
      refetchInterval: 5000
    },
  );

  const getResolvedPastCommandsQuery = useGetResolvedCommands(
    "pastCommands",
    [
      // default filter to show only closed requests, i.e. executed, cancelled or passed their end date
      new FilterSpecificationDto({
        field: "status",
        logicalOperator: "AND",
        operator: "in",
        value: [ResolvedCommandStatus.Cancelled, ResolvedCommandStatus.Executed]
      }),
    ],
    {
      enabled: selectedTabState[0] === "pastCommands",
      refetchInterval: 5000
    },
  );

  // // handle case when user navigates directly to command details page
  // // - fetch data for selected command
  useEffect(() => {
    if (selectedId && data) {
      selectedRowState[1](data);
    }
    
    return (() => {
      selectedRowState[1](undefined);
    })
  }, [selectedId, data]);


  // Update breadcrumbs with current page: commands and command details
  useEffect(() => {
    if (selectedId) {
      breadCrumbsState[1](() => [
        { text: AppName, to: "/" },
        { text: "Commands", to: "/commands" },
        ... selectedRowState[0] ? [{
          text: selectedRowState[0].commandName ?? "",
          suffix: <CommandStatus value={selectedRowState[0].status ?? ""} />
        }] : []
      ]);
    } else {
      breadCrumbsState[1]([{ text: AppName, to: "/" }]);
    }

  }, [selectedId, selectedRowState[0]]);

  // handle action from commands table action column
  const { mutateAsync } = usePostResolvedCommandStatus();

  const [opened, { open, close: closeModal }] = useDisclosure(false);

  async function handleAction(id: string, action: ResolvedCommandAction, comment?: string) {
    await mutateAsync(
      {
        id,
        updateResolvedCommandStatus: new UpdateResolvedCommandStatusDto({
          action,
          comment,
        }),
      },
      invalidateOnSuccess(getResolvedCommandsQuery.key)
    );
    snackbar.show(`${action} command`);
  }

  const modalIdState = useState<string>();
  async function handleVerifyAction(id: string, action: ResolvedCommandAction) {
    if (action === "Execute") {
      modalIdState[1](id);
      open();
    } else {
      await handleAction(id, action, undefined);
    }
  }

  return (
    <CommandsContext.Provider
      value={{
        apiRef,
        selectedRowState,
        handleAction,
        handleVerifyAction,
        closeModal,
        getResolvedCommandsQuery,
        getResolvedPastCommandsQuery,
        selectedTabState,
      }}
    >
      <>
        {children}
        <ConfirmationExecuteActionModal opened={opened} id={modalIdState[0]} />
      </>
    </CommandsContext.Provider>
  );
}
