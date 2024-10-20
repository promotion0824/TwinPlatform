import { useState } from 'react';
import { styled, Typography, CircularProgress, FormControlLabel, Checkbox } from '@mui/material';
import AlertDialog from '../../../../components/Common/AlertDialog';
import { useTwins } from '../../TwinsProvider';
import { Button, Icon } from '@willowinc/ui';

/**
 * Button for deleting selected twins
 * When clicked, opens a dialog box to confirm deletion
 */
export default function DeleteTwinsButton() {
  const { selectedRows, deleteTwinsMutation } = useTwins();
  const [open, setOpen] = useState(false);

  const handleClose = () => {
    setOpen(false);

    // reset checkbox to default value
    setIncludeRelationships(false);
  };

  const { mutateDeleteTwins, setIncludeRelationships, includeRelationships } = deleteTwinsMutation;
  const { mutate: deleteTwins, isLoading } = mutateDeleteTwins;

  const handleSubmit = async () => {
    await deleteTwins({ twinIds: selectedRows as string[] });
    setOpen(false);

    // reset checkbox to default value
    setIncludeRelationships(false);
  };

  return (
    <>
      <Button
        kind="negative"
        onClick={() => setOpen(true)}
        prefix={
          isLoading ? (
            <CircularProgress
              sx={{
                height: '15px !important',
                width: '15px !important',
                marginTop: '-1px !important',
                marginRight: '1px !important',
              }}
            />
          ) : (
            <StyledIcon icon="info" />
          )
        }
        disabled={selectedRows.length === 0 || isLoading}
      >
        Delete
      </Button>

      {/* confirmation deletion popup */}
      <AlertDialog
        width="532px"
        onClose={handleClose}
        open={open}
        title="Delete Twins"
        titleSx={{ padding: '16px 24px 0 !important' }}
        contentSx={{ padding: '10px 24px', width: '600px' }}
        content={
          <>
            <FormControlLabel
              data-cy="checkBox"
              control={
                <Checkbox
                  defaultChecked={includeRelationships}
                  onChange={(event: React.ChangeEvent<HTMLInputElement>) => {
                    setIncludeRelationships(event.target.checked);
                  }}
                />
              }
              label="Include relationships"
            />
            <Typography sx={{ paddingTop: '5px !important' }}>
              You are about to delete {selectedRows.length} selected twins. Are you sure you want to remove them?
            </Typography>
          </>
        }
        onSubmit={handleSubmit}
        actionButtons={
          <>
            <Button variant="contained" color="secondary" onClick={handleClose}>
              Cancel
            </Button>
            <Button variant="contained" color="error" onClick={handleSubmit} autoFocus>
              Delete
            </Button>
          </>
        }
      />
    </>
  );
}

const StyledIcon = styled(Icon)({ fontVariationSettings: `'FILL' 1,'wght' 400,'GRAD' 200,'opsz' 20 !important` });
