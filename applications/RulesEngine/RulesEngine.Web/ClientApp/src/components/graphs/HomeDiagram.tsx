import * as React from "react";
import { HealthStatus, SystemSummaryDto } from "../../Rules";
import { useMemo } from "react";
import ReactFlow, { Background, Handle, Position, Node, Edge, MarkerType } from "reactflow";
import 'reactflow/dist/style.css';
import Logo from '../icons/LogoSimple';
import { Grid, Stack, Typography } from "@mui/material";
import { Moment } from "moment";

interface HomeDiagramProps {
  summary: SystemSummaryDto
}

const RulesEngineNode = ({ data }: { data: { label: string, asOf: Moment, height: number, speed: number } }) => {
  const { label, height, asOf, speed } = data;
  return (
    <>
      <Handle type="target" position={Position.Left} id="a" />
      {height > 0 && <Handle type="target" position={Position.Left} id="b" style={{ top: 3 * height / 4 }} />}
      {height > 0 && <Handle type="target" position={Position.Left} id="t" style={{ top: height / 4 }} />}
      <Stack direction={"column"} padding={"10px"} alignContent={"center"} textAlign={"center"}
        style={{ minWidth: 120, borderStyle: 'double', backgroundColor: '#222', color: 'white', borderRadius: 10, height: data.height, verticalAlign: 'middle' }}>
        <div>
          <Typography fontSize={12}>{label}</Typography>
        </div>
        <div style={{ display: 'flex', justifyContent: 'center', alignContent: 'center', width: '100%', paddingTop: 10, paddingBottom: 10 }}>
          <div style={{ height: 23, width: 65 }}><Logo /></div>
        </div>
        <div>
          <Typography fontSize={10}>{asOf?.fromNow()}</Typography>
        </div>
        {speed && <div>
          <Typography fontSize={10}>{speed.toFixed(1)}x real time</Typography>
        </div>}
      </Stack>
      <Handle type="source" position={Position.Right} id="a" />
      {height>0 && <Handle type="source" position={Position.Right} id="b" style={{ top: 3 * height / 4 }} />}
      {height>0 && <Handle type="source" position={Position.Right} id="t" style={{ top: height / 4 }} />}
    </>);
};

const WillowNode = ({ data }: { data: { label: string, count: number, height: number, health: HealthStatus | undefined } }) => {
  const { label, count, height, health } = data;
  return (
    <>
      <Handle type="target" position={Position.Left} id="a" />
      {height && <Handle type="target" position={Position.Left} id="b" style={{ top: 3 * height / 4 }} />}
      {height && <Handle type="target" position={Position.Left} id="t" style={{ top: height / 4 }} />}
      <Grid container direction={"column"} padding={"10px"} alignContent={"center"} textAlign={"center"}
        style={{
          minWidth: 120, borderStyle: '1px',
          backgroundColor: health == HealthStatus._0 ? 'red' : health == HealthStatus._1 ? 'orange' : 'green',
          color: 'white', borderRadius: 10, height: data.height, verticalAlign: 'middle'
        }}>
        <Grid item>
          <Typography fontSize={12}>{label}</Typography>
        </Grid>
        <Grid item>
          {count > 0 && <Typography fontSize={12}>{count.toLocaleString()}</Typography>}
        </Grid>
      </Grid>
      <Handle type="source" position={Position.Right} id="a" />
      {height && <Handle type="source" position={Position.Right} id="b" style={{ top: 3 * height / 4 }} />}
      {height && <Handle type="source" position={Position.Right} id="t" style={{ top: height / 4 }} />}
    </>);
};


