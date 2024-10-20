import { useDemoData } from '@mui/x-data-grid-generator'
import { act, render } from '../../../jest/testUtils'

import { DataGrid } from './DataGrid'

const Basic = () => {
  const { data } = useDemoData({
    dataSet: 'Commodity',
    rowLength: 6,
    maxColumns: 6,
  })

  return (
    <div style={{ height: 400, width: '100%' }}>
      <DataGrid {...data} />
    </div>
  )
}

describe('DataGrid', () => {
  it('should render successfully', async () => {
    await act(async () => {
      const { baseElement } = render(<Basic />)

      expect(baseElement).toBeTruthy()
    })
  })
})
