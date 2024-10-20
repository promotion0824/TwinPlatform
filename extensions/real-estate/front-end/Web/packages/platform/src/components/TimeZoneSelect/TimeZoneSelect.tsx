/* eslint-disable react/require-default-props */
import { useMemo, useState } from 'react'
import { styled } from 'twin.macro'
import { useTranslation } from 'react-i18next'
import { Select, Option, Progress, Input, Component } from '@willow/ui'
import _ from 'lodash'
import { Site } from '@willow/common/site/site/types'
import useGetTimezones, {
  TimeZoneInfo,
  useTimeZoneInfo,
} from './useGetTimeZones'
import { useSites } from '../../providers/sites/SitesContext'

/**
 * The timeZone option with unique value for the Select Dropdown.
 *
 * There are two types of timeZone option:
 * 1) The general TimeZoneInfo with value composed of timeZoneId, or
 * 2) The user-recommended TimeZoneInfo based on the site context. In
 * this case, both the value are composed of both the timeZoneId and siteId.
 */
export type TimeZoneOption = TimeZoneInfo & {
  value: { timeZoneId: string; siteId?: string }
}

const SearchInput = styled(Input)(({ theme }) => ({
  flexShrink: 0,
  top: 0,
  position: 'sticky',
  background: theme.color.neutral.bg.panel.default,
  zIndex: 'var(--z-header)',
  border: 0,
  borderBottom: `1px solid ${theme.color.neutral.border.default}`,
  borderRadius: 0,
}))

const StyledOption = styled(Option)<{
  value: TimeZoneOption['value']
  $isSearchMatch: boolean
}>(({ value, $isSearchMatch }) => ({
  background:
    value?.siteId != null
      ? 'var(--theme-color-neutral-bg-accent-default)'
      : undefined,
  '&:not([data-is-selected])': {
    display: !$isSearchMatch ? 'none' : undefined,
  },
}))

const isSearchMatch = (tzOption: TimeZoneOption, searchTerms = ''): boolean =>
  Object.values(tzOption).some(
    (value) =>
      typeof value === 'string' &&
      value.toLowerCase().includes(searchTerms.toLowerCase())
  )

type TimeZoneSelectProps = {
  className?: string
  siteIds?: string[]
  onChange?: (
    value: TimeZoneOption['value'] | null,
    timeZoneOption: TimeZoneOption | null
  ) => void
  value?: TimeZoneOption['value']
  required?: boolean
  [formControlProps: string]: unknown
}

const TimeZoneSelect = ({
  className,
  onChange,
  value,
  required = false,
  siteIds,
  ...formControlProps
}: TimeZoneSelectProps) => {
  const { data: timeZones, isLoading } = useGetTimezones()
  const defaultTimeZone = useTimeZoneInfo()
  const sites: Site[] = useSites()
  const [searchTerms, setSearchTerms] = useState<string>('')
  const { t } = useTranslation()

  const siteTimeZoneOptions = useMemo(
    () =>
      (timeZones?.length &&
        siteIds?.length &&
        sites
          .filter((site) => siteIds.includes(site.id))
          .map((site) => {
            const timeZoneInfo = timeZones.find(
              (tzInfo) => tzInfo.id === site.timeZoneId
            )
            return {
              ...timeZoneInfo,
              value: { timeZoneId: timeZoneInfo?.id, siteId: site.id },
              displayName: `${site.name} ${timeZoneInfo?.displayName ?? ''}`,
              siteId: site.id,
            }
          })) ||
      [],
    [siteIds, sites, timeZones]
  )

  const timeZoneOptions = useMemo<TimeZoneOption[]>(
    () =>
      timeZones
        ? timeZones.map((tzInfo) => ({
            ...tzInfo,
            value: { timeZoneId: tzInfo.id },
          }))
        : [],
    [timeZones]
  )

  const toggleOption = (timeZoneInfo: TimeZoneOption) => {
    if (onChange) {
      if (_.isEqual(timeZoneInfo.value, value)) {
        onChange(null, null)
      } else {
        onChange(timeZoneInfo.value, timeZoneInfo)
      }
    }
  }

  return (
    <Select
      {...formControlProps}
      label={t('labels.timezone')}
      className={className}
      value={value}
      required={required}
      placeholder={`${t('plainText.myDeviceTimezone')} ${
        defaultTimeZone?.displayName || ''
      }`}
      partialValueCheckDisabled
    >
      {/* The children of <Select> are used as the Dropdown content. We
      ensure that the searchTerms are reset so that we display the full
      list of options. when the Dropdown content is first mounted */}
      <Component onMount={() => setSearchTerms('')} />
      <SearchInput
        icon="search"
        value={searchTerms}
        onChange={setSearchTerms}
        debounce
        placeholder={t('labels.search')}
      />
      {isLoading ? (
        <Progress />
      ) : (
        [...siteTimeZoneOptions, ...timeZoneOptions].map(
          (tzInfo: TimeZoneOption) => (
            <StyledOption
              key={Object.values(tzInfo.value).join('_')}
              value={tzInfo.value}
              onClick={() => toggleOption(tzInfo)}
              $isSearchMatch={isSearchMatch(tzInfo, searchTerms)}
            >
              {tzInfo.displayName}
            </StyledOption>
          )
        )
      )}
    </Select>
  )
}

export default TimeZoneSelect
