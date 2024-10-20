/* eslint-disable complexity */
import { useRef } from 'react'
import _ from 'lodash'
import { v4 as uuidv4 } from 'uuid'
import { styled } from 'twin.macro'
import { Button, getContainmentHelper, Icon, IconNew } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import {
  getField,
  getDisplayName,
  getEnumValueLabel,
  isNestedField,
} from '@willow/common/twins/view/models'
import {
  splitTwin,
  getMissingFields,
  isEmpty,
} from '@willow/common/twins/view/twinModel'
import { useTwinEditor } from './TwinEditorContext.tsx'
import {
  Property,
  PropertyName,
  PropertyInput,
  PropertyText,
  PropertyValue,
  GroupHeading,
  Subheading,
  MoreLessButton,
  stateColor,
} from '../shared/TwinView'
import {
  VersionHistory,
  VersionHistoryMessage,
} from './VersionHistory/VersionHistory'

const PROPERTIES_GRID_BREAKPOINT = '600px'

const { ContainmentWrapper, getContainerQuery } = getContainmentHelper()

const PropertiesInner = styled.div({
  display: 'grid',

  [getContainerQuery(`max-width: ${PROPERTIES_GRID_BREAKPOINT}`)]: {
    gridTemplateColumns: '100%',
  },

  [getContainerQuery(`min-width: ${PROPERTIES_GRID_BREAKPOINT}`)]: {
    gridTemplateColumns: '50% 50%',
  },
})

const SubheadingContainer = styled.div(({ theme }) => ({
  backgroundColor: theme.color.neutral.bg.panel.default,
  display: 'flex',
  flexDirection: 'column',
  padding: theme.spacing.s16,
  position: 'sticky',
  top: '0px',
  zIndex: 10,
}))

export default function TwinEditor({
  initialTwin,
  expandedModel,
  conflictedFields,
}) {
  const {
    canEdit,
    isExpanded,
    isEditing,

    control,
    values,
    dirtyFields,
    expectedFields,

    submit,

    toggleExpanded,

    versionHistory,
  } = useTwinEditor()

  const { showVersionHistory, versionHistoryEditedFields, selectedVersion } =
    versionHistory

  const { topLevelProperties, groups } = splitTwin(
    selectedVersion ? selectedVersion.twin : values,
    expandedModel,
    versionHistoryEditedFields,
    expectedFields
  )

  return (
    <ContainmentWrapper>
      <form onSubmit={submit}>
        <div>
          <SubheadingContainer>
            <div tw="flex flex-1 justify-between">
              {
                /* Position InformationSubheading based on featureflag "twinViewVersionHistory" */
                !showVersionHistory && <InformationSubheading />
              }
              {!!selectedVersion && <VersionHistoryMessage />}
              {canEdit && !selectedVersion && <Edit />}
              {showVersionHistory && <VersionHistory />}
            </div>
            {showVersionHistory && <InformationSubheading />}
          </SubheadingContainer>

          <div css={{ margin: '0 1rem 1rem' }}>
            <Properties
              control={control}
              expandedModel={expandedModel}
              properties={topLevelProperties}
              conflictedFields={conflictedFields}
              isEditing={isEditing}
              isExpanded={isExpanded}
              dirtyFields={dirtyFields}
            />

            <GroupedProperties
              control={control}
              expandedModel={expandedModel}
              conflictedFields={conflictedFields}
              groups={groups}
              isEditing={isEditing}
              isExpanded={isExpanded}
              dirtyFields={dirtyFields}
            />

            {isEditing &&
              (isExpanded ||
                getMissingFields(initialTwin, expandedModel).length > 0) && (
                <MoreLessButton
                  onClick={toggleExpanded}
                  // This is how we tell the button to have a border
                  color="transparent"
                  tw="mt-6"
                >
                  {isExpanded ? 'Show less' : 'Show more'}
                </MoreLessButton>
              )}
          </div>
        </div>
      </form>
    </ContainmentWrapper>
  )
}

