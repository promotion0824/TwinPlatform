import { Flex, TabsHeader } from '@willow/ui'
import EditorTypeButtons from '../EditorTypeButtons'

export default function UserTabsMenu() {
  return (
    <TabsHeader>
      <Flex align="right middle" padding="0 medium">
        <EditorTypeButtons />
      </Flex>
    </TabsHeader>
  )
}
