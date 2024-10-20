import { titleCase } from '@willow/common'
import { useScopeSelector } from '@willow/ui'
import { PieChart } from '@willowinc/ui'
import { camelCase } from 'lodash'
import { useTranslation } from 'react-i18next'
import { ChartTile } from '../../../../../components/LocationHome/ChartTile/ChartTile'
import useTopActiveTicketCategories from './useTopActiveTicketCategories'

const TopActiveTicketCategoriesChartTile = () => {
  const {
    i18n: { language },
    t,
  } = useTranslation()
  const { location: scope } = useScopeSelector()
  const twinId = scope?.twin?.id
  const { data, isLoading, isError } = useTopActiveTicketCategories({
    twinId: twinId ?? '',
    options: {
      enabled: !!twinId,
    },
  })

  const [labels = [], values = []] = [
    data?.map((obj) =>
      t(`ticketCategory.${camelCase(obj.categoryName)}`, {
        defaultValue: obj.categoryName,
      })
    ),
    data?.map((obj) => obj.count),
  ]

  const isEmpty = labels?.length === 0 || values?.length === 0

  return (
    <ChartTile
      empty={isEmpty}
      loading={isLoading}
      error={isError}
      defaultHeight={200}
      chart={
        <PieChart
          dataset={[
            {
              data: values,
              name: titleCase({
                language,
                text: t('headers.top5ActiveTicketCategories'),
              }),
            },
          ]}
          labels={labels}
          position="left"
          showLabels={false}
        />
      }
      title={titleCase({
        language,
        text: t('headers.top5ActiveTicketCategories'),
      })}
    />
  )
}

export default TopActiveTicketCategoriesChartTile
