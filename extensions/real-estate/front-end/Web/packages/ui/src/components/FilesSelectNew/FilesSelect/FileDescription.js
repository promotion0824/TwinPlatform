import Flex from 'components/Flex/Flex'
import Number from 'components/Number/Number'
import TextNew from 'components/TextNew/Text'
import { useTranslation } from 'react-i18next'

export default function FileDescription({ file }) {
  const { t } = useTranslation()
  const name = (file.name ?? '').split('.').slice(0, -1).join('.')
  const extension = (file.name ?? '').split('.').slice(-1)[0]

  return (
    <Flex>
      <TextNew whiteSpace="nowrap">{name}</TextNew>
      <TextNew color="dark" whiteSpace="nowrap">
        <Flex horizontal align="middle" size="small">
          <TextNew textTransform="uppercase">{extension}</TextNew>
          {file.size != null && (
            <>
              <span>•</span>
              <span>
                <Number value={file.size / 1024} format=",.00" />
                {t('plainText.kb')}
              </span>
            </>
          )}
        </Flex>
      </TextNew>
    </Flex>
  )
}