/**
 * Information subheading for the form in Twin view's Summary tab
 */
function InformationSubheading() {
  const { t } = useTranslation()
  return (
    <div tw="flex-initial">
      <Subheading>
        <IconNew icon="description" />
        <span tw="ml-1">{t('plainText.information')}</span>
      </Subheading>
    </div>
  )
}

/**
 * This is a button that changes the form to "editing" state when clicked.
 * When in "editing" state, two more button appears: Save and cancel.
 *  - When Save button is clicked, form is submitted and enters "saving" state.
 *      - Once save is successful, form enters back to initial state.
 *  - When Cancel button is clicked, form enter back to initial state.
 */
function Edit() {
  const { isEditing, isSaving, beginEditing, cancel, versionHistory } =
    useTwinEditor()

  const { showVersionHistory } = versionHistory

  // Depending on feature flag "twinViewVersionHistory", adjust size of EditButton.
  const EditButton = !showVersionHistory ? EditButtonV1 : EditButtonV2

  return (
    <div tw="flex-1 text-right">
      <ButtonsContainer>
        {isSaving ? (
          <EditButton title="Saving" $first>
            <Icon icon="progress" />
          </EditButton>
        ) : isEditing ? (
          <>
            <EditButton type="submit" title="Save" $first>
              <Icon icon="ok" color="inherit" />
            </EditButton>
            <EditButton onClick={cancel} title="Cancel">
              <Icon icon="close" />
            </EditButton>
          </>
        ) : null}
        <BoxShadowLeft>
          <EditButton
            onClick={beginEditing}
            title="Edit twin information"
            disabled={isSaving}
            $first={!isEditing}
            $last
            style={{ color: isEditing ? 'var(--primary5)' : null }}
          >
            <Icon icon="create" color="inherit" />
          </EditButton>
        </BoxShadowLeft>
      </ButtonsContainer>
    </div>
  )
}

const ButtonsContainer = styled.div({
  display: 'inline',
  boxShadow: '0px 3px 6px #00000029',
})

const EditButtonStyle = {
  backgroundColor: 'var(--theme-color-neutral-bg-accent-default)',
  display: 'inline-flex',
  alignItems: 'center',
  justifyContent: 'center',

  borderRadius: ({ $first, $last }) =>
    $first && $last
      ? '2px'
      : $first
      ? '2px 0px 0px 2px'
      : $last
      ? '0px 2px 2px 0px'
      : null,
}

const EditButtonV1 = styled(Button)({
  width: 28,
  height: 24,
  ...EditButtonStyle,
})

const EditButtonV2 = styled(Button)({
  width: 38,
  height: 32,
  ...EditButtonStyle,
})

// Makes the pencil button cast a shadow on top of the button to the left of
// it, and only to the left of it.
const BoxShadowLeft = styled.div({
  display: 'inline-block',
  boxShadow: '#00000029 0px 0px 6px',
  clipPath: 'inset(0px 0px 0px -3px)',
})

/**
 * How should the value look when it's read only?
 *
 * - Nested objects are JSON-stringified
 * - For enum values we try to get the option's display name
 */
function stringify(val, schema) {
  if (['object', 'boolean'].includes(typeof val)) {
    return JSON.stringify(val)
  } else if (typeof schema === 'object' && schema['@type'] === 'Enum') {
    const enumValue = schema.enumValues.find((v) => v.name === val)
    if (enumValue != null) {
      return getEnumValueLabel(enumValue)
    } else {
      return val ?? ''
    }
  } else {
    return val
  }
}

