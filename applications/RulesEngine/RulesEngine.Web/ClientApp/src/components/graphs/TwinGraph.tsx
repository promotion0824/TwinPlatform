import * as React from "react";
import ReactFlow, {
  Node,
  Position,
  ReactFlowProvider,
  useReactFlow,
} from "reactflow";
import 'reactflow/dist/style.css';
import OpenInFullIcon from "@mui/icons-material/OpenInFull";
import CloseIcon from "@mui/icons-material/Close";
import IconForModel from "../icons/IconForModel";
import { Link } from "react-router-dom";
import { TwinRelationshipDto, TwinNodeDto } from "../../Rules";
import { useQuery } from "react-query";
import ELK, { ElkNode } from "elkjs";
import { useMemo, useRef, useState } from "react";
import useApi from "../../hooks/useApi";
import { FaPlus } from "react-icons/fa";
import edgeColor from "../../hooks/edgeColor";
import nodeStyle from "../../hooks/nodeStyle";
import { stripPrefix } from "../LinkFormatters";
import { groupBy, IBoundingBox, graphStyles } from "./GraphBase";
import useKeyboardEventListener from "../../hooks/useEventListener";
import {
  goDown,
  goLeft,
  goRight,
  goUp,
  goCenter,
  Itransition,
  reducer,
  boxWidth,
  boxHeight,
} from "./keyboardnavigator";
import {
  Box,
  Button,
  Divider,
  Modal,
  useTheme,
} from "@mui/material";

import { OverlayInsightsCount } from "./overlays/OverlayInsightsCount";

interface TwinGraphInputProps {
  twinIds: string[],
  highlightedIds?: string[],
  isCollapsed?: boolean
}

export interface IGraphNodeOverlay {
  twinId: string,
  render: (props: { overlay: IGraphNodeOverlay, uxNode: TwinNodeDtoWithUX }) => JSX.Element;
};

export interface IGraphOverlay {
  load: (twinIds: string[], nodes: TwinNodeDtoWithUX[]) => Promise<IGraphNodeOverlay[]>;
};

//HOC pattern to wrap a class instance with a component (to be able to use useState)
//https://plainenglish.io/blog/which-is-better-class-components-or-functional-component-in-react-a417b4ef6c1a
function newOverlayComponent(OverlayComponent: any, overlay: IGraphNodeOverlay, uxNode: TwinNodeDtoWithUX) {
  return class extends React.Component {
    render() {
      return <OverlayComponent overlay={overlay} uxNode={uxNode} {...this.props} />;
    }
  }
}

function RenderNodeOverlay(nodeId: string, uxNode: TwinNodeDtoWithUX) {
  const NodeOverlay = newOverlayComponent(uxNode.overlay!.render, uxNode.overlay!, uxNode);
  return (<NodeOverlay key={`${nodeId}_Overlay`} />);
}

// TODO: Do this and eliminate those fields from the TwinNodeDto, they are client-side only
// Extend nodeDto with UX elements to track expansion / collapse state
export type TwinNodeDtoWithUX = {
  twinDto: TwinNodeDto;
  collapseKey: string;
  isSelected: boolean;
  isCollapsed: boolean;
  collapsedTwinIds: string[];
  isExpanded: boolean;
  label: string,
  x: number;
  y: number;
  twinIds: string[];
  style: any;
  overlay?: IGraphNodeOverlay
};

