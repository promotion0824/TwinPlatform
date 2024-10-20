import { AccountInfo } from "@azure/msal-browser";
import { useAccount, useMsal } from "@azure/msal-react";


export function useAccountInfo(): IAccountInfo {

    const { accounts } = useMsal();
    const accountInfo = useAccount(accounts[0]) as AccountInfo;
    accountInfo.username = accountInfo.username ? accountInfo.username : (accountInfo.idTokenClaims?.['email'] as string);
    return { account: accountInfo };
}

export interface IAccountInfo {
    account: AccountInfo
}