function EditableProperty({
  expandedModel,
  idPrefix,
  name,
  label,
  groupName,

  /**
   * Period-joined field path, eg "group.groupItem".
   */
  controllerName,
  conflictedField,
  schema,

  /**
   * true if we are in edit mode, regardless of whether this particular field
   * is editable. If we are not in edit mode, all fields are plain text. If we
   * are in edit mode, they are widgets, which may or may not be read only
   * depending on the `readOnly` prop.
   */
  isEditing,
  /**
   * true if the field is expected to be displayed which may or may not have value.
   * If it's false, then it is considered as optional field.
   */
  isExpected,
  readOnly,
}) {
  const { t } = useTranslation()
  const inputId = `${idPrefix}-${name}-input`
  const labelId = `${idPrefix}-${name}-label`

  const { getValues, errors, versionHistory } = useTwinEditor()

  const { versionHistoryEditedFields, previousVersion } = versionHistory

  const hasBeenEdited =
    _.get(versionHistoryEditedFields, controllerName) !== undefined

  const isNullChange =
    _.get(versionHistoryEditedFields, controllerName) === null

  const error = _.get(errors, controllerName.split('.'))
  let state
  // If we are at a leaf object, conflictedField will be a boolean. If we are
  // at a sub-group (which we render by JSON-stringifying), conflictedField
  // will be a (possibly nested) mapping from field names to booleans. So
  // we want to show that there's a conflict if there's a `true`
  // anywhere in the structure.
  if (containsDeep(conflictedField, true)) {
    state = 'conflict'
  } else if (error != null) {
    state = 'error'
  } else {
    state = 'normal'
  }

  const getPropertyValue = (propertyName = undefined, usePlaceholder = true) =>
    previousVersion
      ? stringify(
          _.get(previousVersion?.twin, propertyName ?? controllerName),
          schema
        )
      : stringify(getValues(propertyName ?? controllerName), schema) ??
        (usePlaceholder ? t('placeholder.noEntry') : undefined)

  const getEnumDisplayName = () => {
    const enumValue = getPropertyValue(annotatedByPropertyName, false)
    return isAnnotatedBy.schema.enumValues.find(
      (v) => v.enumValue === enumValue
    )?.displayName
  }

  const groupModelContents =
    groupName &&
    expandedModel.contents.find((c) => c.name === groupName)?.schema.fields

  const modelContents = groupModelContents ?? expandedModel.contents
  const fullSchema = modelContents.find((c) => c.name === name)

  const isCustomProperties = groupName === 'customProperties'
  const isAnnotation = fullSchema?.annotates != null
  if (isAnnotation) return null

  const isAnnotatedBy = modelContents.find((c) => c.annotates === name)

  const annotatedByPropertyName = groupName
    ? `${groupName}.${isAnnotatedBy?.name}`
    : isAnnotatedBy?.name

  const isAnnotatedBySuffix = !isAnnotatedBy
    ? ''
    : isAnnotatedBy?.schema === 'enum' ||
      isAnnotatedBy?.schema?.['@type']?.toLowerCase() === 'enum'
    ? getEnumDisplayName() ?? `(${t('plainText.unspecifiedUnits')})`
    : getPropertyValue(annotatedByPropertyName, false) ??
      `(${t('plainText.unspecifiedUnits')})`

  return (
    <Property key={name}>
      <PropertyName>
        <label id={labelId} htmlFor={inputId}>
          {label}
        </label>
      </PropertyName>
      {isEditing ? (
        <div>
          <PropertyInput
            id={inputId}
            idPrefix={idPrefix}
            ariaLabelledBy={labelId}
            controllerName={controllerName}
            groupName={groupName}
            isAnnotatedBy={isAnnotatedBy}
            schema={schema}
            readOnly={readOnly}
            state={state}
          />
          {state === 'error' ? (
            <FieldMessage $state="error">{error.message}</FieldMessage>
          ) : state === 'conflict' ? (
            <FieldMessage $state="conflict">
              {t('plainText.keepOrUpdate')}
            </FieldMessage>
          ) : null}
        </div>
      ) : (
        <div>
          {hasBeenEdited ? (
            <VersionHistoryProperty
              isNullChange={isNullChange}
              controllerName={controllerName}
              schema={schema}
            />
          ) : (
            <PropertyValue isDisabled={isExpected}>
              {isCustomProperties ? (
                <MapProperty propertyValue={getPropertyValue()} />
              ) : isAnnotatedBy ? (
                `${getPropertyValue()} ${isAnnotatedBySuffix}`
              ) : (
                getPropertyValue()
              )}
            </PropertyValue>
          )}
        </div>
      )}
    </Property>
  )
}

