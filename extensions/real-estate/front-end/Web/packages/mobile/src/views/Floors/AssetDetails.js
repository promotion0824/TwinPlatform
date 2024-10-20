import { Tabs, Tab } from '@willow/mobile-ui'
import Details from './AssetSelector/Details'
import Files from './AssetSelector/Files'

export default function AssetDetails() {
  return (
    <Tabs color="normal">
      <Tab header="Details">
        <Details />
      </Tab>
      <Tab header="Files">
        <Files />
      </Tab>
    </Tabs>
  )
}
