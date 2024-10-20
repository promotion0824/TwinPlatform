import { titleCase } from '@willow/common'
import {
  api,
  Fieldset,
  Flex,
  Form,
  Header,
  Input,
  ModalSubmitButton,
  NumberInput,
  Option,
  Select,
  useAnalytics,
  useDisclosure2,
  useFetchRefresh,
  useLanguage,
  useModal,
  useSnackbar,
  useUser,
  ValidationError,
} from '@willow/ui'
import { Button, ButtonGroup, Icon, Modal } from '@willowinc/ui'
import TimeZoneSelect from 'components/TimeZoneSelect/TimeZoneSelect.tsx'
import _ from 'lodash'
import { useSites } from 'providers'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useMutation, useQueryClient } from 'react-query'
import { useParams } from 'react-router'
import { css } from 'styled-components'
import useDelete3dModule from '../../../../../hooks/ThreeDimensionModule/useDelete3dModule/useDelete3dModule'
import useGet3dModule, {
  useGet3dModuleFile,
} from '../../../../../hooks/ThreeDimensionModule/useGet3dModule/useGet3dModule'
import useUpload3dModule from '../../../../../hooks/ThreeDimensionModule/useUpload3dModule/useUpload3dModule'
import { useClassicExplorerLandingPath } from '../../../../Layout/Layout/Header/utils'
import CategoryButton from '../../CategoryButton/CategoryButton'
import ArcGisMapSelector from './ArcGisMapSelector'
import AddModelModal from './components/AddModelModal/AddModelModal'
import Features from './Features'
import Image from './Image'
import Levels from './Levels'

function getFormData(key, data) {
  const formData = new FormData()
  formData.set(key, data)
  return formData
}

function getError(errors) {
  return errors.find((error) => !!error)
}

const buildingTypes = [
  {
    translationKey: 'plainText.amenity',
    value: 'Amenity',
  },
  {
    translationKey: 'plainText.aviation',
    value: 'Aviation',
  },
  {
    translationKey: 'plainText.education',
    value: 'Education',
  },
  {
    translationKey: 'plainText.foodService',
    value: 'FoodService',
  },
  {
    translationKey: 'plainText.health',
    value: 'Health',
  },
  {
    translationKey: 'plainText.hotel',
    value: 'Hotel',
  },
  {
    translationKey: 'plainText.industrial',
    value: 'Industrial',
  },
  {
    translationKey: 'plainText.office',
    value: 'Office',
  },
  {
    translationKey: 'plainText.parking',
    value: 'Parking',
  },
  {
    translationKey: 'plainText.publicAssembly',
    value: 'PublicAssembly',
  },
  {
    translationKey: 'plainText.residential',
    value: 'Residential',
  },
  {
    translationKey: 'plainText.retail',
    value: 'Retail',
  },
]

