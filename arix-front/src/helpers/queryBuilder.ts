function mapToEndpoints<
  TPrefix extends string,
  TEndpoints extends readonly string[]
>(prefix: TPrefix, endpoints: TEndpoints) {
  return endpoints.map(
    (endpoint) =>
      `${prefix}${endpoint}` as `${TPrefix}${TEndpoints[number]}`
  );
}

const ARIX_LOGIN_ENDPOINT_PREFIX =
  "/LoginController/" as const;
const ARIX_LOGIN_ENDPOINT_NAMES = [
  "Register",
  "Login",
  "oauth",
  "callback-google",
  "ForgotPassword",
  "me"
] as const;

const ARIX_PLAYER_ENDPOINT_PREFIX = "/players/" as const;
const ARIX_PLAYER_ENDPOINT_NAMES = [
  "GetAllPlayers",
  "GetPlayerFromId",
  "GetPlayerFromUsername",
  "CreatePlayer",
  "UpdatePlayer",
  "RemovePlayer"
] as const;

const ARIX_WEAPONS_ENDPOINT_PREFIX = "/Weapons/" as const;
const ARIX_WEAPONS_ENDPOINT_NAMES = [
  "GetAllWeapons",
  "GetWeapon",
  "CreateWeapon",
  "UpdateWeapon",
  "RemoveWeapon"
] as const;

const ARIX_MATCHMAKING_ENDPOINTS_PREFIX =
  "/Websocket/" as const;
const ARIX_MATCHMAKING_ENDPOINTS_NAMES = [
  "ws",
  "GetAllConnections"
] as const;

const ARIX_LOGIN_ENDPOINTS = mapToEndpoints(
  ARIX_LOGIN_ENDPOINT_PREFIX,
  ARIX_LOGIN_ENDPOINT_NAMES
);
const ARIX_PLAYER_ENDPOINTS = mapToEndpoints(
  ARIX_PLAYER_ENDPOINT_PREFIX,
  ARIX_PLAYER_ENDPOINT_NAMES
);
const ARIX_WEAPONS_ENDPOINTS = mapToEndpoints(
  ARIX_WEAPONS_ENDPOINT_PREFIX,
  ARIX_WEAPONS_ENDPOINT_NAMES
);
const ARIX_MATCHMAKING_ENDPOINTS = mapToEndpoints(
  ARIX_MATCHMAKING_ENDPOINTS_PREFIX,
  ARIX_MATCHMAKING_ENDPOINTS_NAMES
);

export const ARIX_ENDPOINTS = [
  ...ARIX_LOGIN_ENDPOINTS,
  ...ARIX_PLAYER_ENDPOINTS,
  ...ARIX_WEAPONS_ENDPOINTS,
  ...ARIX_MATCHMAKING_ENDPOINTS
] as const;

const ARIX_ENDPOINTS_METHOD_MAP: {
  [endpoint in ArixEndpoint]:
    | "GET"
    | "POST"
    | "PUT"
    | "DELETE";
} = {
  "/LoginController/Register": "POST",
  "/LoginController/Login": "POST",
  "/LoginController/oauth": "GET",
  "/LoginController/callback-google": "GET",
  "/LoginController/ForgotPassword": "POST",
  "/LoginController/me": "GET",

  "/players/GetAllPlayers": "GET",
  "/players/GetPlayerFromId": "GET",
  "/players/GetPlayerFromUsername": "GET",
  "/players/CreatePlayer": "POST",
  "/players/UpdatePlayer": "POST",
  "/players/RemovePlayer": "POST",

  "/Weapons/GetAllWeapons": "GET",
  "/Weapons/GetWeapon": "GET",
  "/Weapons/CreateWeapon": "POST",
  "/Weapons/UpdateWeapon": "POST",
  "/Weapons/RemoveWeapon": "POST",

  "/Websocket/ws": "GET",
  "/Websocket/GetAllConnections": "GET"
};

export type ArixEndpoint = (typeof ARIX_ENDPOINTS)[number];

type QueryFnBuilderOptions = {
  fetchOptions?: RequestInit;
};

export const ARIX_SERVER_ORIGIN = "http://localhost:5115";

export const queryFnBuilder = <TExpextedResult>(
  url: ArixEndpoint,
  queryFnBuilderOptions?: QueryFnBuilderOptions
) => {
  const customFetchOptions = {
    ...queryFnBuilderOptions?.fetchOptions
  };
  const fullUrl = `${ARIX_SERVER_ORIGIN}${url}` as const;

  const fetchOptions: RequestInit = {
    ...customFetchOptions,
    headers: {
      "Content-Type": "application/json",
      ...customFetchOptions.headers,
      Authorization: "Bearer " + localStorage.getItem("jwt")
    },
    credentials: "include",
    method: ARIX_ENDPOINTS_METHOD_MAP[url]
  };

  return async () => {
    const response = await fetch(fullUrl, fetchOptions);
    if (response.ok)
      return (await response.json()) as TExpextedResult;
    throw new Error(
      `Error ${response.statusText} (${response.status}) in Request to ${url}: ${await response.text()}`
    );
  };
};
