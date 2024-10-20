import { PanelGroup, Panel, PanelContent } from "@willowinc/ui";
import Activity from "./components/Activity/Activity";
import TrendChart from "./components/TrendChart";
import styled from "@emotion/styled";
import Metrics from "./components/Metrics";
import OverviewProvider from "./OverviewProvider";

export default function OverviewPage() {
  return (
    <OverviewProvider>
      <Metrics />
      <PanelGroup units="percentages">
        <GraphPanel
          collapsible
          title="Command Trends"
          hideHeaderBorder
        >
          <HeightPanelContent>
            <TrendChart />
          </HeightPanelContent>
        </GraphPanel>
        <Panel collapsible title="Activity" hideHeaderBorder>
          <HeightPanelContent>
            <Activity />
          </HeightPanelContent>
        </Panel>
      </PanelGroup>
    </OverviewProvider>
  );
}

const HeightPanelContent = styled(PanelContent)({
  flexGrow: 1,
  overflow: "hidden !important",
});

const GraphPanel = styled(Panel)({
  minWidth: "66.5%",
});
