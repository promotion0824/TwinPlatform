import { useTwinEditor } from '../../TwinEditorProvider';
import { InputGroup, Button, TextInput } from '@willowinc/ui';
import { GroupPropertyName, GroupedPropertyContainer } from './Components';
import { useEffect, useState } from 'react';
import { Pill } from '@mantine/core';
import styled from '@emotion/styled';

export default function CustomTags({
  customTags = [],
  controllerNamePrefix = [],
}: {
  customTags: string[];
  controllerNamePrefix: string[];
}) {
  const { isEditing, setReactHookFormValue, isSaving } = useTwinEditor();
  const tagsState = useState(new Set(customTags));

  // initialize tags state every rerender or when customTags change
  useEffect(() => {
    tagsState[1](new Set(customTags));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [customTags]);

  const inputErrorState = useState<string>();

  const addTag = () => {
    const tag = inputValueState[0];
    if (!tag || tag === '') return;

    // validation
    if (tagsState[0].has(tag)) {
      inputErrorState[1]('Tag already exists');
      return;
    }
    if (tag.includes(' ')) {
      inputErrorState[1]('Tag cannot have spaces');
      return;
    }

    tagsState[1]((prev) => new Set([...prev, tag]));
    setReactHookFormValue(`${controllerNamePrefix}.${tag}`, true);
    inputValueState[1]('');
  };

  const removeTag = (tag: string) => {
    tagsState[1]((prev) => {
      prev.delete(tag);
      return new Set([...prev]);
    });
    setReactHookFormValue(`${controllerNamePrefix}.${tag}`, undefined);
  };
  const inputValueState = useState<string>('');

  const handleKeyDown = (event: React.KeyboardEvent<HTMLInputElement>) => {
    if (event.key === 'Enter') {
      addTag();
    }
  };

  return (
    <>
      {(customTags.length > 0 || isEditing) && (
        <GroupedPropertyContainer key={'customTags'}>
          <GroupPropertyName>{'Custom Tags'}</GroupPropertyName>
          <FlexColumn>
            {isEditing && (
              <PaddingTopInputGroup>
                <TextInput
                  placeholder="Add custom tag"
                  value={inputValueState[0]}
                  onChange={(event) => {
                    inputValueState[1](event.currentTarget.value);
                    if (inputErrorState[0]) inputErrorState[1](undefined);
                  }}
                  onKeyDown={handleKeyDown}
                  error={inputErrorState[0]}
                  readOnly={isSaving}
                />
                <Button kind="secondary" onClick={addTag} disabled={inputValueState[0] === '' || isSaving}>
                  Add
                </Button>
              </PaddingTopInputGroup>
            )}

            <TagsContainer>
              {[...tagsState[0]].map((tag: string) => (
                <Pill
                  key={tag}
                  onRemove={() => {
                    removeTag(tag);
                  }}
                  withRemoveButton={isEditing && !isSaving}
                >
                  {tag}
                </Pill>
              ))}
            </TagsContainer>
          </FlexColumn>
        </GroupedPropertyContainer>
      )}
    </>
  );
}

const TagsContainer = styled('div')({
  display: 'flex',
  flexDirection: 'row',
  gap: 8,
  flexWrap: 'wrap',
  paddingTop: 8,
});

const FlexColumn = styled('div')({ paddingLeft: '1rem', display: 'flex', flexDirection: 'column' });

const PaddingTopInputGroup = styled(InputGroup)({ paddingTop: 8 });
