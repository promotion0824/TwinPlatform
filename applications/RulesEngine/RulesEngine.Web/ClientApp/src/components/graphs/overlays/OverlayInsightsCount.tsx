import { Badge } from "@mui/material";
import { IGraphNodeOverlay, IGraphOverlay, TwinNodeDtoWithUX } from "../TwinGraph";
import useApi from "../../../hooks/useApi";

const apiclient = useApi();

export class OverlayInsightsCount implements IGraphOverlay {
  load(twinIds: string[], nodes: TwinNodeDtoWithUX[]) {
    const promise = apiclient.getInsightsCountForTwinGraph(twinIds);
    return promise.then(result => {
      const overlays: NodeOverlayInsights[] = [];

      nodes.forEach(node => {
        //collapsed nodes sums up child insight counts
        if (node.isCollapsed) {
          let count = 0;
          node.collapsedTwinIds.forEach(twinId => {
            const entry = result.find(r => r.twinId == twinId);
            if (entry !== undefined) {
              count += entry.insightsCount!;
            }
          });

          if (count > 0) {
            overlays.push(new NodeOverlayInsights(node.twinDto.twinId!, count));
          }
        }
        else {
          const entry = result.find(r => r.twinId == node.twinDto.twinId);
          if (entry !== undefined) {
            overlays.push(new NodeOverlayInsights(entry.twinId!, entry.insightsCount!));
          }
        }
      });

      return overlays;
    });
  };
}

export class NodeOverlayInsights implements IGraphNodeOverlay {
  twinId: string;
  insightsCount: number;

  constructor(twinId: string, insightsCount: number) {
    this.twinId = twinId;
    this.insightsCount = insightsCount;
  }

  render(props: { overlay: IGraphNodeOverlay, uxNode: TwinNodeDtoWithUX }) {
    const instance = props.overlay as NodeOverlayInsights;
    return (
      <div style={{ float: "right", marginTop: -12, marginRight: -2 }} title={`${instance.insightsCount} insight${instance.insightsCount > 0 ? 's' : ''}`}>
        <Badge badgeContent={instance.insightsCount} color="error" sx={{
          "& .MuiBadge-badge": {
            fontSize: 9,
            right: 0,
            top: 0,
            marginRight: '-4px',
            marginTop: '-4px'
          }
        }}>
        </Badge>
      </div>);
  }
}
