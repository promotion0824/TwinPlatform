import { LocalPoliceTwoTone } from "@mui/icons-material";
import { Badge, Box, Typography } from "@mui/material";
import { useCallback } from "react";
import { useNavigate } from "react-router-dom";
import { useAccountInfo } from "../Hooks/useAccountInfo";
import { useAuth } from "../Providers/PermissionProvider";
import CustomAvatar from "./CustomAvatar";
import { Icon, Menu } from "@willowinc/ui";
import { IPublicClientApplication } from "@azure/msal-browser/dist/app/IPublicClientApplication";
import { useMsal } from "@azure/msal-react";
import { msalConfig } from "../authConfig";

export default function UserProfileBar() {
  const { account } = useAccountInfo();
  const navigate = useNavigate();
  const authData = useAuth();

  // useMsal hook will return the PublicClientApplication instance you provided to MsalProvider
  const { instance } = useMsal();

  const signOutClickHandler = (instance: IPublicClientApplication) => {

    const logoutRequest = {
      account: account,
      postLogoutRedirectUri: msalConfig.auth.redirectUri,
    };
    instance.logoutRedirect(logoutRequest);
  }


  return (
    <Box sx={{ flexGrow: 0 }}>
      <Menu>
        <Menu.Target>
          <Badge
            style={{ cursor: "pointer" }}
            overlap="circular"
            anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
            badgeContent={authData.isAdminUser && <LocalPoliceTwoTone style={{ fontSize: "1rem" }} titleAccess="Super Administrator" />}
          >
            <CustomAvatar key={account.username} name={account.name as string} color="purple" />
          </Badge>
        </Menu.Target>
        <Menu.Dropdown>
          <Menu.Item prefix={<Icon icon="person" />} onClick={useCallback(() => { navigate(`/users/${encodeURIComponent(account.username)}/`); }, [])}>
            <Typography textAlign="center">Profile</Typography>
          </Menu.Item>

          <Menu.Item key="signout" onClick={() => signOutClickHandler(instance)} prefix={<Icon icon="logout" />}>
            Log Out
          </Menu.Item>
        </Menu.Dropdown>
      </Menu>
    </Box>
  );
}
