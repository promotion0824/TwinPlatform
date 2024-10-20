import { PanelGroup, Panel, PanelContent, Tabs } from "@willowinc/ui";
import Filters from "./CommandsFilters";
import CommandsTable from "./CommandsTable";
import { useCommands } from "../../CommandsProvider";

export default function Commands() {
  const {
    selectedTabState,
    getResolvedCommandsQuery,
    getResolvedPastCommandsQuery,
  } = useCommands();
  const [selectedTab, setSelectedTab] = selectedTabState;
  return (
    <>
      <PanelGroup units="pixels">
        <Panel collapsible defaultSize={292} title="Filters">
          <PanelContent>
            <Filters />
          </PanelContent>
        </Panel>

        <Panel
          tabs={
            <Tabs
              value={selectedTab}
              onTabChange={(val) => setSelectedTab(val!)}
            >
              <Tabs.List>
                <Tabs.Tab value="commands">Commands</Tabs.Tab>
                <Tabs.Tab value="pastCommands">Past Commands</Tabs.Tab>
              </Tabs.List>
              <Tabs.Panel value="commands">
                {selectedTab === "commands" && (
                  <CommandsTable
                    resolvedCommandQuery={getResolvedCommandsQuery}
                    showActions
                  />
                )}
              </Tabs.Panel>
              <Tabs.Panel value="pastCommands">
                {selectedTab === "pastCommands" && (
                  <CommandsTable
                    resolvedCommandQuery={getResolvedPastCommandsQuery}
                  />
                )}
              </Tabs.Panel>
            </Tabs>
          }
        />
      </PanelGroup>
    </>
  );
}
