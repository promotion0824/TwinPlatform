import { useTicketStatuses } from '@willow/common'
import { isTicketStatusEquates, Status } from '@willow/common/ticketStatus'
import {
  useForm,
  Fieldset,
  Select,
  Option,
  Typeahead,
  TypeaheadButton,
} from '@willow/ui'
import routes from '../../../../routes'
import { useSites } from 'providers'
import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router'
import { styled } from 'twin.macro'
import AssetLink from '@willow/common/components/AssetLink'

const defaultFloorCodeAndIssue = {
  floorCode: '',
  issueId: null,
  issueType: null,
  issueName: null,
}

const defaultSiteSpecificData = {
  reporterId: null,
  reporterName: '',
  reporterPhone: '',
  reporterEmail: '',
  reporterCompany: '',
  categoryId: null,
  assignee: null,
}

/**
 * Asset selector to associate asset to a ticket. This includes:
 * - Site selector of sites with ticketing enabled, and
 * - Floor selector (List of floor based on site), and
 * - Asset selector (List of assets with possible ticket issues, based on selected site and floor).
 *
 * The site selector is only editable under the following conditions:
 * - On "All sites" view, and
 * - Ticket status is not "closed", and
 * - Ticket is not created by an external source, and
 * - Ticket is not linked to any site related data (such as insight and comment).
 */
export default function Asset() {
  const form = useForm()
  const sites = useSites()
  const { t } = useTranslation()
  const params = useParams()
  const ticketStatuses = useTicketStatuses()
  const ticketStatus = ticketStatuses.getByStatusCode(form.data.statusCode)

  return (
    <Fieldset icon="assets" legend={t('plainText.asset')}>
      <Select
        name="siteId"
        label={t('labels.site')}
        required
        readOnly={
          (params.siteId && params.siteId === form.data.siteId) ||
          (ticketStatus &&
            isTicketStatusEquates(ticketStatus, Status.closed)) ||
          form.data.externalId != null ||
          form.data.insightId != null ||
          form.data.comments?.length > 0
        }
        onChange={(siteId) => {
          form.setData((prevData) => ({
            ...prevData,
            ...defaultFloorCodeAndIssue,
            ...defaultSiteSpecificData,
            serviceNeededId: null,
            siteId,
          }))
        }}
      >
        {sites
          .filter((site) => !site.features.isTicketingDisabled)
          .map((site) => (
            <Option key={site.id} value={site.id} iconHidden>
              {site.name}
            </Option>
          ))}
      </Select>
      <Select
        name="floorCode"
        data-cy="asset-ticket-floorCode"
        label={t('labels.floor')}
        url={`/api/sites/${form.data.siteId}/floors`}
        cache
        notFound={t('plainText.noFloorsFound')}
        disabled={!form.data.siteId}
        onChange={(floorCode) => {
          form.setData((prevData) => ({
            ...prevData,
            ...defaultFloorCodeAndIssue,
            floorCode,
          }))
        }}
      >
        {(floors) =>
          floors.map((floor) => (
            <Option key={floor.id} value={floor.code}>
              {floor.code}
            </Option>
          ))
        }
      </Select>
      {/*
        display asset name as a clickable link which will redirect user to
        twin explorer page focusing on that asset when siteId and twinId
        of the ticket are both defined.
        reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/76887
      */}
      {form.data.siteId != null && form.data?.twinId != null ? (
        <>
          <StyledAssetName>{t('labels.assetName')}</StyledAssetName>
          <StyledLink
            path={routes.portfolio_twins_view__siteId__twinId(
              form.data.siteId,
              form.data.twinId
            )}
            siteId={form.data.siteId}
            twinId={form.data.twinId}
            assetName={form.data.issueName}
          />
        </>
      ) : (
        <Typeahead
          name="issueName"
          errorName="issueId"
          label={t('plainText.asset')}
          selected={form.data.issueId != null}
          disabled={!form.data.floorCode}
          url={(search) =>
            search.length > 0
              ? `/api/sites/${form.data.siteId}/possibleTicketIssues`
              : undefined
          }
          params={(search) => ({
            floorCode: form.data.floorCode,
            keyword: search,
          })}
          notFound={t('plainText.noAssetsFound')}
          onChange={(issueName) => {
            form.setData((prevData) => ({
              ...prevData,
              issueId: null,
              issueType: null,
              issueName,
            }))
          }}
          onBlur={() => {
            if (form.data.issueId == null) {
              form.setData((prevData) => ({
                ...prevData,
                issueId: null,
                issueType: null,
                issueName: '',
              }))
            }
          }}
          onSelect={(asset) => {
            form.setData((prevData) => ({
              ...prevData,
              issueId: asset.id,
              issueType: asset.type,
              issueName: asset.name,
            }))
          }}
        >
          {(assets) => (
            <>
              {assets?.map((asset) => (
                <TypeaheadButton key={asset.id} value={asset}>
                  {asset.name}
                </TypeaheadButton>
              ))}
            </>
          )}
        </Typeahead>
      )}
    </Fieldset>
  )
}

const StyledAssetName = styled.div({
  fontSize: '11px',
  paddingBottom: '8px',
})

const StyledLink = styled(AssetLink)({
  marginTop: '0px !important',
  paddingLeft: '9px',
  backgroundColor: '#1C1C1C',
  textAlign: 'start',
  height: '30px',
  display: 'flex',
  flexDirection: 'column',
  justifyContent: 'center',
})