const TwinGraphInternal: React.FC<TwinGraphInputProps> = ({ twinIds, highlightedIds, isCollapsed }) => {
  const theme = useTheme();
  const apiclient = useApi();
  const elk = new ELK();
  const proOptions = { hideAttribution: true };

  const [expanded, setExpanded] = useState<Record<string, boolean>>({});

  const toggleExpanded = (s: string) => {
    const expandedCopy = { ...expanded };
    expandedCopy[s] = !expandedCopy[s];
    setExpanded(expandedCopy);
  };

  const TwinFormatter = (
    nodeId: string,
    uxNode: TwinNodeDtoWithUX
  ) =>
  (<>
    {uxNode.overlay && RenderNodeOverlay(nodeId, uxNode)}
    <div style={{ overflow: 'hidden' }}>{
      uxNode.isCollapsed ? (<Link
        to="#"
        style={{ color: uxNode.style.color }}
        onClick={(_e) => toggleExpanded(uxNode.collapseKey)}
      >
        {stripPrefix(uxNode.label ?? "")} <FaPlus size={14} />
      </Link>)
        : uxNode.isExpanded ?
          (<Link title={stripPrefix(uxNode.twinDto.modelId!)} to={`/equipment/${uxNode.twinIds.join("/")}`} style={{ color: uxNode.style.color }}>
            <IconForModel modelId={uxNode.twinDto.modelId!} size={14} />
            &nbsp; {stripPrefix(uxNode.label ?? "")}{" "}
            {/* Might add a collapse back icon later */}
          </Link>) :
          (<Link title={stripPrefix(uxNode.twinDto.modelId!)} to={`/equipment/${uxNode.twinIds.join("/")}`} style={{ color: uxNode.style.color }}>
            <IconForModel modelId={uxNode.twinDto.modelId!} size={14} />
            &nbsp; {stripPrefix(uxNode.label ?? "")}
          </Link>)
    }</div></>
  );

  const edgeFormatter = (x: TwinRelationshipDto) => {
    return {
      id: x.id!,
      source: `${x.startId}`,
      target: `${x.endId}`,
      animated: x.name === "feeds",
      data: { label: `${x.name} ${x.substance}` },
      label: x.name,
      labelStyle: { fill: theme.palette.primary.contrastText },
      labelBgStyle: { fill: theme.palette.primary.main, fillOpacity: 0.1 },
      style: { stroke: edgeColor(x) },
    };
  };

  type TwinNode = Node<any> & { expanded: boolean, uxNode: TwinNodeDtoWithUX };

  const nodeFormatter = (
    x: TwinNodeDtoWithUX
  ): TwinNode => {

    return {
      id: `${x.twinDto.id}`,
      uxNode: x,
      selected: x.isSelected,
      expanded: x.isExpanded,
      data: {
        label: TwinFormatter(`${x.twinDto.id}`, x)
      },
      position: { x: x.x, y: x.y },
      style: x.style,
      sourcePosition: Position.Top,
      targetPosition: Position.Bottom
    };
  };

  const getLayoutedElements = async (elements: any) => {
    if (!elements) return {} as ElkNode;

    const done = await elk.layout(elements, {
      layoutOptions: {
        aspectRatio: '1.77',
        algorithm: 'org.eclipse.elk.layered',
        'org.eclipse.elk.force.temperature': '0.0001',
        'org.eclipse.elk.layered.priority.direction': '1',
        'elk.nodePlacement.strategy': 'LINEAR_SEGMENTS',
        'elk.spacing.nodeNode': '35',
        'elk.layered.spacing.nodeNodeBetweenLayers': '40'
      },
      logging: false,
      measureExecutionTime: false,
    });

    return done;
  };

  const twinQuery = useQuery(["Twin", ...twinIds, expanded], async (x) => {
    let twinIdsToServer = twinIds;
    if (highlightedIds !== undefined) {
      twinIdsToServer = twinIds.concat(highlightedIds!);
    }

    const d = await apiclient.twinsGraph(twinIdsToServer);
    console.log("useQuery(graph)", d);

    // Group must match all the ins and outs, e.g. one entity
    // with multiple capabilities
    const inouts = (g: TwinNodeDto) =>
      [
        ...d.edges!.filter((e) => e.startId === g.id).map((e) => e.endTwinId),
        ...d.edges!.filter((e) => e.endId === g.id).map((e) => e.startTwinId),
      ].join("_");

    // Handle grouping, selected nodes are not grouped
    // Returns a grouping of groups
    const grouped = groupBy(
      d.nodes!,
      (x) => inouts(x) + x.groupKey! + (x.isSelected ? x.id : "")
    );

    // Create a collapsed node for each group except the group that is ""

    const newNodes: TwinNodeDto[] = [];
    const alwaysExpand = (isCollapsed !== undefined && isCollapsed === false);

    for (const key in grouped) {
      const gp = grouped[key];

      // Expand any groups which are a single element or just two elements
      if (gp.length < 3) {
        for (const selector in gp) {
          newNodes.push(gp[selector]);
        }
      }
      // Expand any groups marked expanded but use subgroups on those if necessary
      else if (expanded[key] || alwaysExpand) {
        const subGrouped = groupBy(gp, (x) => inouts(x) + x.groupKey2!);

        const subGroupCount = Object.keys(subGrouped).length;

        for (const subGroupKey in subGrouped) {
          const subgp = subGrouped[subGroupKey];

          // If the subgroup is small, expand it
          if (subgp.length < 3) {
            for (const selector in subgp) {
              newNodes.push(subgp[selector]);
            }
          }
          // If the subgroup is expanded, expand it (or if there is only one)
          else if (expanded[subGroupKey] || subGroupCount === 1 || alwaysExpand) {
            for (const selector in subgp) {
              const nn = subgp[selector];
              //dont flag as expanded otherwise box style looks different
              if (!alwaysExpand) {
                nn.isExpanded = true;
              }
              newNodes.push(nn);
            }
          } else {
            // Create the subgroup
            const [first] = subgp;
            const collapseName2 = `${first.groupKey2} (${subgp.length})`;
            //console.log('Sub group', subGroupKey, collapseName2);
            const collapsed = new TwinNodeDto({
              isCollapsed: true,
              id: first.id,
              twinId: first.twinId,
              collapseKey: subGroupKey,
              //map the group key to all the copllapsed twin ids
              groupKey: subgp.map(v => v.twinId).join(","),
              modelId: first.groupKey,
              name: collapseName2,
            });
            newNodes.push(collapsed);
          }
        }
      } else {
        // Create the collapsed group (first-level grouping)
        const [first] = gp;
        const collapseName = `${first.groupKey} (${gp.length})`;
        //console.log('Top level group', key, collapseName);
        const collapsed = new TwinNodeDto({
          isCollapsed: true,
          id: first.id,
          twinId: first.twinId,
          collapseKey: key,
          //map the group key to all the copllapsed twin ids
          groupKey: gp.map(v => v.twinId).join(","),
          modelId: first.groupKey,
          name: collapseName,
        });
        newNodes.push(collapsed);
      }
    }

    const newEdges: TwinRelationshipDto[] = d.edges!.filter(
      (x) =>
        newNodes.findIndex((y, i) => y.id === x.startId) > -1 &&
        newNodes.findIndex((y, i) => y.id === x.endId) > -1
    );

    const root: ElkNode = {
      id: "root",
      layoutOptions: { "elk.direction": "UP" },
      children: newNodes!.map((x) => ({
        ...x,
        id: `${x.id}`,
        width: 150,
        height: 50,
      })),
      edges: newEdges!.map((x) => ({
        ...x,
        id: x.id!,
        sources: [`${x.startId}`],
        targets: [`${x.endId}`],
      })),
    };

    const els = await getLayoutedElements(root);
    //console.log(els);

    // And convert
    const dnodes = Object.entries(els.children!).map((x: any) => {
      const entry = x[1];
      let isHighlighted: (boolean | undefined) = undefined;

      if (highlightedIds !== undefined && highlightedIds.length > 0) {
        isHighlighted = highlightedIds.includes(entry.twinId);
      }

      const style = nodeStyle({ ...entry, isHighlighted: isHighlighted }, theme);
      const uxNode: TwinNodeDtoWithUX = {
        twinDto: entry,
        collapseKey: entry.collapseKey,
        isSelected: entry.isSelected,
        isCollapsed: entry.isCollapsed,
        collapsedTwinIds: entry.isCollapsed === true ? entry.groupKey.split(',') : [],
        isExpanded: entry.isExpanded,
        label: entry.name,
        x: entry.x,
        y: entry.y,
        twinIds: [entry.twinId, ...twinIds].slice(0, 3),
        style: style,
        overlay: undefined
      };

      return nodeFormatter(uxNode);
    });

    const dedges = els.edges!.map((x: any) => edgeFormatter(x));

    return { dnodes, dedges };
  });

  const graphElement = useRef<any>();

  const flow = useReactFlow();

  const data = twinQuery.data;
  const [open, setOpen] = useState(false);
  const [nodes, setNodes] = useState<TwinNode[]>([]);

  const handleOpen = () => {
    setOpen(true);
  };
  const [myCenter, setMyCenter] = useState({ x: 0, y: 0, zoom: 1 });
  const [currentNode, setCurrentNode] = useState<TwinNode>(null!);
  const [overlay, setOverlay] = useState<IGraphOverlay>(new OverlayInsightsCount());
  const [graphCompleted, setgraphCompleted] = useState(false);

  const handleClose = () => {
    setOpen(false);
  };

  useMemo(() => {
    try {
      if (twinQuery.isFetched && !twinQuery.isError && data) {
        if (data.dnodes.length > 0) {
          (async () => { setNodes(data.dnodes); })()
            .then(async () => {
              await overlay.load(twinIds, data.dnodes.map(v => v.uxNode))
                .then((result) => {
                  result.forEach((overlay) => {
                    const node = data.dnodes.find(n => n.uxNode.twinDto.twinId == overlay.twinId);

                    if (node !== undefined) {
                      node.uxNode.overlay = overlay;

                      node.data = {
                        label: TwinFormatter(node.id, node.uxNode)
                      }

                      updateNode(node);
                    }
                  });
                });
            })
            .then(() => {
              const { dnodes, dedges } = data;
              const anyNode = dnodes[0];
              const startingBounds: IBoundingBox = {
                minX: anyNode.position.x,
                minY: anyNode.position.y,
                maxX: anyNode.position.x,
                maxY: anyNode.position.y,
              };

              const bounds = dnodes.reduce<IBoundingBox>(reducer, startingBounds);

              const selected = dnodes.filter((x) => x.selected);
              // If we have a selected objects, focus on them
              if (selected && selected.length > 0) {
                const selectedBounds = selected.reduce<IBoundingBox>(
                  reducer,
                  startingBounds
                );
                const selectedCenter = {
                  x: (selectedBounds.minX + selectedBounds.maxX) / 2,
                  y: (selectedBounds.minY + selectedBounds.maxY) / 2,
                };
                setMyCenter({
                  x: selectedCenter.x + boxWidth / 2,
                  y: selectedCenter.y + boxHeight / 2,
                  zoom: 1,
                });
              }
              else {
                // Go to the central-most node
                console.log("Center on central-most node");
                const el = goCenter(dnodes, bounds);
                setCurrentNode(el);
                setMyCenter({
                  x: el.position.x + boxWidth / 2,
                  y: el.position.y + boxHeight / 2,
                  zoom: 1,
                });
              }
            })
            .then(() => { setgraphCompleted(true); });
        }
      }
    } catch (e) {
      console.log(e);
    }
  }, [twinIds, twinQuery.isFetched, open]);

  const boundingBox = useQuery(["boundingBox", twinIds, twinQuery.data], () => {
    if (!data || data.dnodes.length === 0) {
      return { minX: 0, minY: 0, maxX: 900, maxY: 500 };
    }
    let startingBounds: IBoundingBox = { minX: 0, minY: 0, maxX: 0, maxY: 0 };
    const bounds = data.dnodes.reduce<IBoundingBox>(reducer, startingBounds);
    return bounds;
  });

  const navigate = (dnodes: TwinNode[], direction: Itransition) => {
    const el = direction(dnodes, currentNode);
    setCurrentNode(el);
    setMyCenter({
      x: el.position.x + boxWidth / 2,
      y: el.position.y + boxHeight / 2,
      zoom: 1,
    });
  };

  const keyDown = (e: KeyboardEvent) => {
    if (!data) return;
    const { dnodes, dedges } = data;
    if (!boundingBox.isFetched || !boundingBox.data) return;
    const bounds = boundingBox.data!;

    switch (e.key) {
      case "c": {
        const el = goCenter(dnodes, bounds);
        setCurrentNode(el);
        const newCenter = { x: el.position.x, y: el.position.y };
        const zoom = Math.max(
          1,
          Math.min(
            11,
            Math.min(
              900 / (bounds.maxX - bounds.minX),
              500 / (bounds.maxY - bounds.minY)
            )
          )
        );
        setMyCenter({
          x: newCenter.x + boxWidth / 2,
          y: newCenter.y + boxHeight / 2,
          zoom: zoom,
        });
        break;
      }
      case "r": {
        navigate(dnodes, goRight);
        break;
      }
      case "l": {
        navigate(dnodes, goLeft);
        break;
      }
      case "u": {
        navigate(dnodes, goUp);
        break;
      }
      case "d": {
        navigate(dnodes, goDown);
        break;
      }
      case "+": {
        setMyCenter({
          x: myCenter.x,
          y: myCenter.y,
          zoom: Math.min(11, myCenter.zoom + 1),
        });
        break;
      }
      case "-": {
        setMyCenter({
          x: myCenter.x,
          y: myCenter.y,
          zoom: Math.max(1, myCenter.zoom - 1),
        });
        break;
      }
    }
  };

  useKeyboardEventListener(keyDown, window);

  if (twinQuery.isLoading) return <div>Loading...</div>;
  if (twinQuery.isError) return <div>Error...</div>;
  if (!twinQuery.isFetched) return <div>Loading...</div>;
  if (!data || data.dnodes.length === 0) return <div>No graph data...</div>;

  const updateNode = (item: TwinNode) => {
    setNodes(oldState => {
      let newState = [...oldState];
      const index = newState.findIndex(v => v.id == item.id);
      newState[index] = item;
      return newState;
    });
  }

  flow.setCenter(myCenter.x, myCenter.y, { zoom: myCenter.zoom, duration: 1000 });

  return twinQuery.isFetched && graphCompleted ? (
    <Box
      sx={{
        width: "100%",
        height: 700,
        padding: 0,
        textAlign: "right"
      }}>
      <Button onClick={handleOpen} endIcon={<OpenInFullIcon />}>
        Expand
      </Button>
      <Modal
        sx={{
          position: "absolute" as "absolute",
          top: "50%",
          left: "50%",
          transform: "translate(-50%, -50%)",
          width: "90%",
          height: "90%",
          bgcolor: "background.paper",
          border: "0.1px solid rgb(201, 185, 185, 15%)",
          boxShadow: 24,
        }}
        open={open}
        onClose={handleClose}
        aria-labelledby="modal-modal-title"
        aria-describedby="modal-modal-description"
      >
        <Box
          sx={{
            height: "100%",
            width: "100%",
            textAlign: "right",
            overflow: "hidden",
          }}
        >
          <Button
            sx={{ width: 120, height: 60 }}
            onClick={handleClose}
            endIcon={<CloseIcon />}
          >
            Close
          </Button>
          <Divider component="div" />
          <ReactFlow
            defaultNodes={nodes}
            defaultEdges={data.dedges}
            ref={graphElement}
            proOptions={proOptions}
            style={graphStyles && { border: "none", marginTop: "1rem" }}
          />
        </Box>
      </Modal>
      {!open && (
        <ReactFlow
          defaultNodes={nodes}
          defaultEdges={data.dedges}
          ref={graphElement}
          proOptions={proOptions}
          style={graphStyles}
        />
      )}
    </Box>
  ) : twinQuery.isError ? (
    <div>Error</div>
  ) : (
    <div>Loading...</div>
  );
};

const TwinGraph: React.FC<TwinGraphInputProps> = ({ twinIds, highlightedIds, isCollapsed }) => {
  return (
    <ReactFlowProvider>
      <TwinGraphInternal twinIds={twinIds} highlightedIds={highlightedIds} isCollapsed={isCollapsed} />
    </ReactFlowProvider>
  );
};

export default TwinGraph;
