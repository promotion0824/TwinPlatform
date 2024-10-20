import { useMsal } from "@azure/msal-react";
import { Avatar, Menu, Icon } from "@willowinc/ui";
import useUserInfo from "../hooks/useUserInfo";
import { SilentRequest } from "@azure/msal-browser";
import styled from "styled-components";
//import { useQuery } from "@tanstack/react-query";
import { getInitials } from "../utils/getInitials";

/**
 * UserIcon represents the logged in user with options to login or out or a logged out user
 */
const UserIcon = () => {
  const { instance, accounts } = useMsal();
  const { userName } = useUserInfo();

  const logout = async () => {
    if (localStorage.getItem("token")) localStorage.removeItem("token");
    const logoutRequest = {
      account: accounts[0],
    };
    instance.logoutRedirect(logoutRequest);
  };

  const login = async () => {
    await instance.handleRedirectPromise();
    await instance.loginRedirect();
  };

  /*useQuery({
    queryKey: ["token", accounts],
    queryFn: async (_x) => {
      const account = accounts[0];
      const silentRequest: SilentRequest = {
        account: account,
        scopes: ["https://graph.microsoft.com/.default"],
      };
      try {
        return await instance.acquireTokenSilent(silentRequest);
      } catch (e) {
        logout();
      }
      return null;
    }
  });*/

  const isUserLoggedIn = accounts.length > 0;

  if (isUserLoggedIn) {
    return (
      <Menu position="bottom-end" width={200}>
        <Menu.Target>
          <ClickableAvatar>{getInitials(userName)}</ClickableAvatar>
        </Menu.Target>

        <Menu.Dropdown>
          <Menu.Item
            onClick={logout}
            prefix={<Icon icon="info" />}
            intent="negative"
          >
            Log Out
          </Menu.Item>
        </Menu.Dropdown>
      </Menu>
    );
  } else {
    /*@ts-ignore*/
    return <ClickableAvatar onClick={login}>?</ClickableAvatar>;
  }
};

export default UserIcon;

const ClickableAvatar = styled(Avatar)({ "&:hover": { cursor: "pointer" } });
