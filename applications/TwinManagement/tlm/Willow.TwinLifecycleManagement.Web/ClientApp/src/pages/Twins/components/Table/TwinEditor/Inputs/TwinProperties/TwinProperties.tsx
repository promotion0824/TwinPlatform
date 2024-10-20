import React, { useEffect, useMemo, useState } from 'react';
import { Controller } from 'react-hook-form';
import { styled } from '@mui/material';

import { useTwinEditor } from '../../TwinEditorProvider';
import { TextInput, TextInputProps, DateInputProps } from '@willowinc/ui';
import { filterUndefined, validate, accessValueByKey, getSchemaType } from '../../utils';
import { getField, isEnum, Model, Schema } from '../../../../../../../hooks/useOntology/useOntology';
import { PopUpExceptionTemplate } from '../../../../../../../components/PopUps/PopUpExceptionTemplate';
import CustomTags from './CustomTags';
import {
  FieldContainer,
  GroupPropertyName,
  GroupedPropertyContainer,
  DateInput,
  EnumInput,
  BooleanInput,
  DateTimeInput,
  DurationInput,
  formatDuration,
} from './Components';
import CustomProperties from './CustomProperties';

export default function TwinProperties() {
  const { twin, twinById, expandedModel } = useTwinEditor();
  const [showPopUp, setShowPopUp] = useState<boolean>(false);
  const [errorMessage, setErrorMessage] = useState<string | undefined>(undefined);
  useEffect(() => {
    setShowPopUp(!!errorMessage);
  }, [errorMessage]);

  // twinData comes from ADX
  const { twinData } = twinById || {};
  const { ExportTime, LastUpdateTime, Location: location = {} } = twinData || {};

  const [grouped, properties, customTags] = useMemo(
    () => parseTwinProperties(twin, expandedModel!),
    [twin, expandedModel]
  );

  return (
    <>
      <FieldContainer>
        <Properties properties={properties} setErrorMessage={setErrorMessage} />
        {!!twinData && (
          <>
            <ReadOnlyField name={'Export Time'} value={ExportTime} />
            <ReadOnlyField name={'Last Update Time'} value={LastUpdateTime} />
          </>
        )}
      </FieldContainer>

      {!!twinData && <ReadOnlyGroupedProperties grouped={Object.entries({ Location: location ?? {} })} />}

      {shouldDisplayProperty(expandedModel!, 'customProperties') && (
        <CustomProperties controllerNamePrefix={['customProperties']} />
      )}

      {shouldDisplayProperty(expandedModel!, 'customTags') && (
        <CustomTags customTags={customTags as string[]} controllerNamePrefix={['customTags']} />
      )}

      <GroupedProperties grouped={grouped} controllerNamePrefix={[]} setErrorMessage={setErrorMessage} />

      <PopUpExceptionTemplate isCurrentlyOpen={showPopUp} onOpenChanged={setShowPopUp} errorObj={errorMessage} />
    </>
  );
}

/**
 * Loop through the model's content to check if it contains the property
 * @param expandedModel
 * @param property
 */
function shouldDisplayProperty(expandedModel: Model, property: string) {
  return !!getField(expandedModel, [property]);
}

function Properties({ properties, setErrorMessage }: { properties: any; setErrorMessage: any }) {
  return (
    <>
      {properties?.map(([property, value]: [property: any, value: any]) => (
        <Field key={property} name={property} value={value} setErrorMessage={setErrorMessage} />
      ))}
    </>
  );
}

