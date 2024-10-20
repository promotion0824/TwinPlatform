import { Modal, Flex } from '@willow/mobile-ui'
import CountrySelect from '../../../../Account/CountrySelect/CountrySelect'

export default function ChangeRegionModal({ onClose }) {
  return (
    <Modal header="Change region" onClose={onClose}>
      <Flex padding="large" size="extraLarge">
        Select a new country to change your current region
        <br />
        <CountrySelect />
      </Flex>
    </Modal>
  )
}
