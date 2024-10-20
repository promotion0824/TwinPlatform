import { Text } from '@willow/ui'

export default function CheckCell({ title, subTitle }) {
  return (
    <>
      <div>
        <Text color="white">{title}</Text>
      </div>
      {subTitle != null && (
        <div>
          <Text size="small">{subTitle}</Text>
        </div>
      )}
    </>
  )
}
