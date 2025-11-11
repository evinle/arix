import { useQuery } from "@tanstack/react-query";
import {
  Route,
  Routes,
  useLocation,
  useNavigate
} from "react-router";
import type { Weapon } from "./apiTypes/weapons.type";
import Background from "./components/Containers/Background";
import CenterOnContainer from "./components/Containers/CenterOnContainer";
import Menu from "./components/Menu/Menu";
import MenuItem from "./components/Menu/MenuItem";
import type { MenuItemConfig } from "./components/Menu/MenuItem.type";
import { useEffect } from "react";

const GetJWT: React.FC = () => {
  const navigate = useNavigate();
  const { search } = useLocation();
  const queryParams = new URLSearchParams(search);

  // Example: get "code" param
  const jwt = queryParams.get("code");

  console.log("jwt", jwt);

  if (jwt) localStorage.setItem("jwt", jwt);
  navigate("/");
  return <></>;
};

function App() {
  const navigate = useNavigate();
  const MenuItems: MenuItemConfig[] = [
    {
      id: "start",
      label: "Start Game",
      onClick: () => navigate("/game")
    },
    {
      id: "add-friends",
      label: "Add Friends",
      onClick: () =>
        console.log("Open Dialogue for new friends")
    },
    {
      id: "get-weapons",
      label: "Debug Weapons",
      onClick: () => refetch()
    },
    {
      id: "login",
      label: "Test Login",
      onClick: () => {
        window.location.href =
          "http://localhost:5115/oauth";
      }
    },
    {
      id: "exit",
      label: "Exit",
      onClick: () => console.log("Exiting")
    }
  ];
  const {
    data: weaponsQuery,
    refetch,
    isPending,
    isFetching,
    isError
  } = useQuery({
    queryKey: [],
    queryFn: async () => {
      const res = await fetch(
        "http://localhost:5115/Weapons/GetAllWeapons"
      );
      if (res.ok) return (await res.json()) as Weapon[];
      throw new Error("failed to fetch");
    },
    staleTime: () => 1,
    gcTime: 0,
    enabled: false
  });

  const { search } = useLocation();
  const queryParams = new URLSearchParams(search);

  // Example: get "code" param
  const jwt = queryParams.get("code");

  useEffect(() => {
    console.log("jwt", jwt, localStorage.getItem("jwt"));
  }, [jwt]);

  if (jwt) localStorage.setItem("jwt", jwt);
  function renderDebugWeapons() {
    if (isPending && isFetching)
      return <div>Loading...</div>;
    if (isError)
      return <div>Something went wrong :(...</div>;
    return (
      <ul>
        {weaponsQuery?.map((w) => (
          <li key={w.id}>{JSON.stringify(w)}</li>
        ))}
      </ul>
    );
  }

  const { data, isLoading } = useQuery({
    queryKey: ["me"],
    queryFn: async () =>
      (
        await fetch("http://localhost:5115/me", {
          headers: {
            Authorization:
              "Bearer " + localStorage.getItem("jwt")
          }
        })
      ).json()
  });

  const routes = (
    <Routes>
      <Route
        path="/"
        element={
          <CenterOnContainer className={`flex-col`}>
            {isLoading
              ? "Loading user info"
              : JSON.stringify(data)}
            {renderDebugWeapons()}
            <Menu items={MenuItems}></Menu>
          </CenterOnContainer>
        }
      ></Route>

      <Route
        path="/game"
        element={
          <CenterOnContainer className={`flex-col`}>
            Game Page
            <MenuItem
              config={{
                id: "back",
                label: "Back",
                onClick: () => navigate("/")
              }}
            ></MenuItem>
          </CenterOnContainer>
        }
      ></Route>
      <Route
        path="/jwtCallback"
        element={<GetJWT />}
      ></Route>
    </Routes>
  );

  return (
    <Background className="font-poppins">
      {routes}
    </Background>
  );
}

export default App;
