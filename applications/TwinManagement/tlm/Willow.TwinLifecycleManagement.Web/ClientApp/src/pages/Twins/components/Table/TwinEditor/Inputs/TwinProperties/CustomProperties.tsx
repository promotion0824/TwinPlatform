import { useTwinEditor } from '../../TwinEditorProvider';
import { Button, TextInput, IconButton, Icon } from '@willowinc/ui';
import { GroupPropertyName, GroupedPropertyContainer, NestedGroupedPropertyContainer } from './Components';
import styled from '@emotion/styled';
import { useFieldArray, Controller, UseFieldArrayRemove, Control, FieldValues, useWatch } from 'react-hook-form';
import { accessValueByKey } from '../../utils';
import { useMemo } from 'react';

export default function CustomProperties({ controllerNamePrefix = [] }: { controllerNamePrefix: any }) {
  const { isEditing, control, isSaving } = useTwinEditor();
  const fieldName = `${controllerNamePrefix.join('.')}`;
  const { fields, append, remove } = useFieldArray({
    name: fieldName,
    control,
    shouldUnregister: true,
  });

  return (
    <>
      {(fields.length > 0 || isEditing) && (
        <>
          <GroupedPropertyContainer key={'customProperties'}>
            <GroupPropertyName>{'Custom Properties'}</GroupPropertyName>
            <FlexColumn>
              {isEditing && !isSaving && (
                <MarginTopButton
                  kind="secondary"
                  prefix={<Icon icon="add" />}
                  onClick={() => {
                    append({ sourceName: undefined, nestedFields: [] });
                  }}
                >
                  Add Custom Property Group
                </MarginTopButton>
              )}

              <FieldsContainer>
                {fields.map((field, index) => (
                  <CustomProperty
                    key={field.id}
                    field={field}
                    fieldName={`${fieldName}.${index}`}
                    index={index}
                    remove={remove}
                  />
                ))}
              </FieldsContainer>
            </FlexColumn>
          </GroupedPropertyContainer>
        </>
      )}
    </>
  );
}

const FieldsContainer = styled('div')({
  display: 'flex',
  flexDirection: 'row',
  gap: 8,
  flexWrap: 'wrap',
  paddingTop: 5,
});

const FlexColumn = styled('div')({ paddingBottom: 3, paddingLeft: '1rem', display: 'flex', flexDirection: 'column' });

const MarginTopButton = styled(Button)({ marginTop: '8px !important' });

function CustomProperty({
  field,
  fieldName,
  index,
  remove: removeObject,
}: {
  field: any;
  fieldName: string;
  index: number;
  remove: UseFieldArrayRemove;
}) {
  const nestedFieldName = `${fieldName}.nestedFields`;

  const { isEditing, control, errors, isSaving } = useTwinEditor();
  const {
    fields: nestedFields,
    append,
    remove: removeNested,
  } = useFieldArray({
    name: nestedFieldName,
    control,
    shouldUnregister: true,
  });

  const error = accessValueByKey(errors, nestedFieldName);

  const formValues = useWatch({ name: 'customProperties', control });

  const countMap = useMemo(() => {
    const existingKeys = formValues?.map((field: { sourceName: string }) => field.sourceName) || [];
    return getCountMap(existingKeys);
  }, [formValues]);

  return (
    <NestedGroupedPropertyContainer isError={!!error?.root?.message}>
      <Section>
        <RowFlex>
          {isEditing ? (
            <FlexSpaceBetween>
              <GroupPropertyName>
                <Controller
                  render={(field) => (
                    <TextInputField {...field} placeholder="Custom Property Name" readOnly={isSaving} />
                  )}
                  name={`${fieldName}.sourceName`}
                  control={control}
                  rules={{
                    required: 'Invalid value',
                    validate: {
                      duplicateKey: (value) => {
                        return countMap.get(value) === 1 || 'Duplicate custom property name';
                      },
                    },
                  }}
                />
              </GroupPropertyName>

              {!isSaving && (
                <IconButton kind="secondary" background="transparent">
                  <Icon
                    icon="close"
                    onClick={() => {
                      removeObject(index);
                    }}
                  />
                </IconButton>
              )}
            </FlexSpaceBetween>
          ) : (
            <GroupPropertyName>{field.sourceName}</GroupPropertyName>
          )}
        </RowFlex>
      </Section>
      <PaddingContainer>
        {nestedFields.length === 0 && (!isEditing || isSaving) && <SpanPaddingLeft> No fields set</SpanPaddingLeft>}
        {error?.root?.message && <ErrorText>{error?.root?.message}</ErrorText>}
        <NestedTable
          fieldName={nestedFieldName}
          control={control}
          fields={nestedFields as NestedField[]}
          remove={removeNested}
        />

        {isEditing && !isSaving && (
          <StyledButton
            prefix={<Icon icon="add" />}
            kind="secondary"
            background="transparent"
            onClick={() => append({ propertyName: undefined, propertyValue: undefined })}
          >
            Add Field
          </StyledButton>
        )}
      </PaddingContainer>
    </NestedGroupedPropertyContainer>
  );
}

