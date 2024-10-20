import { titleCase } from '@willow/common'
import { DocumentTitle } from '@willow/ui'
import { Button } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import styled, { css } from 'styled-components'
import SplitHeaderPanel from '../Layout/Layout/SplitHeaderPanel'
import AdminTabs from './AdminTabs'

/**
 * This is an internal use only page for demo purpose, hence no need to translate
 * reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/96817
 */
export default function Sandbox() {
  const {
    t,
    i18n: { language },
  } = useTranslation()

  return (
    <>
      <DocumentTitle
        scopes={[
          titleCase({ text: t('headers.sandbox'), language }),
          t('headers.admin'),
        ]}
      />

      <SplitHeaderPanel leftElement={<AdminTabs />} />
      <div
        css={css(({ theme }) => ({
          display: 'flex',
          gap: theme.spacing.s4,
          padding: theme.spacing.s4,
        }))}
      >
        {[
          { href: sustainabilityUrl, text: 'Microsoft Sustainability Manager' },
          { href: dynamicsUrl, text: 'Dynamics 365' },
        ].map(({ href, text }) => (
          <Button key={text}>
            <UnstyledLink href={href} target="_blank">
              {text}
            </UnstyledLink>
          </Button>
        ))}
      </div>
    </>
  )
}

const UnstyledLink = styled.a`
  text-decoration: none;
  color: inherit;
  height: 100%;
  width: 100%;
`
const sustainabilityUrl =
  'https://org8b03b8ab.crm6.dynamics.com/main.aspx?appid=79db9eda-0c8f-ee11-be36-000d3a79b33c&pagetype=control&controlName=msdyn_MscrmControls.SUS.HomePageControl'
const dynamicsUrl =
  'https://org8b03b8ab.crm6.dynamics.com/main.aspx?appid=d97e615f-523a-ee11-bdf5-002248e41450&pagetype=dashboard&id=f0698b5f-5c5e-e611-810b-00155dbd6a1d&type=system&_canOverride=true'