export default function SiteForm({ site, workgroups }) {
  const queryClient = useQueryClient()
  const fetchRefresh = useFetchRefresh()
  const params = useParams()
  const user = useUser()
  const snackbar = useSnackbar()
  const { t } = useTranslation()
  const { countryList, language } = useLanguage()
  const modal = useModal()
  const analytics = useAnalytics()
  const sites = useSites()
  const classicExplorerLandingPath = useClassicExplorerLandingPath({
    siteId: site.id,
    // as this is used to navigate user to floor viewer (classic viewer) and
    // allow user to upload 3D model, so we don't care if it has base module
    hasBaseModuleOption: {
      hasBaseModule: false,
    },
  })

  const [yearOpened, setYearOpened] = useState(site.dateOpened?.split('-')[0])

  const [modelModalOpened, setModelModalOpened] = useState(false)
  const [isDeleteBuildingModalOpened, deleteBuildingModal] =
    useDisclosure2(false)
  const deleteSiteMutation = useMutation(
    () => api.delete(`/sites/${site.id}`),
    {
      onSuccess: () => {
        deleteBuildingModal.close()
        modal.close()
        snackbar.show(t('plainText.buildingDeleteSuccess'), { isToast: true })
      },
      onError: () => {
        deleteBuildingModal.close()
        snackbar.show(t('plainText.buildingDeleteError'))
      },
    }
  )

  const { id: siteId } = site
  const {
    error: uploading3dModuleError,
    mutate: upload3dModule,
    reset: reset3dUploadState,
    isSuccess: isUploadSuccess,
    isLoading: isUploadLoading,
    progress,
  } = useUpload3dModule({ enabled: modelModalOpened })

  const { error: get3dModuleError, data: moduleData } = useGet3dModule(siteId, {
    enabled: modelModalOpened,
    staleTime: 0,
  })

  const {
    error: get3dModuleFileError,
    data: moduleFile,
    remove: remove3dModuleFile,
  } = useGet3dModuleFile(moduleData, {
    enabled: !!moduleData?.url && !!moduleData?.name && modelModalOpened,
    staleTime: 0,
  })

  const { error: delete3dModuleError, mutate: delete3dModule } =
    useDelete3dModule()
  const moduleError = getError([
    get3dModuleError,
    get3dModuleFileError,
    uploading3dModuleError,
    delete3dModuleError,
  ])

  useEffect(() => {
    if (!modelModalOpened) {
      reset3dUploadState()
      remove3dModuleFile()
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [modelModalOpened])

  const [uploadModalRef, setUploadModalRef] = useState()
  useEffect(() => {
    if (isUploadSuccess && uploadModalRef) {
      snackbar.show(t('plainText.fileWasUploadedSuccessfully'), { icon: 'ok' })
      uploadModalRef.close()
      analytics.track('3D_Building_Model_Added', {
        Site: sites.find((siteObject) => siteObject.id === siteId),
        page: 'Building 3D Model',
      })
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isUploadSuccess, uploadModalRef])

  function handleUpload3dModuleSubmit(file) {
    const formData = getFormData('file', file)
    upload3dModule({ siteId, formData })
  }
  function handleUpload3dModuleSubmitted(form) {
    setUploadModalRef(form.modal)
  }
  function handleFilesChange(files) {
    if (moduleData && files.length === 0) {
      delete3dModule(siteId)
    }
  }

  function handleSubmit(form) {
    const data = {
      name: form.data.name,
      type: form.data.type,
      status: form.data.status,
      area: form.data.area,
      dateOpened: yearOpened ? `${yearOpened}-01-01` : undefined,
      address: form.data.address,
      suburb: form.data.suburb,
      state: form.data.state,
      country: form.data.country,
      latitude: form.data.latitude,
      longitude: form.data.longitude,
      timeZoneId: form.data.timeZoneOption?.timeZoneId,
      features: form.data.features,
      webMapId: form.data.webMapId,
    }

    if (site.isNewSite) {
      let levels = form.data.levels?.filter((level) => level[1] != null) ?? []
      const isAllPrefixesUnique =
        levels.length ===
        _(levels)
          .uniqBy((level) => level[0])
          .value().length

      if (!isAllPrefixesUnique) {
        throw new ValidationError({
          name: 'floorCodes',
          message: t('messages.levelPrefixUnique'),
        })
      }

      levels = form.data.levels
        .map((level) => {
          const levelCount = level[1] !== 0 ? level[1] : 1

          return [
            level[0],
            [...Array(levelCount ?? 0)].map((n, i) =>
              level[1] !== 0 ? `${level[0]}${i + 1}` : level[0]
            ),
          ]
        })
        .map((level, i) => (i === 0 ? [level[0], level[1].reverse()] : level))
        .flatMap((level) => level[1])

      return form.api.post(
        `/api/customers/${user.customer.id}/portfolios/${params.portfolioId}/sites`,
        {
          ...data,
          code: form.data.code,
          floorCodes: levels,
        }
      )
    } else {
      return form.api.put(
        `/api/customers/${user.customer.id}/portfolios/${params.portfolioId}/sites/${site.id}`,
        {
          ...data,
          settings: {
            inspectionDailyReportWorkgroupId: form.data.workgroup?.id,
          },
        }
      )
    }
  }

  function handleSubmitted(form) {
    form.modal.close()
    fetchRefresh('sites')
    queryClient.invalidateQueries(['sites'])
  }

  return (
    <>
      <Form
        defaultValue={site}
        onSubmit={handleSubmit}
        onSubmitted={handleSubmitted}
      >
        <Flex>
          <Flex horizontal fill="content">
            <Flex padding="extraLarge 0 extraLarge extraLarge">
              <Image site={site} />
            </Flex>
            <Flex>
              {!site.isNewSite && (
                <Flex padding="0 extraLarge 0 extraLarge">
                  <Header fill="header" padding="extraLarge 0">
                    <Flex horizontal fill="wrap" size="small">
                      <CategoryButton
                        icon="users"
                        to={`/admin/users?siteId=${site.id}`}
                        data-testid="manage-users-button"
                      >
                        {t('plainText.manageUsers')}
                      </CategoryButton>
                      <CategoryButton
                        icon="floorsAdmin"
                        to={`/admin/portfolios/${params.portfolioId}/sites/${site.id}/floors`}
                        data-testid="manage-floors-button"
                      >
                        {t('plainText.manageFloors')}
                      </CategoryButton>
                      <CategoryButton
                        icon="power"
                        to={`/admin/portfolios/${params.portfolioId}/sites/${site.id}/connectors`}
                        data-testid="manage-connector-button"
                      >
                        {t('plainText.manageConnectors')}
                      </CategoryButton>
                      <CategoryButton
                        icon="floors"
                        to={
                          classicExplorerLandingPath
                            ? `${classicExplorerLandingPath}?admin=true`
                            : `/sites/${site.id}?admin=true`
                        }
                        data-testid="go-floor-button"
                      >
                        {t('plainText.goToFloors')}
                      </CategoryButton>
                      <CategoryButton
                        icon="layers"
                        to={`/admin/portfolios/${params.portfolioId}/sites/${site.id}/disciplines`}
                        data-testid="manage-discipline-code-button"
                      >
                        {t('plainText.manageDisciplinesCode')}
                      </CategoryButton>
                      <CategoryButton
                        icon="iconThreeDimension"
                        onClick={() => {
                          setModelModalOpened(!modelModalOpened)
                        }}
                        data-testid="building-3d-button"
                      >
                        {t('plainText.building3dModel')}
                      </CategoryButton>
                    </Flex>
                  </Header>
                </Flex>
              )}
              <Fieldset legend={t('plainText.siteInfo')}>
                <Flex horizontal fill="equal" size="large">
                  <Input
                    name="name"
                    label={titleCase({ language, text: t('labels.siteName') })}
                    required
                  />
                  <Input
                    name="code"
                    label={titleCase({ language, text: t('labels.siteCode') })}
                    required
                    readOnly={!site.isNewSite}
                  />
                </Flex>
                <Flex horizontal fill="equal" size="large">
                  <Select name="type" label={t('labels.type')} required>
                    {buildingTypes
                      .map(({ translationKey, value }) => ({
                        text: t(translationKey),
                        value,
                      }))
                      .sort((a, b) => a.text.localeCompare(b.text))
                      .map(({ text, value }) => (
                        <Option key={value} value={value}>
                          {titleCase({ language, text })}
                        </Option>
                      ))}
                  </Select>
                  <Select name="status" label={t('labels.status')} required>
                    <Option value="Construction">
                      {t('plainText.construction')}
                    </Option>
                    <Option value="Design">{t('plainText.design')}</Option>
                    <Option value="Operations">
                      {t('plainText.operations')}
                    </Option>
                    <Option value="Selling">{t('plainText.selling')}</Option>
                    {!site.isNewSite && user.isCustomerAdmin && (
                      <Option value="Deleted">{t('plainText.deleted')}</Option>
                    )}
                  </Select>
                </Flex>
                <Flex horizontal fill="equal" size="large">
                  <Input name="area" label={t('labels.size')} required />
                  <NumberInput
                    label={titleCase({
                      language,
                      text: t('labels.yearOpened'),
                    })}
                    name="yearOpened"
                    onChange={(newValue) => {
                      if (!newValue || /^\d+$/.test(newValue)) {
                        setYearOpened(newValue)
                      }
                    }}
                    value={yearOpened}
                  />
                </Flex>
              </Fieldset>
              <Fieldset legend={t('plainText.siteLocation')}>
                <Flex horizontal fill="equal" size="large">
                  <Input name="address" label={t('labels.address')} required />
                  <Input name="suburb" label={t('labels.suburb')} required />
                </Flex>
                <Flex horizontal fill="equal" size="large">
                  <Input name="state" label={t('labels.state')} required />
                  <Select name="country" label={t('labels.country')} required>
                    {countryList.map((country) => (
                      <Option value={country} key={country}>
                        {t('interpolation.countries', {
                          key: _.camelCase(country),
                        })}
                      </Option>
                    ))}
                  </Select>
                </Flex>
                <Flex horizontal fill="equal" size="large">
                  <NumberInput
                    name="latitude"
                    label={t('labels.latitude')}
                    min={-85}
                    max={85}
                    required
                  />
                  <NumberInput
                    name="longitude"
                    label={t('labels.longitude')}
                    min={-180}
                    max={180}
                    required
                  />
                </Flex>
                <Flex horizontal fill="equal" size="large">
                  <TimeZoneSelect
                    name="timeZoneOption"
                    errorName="timeZoneId"
                    required
                  />
                  <div />
                </Flex>
              </Fieldset>
              {!site.isNewSite && (
                <Fieldset legend={t('plainText.siteNotifications')}>
                  <Flex horizontal fill="equal" size="large">
                    <Select
                      name="workgroup"
                      label={t('labels.inspectionsSummary')}
                      header={(workgroup) => workgroup?.name}
                    >
                      {_(workgroups)
                        .orderBy((workgroup) => workgroup.name.toLowerCase())
                        .map((workgroup) => (
                          <Option key={workgroup.id} value={workgroup}>
                            {workgroup.name}
                          </Option>
                        ))
                        .value()}
                    </Select>
                    <div />
                  </Flex>
                </Fieldset>
              )}
              <Features site={site} />

              {site?.features?.isArcGisEnabled && (
                <Fieldset>
                  <div tw="flex width[50%]">
                    <div tw="flex-1">
                      <ArcGisMapSelector siteId={site.id} />
                    </div>
                  </div>
                </Fieldset>
              )}

              {site.isNewSite ? (
                <Levels name="levels" errorName="floorCodes" />
              ) : (
                <>
                  <div css={{ paddingLeft: 'var(--padding-extra-large)' }}>
                    <hr
                      css={({ theme }) => ({ marginBottom: theme.spacing.s24 })}
                    />
                    <Button
                      kind="negative"
                      prefix={<Icon icon="delete" />}
                      onClick={() => {
                        deleteBuildingModal.open()
                      }}
                    >
                      {titleCase({
                        text: t('labels.deleteBuilding'),
                        language,
                      })}
                    </Button>
                  </div>
                </>
              )}
            </Flex>
          </Flex>
        </Flex>
        <ModalSubmitButton>{t('plainText.save')}</ModalSubmitButton>
      </Form>
      {modelModalOpened && (
        <AddModelModal
          onClose={() => {
            setModelModalOpened(false)
          }}
          onChange={handleFilesChange}
          onSubmit={handleUpload3dModuleSubmit}
          onSubmitted={handleUpload3dModuleSubmitted}
          errorMessage={moduleError?.message}
          successful={isUploadSuccess}
          loading={isUploadLoading}
          value={moduleFile}
          percentage={moduleFile ? progress : 0}
        />
      )}
      <Modal
        opened={isDeleteBuildingModalOpened}
        onClose={deleteBuildingModal.close}
        centered
        withinPortal
        header="Warning"
      >
        <div css={css(({ theme }) => ({ padding: theme.spacing.s16 }))}>
          <div>Are you sure you want to delete this building?</div>
          <div
            css={css(({ theme }) => ({
              paddingTop: theme.spacing.s8,
              display: 'flex',
              width: '100%',
              justifyContent: 'flex-end',
            }))}
          >
            <ButtonGroup>
              <Button kind="secondary" onClick={deleteBuildingModal.close}>
                {t('plainText.cancel')}
              </Button>
              <Button
                kind="negative"
                disabled={deleteSiteMutation.isLoading}
                onClick={deleteSiteMutation.mutate}
              >
                {titleCase({ text: t('labels.deleteBuilding'), language })}
              </Button>
            </ButtonGroup>
          </div>
        </div>
      </Modal>
    </>
  )
}