const SpanPaddingLeft = styled('span')({ paddingLeft: '0.5rem' });
function getCountMap(arr: string[]) {
  const count = new Map();
  for (const item of arr) {
    count.set(item, (count.get(item) || 0) + 1);
  }
  return count;
}
const ErrorText = styled('span')({ color: '#d77570' });

const FlexSpaceBetween = styled('div')({
  display: 'flex',
  flexDirection: 'row',
  width: '100%',
  justifyContent: 'space-between',
});

const Section = styled('div')({
  display: 'block',
  borderBottom: '0.0625rem solid #3B3B3B',
  margin: 0,
  padding: '0.5rem 0',
  paddingRight: '0.5rem',
});

const RowFlex = styled('div')({
  display: 'flex',
  flexDirection: 'row',
  gap: '1rem',
  alignItems: 'center',
  justifyContent: 'space-between',
});

interface NestedField {
  id: string;
  propertyName: string;
  propertyValue: string;
}

const NestedTable = ({
  fieldName,
  control,
  fields,
  remove,
}: {
  fieldName: string;
  control: Control<FieldValues, object>;
  fields: NestedField[];
  remove: UseFieldArrayRemove;
}) => {
  const { isEditing, isSaving } = useTwinEditor();
  const formValues = useWatch({ name: fieldName, control });

  const countMap = useMemo(() => {
    const existingKeys = formValues?.map((field: { propertyName: string }) => field.propertyName) || [];
    return getCountMap(existingKeys);
  }, [formValues]);
  return (
    <StyledTable>
      <tbody>
        {fields.length !== 0 && (
          <tr>
            <FlexRow>
              <StyledTh>Property Name</StyledTh>
              <StyledTh>Property Value</StyledTh>
              <th></th>
            </FlexRow>
          </tr>
        )}
        {fields.map((field, index) => (
          <tr key={field.id}>
            <FlexRow>
              <StyledTdKey>
                {isEditing ? (
                  <Controller
                    render={(field) => <TextInputField {...field} readOnly={isSaving} />}
                    name={`${fieldName}.${index}.propertyName`}
                    control={control}
                    rules={{
                      required: 'Invalid value',
                      validate: {
                        duplicateKey: (value) => {
                          return !countMap.has(value) || countMap.get(value) === 1 || 'Duplicate property name';
                        },
                        noSpaces: (value) => {
                          return !value.includes(' ') || 'Spaces are not allowed';
                        },
                      },
                    }}
                  />
                ) : (
                  <span title={field.propertyName}>{field.propertyName}</span>
                )}
              </StyledTdKey>
              <StyledTdValue>
                {isEditing ? (
                  <Controller
                    render={(field) => <TextInputField {...field} readOnly={isSaving} />}
                    name={`${fieldName}.${index}.propertyValue`}
                    control={control}
                    rules={{ required: 'Invalid value' }}
                  />
                ) : (
                  <span title={field.propertyValue}>{field.propertyValue}</span>
                )}
              </StyledTdValue>

              <td>
                {isEditing && !isSaving && (
                  <IconButton
                    kind="secondary"
                    background="transparent"
                    onClick={() => {
                      remove(index);
                    }}
                  >
                    <Icon icon="close" />
                  </IconButton>
                )}
              </td>
            </FlexRow>
          </tr>
        ))}
      </tbody>
    </StyledTable>
  );
};

const FlexRow = styled('div')({ display: 'flex', flexDirection: 'row', gap: 5 });

function TextInputField(props: any) {
  const { fieldState, field, placeholder, readOnly } = props;
  return (
    <TextInput
      autoComplete="off"
      {...field}
      error={fieldState?.error?.message}
      placeholder={placeholder}
      disabled={readOnly}
    />
  );
}

const StyledButton = styled(Button)({ marginLeft: '8px !important', marginBottom: '0.5rem !important' });

const tdStyle = {
  minWidth: 150,
  width: 0,
  fontWeight: 500,
  fontSize: '11px',
  textOverflow: 'ellipsis',
  whiteSpace: 'nowrap',
  overflow: 'hidden',
};

const StyledTh = styled('th')({ ...tdStyle, textAlign: 'left', color: '#919191' });
const StyledTdKey = styled('td')({
  ...tdStyle,
  color: '#d9d9d9',
});

const StyledTdValue = styled('td')({ ...tdStyle, color: '#d9d9d9' });
const StyledTable = styled('table')({ borderCollapse: 'separate', borderSpacing: '7px' });

const PaddingContainer = styled('div')({ padding: '0.5rem' });
