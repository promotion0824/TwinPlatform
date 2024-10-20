import { styled, CircularProgress } from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import CheckIcon from '@mui/icons-material/Check';
import ClearIcon from '@mui/icons-material/Clear';
import TwinEditorProvider, { useTwinEditor } from './TwinEditorProvider';
import TwinProperties from './Inputs/TwinProperties/TwinProperties';
import { useGridApiRef } from '@mui/x-data-grid-pro';

export default function TwinEditor({
  twinData = {},
  twinId,
  noEdit = false,
  apiRef,
}: {
  twinData?: any;
  twinId?: string;
  noEdit?: boolean;
  apiRef?: ReturnType<typeof useGridApiRef>;
}) {
  return (
    <TwinEditorProvider twinData={twinData} twinId={twinId} apiRef={apiRef}>
      <>
        {!noEdit && <EditButton />}
        <OverflowWrap>
          <TwinProperties />
        </OverflowWrap>
      </>
    </TwinEditorProvider>
  );
}

const OverflowWrap = styled('div')({ maxHeight: '35vh', overflowY: 'auto', margin: '0.5rem' });

/**
 * This is a button that changes the form to "editing" state when clicked.
 * When in "editing" state, two more button appears: Save and cancel.
 *  - When Save button is clicked, form is submitted and enters "saving" state.
 *      - Once save is successful, form enters back to default "read" state.
 *  - When Cancel button is clicked, form enter back to default "read" state.
 */
function EditButton() {
  const { isSaving, isEditing, enableEditMode, cancel, submit } = useTwinEditor();

  return (
    <Container>
      <ButtonsContainer>
        {/* Edit button */}
        <BoxShadowRight>
          <Button onClick={enableEditMode} title="Edit twin properties" disabled={isSaving}>
            <EditIcon sx={isEditing ? { ...iconStyle, color: '#5340D6' } : iconStyle} />
          </Button>
        </BoxShadowRight>

        {isSaving ? (
          <Button title="Saving">
            <div>
              <CircularProgress size={15} sx={{ marginTop: '5px !important' }} />
            </div>
          </Button>
        ) : isEditing ? (
          <>
            {/* Save button */}
            <Button onClick={submit} type="submit" title="Save">
              <CheckIcon sx={iconStyle} />
            </Button>

            {/* Cancel button */}
            <Button onClick={cancel} title="Cancel">
              <ClearIcon sx={iconStyle} />
            </Button>
          </>
        ) : null}
      </ButtonsContainer>
    </Container>
  );
}

const Container = styled('div')({ padding: '0.2rem 0 0 1rem' });

const iconStyle = { color: '#a4a5a6', height: 18, width: 18 };

const ButtonsContainer = styled('div')({
  display: 'inline',
  boxShadow: '5px 6px 6px #00000029',
});

const Button = styled('button')({
  width: 28,
  height: 24,
  backgroundColor: '#2b2b2b',
  display: 'inline-flex',
  alignItems: 'center',
  justifyContent: 'center',
  outline: 0,
  padding: 0,
  userSelect: 'none',
  transition: 'all 0.2s ease',
  border: 0,
});

// Makes the pencil button cast a shadow on top of the button to the right
const BoxShadowRight = styled('div')({
  display: 'inline-block',
  boxShadow: '#00000029 0px 0px 6px',
  clipPath: 'inset(0px -3px 0px 0px)',
});