function MapProperty({ propertyValue }) {
  try {
    const map = JSON.parse(propertyValue)
    const text = Object.entries(map)
      .map(([key, value]) => `${key}: ${value}`)
      .join('\n')

    return <div style={{ whiteSpace: 'pre-wrap' }}>{text}</div>
  } catch {
    return propertyValue
  }
}

/**
 * When viewing a previous version,
 * If the field has been edited, the field will be highlighted with its changed values.
 * If the field has been nulled, highlight previous value and strike through.
 */
function VersionHistoryProperty({ isNullChange, controllerName, schema }) {
  const { versionHistory } = useTwinEditor()
  const { versionHistoryEditedFields, previousVersion } = versionHistory

  return (
    <PropertyValue hasBeenEdited>
      <PropertyText isNullChange={isNullChange}>
        {isNullChange
          ? stringify(_.get(previousVersion?.twin, controllerName), schema)
          : stringify(
              _.get(versionHistoryEditedFields, controllerName),
              schema
            )}
      </PropertyText>
    </PropertyValue>
  )
}

const FieldMessage = styled.div({
  color: ({ $state }) => stateColor($state),
  marginTop: 4,
  fontSize: 11,
})

/**
 * Return `true` if `val` exists anywhere in `ob`.
 */
function containsDeep(ob, val) {
  if (ob === val) {
    return true
  } else if (typeof ob === 'object' && ob != null) {
    return Object.values(ob).some((v) => containsDeep(v, val))
  } else if (Array.isArray(ob)) {
    return ob.some((v) => containsDeep(v, val))
  } else {
    return false
  }
}

function PropertyFields({
  expandedModel,
  properties,
  conflictedFields,
  groupName,
  idPrefix,
  isEditing,
  isExpanded,
  control,
  controllerNamePrefix,
  dirtyFields,
  readOnly,
}) {
  const { hiddenFields, readOnlyFields, versionHistory, expectedFields } =
    useTwinEditor()

  const { versionHistoryEditedFields } = versionHistory

  return (
    <PropertiesInner data-testid="tab-summary-information">
      {Object.entries(properties).map(([propName, propVal]) => {
        const path = [...controllerNamePrefix, propName]
        const isExpected = expectedFields.some((p) => _.isEqual(path, [p]))

        if (
          propName === '$metadata' ||
          propName === 'etag' ||
          hiddenFields.some((p) => matchesPointer(path, p)) ||
          // Case for when "customProperties" has a key other than "Warranty",
          // remove "Warranty" and keep the "customProperties" section.
          _.isEqual(path, ['customProperties', 'Warranty'])
        ) {
          return null
        }

        if (
          // We do not want to display fields with null / empty string values
          // unless we are expanded, or the field is dirty. If we don't check
          // for the field being dirty, the field will disappear if the user
          // clears it.
          (propVal == null || propVal === '') &&
          !isExpanded &&
          !_.get(dirtyFields, path) &&
          !isExpected &&
          // When we're viewing a previous version, we want to display fields that've been nulled.
          _.get(versionHistoryEditedFields, path) === undefined
        ) {
          return null
        }

        return (
          <EditableProperty
            expandedModel={expandedModel}
            key={propName}
            idPrefix={idPrefix}
            name={propName}
            groupName={groupName}
            label={getDisplayName(expandedModel, path)}
            schema={
              _.isEqual(path, ['id'])
                ? 'string'
                : getField(expandedModel, path).schema
            }
            isEditing={isEditing}
            readOnly={
              // We are read-only if the whole group is read only, if the value
              // is a dictionary (because editing of dictionaries is not yet
              // implemented), or if the field is one of the fields in the read
              // only list.
              readOnly ||
              readOnlyFields.some((p) => matchesPointer(path, p)) ||
              isNestedField(expandedModel, path)
            }
            isExpected={isExpected}
            control={control}
            controllerName={path.join('.')}
            conflictedField={_.get(conflictedFields, path)}
          />
        )
      })}
    </PropertiesInner>
  )
}

