import { useNavigate } from 'react-router-dom';
import { AuthHandler } from '../../../components/AuthHandler';
import { AppPermissions } from '../../../AppPermissions';
import { Button } from '@willowinc/ui';
import ApproveAcceptNavButton from './Buttons/ApproveAcceptNavButton';
import DeleteTwinsButton from './Buttons/DeleteTwinsButton';
import { configService } from '../../../services/ConfigService';
import styled from '@emotion/styled';

/**
 * Buttons used to navigate to the different twin actions pages
 */
export const LeftTwinsActionsNavBarButtons = () => {
  const navigate = useNavigate();

  return (
    <>
      <AuthHandler requiredPermissions={[AppPermissions.CanImportTwins]}>
        <Button kind="primary" onClick={() => navigate('../import-twins')}>
          Import
        </Button>
      </AuthHandler>

      <AuthHandler requiredPermissions={[AppPermissions.CanExportTwins]}>
        <Button kind="primary" onClick={() => navigate('../export-twins')}>
          Export
        </Button>
      </AuthHandler>

      {!configService.config.mtiOptions.isMappedDisabled && (
        <AuthHandler requiredPermissions={[AppPermissions.CanReadMappings]}>
          <ApproveAcceptNavButton />
        </AuthHandler>
      )}
    </>
  );
};

export const RightTwinsActionsNavBarButtons = () => {
  const navigate = useNavigate();

  return (
    <ActionsButtonsContainer>
      {/* flex direction is row-reverse so gap is removed from last button on right end side.  */}
      <AuthHandler requiredPermissions={[AppPermissions.CanDeleteTwins]}>
        <DeleteTwinsButton />
      </AuthHandler>

      <AuthHandler requiredPermissions={[AppPermissions.CanDeleteTwinsorRelationshipByFile]}>
        <Button kind="negative" onClick={() => navigate('../delete-file-twins')}>
          Delete from File
        </Button>
      </AuthHandler>

      <AuthHandler requiredPermissions={[AppPermissions.CanDeleteTwins]}>
        <Button kind="negative" onClick={() => navigate('../delete-siteId-twins')}>
          Delete by Location
        </Button>
      </AuthHandler>

      <AuthHandler requiredPermissions={[AppPermissions.CanDeleteAllTwins]}>
        <Button kind="negative" onClick={() => navigate('../delete-all-twins')}>
          Delete All
        </Button>
      </AuthHandler>
    </ActionsButtonsContainer>
  );
};

const ActionsButtonsContainer = styled('div')({
  display: 'flex',
  flexDirection: 'row',
  gap: 8,
  flexFlow: 'row-reverse',
  flexWrap: 'wrap-reverse',
});
