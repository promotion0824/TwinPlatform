/* eslint-disable import/prefer-default-export */
import { rest } from 'msw'
import _ from 'lodash'
import { statusMap } from '../services/Insight/InsightsService'

const eightDaysAgo = new Date(
  Date.now() - 8 * 24 * 60 * 60 * 1000
).toISOString()
const thirtyTwoDaysAgo = new Date(
  Date.now() - 32 * 24 * 60 * 60 * 1000
).toISOString()

export const makeActiveInsightTypes = (
  firstDate = eightDaysAgo,
  secondDate = thirtyTwoDaysAgo
) => ({
  cards: {
    before: 0,
    after: 0,
    total: 18,
    items: [
      {
        priority: 2,
        insightCount: 10,
        lastOccurredDate: firstDate,
        impactScores: [],
        recommendation: 'Walmart Alert Test',
        ruleId: 'walmart_alert',
        ruleName: 'Walmart Alert',
        insightType: 'alert',
        id: 'test-id',
      },
      {
        priority: 2,
        sourceName: '',
        insightCount: 45,
        lastOccurredDate: firstDate,
        impactScores: [],
        recommendation:
          "1. Inspect Dampers: Thoroughly examine and adjust dampers to ensure they are functioning correctly and regulating outside airflow as intended.\n 2. Review Control Settings: Verify the AHU's control system settings and configuration. Adjust the control parameters to align with the desired outside airflow setpoint.\n 3. Mechanical Assessment: Inspect fans, motors, and related mechanical components. Repair or replace any malfunctioning parts affecting outside airflow.\n 4. Sensor Validation: Test and validate the accuracy of sensors related to outside air temperature and airflow. Calibrate or replace sensors if they are found to be inaccurate.",
      },
      {
        id: 'ahu-chw-leaking-2-nrel',
        ruleId: 'ahu-chw-leaking-2-nrel',
        ruleName:
          'NREL: AHU Chilled Water Valve Leaking 2 NREL: AHU Chilled Water Valve Leaking 2 NREL: AHU Chilled Water Valve Leaking 2 NREL: AHU Chilled Water Valve Leaking 2 NREL: AHU Chilled Water Valve Leaking 2',
        insightType: 'energy',
        priority: 2,
        sourceId: '7caed3b8-c0b6-4f91-ad3e-29d68882efeb',
        sourceName: 'Willow Activate',
        insightCount: 36,
        lastOccurredDate: firstDate,
        impactScores: [
          {
            fieldId: 'reliability_impact',
            name: 'Reliability impact',
            value: 23,
            unit: '%',
          },
        ],
        recommendation:
          '1. Demand Analysis: Assess the cooling and heating demand. Adjust the fan speed control strategy to ensure it meets the demand without exceeding design specifications.\n 2. Duct and Filter Inspection: Inspect the ducts and filters for blockages or obstructions. Clear any obstacles to ensure proper airflow and reduce unnecessary fan strain.\n 3. Damper Alignment: Verify that dampers are correctly positioned to achieve balanced airflow. Proper airflow distribution can prevent overloading the fan.',
      },
      {
        id: 'ahu-chw-overriden-open-nrel',
        ruleId: 'ahu-chw-overriden-open-nrel',
        ruleName: 'NREL: AHU Chilled Water Valve Overriden Open (Not Auto)',
        insightType: 'energy',
        priority: 2,
        sourceId: '7caed3b8-c0b6-4f91-ad3e-29d68882efeb',
        sourceName: 'Willow Activate',
        insightCount: 27,
        lastOccurredDate: firstDate,
        impactScores: [
          {
            fieldId: 'reliability_impact',
            name: 'Reliability impact',
            value: 25,
            unit: '%',
          },
        ],
        recommendation:
          '1. Demand Analysis: Assess the cooling and heating demand. Adjust the fan speed control strategy to ensure it meets the demand without exceeding design specifications.\n 2. Duct and Filter Inspection: Inspect the ducts and filters for blockages or obstructions. Clear any obstacles to ensure proper airflow and reduce unnecessary fan strain.\n 3. Damper Alignment: Verify that dampers are correctly positioned to achieve balanced airflow. Proper airflow distribution can prevent overloading the fan.',
      },
      {
        id: 'ahu-chw-stuck-closed-2-nrel',
        ruleId: 'ahu-chw-stuck-closed-2-nrel',
        ruleName: 'NREL: AHU Chilled Water Valve Stuck Closed 2',
        insightType: 'energy',
        priority: 2,
        sourceId: '7caed3b8-c0b6-4f91-ad3e-29d68882efeb',
        sourceName: 'Willow Activate',
        insightCount: 36,
        lastOccurredDate: firstDate,
        impactScores: [
          {
            fieldId: 'reliability_impact',
            name: 'Reliability impact',
            value: 34,
            unit: '%',
          },
        ],
        recommendation:
          "1. Inspect Dampers: Thoroughly examine and adjust dampers to ensure they are functioning correctly and regulating outside airflow as intended.\n 2. Review Control Settings: Verify the AHU's control system settings and configuration. Adjust the control parameters to align with the desired outside airflow setpoint.\n 3. Mechanical Assessment: Inspect fans, motors, and related mechanical components. Repair or replace any malfunctioning parts affecting outside airflow.\n 4. Sensor Validation: Test and validate the accuracy of sensors related to outside air temperature and airflow. Calibrate or replace sensors if they are found to be inaccurate.",
      },
      {
        id: 'ahu-discharge-fan-mismatch-nrel',
        ruleId: 'ahu-discharge-fan-mismatch-nrel',
        ruleName: 'NREL: AHU Supply Fan Run/Cmd Mismatch',
        insightType: 'energy',
        priority: 2,
        sourceId: '7caed3b8-c0b6-4f91-ad3e-29d68882efeb',
        sourceName: 'Willow Activate',
        insightCount: 37,
        lastOccurredDate: firstDate,
        impactScores: [
          {
            fieldId: 'reliability_impact',
            name: 'Reliability impact',
            value: 31,
            unit: '%',
          },
        ],
        recommendation: '',
      },
      {
        id: 'ahu-hw-leaking-2-nrel',
        ruleId: 'ahu-hw-leaking-2-nrel',
        ruleName: 'NREL: AHU Hot Water Valve Leaking 2',
        insightType: 'energy',
        priority: 2,
        sourceId: '7caed3b8-c0b6-4f91-ad3e-29d68882efeb',
        sourceName: 'Willow Activate',
        insightCount: 7,
        lastOccurredDate: firstDate,
        impactScores: [
          {
            fieldId: 'reliability_impact',
            name: 'Reliability impact',
            value: 7,
            unit: '%',
          },
        ],
        recommendation: '',
      },
      {
        id: 'ahu-hw-stuck-closed-2-nrel',
        ruleId: 'ahu-hw-stuck-closed-2-nrel',
        ruleName: 'NREL: AHU Hot Water Valve Stuck Closed 2',
        insightType: 'energy',
        priority: 2,
        sourceId: '7caed3b8-c0b6-4f91-ad3e-29d68882efeb',
        sourceName: 'Willow Activate',
        insightCount: 7,
        lastOccurredDate: firstDate,
        impactScores: [
          {
            fieldId: 'reliability_impact',
            name: 'Reliability impact',
            value: 6,
            unit: '%',
          },
        ],
        recommendation: '',
      },
      {
        id: 'ahu-on-unoccupied-nrel',
        ruleId: 'ahu-on-unoccupied-nrel',
        ruleName: 'NREL: AHU Off Hours Operation',
        insightType: 'energy',
        priority: 2,
        sourceId: '7caed3b8-c0b6-4f91-ad3e-29d68882efeb',
        sourceName: 'Willow Activate',
        insightCount: 5,
        lastOccurredDate: firstDate,
        impactScores: [
          {
            fieldId: 'reliability_impact',
            name: 'Reliability impact',
            value: 5,
            unit: '%',
          },
        ],
        recommendation: '',
      },
      {
        id: 'ahu-overcooling-supply-nrel',
        ruleId: 'ahu-overcooling-supply-nrel',
        ruleName: 'NREL: AHU Supply Overcooling',
        insightType: 'energy',
        priority: 2,
        sourceId: '7caed3b8-c0b6-4f91-ad3e-29d68882efeb',
        sourceName: 'Willow Activate',
        insightCount: 27,
        lastOccurredDate: firstDate,
        impactScores: [
          {
            fieldId: 'reliability_impact',
            name: 'Reliability impact',
            value: 25,
            unit: '%',
          },
        ],
        recommendation:
          "1. Check if the cooling command has been manually overriden.\n2. Recalibrate or replace the zone temperature sensor.\n3. Confirm that the controller's temperature setpoint deadband isn't too large such that the unit continues to cool past setpoint.",
      },
      {
        id: 'ahusc-chwv-over-design-operation',
        ruleId: 'ahusc-chwv-over-design-operation',
        ruleName: 'AHU Chilled Water Valve Over Design Operation',
        insightType: 'energy',
        priority: 2,
        sourceId: '7caed3b8-c0b6-4f91-ad3e-29d68882efeb',
        sourceName: 'Willow Activate',
        insightCount: 14,
        lastOccurredDate: firstDate,
        impactScores: [],
        recommendation: 'None',
        primaryModelId: 'dtmi:com:willowinc:AirHandlingUnit;1',
      },
      {
        id: 'ahu-simultaneous-heating-cooling-nrel',
        ruleId: 'ahu-simultaneous-heating-cooling-nrel',
        ruleName: 'NREL: AHU Simultaneous Heating and Cooling',
        insightType: 'energy',
        priority: 2,
        sourceId: '7caed3b8-c0b6-4f91-ad3e-29d68882efeb',
        sourceName: 'Willow Activate',
        insightCount: 10,
        lastOccurredDate: firstDate,
        impactScores: [
          {
            fieldId: 'reliability_impact',
            name: 'Reliability impact',
            value: 10,
            unit: '%',
          },
        ],
        recommendation: '',
      },
      {
        id: 'chiller_variable_primary_efficiency',
        ruleId: 'chiller_variable_primary_efficiency',
        ruleName: 'Poor Chiller Efficiency',
        insightType: 'energy',
        priority: 3,
        sourceId: '7caed3b8-c0b6-4f91-ad3e-29d68882efeb',
        sourceName: 'Willow Activate',
        insightCount: 4,
        lastOccurredDate: secondDate,
        impactScores: [
          {
            fieldId: 'daily_avoidable_cost',
            name: 'Daily Avoidable Cost',
            value: 163.57,
            unit: '$',
          },
          {
            fieldId: 'daily_avoidable_energy',
            name: 'Daily Avoidable Energy',
            value: 3271.41,
            unit: 'kWh',
          },
          {
            fieldId: 'priority_impact',
            name: 'Priority',
            value: 200,
            unit: '',
          },
          {
            fieldId: 'total_cost_to_date',
            name: 'Total Cost to Date',
            value: 112123.62,
            unit: '$',
          },
          {
            fieldId: 'total_energy_to_date',
            name: 'Total Energy to Date',
            value: 2242472.34,
            unit: 'kWh',
          },
        ],
        recommendation:
          "1. Chiller Maintenance: Schedule and perform regular maintenance, including cleaning coils and heat exchangers, to improve chiller efficiency.\n\n2. Refrigerant Assessment: Check refrigerant levels and address any leaks or charge issues promptly.\n\n3. Control System Review: Evaluate the control system's programming and logic for chiller operation. Correct any errors or misconfigurations affecting efficiency.\n\n4. Chiller Replacement: Consider upgrading to a more energy-efficient chiller model if the current unit is old or inefficient.",
        primaryModelId: 'dtmi:com:willowinc:Chiller;1',
      },
      {
        id: 'chiller-chilled-water-deltat-low',
        ruleId: 'chiller-chilled-water-deltat-low',
        ruleName: 'Chilled Water Delta T Low',
        insightType: 'note',
        priority: 3,
        sourceId: '7caed3b8-c0b6-4f91-ad3e-29d68882efeb',
        sourceName: 'Willow Activate',
        insightCount: 5,
        lastOccurredDate: secondDate,
        impactScores: [
          {
            fieldId: 'daily_avoidable_cost',
            name: 'Daily Avoidable Cost',
            value: 0,
            unit: '',
          },
          {
            fieldId: 'daily_avoidable_energy',
            name: 'Daily Avoidable Energy',
            value: 0,
            unit: 'kWh',
          },
          {
            fieldId: 'priority_impact',
            name: 'Priority',
            value: 250,
            unit: '',
          },
          {
            fieldId: 'total_cost_to_date',
            name: 'Total Cost to Date',
            value: 0,
            unit: '',
          },
          {
            fieldId: 'total_energy_to_date',
            name: 'Total Energy to Date',
            value: 0,
            unit: 'kWh',
          },
        ],
        recommendation:
          '1. Flow Rate Analysis: Monitor and adjust the water flow rate through the chiller and cooling coils to ensure sufficient heat transfer.\n\n2. Cleaning and Maintenance: Regularly clean and inspect heat exchange surfaces to prevent scaling and fouling that hinder heat transfer.\n\n3. Temperature Sensor Validation: Test and calibrate temperature sensors to ensure accurate readings for delta T calculations.\n\n4. Chiller Performance Check: Assess chiller components and refrigerant levels for any issues affecting temperature differences.',
        primaryModelId: 'dtmi:com:willowinc:Chiller;1',
      },
      {
        id: 'chwv-closed-during-cooling-mode',
        ruleId: 'chwv-closed-during-cooling-mode',
        ruleName: 'CHWV Closed During Cooling Mode',
        insightType: 'energy',
        priority: 4,
        sourceId: '7caed3b8-c0b6-4f91-ad3e-29d68882efeb',
        sourceName: 'Willow Activate',
        insightCount: 10,
        lastOccurredDate: secondDate,
        impactScores: [],
        recommendation: '',
        primaryModelId: 'dtmi:com:willowinc:AirHandlingUnit;1',
      },
      {
        id: 'dat-above-setpoint-heating-element-enabled',
        ruleId: 'dat-above-setpoint-heating-element-enabled',
        ruleName: 'DAT Above Setpoint Heating Element Enabled',
        insightType: 'energy',
        priority: 4,
        sourceId: '7caed3b8-c0b6-4f91-ad3e-29d68882efeb',
        sourceName: 'Willow Activate',
        insightCount: 1,
        lastOccurredDate: secondDate,
        impactScores: [],
        recommendation: 'Check outside air damper or disable heating element',
        primaryModelId: 'dtmi:com:willowinc:AirHandlingUnit;1',
      },
      {
        id: 'plane-docked-at-gate-',
        ruleId: 'plane-docked-at-gate-',
        ruleName: 'Plane Docked at Gate ',
        insightType: 'note',
        priority: 3,
        sourceId: '7caed3b8-c0b6-4f91-ad3e-29d68882efeb',
        sourceName: 'Willow Activate',
        insightCount: 164,
        lastOccurredDate: secondDate,
        impactScores: [
          {
            fieldId: 'comfort_impact',
            name: 'Comfort impact',
            value: 10845300,
            unit: '',
          },
          {
            fieldId: 'cost_impact',
            name: 'Cost impact',
            value: 10845300,
            unit: '',
          },
          {
            fieldId: 'reliability_impact',
            name: 'Reliability impact',
            value: 0,
            unit: '',
          },
        ],
        recommendation: 'TODO',
        primaryModelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
      },
      {
        id: 'plane-not-connected-to-ground-power-unit-',
        ruleId: 'plane-not-connected-to-ground-power-unit-',
        ruleName: 'Plane Not Connected to Ground Power Unit ',
        insightType: 'energy',
        priority: 3,
        sourceId: '7caed3b8-c0b6-4f91-ad3e-29d68882efeb',
        sourceName: 'Willow Activate',
        insightCount: 1,
        lastOccurredDate: secondDate,
        impactScores: [],
        recommendation: 'Alert airline to notify staff. ',
        primaryModelId: 'dtmi:com:willowinc:airport:PassengerBoardingBridge;1',
      },
    ],
  },
  filters: {
    insightTypes: ['', 'Energy', 'Note'],
    sourceNames: [
      '{"sourceName":""}',
      '{"sourceName":"Ruling Engine V3","sourceId":"7caed3b8-c0b6-4f91-ad3e-29d68882efeb"}',
    ],
  },
  impactScoreSummary: [
    {
      fieldId: 'reliability_impact',
      name: 'Reliability impact',
      value: 166,
      unit: '%',
    },
    {
      fieldId: 'daily_avoidable_cost',
      name: 'Daily Avoidable Cost',
      value: 163.57,
      unit: '$',
    },
    {
      fieldId: 'daily_avoidable_energy',
      name: 'Daily Avoidable Energy',
      value: 3271.41,
      unit: 'kWh',
    },
    {
      fieldId: 'priority_impact',
      name: 'Priority',
      value: 450,
      unit: '',
    },
    {
      fieldId: 'total_cost_to_date',
      name: 'Total Cost to Date',
      value: 112123.62,
      unit: '$',
    },
    {
      fieldId: 'total_energy_to_date',
      name: 'Total Energy to Date',
      value: 2242472.34,
      unit: 'kWh',
    },
    {
      fieldId: 'comfort_impact',
      name: 'Comfort impact',
      value: 10845300,
      unit: '',
    },
    {
      fieldId: 'cost_impact',
      name: 'Cost impact',
      value: 10845300,
      unit: '',
    },
  ],
})

