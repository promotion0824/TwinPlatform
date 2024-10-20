import _ from 'lodash'

/**
 * Display "Ungrouped" in the ruleID column if its value is empty
 * Else return Cell Value.
 */
const renderInsightsGroupedCell = (cell, t) => {
  if (cell.column.id === 'ruleID') {
    if (cell.value === '') {
      return _.capitalize(t('plainText.ungrouped'))
    }
  }

  return cell.render('Cell')
}

export default renderInsightsGroupedCell
