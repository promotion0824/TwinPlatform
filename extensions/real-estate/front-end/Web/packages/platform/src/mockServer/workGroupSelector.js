/* eslint-disable import/prefer-default-export */
import { rest } from 'msw'

const workgroups = [
  {
    id: '1',
    name: '1Main Street - Cleaning',
    siteId: '2bada6d2-ccd7-43dd-a42a-c8ab0873df64',
    numberIds: ['1'],
  },
  {
    id: '2',
    name: '1Main Street - Engineering',
    siteId: '2bada6d2-ccd7-43dd-a42a-c8ab0873df64',
    numberIds: ['2'],
  },
  {
    id: '3',
    name: 'ParkLane - Cleaning',
    siteId: 'e3ea6775-50e5-4d19-afec-b103d08658a3',
    numberIds: ['1', '3'],
  },
  {
    id: '4',
    name: 'ParkLane - Management',
    siteId: 'e3ea6775-50e5-4d19-afec-b103d08658a3',
    numberIds: ['1', '3', '7', '9'],
  },
]

export const handlers = [
  rest.get('/:region/api/management/workgroups/all', (req, res, ctx) =>
    res(ctx.json(workgroups))
  ),
]
