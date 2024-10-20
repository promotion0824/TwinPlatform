import { PanelGroup, Panel, PanelContent } from "@willowinc/ui";
import Filters from "./RequestsFilters";
import RequestsTable from "./RequestsTable";
import { useRequests } from "../RequestsProvider";

export default function Requests() {
  const { getNewRequestedCommandsQuery } = useRequests();

  return (
    <>
      <PanelGroup units="pixels">
        <Panel collapsible defaultSize={292} title="Filters">
          <PanelContent>
            <Filters />
          </PanelContent>
        </Panel>

        <Panel title="Requests">
          <RequestsTable
            type="requests"
            requestedCommandsQuery={getNewRequestedCommandsQuery}
          />
        </Panel>
      </PanelGroup>
    </>
  );
}
