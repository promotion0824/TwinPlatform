/* eslint-disable complexity */
import { useState, useEffect } from 'react'

const useNumericTaskFormValidation = (desc, unit, decimal) => {
  const [showAllRequiredErrors, setShowAllRequiredErrors] = useState(false)
  const [unitRequired, setUnitRequired] = useState(false)
  const [decimalRequired, setDecimalRequired] = useState(false)

  useEffect(() => {
    setUnitRequired(!unit)

    if (
      decimal === null ||
      decimal === undefined ||
      decimal === '' ||
      decimal < 0
    ) {
      setDecimalRequired(true)
    } else {
      setDecimalRequired(false)
    }
  }, [unit, decimal])

  return {
    unitRequired,
    decimalRequired,
    showAllRequiredErrors,
    setShowAllRequiredErrors,
    showUnitError: unitRequired && desc && showAllRequiredErrors,
    showDecimalError: decimalRequired && desc && showAllRequiredErrors,
  }
}

export default useNumericTaskFormValidation