const HomeDiagram: React.FC<HomeDiagramProps> = (props: HomeDiagramProps) => {

  const { summary } = props;

  let calculatedPointsHealth = HealthStatus._2;
  let authorizationHealth = HealthStatus._2;
  let publicAPIHealth = HealthStatus._2;
  let commandAPIHealth = HealthStatus._2;

  summary.health?.map(x => {
    switch (x.key) {
      case "CalculatedPoints":
        calculatedPointsHealth = x.status ?? HealthStatus._1;
        break;
      case "Authorization Service":
        authorizationHealth = x.status ?? HealthStatus._1;
        break;
      case "Public API":
        publicAPIHealth = x.status ?? HealthStatus._1;
        break;
      case "Command API":
        commandAPIHealth = x.status ?? HealthStatus._1;
        break;
    }
    return x;
  });

  const nodeTypes = useMemo(() => ({ rulesEngineNode: RulesEngineNode, WillowNode: WillowNode }), []);

  const nodes: Node<any>[] = [
    {
      id: "rules",
      position: { x: 0, y: 70 },
      data: { label: 'Skills', count: summary.countRules },
      type: "WillowNode"
    },
    {
      id: "relationships",
      position: { x: 0, y: 140 },
      data: { label: 'Relationships', count: summary.countRelationships },
      type: "WillowNode"
    },
    {
      id: "twins",
      position: { x: 0, y: 210 },
      data: { label: 'Twins', count: summary.countTwins },
      type: "WillowNode"
    },
    {
      id: "livedata",
      position: { x: 0, y: 280 },
      data: { label: 'Live data', count: summary.countLiveData },
      type: "WillowNode"
    },


    {
      id: "instancegeneration",
      position: { x: 200, y: 110 },
      data: { label: 'Generation', height: 110, asOf: summary.adtAsOfDate },
      type: "rulesEngineNode"
    },

    {
      id: "capabilities",
      position: { x: 200, y: 230 },
      data: { label: 'Capabilities', count: summary.countCapabilities },
      type: "WillowNode"
    },


    {
      id: "timeseriesbuffers",
      position: { x: 400, y: 250 },
      data: { label: 'Time Series Buffers', count: summary.countTimeSeriesBuffers, height: 75 },
      type: "WillowNode"
    },


    {
      id: "instances",
      position: { x: 400, y: 90 },
      data: { label: 'Instances', count: summary.countRuleInstances },
      type: "WillowNode"
    },

    {
      id: "calculatedpoints",
      position: { x: 400, y: 160 },
      data: { label: 'Calculated Points', count: summary.countCalculatedPoints },
      type: "WillowNode"
    },

    {
      id: "rulesengine",
      position: { x: 650, y: 140 },
      data: { label: 'Willow Activate', height: 130, asOf: summary.lastTimeStamp, speed: summary.speed },
      type: "rulesEngineNode"
    },

    {
      id: "insights",
      position: { x: 840, y: 160 },
      data: { label: 'Insights', count: summary.countInsightsFaulted },
      type: "WillowNode"
    },

    {
      id: "commands",
      position: { x: 840, y: 230 },
      data: { label: 'Commands', count: summary.countCommands },
      type: "WillowNode"
    },

    {
      id: "commandAndControl",
      position: { x: 1000, y: 230 },
      data: { label: 'C&C App', count: summary.countCommandsTriggering, health: commandAPIHealth },
      type: "WillowNode"
    },

    {
      id: "calculatedpointsoutput",
      position: { x: 1000, y: 70 },
      data: { label: 'Calculated Points', count: summary.countCalculatedPoints, health: calculatedPointsHealth },
      type: "WillowNode"
    },

    {
      id: "WTinsights",
      position: { x: 1000, y: 150 },
      data: { label: 'WillowTwin App', count: summary.countCommandInsights, health: publicAPIHealth },
      type: "WillowNode"
    },

    {
      id: "dataquality",
      position: { x: 1000, y: 300 },
      data: { label: 'Data Quality', count: summary.countTimeSeriesBuffers },
      type: "WillowNode"
    },

    {
      id: "authorization",
      position: { x: 635, y: 280 },
      data: { label: 'Authorization Service', count: 0, health: authorizationHealth },
      type: "WillowNode"
    },

  ];

  const edgeDefault = {
    //markerStart: { type: MarkerType.ArrowClosed, color: "#FF0072" },
    markerEnd: { type: MarkerType.ArrowClosed, color: "#FF0072" },
    style: {
      strokeWidth: 2,
      stroke: '#FF0072',
    }
  };

  const edges: Edge<any>[] = [
    {
      ...edgeDefault,
      id: "1",
      source: "twins",
      target: "instancegeneration",
      targetHandle: "b",
    },
    {
      ...edgeDefault,
      id: "1t",
      source: "twins",
      target: "capabilities",
      targetHandle: "a",
    },
    {
      ...edgeDefault,
      id: "1c",
      source: "capabilities",
      target: "timeseriesbuffers",
      targetHandle: "t",
    },
    {
      ...edgeDefault,
      id: "2",
      source: "relationships",
      target: "instancegeneration",
      targetHandle: "a"
    },
    {
      ...edgeDefault,
      id: "3",
      source: "rules",
      target: "instancegeneration",
      targetHandle: "t"
    },
    {
      ...edgeDefault,
      id: "3b",
      source: "livedata",
      target: "timeseriesbuffers",
      targetHandle: "b",
      animated: true
    },
    {
      ...edgeDefault,
      id: "4",
      source: "instancegeneration",
      sourceHandle: "t",
      target: "instances"
    },
    {
      ...edgeDefault,
      id: "4b",
      source: "instancegeneration",
      sourceHandle: "b",
      target: "calculatedpoints"
    },
    {
      ...edgeDefault,
      id: "4i",
      source: "instances",
      target: "rulesengine",
      targetHandle: "t"
    },
    {
      ...edgeDefault,
      id: "5",
      source: "timeseriesbuffers",
      sourceHandle: "t",
      target: "rulesengine",
      targetHandle: "b",
      animated: true
    },
    {
      ...edgeDefault,
      id: "6",
      source: "rulesengine",
      sourceHandle: "a",
      target: "insights",
      targetHandle: "a",
      animated: true
    },
    {
      ...edgeDefault,
      id: "7",
      source: "insights",
      target: "WTinsights",
      animated: true
    },
    {
      ...edgeDefault,
      id: "7b",
      source: "rulesengine",
      sourceHandle: "t",
      target: "calculatedpointsoutput",
      animated: true
    },
    {
      ...edgeDefault,
      id: "8",
      source: "rulesengine",
      sourceHandle: "b",
      target: "dataquality",
      animated: true
    },
    {
      ...edgeDefault,
      id: "9",
      source: "calculatedpoints",
      target: "rulesengine",
      targetHandle: "a"
    },
    {
      ...edgeDefault,
      id: "10",
      source: "rulesengine",
      sourceHandle: "a",
      target: "commands",
      targetHandle: "a",
      animated: true
    },
    {
      ...edgeDefault,
      id: "11",
      source: "commands",
      target: "commandAndControl",
      animated: true
    },
  ];


  return (
    <div style={{ width: '100%', height: 400 }}>
      <ReactFlow
        nodes={nodes}
        edges={edges}
        nodeTypes={nodeTypes}
        fitView
        attributionPosition="top-right"
        maxZoom={1.5}
        minZoom={1}
        proOptions={{ hideAttribution: true }}>
        <Background color="#aaa" gap={16} />
      </ReactFlow>
    </div>
  );
}

export default HomeDiagram;