const inactiveInsightTypes = {
  cards: {
    before: 0,
    after: 0,
    total: 4,
    items: [
      {
        priority: 2,
        sourceName: '',
        insightCount: 1,
        lastOccurredDate: '2023-02-10T22:28:40.006Z',
        impactScores: [
          {
            fieldId: 'comfort_impact',
            name: 'Comfort impact',
            value: 0,
            unit: '',
          },
          {
            fieldId: 'cost_impact',
            name: 'Cost impact',
            value: 0,
            unit: '',
          },
          {
            fieldId: 'reliability_impact',
            name: 'Reliability impact',
            value: 0,
            unit: '',
          },
        ],
        recommendation:
          '1. Check the zone air temperature sensor for a failure.\n2. Confirm sensor has communication.',
      },
      {
        id: 'meter-water-waste-detection-metric',
        ruleId: 'meter-water-waste-detection-metric',
        ruleName: 'Water Meter Waste Detection',
        insightType: 'energy',
        priority: 3,
        sourceId: '7caed3b8-c0b6-4f91-ad3e-29d68882efeb',
        sourceName: 'Willow Activate',
        insightCount: 1,
        lastOccurredDate: '2023-05-09T02:03:09.256Z',
        impactScores: [],
        recommendation: '',
        primaryModelId: 'dtmi:com:willowinc:WaterMeter;1',
      },
      {
        id: 'terminal-unit-on-unoccupied',
        ruleId: 'terminal-unit-on-unoccupied',
        ruleName: 'Terminal Unit On When Space Unoccupied',
        insightType: 'energy',
        priority: 4,
        sourceId: '7caed3b8-c0b6-4f91-ad3e-29d68882efeb',
        sourceName: 'Willow Activate',
        insightCount: 6,
        lastOccurredDate: '2023-10-27T09:30:00.000Z',
        impactScores: [
          {
            fieldId: 'daily_avoidable_cost',
            name: 'Daily Avoidable Cost',
            value: 0.26078848710945457,
            unit: 'USD',
          },
          {
            fieldId: 'daily_avoidable_energy',
            name: 'Daily Avoidable Energy',
            value: 1.4488249283858587,
            unit: 'kWh',
          },
          {
            fieldId: 'priority_impact',
            name: 'Priority',
            value: 240,
            unit: '',
          },
          {
            fieldId: 'total_cost_to_date',
            name: 'Total Cost to Date',
            value: 17.139605229046744,
            unit: 'USD',
          },
          {
            fieldId: 'total_energy_to_date',
            name: 'Total Energy to Date',
            value: 95.22002905025973,
            unit: 'kWh',
          },
        ],
        recommendation:
          'Dynamically adjust the occupancy mode based on real-time room occupancy',
        primaryModelId: 'dtmi:com:willowinc:OccupancyZone;1',
      },
      {
        id: 'vav-over-design-operation',
        ruleId: 'vav-over-design-operation',
        ruleName: 'VAV Over Design Operation',
        insightType: 'note',
        priority: 4,
        sourceId: '7caed3b8-c0b6-4f91-ad3e-29d68882efeb',
        sourceName: 'Willow Activate',
        insightCount: 1,
        lastOccurredDate: '2023-10-31T15:12:15.982Z',
        impactScores: [
          {
            fieldId: 'daily_avoidable_cost',
            name: 'Daily Avoidable Cost',
            value: 0,
            unit: 'kWh',
          },
          {
            fieldId: 'daily_avoidable_energy',
            name: 'Daily Avoidable Energy',
            value: 0,
            unit: 'kWh',
          },
          {
            fieldId: 'priority_impact',
            name: 'Priority',
            value: 25,
            unit: '',
          },
          {
            fieldId: 'total_cost_to_date',
            name: 'Total Cost to Date',
            value: 0,
            unit: 'kWh',
          },
          {
            fieldId: 'total_energy_to_date',
            name: 'Total Energy to Date',
            value: 0,
            unit: 'kWh',
          },
        ],
        recommendation:
          "1. Calibration and Damper Adjustment: Ensure that the VAV system's dampers are accurately calibrated and adjusted to respond to changing temperature and occupancy conditions.\n\n2. Commissioning and Balancing: Conduct a thorough commissioning process to verify the VAV system's proper installation and functioning. Balancing the airflow distribution to individual zones will help ensure that each area receives the necessary amount of conditioned air without unnecessary overdesign. ",
        primaryModelId: 'dtmi:com:willowinc:VAVBox;1',
      },
    ],
  },
  filters: {
    InsightTypes: ['', 'Energy', 'Note'],
    SourceNames: ['', 'Willow Activate'],
  },
  impactScoreSummary: [
    {
      fieldId: 'comfort_impact',
      name: 'Comfort impact',
      value: 0,
      unit: '',
    },
    {
      fieldId: 'cost_impact',
      name: 'Cost impact',
      value: 0,
      unit: '',
    },
    {
      fieldId: 'reliability_impact',
      name: 'Reliability impact',
      value: 0,
      unit: '',
    },
    {
      fieldId: 'daily_avoidable_cost',
      name: 'Daily Avoidable Cost',
      value: 0.26078848710945457,
      unit: 'USD',
    },
    {
      fieldId: 'daily_avoidable_energy',
      name: 'Daily Avoidable Energy',
      value: 1.4488249283858587,
      unit: 'kWh',
    },
    {
      fieldId: 'priority_impact',
      name: 'Priority',
      value: 265,
      unit: '',
    },
    {
      fieldId: 'total_cost_to_date',
      name: 'Total Cost to Date',
      value: 17.139605229046744,
      unit: 'USD',
    },
    {
      fieldId: 'total_energy_to_date',
      name: 'Total Energy to Date',
      value: 95.22002905025973,
      unit: 'kWh',
    },
  ],
}

export const handlers = [
  rest.post('/:region/api/insights/cards', (req, res, ctx) => {
    const statusSpec = req.body.filterSpecifications.filter(
      (spec) => spec.field === 'status'
    )
    if (_.isEqual(statusSpec[0].value, statusMap.inactive)) {
      return res(ctx.delay(0), ctx.json(inactiveInsightTypes))
    }

    return res(ctx.delay(0), ctx.json(makeActiveInsightTypes()))
  }),
]
