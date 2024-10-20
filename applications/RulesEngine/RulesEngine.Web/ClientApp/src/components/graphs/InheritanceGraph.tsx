import * as React from 'react';
import { useEffect, useState } from 'react';
import ReactFlow, { Node, ReactFlowProvider, useReactFlow, Viewport } from 'reactflow';
import 'reactflow/dist/style.css';
import IconForModel from '../icons/IconForModel';
import useApi from '../../hooks/useApi';
import { useQuery } from 'react-query';
import ELK, { ElkNode } from 'elkjs';
import { useTheme } from '@mui/material';
import nodeStyle from '../../hooks/nodeStyle';
import { IBoundingBox } from './GraphBase';
import StyledLink from '../styled/StyledLink';
import { ModelSimpleGraphDto, ModelSimpleDto, ModelSimpleRelationshipDto, BatchRequestDto } from '../../Rules';
import { goDown, goLeft, goRight, goUp, goCenter, Itransition, reducer } from './keyboardnavigator';
import useKeyboardEventListener from '../../hooks/useEventListener';

interface InheritanceGraphInputProps {
  modelId: string | undefined;
}

type GraphNode = Node<ModelSimpleDto> & { expanded: boolean };

const boxWidth = 70;
const boxHeight = 40;

const InheritanceGraphInternal: React.FC<InheritanceGraphInputProps> = ({ modelId }) => {

  const theme = useTheme();
  const apiclient = useApi();
  const proOptions = { hideAttribution: true };

  const pickALanguage = (names: { [key: string]: string; }): string => {
    return names["en"] ?? "Missing english text";
  };

  // TODO: Tooltip with description too
  const ModelFormatter = (model: ModelSimpleDto) =>
    (<StyledLink to={`/model/${model.modelId!}`}><IconForModel modelId={model.modelId!} size={14} />&nbsp; {pickALanguage(model.languageDisplayNames!)} ({model.count}/{(model.count! + model.countInherited!)}) </StyledLink>);

  const edgeFormatter = (x: ModelSimpleRelationshipDto) => ({
    id: `e${x.startId}-${x.relationship!}-${x.endId}`, target: `${x.endId}`, source: `${x.startId}`,
    data: { label: x.relationship! },
    label: x.relationship,
    labelStyle: { fill: theme.palette.primary.contrastText },
    labelBgStyle: { fill: theme.palette.primary.main, fillOpacity: 0.1 },
    style: {
      stroke: "purple",
    }
  });

  // d extends ModelDto, fix Typescript for that?
  const nodeFormatter = (d: { id: number, modelId: string, type: string, x: number, y: number, name: string, isSelected: boolean }): any => {

    const isSelected = d.modelId == modelId;
    const style = nodeStyle({ ...d, isCollapsed: false, isExpanded: false, isSelected: isSelected }, theme);

    return ({
      id: `${d.id}`,
      modelId: d.id,
      type: "default",
      sourcePosition: "top",
      targetPosition: "bottom",
      data: { label: ModelFormatter(d as any as ModelSimpleDto) },
      position:
      {
        x: d.x,
        y: d.y
      },
      style: style
    })
  };

  const elk = new ELK({
    // TODO: Figure out how to use WebWorker
    //workerFactory: (uri) => new Worker('./node_modules/elkjs/lib/elk-worker.min.js')
    //workerUrl: './node_modules/elkjs/lib/elk-worker.min.js'
  })

  // const [elements, setElements] = useState<Elements<IModelSimpleDtoWithCoordinates>>([]);
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
      measureExecutionTime: false
    });

    return done;
  };

  var graphQuery = useQuery(["modelInheritance", modelId], async () => {

    console.log('Request model inheritance graph');
    const graph = await apiclient.ontology(modelId, new BatchRequestDto());
    console.log('Got model inheritance graph', graph);

    var start = graph.items?.find(y => y.modelId == modelId);

    // Is this model used in the current Twin graph?
    if (start) {

      const nodes = graph.items!.map(x => ({ ...x, id: `${x.id!}`, width: 150, height: 40 }));
      const edges = graph.relationships!
        //.filter(x => x.relationship != 'isCapabilityOf')
        .map((x, i) => ({ ...x, id: `${i}`, sources: [`${x.startId}`], targets: [`${x.endId}`] }));

      const root: ElkNode =
      {
        id: "root",
        layoutOptions: { "elk.direction": "UP" },
        children: nodes,
        edges: edges
      };

      const els = await getLayoutedElements(root);

      // And convert
      const dnodes = Object.entries(els.children!).map((x: any) => nodeFormatter(x[1]));
      const dedges = els.edges!.map((x: any) => edgeFormatter(x));

      return { dnodes, dedges };
    }
    else {
      console.log("Model graph did not find", modelId);
    }
  },
    {
      enabled: !!modelId
    });

  const [reactFlowInstance, setReactFlowInstance] = useState<any>(null);

  const onLoadHandler = (rf: any) => {
    setReactFlowInstance(rf);
  }

  const flow = useReactFlow();

  const data = graphQuery.data;

  useEffect(() => {
    try {
      if (graphQuery.isFetched && data) {

        const { dnodes, dedges } = data;

        //console.log(elements);

        let startingBounds: IBoundingBox = { minX: 0, minY: 0, maxX: 0, maxY: 0 };

        const reducer = (b: IBoundingBox, e: any, i: number, a: any[]): IBoundingBox =>
        ({
          minX: Math.min(e.position?.x ?? 0, b.minX), maxX: Math.max(e.position?.x ?? 0, b.maxX),
          minY: Math.min(e.position?.y ?? 0, b.minY), maxY: Math.max(e.position?.y ?? 0, b.maxY)
        });

        const bounds = dnodes.reduce<IBoundingBox>(reducer, startingBounds);

        //console.log(bounds);

        const selected = dnodes.filter(x => x.selected);
        if (selected && selected.length > 0) {
          const x = selected[0].position.x;
          const y = selected[0].position.x;

          // If our selected object is off screen, bring it to the center
          const newCenter = { x: bounds.maxX / 2, y: bounds.maxY / 2 };
          if (x > bounds.maxX) newCenter.x = x;
          if (y > bounds.maxY) newCenter.y = y;

          flow.setCenter(newCenter.x, newCenter.y, { zoom: 1, duration: 5000 });
        }

        flow.fitView({ duration: 1000 });
      }
      //graphElement.current?.fitView({ padding: 5 });
    }
    catch (e) {
      console.log(e);
    }
  }, [graphQuery, graphQuery.data, modelId]);

  const [currentNode, setCurrentNode] = useState<GraphNode>(null!);
  const [myCenter, setMyCenter] = useState({ x: 0, y: 0, zoom: 10 });

  const setCenter = (x: number, y: number, zoom: number | undefined) => {
    setMyCenter({ x: x + boxWidth / 2, y: y + boxHeight / 2, zoom: zoom ?? myCenter.zoom });
  };

  useEffect(() => {
    if (myCenter.x != 0 && myCenter.y != 0) {
      flow.setCenter(myCenter.x, myCenter.y, { zoom: myCenter.zoom, duration: 1000 });
    }
  }, [myCenter])

  const navigate = (dnodes: GraphNode[], direction: Itransition) => {
    const el = direction(dnodes, currentNode);
    setCurrentNode(el);
    setCenter(el.position.x, el.position.y, 2);
  };

  const boundingBox = useQuery(["boundingBox", modelId, data], () => {
    if (!data || data.dnodes.length === 0) {
      return { minX: 0, minY: 0, maxX: 900, maxY: 500 };
    }
    let startingBounds: IBoundingBox = { minX: 0, minY: 0, maxX: 0, maxY: 0 };
    const bounds = data.dnodes.reduce<IBoundingBox>(reducer, startingBounds);
    return bounds;
  });

  const keyDown = (e: KeyboardEvent) => {
    if (!data) return;
    const { dnodes, dedges } = data;
    if (!boundingBox.isFetched || !boundingBox.data) return;
    const bounds = boundingBox.data!;

    switch (e.key) {
      case 'c':
        {
          const el = goCenter(dnodes, bounds);
          setCurrentNode(el);
          const newCenter = { x: el.position.x, y: el.position.y };
          const zoom = Math.max(1, Math.min(11, Math.min(900 / (bounds.maxX - bounds.minX), 500 / (bounds.maxY - bounds.minY))));
          setCenter(newCenter.x, newCenter.y, zoom);
          break;
        }
      case 'r':
        {
          navigate(dnodes, goRight);
          break;
        }
      case 'l':
        {
          navigate(dnodes, goLeft);
          break;
        }
      case 'u':
        {
          navigate(dnodes, goUp);
          break;
        }
      case 'd':
        {
          navigate(dnodes, goDown);
          break;
        }
      case '+':
        {
          setCenter(myCenter.x, myCenter.y, Math.min(11, myCenter.zoom + 1));
          break;
        }
      case '-':
        {
          setCenter(myCenter.x, myCenter.y, Math.max(1, myCenter.zoom - 1));
          break;
        }
    }
  };

  useKeyboardEventListener(keyDown, window);

  if (graphQuery.isLoading) return (<div>Loading...</div>);
  if (graphQuery.isError) return (<div>Error...</div>);
  if (!graphQuery.isFetched) return (<div>Loading...</div>);
  if (!data) return (<div>No data...</div>);

  return (
    (graphQuery.isFetched && data.dnodes && data.dnodes.length > 0) ?
      (
        <div style={{ height: 400, width: "100%" }}>
          <ReactFlow
            defaultNodes={data.dnodes}
            defaultEdges={data.dedges}
            onLoad={onLoadHandler}
            proOptions={proOptions}
          >
          </ReactFlow>
        </div>
      ) :
      (graphQuery.isFetched) ?
        (<div>Not in use in Twin</div>) :
        (graphQuery.isError) ?
          (<div>Error</div>) :
          (<div>Loading...</div>)
  );
};

const InheritanceGraph: React.FC<InheritanceGraphInputProps> = ({ modelId }) => {

  return (
    <ReactFlowProvider>
      <InheritanceGraphInternal modelId={modelId} />
    </ReactFlowProvider>
  );
}

export default InheritanceGraph;
