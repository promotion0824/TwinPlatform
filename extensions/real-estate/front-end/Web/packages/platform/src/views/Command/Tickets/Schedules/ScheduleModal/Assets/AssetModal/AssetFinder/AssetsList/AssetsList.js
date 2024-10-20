import { useQuery } from 'react-query'
import _ from 'lodash'
import { styled } from 'twin.macro'
import {
  AssetsList as BaseAssetsList,
  Fieldset,
  Flex,
  useForm,
  Checkbox,
} from '@willow/ui'
import fetchAssets from '@willow/ui/utils/fetchAssets'
import { useTranslation } from 'react-i18next'
import AssetButton from './AssetButton'

export default function AssetsList() {
  const form = useForm()
  const { t } = useTranslation()
  const params = {
    siteId: form.data.siteId,
    floorCode: form.data.floorCode !== 'BLDG' ? form.data.floorCode : undefined,
    searchKeyword: form.data.search,
    categoryId: form.data.category?.id,
    subCategories: true,
  }

  /**
   * Fetch 'totalAssets' depending on param values,
   * particularly if categoryId && floorCode && searchKeyword is non-empty.
   */
  const { data: totalAssets = [] } = useQuery(
    ['assetFindQuery', params],
    () => {
      if (
        params.categoryId == null &&
        params.floorCode == null &&
        params.searchKeyword == null
      ) {
        return []
      }

      return fetchAssets({ ...params })
    }
  )

  const handleCheckboxChange = (value) => {
    // if checkbox selection is true, selected assets are a combination of existing assets and total assets of floor.
    // if checkbox selection is false, selected assets should only consist of another floor's asset values.
    form.setData((prevData) => ({
      ...prevData,
      assets: value
        ? _.unionBy(prevData.assets, totalAssets, 'name')
        : _.differenceBy(prevData.assets, totalAssets, 'name'),
    }))
  }

  return (
    <Fieldset legend={t('plainText.assetsFound')}>
      <Flex size="small">
        <StyledCheckBox
          value={
            !!(
              form.data.assets.length > 0 &&
              totalAssets.every((val) =>
                form.data.assets.find((item) => item.name === val.name)
              )
            )
          }
          onChange={handleCheckboxChange}
          opacity={totalAssets.length > 0 ? 1 : 0}
          readOnly={totalAssets.length === 0}
        >
          {_.startCase(t('plainText.addAll'))}
        </StyledCheckBox>
        <BaseAssetsList params={params} AssetComponent={AssetButton} />
      </Flex>
    </Fieldset>
  )
}

const StyledCheckBox = styled(Checkbox)(({ opacity }) => ({
  marginBottom: '16px',
  opacity,
  cursor: opacity === 1 ? 'cursor' : 'default',
  transition: 'opacity 0.6s linear',

  '& > div': {
    justifyContent: 'left',
  },
}))
