import { useState, useEffect } from 'react'
import { useTranslation } from 'react-i18next'
import { useForm, Input, Fieldset, Flex, Select, Option } from '@willow/ui'
import { useParams } from 'react-router'
import { useSites } from '../../../../providers/sites/SitesContext'
import SiteSelect from '../../SiteSelect'

function ReportDetails({ report, onChange, categoriesList }) {
  const { portfolioId } = useParams()
  const sites = useSites()
  const { t } = useTranslation()
  const { data, setData } = useForm()
  const [newReportName, setNewReportName] = useState(data.metadata.name)
  const [newCategory, setNewCategory] = useState(data.metadata.category)

  const handleReportNameChange = (name) => {
    setData((prevData) => {
      const { metadata } = prevData
      return {
        ...prevData,
        metadata: {
          ...metadata,
          name,
        },
      }
    })
    setNewReportName(name)
  }

  const handleCategoryChange = (category) => {
    setData((prevData) => {
      const { metadata } = prevData
      return {
        ...prevData,
        metadata: {
          ...metadata,
          category,
        },
      }
    })
    setNewCategory(category)
  }

  const handlePositionsChange = (newPositions) => {
    setData((prevState) => ({
      ...prevState,
      positions: newPositions,
    }))
  }

  useEffect(() => {
    onChange(data)
  }, [data, onChange, categoriesList, report])

  return (
    <Fieldset icon="details" legend={t('plainText.reportDetails')}>
      <Flex horizontal fill="header" size="medium">
        <Input
          name="name"
          label={t('plainText.reportName')}
          value={newReportName}
          onChange={handleReportNameChange}
        />
      </Flex>
      <Flex horizontal fill="equal" size="large">
        <SiteSelect
          t={t}
          portfolioId={portfolioId}
          sites={sites}
          selectedPositions={data.positions}
          onSelectedPositionsChange={handlePositionsChange}
        />
        <Select
          name="category"
          label={t('labels.category')}
          placeholder={t('plainText.unspecified')}
          cache
          value={newCategory}
          notFound={t('plainText.noCategoriesFound')}
          onChange={handleCategoryChange}
        >
          {categoriesList &&
            categoriesList.map((category, i) => (
              <Option key={i} value={category}>
                {category}
              </Option>
            ))}
        </Select>
      </Flex>
    </Fieldset>
  )
}

export default ReportDetails
