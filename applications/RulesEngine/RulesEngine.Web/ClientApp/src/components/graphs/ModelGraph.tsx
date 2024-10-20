import * as React from "react";
import { ModelSimpleRelationshipDto } from "../../Rules";
import { useEffect, useState } from "react";
import ReactFlow, {
  Node,
  Position,
  ReactFlowProvider,
  useReactFlow,
  Viewport,
} from "reactflow";
import 'reactflow/dist/style.css';
import IconForModel from "../icons/IconForModel";
import useApi from "../../hooks/useApi";
import { useQuery } from "react-query";
import ELK, { ElkNode } from "elkjs";
import { Box, Button, Divider, Modal, useTheme } from "@mui/material";
import edgeColor from "../../hooks/edgeColor";
import nodeStyle from "../../hooks/nodeStyle";
import { graphStyles, IBoundingBox } from "./GraphBase";
import StyledLink from "../styled/StyledLink";
import useKeyboardEventListener from "../../hooks/useEventListener";
import {
  boxHeight,
  boxWidth,
  goCenter,
  goDown,
  goLeft,
  goRight,
  goUp,
  Itransition,
  reducer,
} from "./keyboardnavigator";
import OpenInFullIcon from "@mui/icons-material/OpenInFull";
import CloseIcon from "@mui/icons-material/Close";

interface ModelGraphInputProps {
  modelId: string | undefined;
  secondaryModelIds?: string[] | undefined;
}

type TwinNode = Node<any> & { expanded: boolean };

