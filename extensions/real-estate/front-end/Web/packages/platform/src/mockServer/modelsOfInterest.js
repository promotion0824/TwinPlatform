/* eslint-disable import/prefer-default-export */
import _ from 'lodash'
import { v4 as uuidv4 } from 'uuid'
import { rest } from 'msw'

export const defaultModelsOfInterest = [
  {
    id: uuidv4(),
    modelId: 'dtmi:com:willowinc:Asset;1',
    name: 'Asset',
    text: 'As',
    color: '#DD4FC1',
  },
  {
    id: uuidv4(),
    modelId: 'dtmi:com:willowinc:BuildingComponent;1',
    name: 'Building Component',
    text: 'Co',
    color: '#FD6C76',
  },
  {
    id: uuidv4(),
    modelId: 'dtmi:com:willowinc:Building;1',
    name: 'Building',
    text: 'Bu',
    color: '#D9D9D9',
  },
  {
    id: uuidv4(),
    modelId: 'dtmi:com:willowinc:Level;1',
    name: 'Level',
    text: 'Lv',
    color: '#E57936',
  },
  {
    id: uuidv4(),
    modelId: 'dtmi:com:willowinc:Room;1',
    name: 'Room',
    text: 'Rm',
    color: '#55FFD1',
  },
  {
    id: uuidv4(),
    modelId: 'dtmi:com:willowinc:System;1',
    name: 'System',
    text: 'Sy',
    color: '#78949F',
  },
  {
    id: uuidv4(),
    modelId: 'dtmi:com:willowinc:EquipmentGroup;1',
    name: 'Equipment Group',
    text: 'Gr',
    color: '#417CBF',
  },
  {
    id: uuidv4(),
    modelId: 'dtmi:com:willowinc:TenantUnit;1',
    name: 'Tenancy',
    text: 'Te',
    color: '#FFC11A',
  },
  {
    id: uuidv4(),
    modelId: 'dtmi:com:willowinc:Zone;1',
    name: 'Zone',
    text: 'Zn',
    color: '#33CA36',
  },
]

export function makeHandlers(
  initialModelsOfInterest = defaultModelsOfInterest
) {
  // The reorder endpoint will mutate this.
  const modelsOfInterest = _.cloneDeep(initialModelsOfInterest)

  const handlers = [
    rest.get(
      '/:region/api/customers/:customerId/modelsOfInterest',
      (req, res, ctx) => res(ctx.json(modelsOfInterest))
    ),

    rest.put(
      '/:region/api/customers/:customerId/modelsOfInterest/:id/reorder',
      (req, res, ctx) => {
        const currentModelIndex = _.findIndex(
          modelsOfInterest,
          (m) => m.id === req.params.id
        )
        const currentModel = modelsOfInterest[currentModelIndex]
        // Remove the model
        modelsOfInterest.splice(currentModelIndex, 1)
        // Add it at the requested index
        modelsOfInterest.splice(req.body.index, 0, currentModel)
        return res()
      }
    ),

    rest.post(
      '/:region/api/customers/:customerId/modelsOfInterest',
      (req, res, ctx) => res(ctx.json(req.body))
    ),

    rest.put(
      '/:region/api/customers/:customerId/modelsOfInterest/:id',
      (req, res, ctx) => res(ctx.json(req.body))
    ),

    rest.delete(
      '/:region/api/customers/:customerId/modelsOfInterest/:id',
      (req, res, ctx) => res(ctx.json(req.body))
    ),
  ]

  return {
    handlers,
    state: modelsOfInterest,
    reset: () => {
      modelsOfInterest.splice(
        0,
        modelsOfInterest.length,
        ..._.cloneDeep(initialModelsOfInterest)
      )
    },
  }
}
