import { t } from 'i18next'

function getAllErrors(err, locale) {
  if (err?.status === 422) {
    if (Array.isArray(err.data)) {
      return err.data
    }

    if (Array.isArray(err.data?.items)) {
      let errors = err.data.items
      if (err.data.message != null) {
        errors = [{ message: err.data.message }, ...errors]
      }

      return errors
    }

    if (Array.isArray(err.errors)) {
      return err.errors
    }
  }

  return [
    {
      message:
        err?.data?.message ||
        t('plainText.errorOccurred', {
          lng: locale,
          defaultValue: err?.data?.message || 'An error has occurred',
        }),
    },
  ]
}

export default function getErrors(err, locale = 'en') {
  const allErrors = getAllErrors(err, locale)

  return {
    allErrors,
    snackbarErrors: allErrors.filter((error) => error.name == null),
  }
}
