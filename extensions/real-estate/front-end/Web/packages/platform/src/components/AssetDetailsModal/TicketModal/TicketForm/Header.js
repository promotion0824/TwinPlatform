import { useForm, Flex, ModalHeader, Text, Time } from '@willow/ui'

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
            Ticket Id:
          </Text>
          <Text size="extraTiny" color="grey">
            {form.data.sequenceNumber}
          </Text>
        </Flex>
        <Flex horizontal size="small">
          <Text size="extraTiny" color="grey">
            Created:
          </Text>
          <Text size="extraTiny" color="grey">
            <Time value={form.data.createdDate} />
          </Text>
        </Flex>
      </Flex>
    </ModalHeader>
  )
}
