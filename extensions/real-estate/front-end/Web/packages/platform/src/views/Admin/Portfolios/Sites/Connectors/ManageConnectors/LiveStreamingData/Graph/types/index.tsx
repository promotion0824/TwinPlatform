import { MutableRefObject } from 'react'

export type Timestamp = string

export type GraphContextType = {
  svgRef: MutableRefObject<SVGSVGElement | null>
  columns: Columns
  maxValue: number
  timestamps: Timestamp[]
  liveStreamingDataRef: MutableRefObject<HTMLDivElement | null>
}

export type Columns = Column[]
export type Column = {
  timestamp: string
  x: number
  y: number
  height: number
  width: number
  left: number
  value: number
}
export type GraphData = { timestamp: Timestamp; value: number }[]
