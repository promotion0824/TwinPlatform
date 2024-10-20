import { Flex, Input } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import AssetsList from './AssetsList/AssetsList'
import FloorSelect from './FloorSelect/FloorSelect'
import CategorySelect from './CategorySelect/CategorySelect'
import styles from './AssetFinder.css'

export default function AssetFinder() {
  const { t } = useTranslation()
  return (
    <Flex fill="content">
      <Flex size="large" padding="extraLarge" className={styles.header}>
        <Input name="search" label={t('labels.search')} icon="search" />
        <FloorSelect />
        <CategorySelect />
      </Flex>
      <AssetsList />
    </Flex>
  )
}
