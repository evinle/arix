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
import TopBar from "./components/TopBar/TopBar";
import { useLocalStorage } from "./hooks/useLocalStorage";
import { useEffect } from "react";
import { useUser } from "./hooks/useUser";

const GetJWT: React.FC = () => {
  const navigate = useNavigate();
  const { search } = useLocation();
  const queryParams = new URLSearchParams(search);

  // Example: get "code" param
  const jwt = queryParams.get("code");

  const { value } = useLocalStorage("jwt", jwt);

  useEffect(() => {
    if (value != null) navigate("/");
  }, [value, navigate]);

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

  const { isLoading, user } = useUser();

  const routes = (
    <Routes>
      <Route
        path="/"
        element={
          <CenterOnContainer className={`flex-col`}>
            {isLoading
              ? "Loading user info"
              : JSON.stringify(user)}
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
      <TopBar />
      {routes}
    </Background>
  );
}

export default App;
