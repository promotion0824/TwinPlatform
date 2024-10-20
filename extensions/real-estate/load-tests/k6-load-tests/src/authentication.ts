import { check } from "k6";
import http from "k6/http";
import { SharedArray } from "k6/data";

interface User {
  username: string;
  password: string | undefined | null
}

// Get the users token once to avoid rate limiting from B2C
export const users = new SharedArray("users", function () {

  const users:User[] = JSON.parse(open(__ENV.USER_FILE || './users.json'));
  return users
  
});

export function getAllUserLogin() {
  return users.map((u) => login(u.username, u.password));
}

export function login(
  username: string | undefined | null,
  password: string | undefined | null
) {
  const authUrl = "https://willowdevb2c.b2clogin.com/willowdevb2c.onmicrosoft.com/B2C_1A_SeamlessMigration_ROPC/oauth2/v2.0/token";

  const params = {
    grant_type: "password",
    username: username || __ENV.USER_DEFAULT_NAME,
    password: password || __ENV.USER_DEFAULT_PASSWORD,
    scope: __ENV.SCOPE,
    client_id: __ENV.CLIENT_ID,
    response_type: "token id_token",

    headers: {
      "Content-Type": "application/json",
    },
    tags: {
      name: "login",
    },
  };

  let login_response = http.post(authUrl, params);
  console.log(login_response);
  check(login_response, {
    "is status 200": (r) => r.status === 200,
    "has access token returned": (r) =>
      (r?.json() ?? {}).hasOwnProperty("access_token"),
  });

  const data = {
    params: {
      headers: {
        "Content-Type": "application/json",
        // @ts-ignore
        Authorization: "Bearer " + login_response.json().access_token,
      },
    },
  };
  return data;
}
