/* eslint-disable arrow-body-style */
import * as React from 'react'
import { useEffect, useRef } from 'react'
import { styled } from 'twin.macro'
import ReactFlow, {
  Background,
  Node,
  ReactFlowInstance,
  ReactFlowProvider,
  useReactFlow,
} from 'react-flow-renderer'
import { ModelOfInterest } from '@willow/common/twins/view/modelsOfInterest'
import { Ontology } from '@willow/common/twins/view/models'
import Edge from './ui/Edge'
import TwinChipNode from './ui/TwinChipNode'
import ModelChipNode from './ui/ModelChipNode'
import { LaidOutGraph } from './funcs/layoutGraph'
import TwinOverlay from './ui/TwinOverlay'
import {
  ExpandableDirection,
  GraphState,
  LayoutDirection,
  RelationshipDirection,
  RelationshipGroup,
} from './types'

const centerGraphOnNode = (
  node: Node,
  reactFlowInstance: ReactFlowInstance
) => {
  reactFlowInstance.setCenter(
    node.position.x + (node.width ?? 0) / 2,
    node.position.y + (node.height ?? 0) / 2,
    { zoom: 1 }
  )
}

interface Props {
  graph?: LaidOutGraph
  direction: LayoutDirection
  graphState: GraphState
  ontology: Ontology
  modelsOfInterest: ModelOfInterest[]
  selectedTwinId: string
  isTwinOverlayVisible: boolean
  onTwinClick: (twin: { id: string }) => void
  onToggleNodeExpansionClick: (
    twin: { id: string },
    expandDirection: ExpandableDirection
  ) => void
  onModelClick: (group: RelationshipGroup) => void
  onGoToTwinClick: (twinId: string) => void
  onCloseTwinOverlay: () => void
}

const Graph: React.FC<Props> = ({
  graph,
  direction,
  graphState,
  ontology,
  modelsOfInterest,
  selectedTwinId,
  isTwinOverlayVisible,
  onToggleNodeExpansionClick,
  onTwinClick,
  onModelClick,
  onGoToTwinClick,
  onCloseTwinOverlay,
}: Props) => {
  const nodeTypes = useRef({
    twin: ({ data, id }) => {
      return (
        <TwinChipNode
          data={data}
          ontology={ontology}
          onTwinClick={() => onTwinClick({ id })}
          onToggleOutClick={() =>
            onToggleNodeExpansionClick({ id }, RelationshipDirection.out)
          }
          onToggleInClick={() =>
            onToggleNodeExpansionClick({ id }, RelationshipDirection.in)
          }
          modelsOfInterest={modelsOfInterest}
        />
      )
    },
    model: ({
      data,
    }: {
      data: {
        twinId: string
        direction: RelationshipDirection
        relationshipName: string
        modelId: string
        count: number
      }
    }) => {
      return (
        <ModelChipNode
          modelId={data.modelId}
          count={data.count}
          onClick={() => {
            onModelClick({
              twinId: data.twinId,
              direction: data.direction,
              relationshipName: data.relationshipName,
              modelId: data.modelId,
            })
          }}
          ontology={ontology}
          modelsOfInterest={modelsOfInterest}
        />
      )
    },
  }).current
  const edgeTypes = useRef({ edge: Edge }).current
  // Only center a selected twin once
  const hasCenteredSelectedTwinRef = useRef(false)
  useEffect(() => {
    // When the selected twin changes, reset this
    hasCenteredSelectedTwinRef.current = false
  }, [selectedTwinId])

  const reactFlowInstance = useReactFlow()
  const updateCenter = () => {
    if (!hasCenteredSelectedTwinRef.current) {
      const selectedNode = reactFlowInstance
        .getNodes()
        .find((node) => node.data.id === selectedTwinId)

      // if we can find the selected node, and it has been processed (i.e. has a width)
      if (selectedNode && selectedNode.width) {
        centerGraphOnNode(selectedNode, reactFlowInstance)
        hasCenteredSelectedTwinRef.current = true
      }
    }
  }
  useEffect(updateCenter, [reactFlowInstance, selectedTwinId, graph])

  const selectedTwin =
    graph != null && isTwinOverlayVisible && selectedTwinId != null
      ? graph.nodes.find((n) => n.id === selectedTwinId)
      : null

  return (
    <GraphContainer>
      {graph != null && (
        <>
          <ReactFlow
            nodeTypes={nodeTypes}
            edgeTypes={edgeTypes}
            nodes={graph.nodes}
            edges={graph.edges}
            onInit={updateCenter}
            // This is all we need to do to hide the "React Flow" watermark in
            // the corner of the graph.
            proOptions={{
              account: 'paid-pro',
              hideAttribution: true,
            }}
          >
            <Background />
          </ReactFlow>
          {selectedTwin != null && (
            <TwinOverlay
              twin={selectedTwin}
              state={graphState.nodes[selectedTwinId]}
              modelsOfInterest={modelsOfInterest}
              ontology={ontology}
              direction={direction}
              onToggleInClick={() =>
                onToggleNodeExpansionClick(
                  { id: selectedTwinId },
                  RelationshipDirection.in
                )
              }
              onToggleOutClick={() =>
                onToggleNodeExpansionClick(
                  { id: selectedTwinId },
                  RelationshipDirection.out
                )
              }
              onToggleBothClick={() =>
                onToggleNodeExpansionClick({ id: selectedTwinId }, 'both')
              }
              onGoToTwinClick={() => onGoToTwinClick(selectedTwinId)}
              onClose={onCloseTwinOverlay}
            />
          )}
        </>
      )}
    </GraphContainer>
  )
}

const RelationsMap: React.FC<Props> = (props: Props) => {
  return (
    <ReactFlowProvider>
      <Graph {...props} />
    </ReactFlowProvider>
  )
}

const GraphContainer = styled.div({
  width: '100%',
  height: '100%',
  position: 'relative',
})

export default RelationsMap
