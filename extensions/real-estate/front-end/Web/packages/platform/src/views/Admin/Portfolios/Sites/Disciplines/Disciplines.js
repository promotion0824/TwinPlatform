import { titleCase } from '@willow/common'
import {
  DocumentTitle,
  Fetch,
  Flex,
  Tab,
  TabBackButton,
  Tabs,
  useApi,
} from '@willow/ui'
import { Button, Icon } from '@willowinc/ui'
import _ from 'lodash'
import { useSite } from 'providers'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router'
import DisciplineModal from './DisciplineModal'
import DisciplinesTable from './DisciplinesTable'

export default function Disciplines() {
  const api = useApi()
  const params = useParams()
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const { name } = useSite()

  const [selectedDiscipline, setSelectedDiscipline] = useState()
  const [disciplines2d, setDisciplines2d] = useState([])
  const [disciplines3d, setDisciplines3d] = useState([])
  const [activeTab, setActiveTab] = useState('2D')

  const disciplines = [
    ...disciplines2d.flatMap((x) =>
      x.isModuleGroupParent ? x.disciplinesInGroup : [x]
    ),
    ...disciplines3d.flatMap((x) =>
      x.isModuleGroupParent ? x.disciplinesInGroup : [x]
    ),
  ]

  const handleDataResponse = ([
    disciplinesResponse,
    disciplinesSortOrderResponse,
  ]) => {
    // If sort order doesn't exist then create sortOrder list and save to preferences
    let disciplinesSortOrder = disciplinesSortOrderResponse
    if (
      !disciplinesSortOrderResponse ||
      !disciplinesSortOrderResponse.sortOrder2d ||
      !disciplinesSortOrderResponse.sortOrder3d
    ) {
      disciplinesSortOrder = createInitialSortedDisciplines(disciplinesResponse)
    }

    createSortedDisciplinesList(
      disciplinesResponse,
      disciplinesSortOrder.sortOrder2d,
      '2D'
    )
    createSortedDisciplinesList(
      disciplinesResponse,
      disciplinesSortOrder.sortOrder3d,
      '3D'
    )
  }

  const saveDisciplinesSortOrder = async (disciplinesSortOrder) => {
    await api.put(
      `/api/sites/${params.siteId}/preferences/moduleGroups`,
      disciplinesSortOrder
    )
  }

  const createInitialSortedDisciplines = (disciplinesResponse) => {
    const sortOrder2d = sortDisciplinesByType(disciplinesResponse, '2D')
    const sortOrder3d = sortDisciplinesByType(disciplinesResponse, '3D')
    const sortOrder = {
      sortOrder2d,
      sortOrder3d,
    }
    saveDisciplinesSortOrder(sortOrder)
    return sortOrder
  }

  const sortDisciplinesByType = (disciplinesResponse, defaultType) => {
    const filteredDisciplines = disciplinesResponse.filter(
      (discipline) => discipline.is3D === (defaultType === '3D')
    )
    const sortedDisciplineGroups = _(filteredDisciplines)
      .groupBy((discipline) => discipline.group?.id)
      .flatMap((disciplinesInGroup, groupIdKey) => {
        if (groupIdKey !== 'undefined') {
          return [
            {
              isModuleGroupParent: true,
              ...disciplinesInGroup[0].group,
              disciplinesInGroup: _(disciplinesInGroup)
                .orderBy((item) => item.sortOrder)
                .value(),
            },
          ]
        }

        return [...disciplinesInGroup]
      })
      .orderBy((groupItem) => groupItem.sortOrder)
      .value()

    return getSortOrder(sortedDisciplineGroups)
  }

  const getSortOrder = (sortedDisciplineGroups) => {
    const retSortOrder = []

    for (let i = 0; i < sortedDisciplineGroups.length; i++) {
      const sortedDisciplineGroup = sortedDisciplineGroups[i]
      retSortOrder.push(sortedDisciplineGroup.id)

      if (sortedDisciplineGroup.disciplinesInGroup?.length) {
        for (
          let j = 0;
          j < sortedDisciplineGroup.disciplinesInGroup.length;
          j++
        ) {
          const sortedDisciplineGroupElement =
            sortedDisciplineGroup.disciplinesInGroup[j]
          retSortOrder.push(sortedDisciplineGroupElement.id)
        }
      }
    }

    return retSortOrder
  }

  const createSortedDisciplinesList = (
    disciplinesResponse,
    disciplinesSortOrder,
    type
  ) => {
    const disciplines = []
    let currentGroupIndex = 0
    for (let i = 0; i < disciplinesSortOrder.length; i++) {
      const searchDisciplineId = disciplinesSortOrder[i]
      const disciplineToAdd = disciplinesResponse.find(
        (x) => x.id === searchDisciplineId
      )
      if (disciplineToAdd) {
        if (disciplineToAdd.group) {
          disciplines[currentGroupIndex].disciplinesInGroup.push(
            disciplineToAdd
          )
        } else {
          disciplines.push(disciplineToAdd)
        }
      } else {
        const groupToAdd = disciplinesResponse.find(
          (x) => x.group?.id === searchDisciplineId
        )?.group
        if (groupToAdd) {
          disciplines.push({
            isModuleGroupParent: true,
            disciplinesInGroup: [],
            ...groupToAdd,
          })
          currentGroupIndex = disciplines.length - 1
        }
      }
    }
    setDisciplines(disciplines, type)
  }

  const setDisciplines = (disciplines, type) => {
    if (type === '2D') {
      setDisciplines2d(disciplines)
    } else {
      setDisciplines3d(disciplines)
    }
  }

  function handleAddDisciplineClick() {
    setSelectedDiscipline({
      name: '',
      prefix: '',
      moduleGroup: '',
      is3D: activeTab === '3D',
      canBeDeleted: false,
      isDefault: false,
    })
  }

  const handleDisciplinesUpdate = (nextDisciplines, type) => {
    const sortOrder2d = getSortOrder(
      type === '2D' ? nextDisciplines : disciplines2d
    )
    const sortOrder3d = getSortOrder(
      type === '3D' ? nextDisciplines : disciplines3d
    )
    const sortOrder = {
      sortOrder2d,
      sortOrder3d,
    }
    saveDisciplinesSortOrder(sortOrder)
    setDisciplines(nextDisciplines, type)
  }

  return (
    <>
      <DocumentTitle
        scopes={[
          titleCase({ text: t('headers.disciplines'), language }),
          name,
          t('plainText.buildings'),
          t('headers.admin'),
        ]}
      />

      <Flex fill="header" padding="small 0 0 0">
        <Fetch
          name="disciplines"
          url={[
            `/api/sites/${params.siteId}/ModuleTypes`,
            `/api/sites/${params.siteId}/preferences/moduleGroups`,
          ]}
          onResponse={handleDataResponse}
        >
          <Tabs $borderWidth="1px 0 0 0">
            <TabBackButton />
            <Tab
              header={t('headers.disciplines2d')}
              selected={activeTab === '2D'}
              onClick={() => setActiveTab('2D')}
              data-testid="manage-discipline-tab-2d"
            >
              <DisciplinesTable
                sortedDisciplineGroups={disciplines2d}
                updateDisciplinesSortOrder={(nextDisciplines) =>
                  handleDisciplinesUpdate(nextDisciplines, '2D')
                }
                selectedDiscipline={selectedDiscipline}
                setSelectedDiscipline={setSelectedDiscipline}
              />
            </Tab>
            <Tab
              header={t('headers.disciplines3d')}
              selected={activeTab === '3D'}
              onClick={() => setActiveTab('3D')}
              data-testid="manage-discipline-tab-3d"
            >
              <DisciplinesTable
                sortedDisciplineGroups={disciplines3d}
                updateDisciplinesSortOrder={(nextDisciplines) =>
                  handleDisciplinesUpdate(nextDisciplines, '3D')
                }
                selectedDiscipline={selectedDiscipline}
                setSelectedDiscipline={setSelectedDiscipline}
              />
            </Tab>
            <Flex align="right middle" padding="0 medium">
              <Button
                onClick={handleAddDisciplineClick}
                prefix={<Icon icon="add" />}
              >
                {t('plainText.addDiscipline')}
              </Button>
            </Flex>
          </Tabs>
        </Fetch>
      </Flex>
      {selectedDiscipline != null && (
        <DisciplineModal
          disciplines={disciplines}
          discipline={selectedDiscipline}
          sortedDisciplineGroups={
            activeTab === '2D' ? disciplines2d : disciplines3d
          }
          updateDisciplinesSortOrder={(nextDisciplines) =>
            handleDisciplinesUpdate(nextDisciplines, activeTab)
          }
          onClose={() => setSelectedDiscipline()}
        />
      )}
    </>
  )
}
