import { useForm, Flex, ModalHeader, Text } from '@willow/ui'

export default function Header() {
  const form = useForm()

  if (form.data.id == null) {
    return null
  }
  return (
    <ModalHeader>
      <Flex horizontal fill="header">
        <Flex horizontal size="small">
          <Text size="extraTiny" color="grey">
            Report Id:
          </Text>
          <Text size="extraTiny" color="grey">
            {form.data.id}
          </Text>
        </Flex>
      </Flex>
    </ModalHeader>
  )
}