/**
 * A field "matches" a JSON pointer if its path or the path to a field that
 * contains it is read-only. eg. if `fieldPath` is ["group", "subgroup"],
 * it will match "/group" and it will also match "/group/subgroup".
 */
function matchesPointer(fieldPath, pointer) {
  // `pointer` will be something like "/occupancy" or
  // "/temperature/temperatureDelta". Splitting by "/" will give us an empty
  // string at the start so we discard that.
  const pointerParts = pointer.split('/').slice(1)
  return _.isEqual(pointerParts, fieldPath.slice(0, pointerParts.length))
}

function GroupedProperties({
  control,
  expandedModel,
  conflictedFields,
  groups,
  isEditing,
  isExpanded,
  dirtyFields,
}) {
  const { versionHistory } = useTwinEditor()
  const { versionHistoryEditedFields } = versionHistory

  // Make sure the HTML ids we output are unique.
  const idPrefix = useRef(uuidv4()).current

  return (
    <>
      {Object.entries(groups).map(([groupName, properties]) => {
        // Exception is being made for twin with warranty where twin's
        // customProperties only contains "Warranty". "customProperties"
        // GroupHeading will not be displayed, along with the "Warranty"
        // PropertiesFields.
        if (_.isEqual(Object.keys(properties), ['Warranty'])) {
          return null
        }

        const groupIsEmpty = isEmpty(_.omit(properties, ['$metadata']))

        // If not expanded, only display groups that are not empty (excluding
        // $metadata).
        if (
          !isExpanded &&
          groupIsEmpty &&
          _.get(versionHistoryEditedFields, groupName) === undefined
        ) {
          return null
        }

        // Components are not editable, and hence if they are empty, we do not
        // display them at all.
        let readOnly = false
        if (
          expandedModel.contents.find((c) => c.name === groupName)?.[
            '@type'
          ] === 'Component'
        ) {
          if (groupIsEmpty) {
            return null
          }
          readOnly = true
        }

        return (
          <div
            data-testid="tab-summary-other"
            tw="mt-8"
            key={`${idPrefix}-${groupName}`}
          >
            <GroupHeading>
              {getDisplayName(expandedModel, [groupName])}
            </GroupHeading>
            <PropertyFields
              properties={properties}
              expandedModel={expandedModel}
              conflictedFields={conflictedFields}
              groupName={groupName}
              idPrefix={idPrefix}
              isEditing={isEditing}
              isExpanded={isExpanded}
              control={control}
              controllerNamePrefix={[groupName]}
              dirtyFields={dirtyFields}
              readOnly={readOnly}
            />
          </div>
        )
      })}
    </>
  )
}

function Properties({
  control,
  expandedModel,
  properties,
  conflictedFields,
  isEditing,
  isExpanded,
  dirtyFields,
}) {
  // Make sure the HTML ids we output are unique.
  const idPrefix = useRef(uuidv4()).current

  /**
    First, display all the nonempty properties. Then, if expanded, display
    all the empty properties. Do this separately so that hitting "Show more"
    doesn't cause a bunch of properties to appear in the middle of the
    properties that are already there.
  */
  return (
    <PropertyFields
      properties={properties}
      idPrefix={idPrefix}
      isEditing={isEditing}
      isExpanded={isExpanded}
      conflictedFields={conflictedFields}
      control={control}
      controllerNamePrefix={[]}
      expandedModel={expandedModel}
      dirtyFields={dirtyFields}
    />
  )
}
