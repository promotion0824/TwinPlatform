import { Insight } from '@willow/common/insights/insights/types'
import { Site } from '@willow/common/site/site/types'

export type MapViewItem = MapViewSite | MapViewTwin
/**
 * for the purpose of this POC, we want to determine whether
 * a specific date falls in between a date range, so it is easier
 * to achieve the goal by converting string date value such as
 * "2023-03-03T16:09:38.775Z" to a number value such as 1646872178775
 * and do the comparison
 */
export type MapViewInsight = Omit<Insight, 'occurredDate'> & {
  occurredDate: number
  occurrences?: Array<{
    id: string
    insightId: string
    isValid: boolean
    isFaulted: boolean
    started: string
    ended: string
    text: string
  }>
}

/**
 * types to be used only for DFW MapView POC,
 * we only care specific subset of properties for each type
 * so we use Partial to make all other properties optional
 */
export type MapViewSite = Partial<Omit<Site, 'id' | 'name' | 'address'>> & {
  id: string
  name: string
  address: string
} & { insights?: MapViewInsight[]; location?: [number, number] }

export type MapViewTwin = {
  id: string
  name: string
  siteId: string
} & { insights?: MapViewInsight[]; location?: [number, number] }

export const isMapViewTwin = (item: MapViewItem): item is MapViewTwin =>
  'siteId' in item

export enum MapViewTwinType {
  Buildings = 'Buildings',
  PassengerBoardingBridges = 'Passenger Boarding Bridges',
}

export enum MapViewPlaneStatus {
  Docked = 'Docked',
  Undocked = 'Not Docked',
  Faulted = 'Docked with Fault',
  Hidden = 'Hidden',
}

export const passengerBoardingBridgesModelId =
  'dtmi:com:willowinc:airport:PassengerBoardingBridge;1'

/**
 * Dallas Fort Worth Airport (DFW) is in Central Standard Time timezone
 * and that timezone is called "America/Chicago"
 * we will worry about dynamic timezone later
 */
export const dfwTimeZone = 'America/Chicago'

/**
 * the rule id for the rule triggered when
 * insight is created for a plane docked at a gate
 */
export const dockedRuleId = 'plane-docked-at-gate-'
/**
 * the rule id for the rule triggered when
 * insight is created for a plane docked at a gate
 * but not connect to group power unit
 */
export const dockedButNotConnectedRuleId =
  'plane-not-connected-to-ground-power-unit-'
