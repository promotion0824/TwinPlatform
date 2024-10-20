import styled from "@emotion/styled";
import { Panel, PanelContent, PanelGroup } from "@willowinc/ui";

import { ActivityLogModal } from "./ActivityLogModal";
import { ActivityLogsFilters as Filters } from "./ActivityLogsFilters";
import { ActivityLogsTable } from "./ActivityLogsTable";
import { ActivityLogDownload } from "./ActivityLogDownload";

export const ActivityLogs = () => {

  return (
    <>
      <PanelGroup units="pixels">
        <Panel collapsible defaultSize={292} title="Filters">
          <PanelContent>
            <Filters />
          </PanelContent>
        </Panel>

        <Panel title="Activity Logs" headerControls={[<ActivityLogDownload />]}>
          <ActivityLogsTable />
        </Panel>
      </PanelGroup >
      <ActivityLogModal />
    </>
  );
}