function GroupedProperties({
  grouped,
  controllerNamePrefix,
  setErrorMessage,
}: {
  grouped: any;
  controllerNamePrefix: string[];
  setErrorMessage: any;
}) {
  const { hiddenFields, isEditing, expandedModel } = useTwinEditor();

  return (
    <>
      {grouped.map(([groupedPropertyName, groupedProperty]: [groupedPropertyName: string, groupedProperty: any]) => {
        if (hiddenFields.includes(groupedPropertyName)) return null;

        //@ts-expect-error
        const displayName = getField(expandedModel!, [groupedPropertyName])?.displayName?.['en'] || groupedPropertyName;

        const filteredGroupedProperty = isEditing ? groupedProperty : filterUndefined(groupedProperty);
        return (
          <>
            {(isEditing || Object.keys(filteredGroupedProperty).filter((key) => key !== '$metadata').length > 0) && (
              <GroupedPropertyContainer key={groupedPropertyName}>
                <GroupPropertyName>
                  {displayName}

                  {Object.keys(filteredGroupedProperty).filter((key) => key !== '$metadata').length === 0 &&
                    ' (not set)'}
                </GroupPropertyName>
                <FieldContainer>
                  {Object.entries(filteredGroupedProperty).map(([property, value]: [property: any, value: any]) => (
                    <Field
                      key={`${groupedPropertyName} = ${property}}`}
                      name={property}
                      value={value}
                      controllerNamePrefix={[...controllerNamePrefix, groupedPropertyName]}
                      setErrorMessage={setErrorMessage}
                    />
                  ))}
                </FieldContainer>
              </GroupedPropertyContainer>
            )}
          </>
        );
      })}
    </>
  );
}

function parseTwinProperties(twin: any, expandedModel: Model) {
  let { customTags = {}, customProperties, ...rest } = twin || {};

  let grouped = Object.entries(rest).filter(([key, property]) => {
    let schema = getSchemaType(getField(expandedModel, [key])?.schema as Schema);
    return typeof property === 'object' && !(property instanceof Date) && property !== null && schema !== 'duration';
  });

  const _customTags = Object.entries(customTags)
    .filter(([_, value]) => value)
    .map(([key, _]) => key);

  const property = Object.entries(rest).filter(([key, property]) => {
    let schema = getSchemaType(getField(expandedModel, [key])?.schema as Schema);
    return typeof property !== 'object' || property instanceof Date || property === null || schema === 'duration';
  });

  // display model metadata
  const modelMetadata = rest.$metadata?.$model;
  if (modelMetadata) {
    property.splice(1, 0, ['$metadata.$model', modelMetadata]);
  }

  return [grouped, property, (customTags = _customTags)];
}

function Field({
  name,
  value,
  controllerNamePrefix = [],
  setErrorMessage,
}: {
  name: string;
  value: any;
  controllerNamePrefix?: string[];
  setErrorMessage: any;
}) {
  const { isEditing, isSaving, hiddenFields, readOnlyFields, expandedModel, errors } = useTwinEditor();

  const tranlateFieldName = (name: string): string => {
    const map: Record<string, string> = {
      $dtId: 'ID',
      $lastUpdateTime: 'Last Update Time',
      '$metadata.$model': 'Model ID',
    };

    //@ts-expect-error
    const displayName = getField(expandedModel!, path)?.displayName?.['en'] || name;

    let fieldName = map[name] || displayName || name;

    return fieldName;
  };

  // Hide hidden fields
  if (hiddenFields.includes(name)) return null;

  // Hide fields that are not set when in Read mode
  if (!isEditing && value === undefined) return null;

  const isReadOnlyField = readOnlyFields.includes(name);

  let path = [...controllerNamePrefix, name]; // used to identify the (nested) fields in react-hook-form

  let schema: Schema | undefined;
  if (expandedModel && !isReadOnlyField) {
    try {
      schema = getField(expandedModel, path)?.schema as Schema;
    } catch (e) {
      setErrorMessage('Field Parse Error ' + path.join('.') + ' . ' + e);
    }
  }
  const isObjectValue =
    typeof value === 'object' && !(value instanceof Date) && Object.keys(value).length > 0 && schema !== 'duration';

  const isEnumType = isEnum(schema!);
  //@ts-expect-error
  const enumValue = schema?.enumValues?.find(({ enumValue }) => enumValue === value);

  let error = accessValueByKey(errors, path.join('.'));

  let propertyName = tranlateFieldName(name);
  let propertyValue = isObjectValue
    ? parseObjectValue(value)
    : value instanceof Date
    ? value.toLocaleDateString('en-US', {
        month: 'short',
        day: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
        hour12: false,
      })
    : isEnumType
    ? enumValue?.displayName?.en || enumValue?.displayName || enumValue?.name || String(value)
    : schema === 'duration'
    ? formatDuration(value)
    : String(value);

  return (
    <Property>
      <PropertyName title={propertyName}>{propertyName}</PropertyName>
      {isEditing && !isObjectValue ? (
        <>
          <PropertyInput
            controllerName={path.join('.')}
            id={`id-${name}`}
            readOnly={isReadOnlyField || isSaving || isObjectValue}
            schema={schema || 'string'}
            error={error?.message}
          />
        </>
      ) : (
        <PropertyValue title={propertyValue}>{propertyValue}</PropertyValue>
      )}
    </Property>
  );
}

