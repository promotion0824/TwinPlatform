import { useEffect, useState } from "react";
import { useMsal, useAccount } from "@azure/msal-react";

export const useUserInfo = () => {
  //Get the currently logged on accounts and take the first one
  const { accounts } = useMsal();
  const helperAccount = useAccount(accounts[0] || {});

  const [userName, setUserName] = useState("");
  const [userEmail, setUserEmail] = useState("");

  //Update the account details
  useEffect(() => {
    setUserName(helperAccount?.name ?? "");
    setUserEmail(
      helperAccount?.username ||
        (helperAccount?.idTokenClaims?.["email"] as string) ||
        ""
    );
  }, [helperAccount]);

  return { userName, userEmail };
};

export default useUserInfo;