const ModelGraphInternal: React.FC<ModelGraphInputProps> = ({ modelId, secondaryModelIds }) => {
  const theme = useTheme();
  const apiclient = useApi();
  const proOptions = { hideAttribution: true };

  const ModelFormatter = (
    label: string,
    modelId: string,
    count: number,
    countInherited: number
  ) => (
    <StyledLink to={`/model/${modelId}`}>
      <IconForModel modelId={modelId} size={14} />
      &nbsp; {label} ({count}/{countInherited}){" "}
    </StyledLink>
  );

  const edgeFormatter = (x: ModelSimpleRelationshipDto) => ({
    id: `e${x.startId}-${x.endId}-${x.relationship}-${x.substance}`,
    target: `${x.startId}`,
    source: `${x.endId}`,
    animated: x.relationship === "isFedBy",
    data: { label: `${x.relationship} ${x.substance}` },
    label: x.relationship === "isFedBy" ? x.substance : x.relationship,
    labelStyle: { fill: theme.palette.primary.contrastText },
    labelBgStyle: { fill: theme.palette.primary.main, fillOpacity: 0.1 },
    style: {
      stroke: edgeColor(x),
    },
  });

  const nodeFormatter = (d: {
    id: number;
    type: string;
    x: number;
    y: number;
    modelId: string;
    count: number;
    countInherited: number;
    label: string;
    selected: boolean;
  }): any => {
    const style = nodeStyle(
      { ...d, isSelected: d.selected, isCollapsed: false, isExpanded: false },
      theme
    );

    return {
      id: `${d.id}`,
      modelId: d.modelId,
      type: "default",
      data: {
        label: ModelFormatter(d.label, d.modelId, d.count, d.countInherited),
      },
      position: {
        x: d.x,
        y: d.y,
      },
      style: style,
      selected: d.selected,
      sourcePosition: Position.Bottom,
      targetPosition: Position.Top,
    };
  };

  const elk = new ELK({
    // TODO: Figure out how to use WebWorker
    //workerFactory: (uri) => new Worker('./node_modules/elkjs/lib/elk-worker.min.js')
    //workerUrl: './node_modules/elkjs/lib/elk-worker.min.js'
  });

  // const [elements, setElements] = useState<Elements<IModelSimpleDtoWithCoordinates>>([]);
  const getLayoutedElements = async (elements: any) => {
    if (!elements) return {} as ElkNode;

    const done = await elk.layout(elements, {
      layoutOptions: {
        aspectRatio: "1.77",
        //algorithm: 'org.eclipse.elk.sporeOverlap',
        //algorithm: 'org.eclipse.elk.layered',
        algorithm: "org.eclipse.elk.force",
        "org.eclipse.elk.force.temperature": "0.0001",
        "org.eclipse.elk.layered.priority.direction": "1",
        strategy: "LINEAR_SEGMENTS",
        "org.eclipse.elk.nodePlacement.strategy": "LINEAR_SEGMENTS",
        "org.eclipse.elk.spacing.nodeNode": "25",
        "org.eclipse.elk.layered.spacing.nodeNodeBetweenLayers": "35",
        // org.eclipse.elk.sporeOverlap
        // algorithm: 'org.eclipse.elk.force'
      },
      logging: false,
      measureExecutionTime: false,
    });

    return done;
  };

  var graphQuery = useQuery(
    ["modelGraph", modelId],
    async (x) => {
      console.log("Request model system graph");
      const graph = await apiclient.modelSystemGraph(modelId ?? "missing");
      console.log("Got model system graph", graph);

      var start = graph.nodes?.find((x) => x.modelId == modelId);

      // Is this model used in the current Twin graph?
      if (start) {
        const nodes = graph.nodes!.map((x) => ({
          ...x,
          id: `${x.id!}`,
          width: 150,
          height: 40,
          selected: x.modelId === modelId || secondaryModelIds?.includes(x.modelId ?? '')
        }));
        const edges = graph
          .relationships!.filter((x) => x.relationship != "isCapabilityOf")
          .map((x, i) => ({
            ...x,
            id: `${i}`,
            sources: [`${x.startId}`],
            targets: [`${x.endId}`],
          }));

        const root: ElkNode = {
          id: "root",
          layoutOptions: { "elk.direction": "UP" },
          children: nodes,
          edges: edges,
        };

        const els = await getLayoutedElements(root);

        // And convert
        const dnodes = Object.entries(els.children!).map((dn: any) =>
          nodeFormatter(dn[1])
        );
        const dedges = els.edges!.map((edge: any) => edgeFormatter(edge));

        let startingBounds =
          dnodes.length === 0
            ? { minX: 0, minY: 0, maxX: 900, maxY: 500 }
            : {
              minX: dnodes[0].x,
              minY: dnodes[0].y,
              maxX: dnodes[0].x,
              maxY: dnodes[0].y,
            };
        const bounds = dnodes.reduce<IBoundingBox>(reducer, startingBounds);
        return { dnodes, dedges, bounds };
      } else {
        console.log("Model graph did not find", modelId);
      }
    },
    {
      enabled: !!modelId,
    }
  );

  const flow = useReactFlow();

  const data = graphQuery.data;

  const [myCenter, setMyCenter] = useState({ x: 0, y: 0, zoom: 10 });
  const [currentNode, setCurrentNode] = useState<TwinNode>(null!);

  useEffect(() => {
    if (myCenter.x != 0 && myCenter.y != 0) {
      flow.setCenter(myCenter.x, myCenter.y, {
        zoom: myCenter.zoom,
        duration: 1000,
      });
    }
  }, [myCenter]);

  const navigate = (dnodes: TwinNode[], direction: Itransition) => {
    const el = direction(dnodes, currentNode);
    setCurrentNode(el);
    setMyCenter({
      x: el.position.x + boxWidth / 2,
      y: el.position.y + boxHeight / 2,
      zoom: 2,
    });
  };

  const getSelectedCenter = (dnodes: TwinNode[]) => {
    const selected = dnodes.filter((n) => n.selected);
    // TODO: Bounding box of all selected and figure out zoom
    if (selected && selected.length > 0) {
      const x = selected[0].position.x;
      const y = selected[0].position.y;
      return ({ x: (x + boxWidth / 2), y: (y + boxHeight / 2) });
    }
    else {
      return { x: 0, y: 0 };
    }
  }

  const keyDown = (e: KeyboardEvent) => {
    if (!data) return;

    const { dnodes, dedges, bounds } = data;

    switch (e.key) {
      case "c": {

        const { x, y } = getSelectedCenter(dnodes);

        if (x > 0) {
          setMyCenter({ x, y, zoom: 1, });
          break;
        }

        console.log('Did not find a selection');

        const el = goCenter(dnodes, bounds);
        console.log('center', el);
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
          zoom: 2,
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

  const [open, setOpen] = React.useState(false);

  const handleOpen = () => {
    setOpen(true);
    console.log("handling close modal");
  };

  const handleClose = () => {
    setOpen(false);
    console.log("handling close modal");
  };

  useEffect(() => {
    try {
      if (graphQuery.isFetched && data) {
        const { dnodes, dedges } = data;

        //console.log(elements);

        let startingBounds: IBoundingBox = {
          minX: 0,
          minY: 0,
          maxX: 0,
          maxY: 0,
        };

        const reducer = (
          b: IBoundingBox,
          e: any,
          i: number,
          a: any[]
        ): IBoundingBox => ({
          minX: Math.min(e.position?.x ?? 0, b.minX),
          maxX: Math.max(e.position?.x ?? 0, b.maxX),
          minY: Math.min(e.position?.y ?? 0, b.minY),
          maxY: Math.max(e.position?.y ?? 0, b.maxY),
        });

        const bounds = dnodes.reduce<IBoundingBox>(reducer, startingBounds);

        //console.log(bounds);

        const selected = dnodes.filter((x) => x.selected);
        if (selected && selected.length > 0) {
          const x = selected[0].position.x;
          const y = selected[0].position.x;

          console.log('Starting centered on selected node', selected[0]);

          flow.setCenter(x + boxWidth / 2, y + boxHeight / 2, { zoom: .5, duration: 5000 });
        }
        else {
          flow.fitView({ duration: 1000 });
        }
      }
      //graphElement.current?.fitView({ padding: 5 });
    } catch (e) {
      console.log(e);
    }
  }, [graphQuery.data, modelId]);

  if (graphQuery.isLoading) return <div>Loading...</div>;
  if (graphQuery.isError) return <div>Error...</div>;
  if (!graphQuery.isFetched) return <div>Loading...</div>;
  if (!data) return <div>No data...</div>;

  return graphQuery.isFetched && data.dnodes && data.dnodes.length > 0 ? (
    <Box
      sx={{
        width: "100%",
        height: 690,
        textAlign: "right",
        marginBottom: "2rem",

      }}
    >
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
            defaultNodes={data.dnodes}
            defaultEdges={data.dedges}
            proOptions={proOptions}
            style={
              graphStyles && {
                border: "none",
                marginTop: "1rem",
              }
            }
          />
        </Box>
      </Modal>
      {!open && (
        <ReactFlow
          defaultNodes={data.dnodes}
          defaultEdges={data.dedges}
          proOptions={proOptions}
          style={graphStyles}
        />
      )}
    </Box>
  ) : graphQuery.isFetched ? (
    <div>Not in use in Twin</div>
  ) : graphQuery.isError ? (
    <div>Error</div>
  ) : (
    <div>Loading...</div>
  );
};

const ModelGraph: React.FC<ModelGraphInputProps> = ({ modelId, secondaryModelIds }) => {
  return (
    <ReactFlowProvider>
      <ModelGraphInternal modelId={modelId} secondaryModelIds={secondaryModelIds} />
    </ReactFlowProvider>
  );
};

export default ModelGraph;