function ReadOnlyField({ name, value }: { name: string; value: any }) {
  return (
    <Property>
      <PropertyName>{name}</PropertyName>
      <PropertyValue>{String(value)}</PropertyValue>
    </Property>
  );
}

function ReadOnlyGroupedProperties({ grouped }: { grouped: any }) {
  return (
    <>
      {grouped.map(
        ([groupedPropertyName, groupedProperty = {}]: [groupedPropertyName: string, groupedProperty: any]) => {
          return (
            <GroupedPropertyContainer key={groupedPropertyName}>
              <GroupPropertyName>{groupedPropertyName}</GroupPropertyName>
              <FieldContainer>
                {Object.entries(groupedProperty).map(([property, value]: [property: any, value: any]) => (
                  <ReadOnlyField key={`${groupedPropertyName} = ${property}}`} name={property} value={value} />
                ))}
              </FieldContainer>
            </GroupedPropertyContainer>
          );
        }
      )}
    </>
  );
}

const parseObjectValue = (obj: any) => {
  let val = JSON.stringify(obj);

  return val;
};

const Property = styled('div')({
  marginBottom: '1rem',
  paddingRight: '2rem',
  minWidth: '10rem',
  maxWidth: '10rem',
  boxSizing: 'content-box',
});

const PropertyName = styled('div')({
  fontFamily: 'Poppins, Arial, sans-serif',
  fontWeight: 400,
  fontSize: '0.75rem',
  lineHeight: '1.25rem',
  color: 'rgb(145, 145, 145)',
});

const PropertyValue = styled('div')({
  color: '#d9d9d9',
  fontFamily: 'Poppins, Arial, sans-serif',
  fontWeight: 400,
  fontSize: '0.75rem',
  lineHeight: '1.25rem',
  overflowWrap: 'break-word',
});

type FieldProps = {
  id: string;
  value: any;
  onChange: (val: any) => void;
  onBlur: () => void;
  readOnly?: boolean;
  schema: any;
  error: string;
  disabled: boolean;
  autocomplete?: string;
};

type WidgetSet = {
  [key: string]:
    | React.FunctionComponent<FieldProps>
    | React.FunctionComponent<TextInputProps>
    | React.FunctionComponent<DateInputProps>;
};

const twinEditorWidgets: WidgetSet = {
  string: TextInput,
  float: TextInput,
  double: TextInput,
  long: TextInput,
  integer: TextInput,
  date: DateInput,
  dateTime: DateTimeInput,
  time: TextInput, // todo: add time input
  duration: DurationInput,
  boolean: BooleanInput,
  Enum: EnumInput,
};

const PropertyInput = ({
  id,
  controllerName,
  readOnly = false,
  schema,
  error,
}: {
  id: string;
  controllerName: string;
  readOnly?: boolean;
  schema: Schema;
  error: string;
}) => {
  const { control } = useTwinEditor();

  const Component = twinEditorWidgets[getSchemaType(schema) || 'string'];

  const rules = {
    validate: (ob: any) => {
      return validate(ob, schema);
    },
  };

  return (
    <Controller
      name={controllerName}
      control={control}
      rules={rules}
      render={({ field }) => <Component {...field} id={id} disabled={readOnly} schema={schema} error={error} />}
    />
  );
};
