import { titleCase } from '@willow/common'
import { EmptyState, Link, Stack } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import { Link as ReactRouterLink } from 'react-router-dom'

const NoPermissionState = ({ homeUrl }: { homeUrl: string }) => {
  const {
    t,
    i18n: { language },
  } = useTranslation()

  return (
    <Stack h="100%" align="center" justify="center">
      <EmptyState
        illustration="no-permissions"
        title={titleCase({ text: t('headers.noPermission'), language })}
        description={`${t('plainText.noPermission')}.`}
        primaryActions={
          <Link to={homeUrl} component={ReactRouterLink}>
            {t('interpolation.goTo', { value: t('headers.home') })}
          </Link>
        }
      />
    </Stack>
  )
}

export default NoPermissionState
