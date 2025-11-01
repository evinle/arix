import { useQuery } from "@tanstack/react-query";
import Background from "./components/Containers/Background";
import CenterOnContainer from "./components/Containers/CenterOnContainer";
import Menu from "./components/Menu/Menu";
import type { MenuItemConfig } from "./components/Menu/MenuItem.type";
import type { Weapon } from "./apiTypes/weapons.type";

//   {
//   defaultOptions: {
//     queries: {
//       gcTime: 1000 * 60 * 60 * 24 // 24 hours
//     }
//   }
// }

function App() {
  const MenuItems: MenuItemConfig[] = [
    {
      id: "start",
      label: "Start Game",
      onClick: () => console.log("Matchmaking started")
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
    select: (d) => d,
    enabled: false
  });

  if (isPending && isFetching) return <div>Loading...</div>;
  if (isError) return <div>Something went wrong :(...</div>;

  return (
    <Background className="font-poppins">
      <CenterOnContainer className="flex-col">
        <ul>
          {weaponsQuery?.map((w) => (
            <li key={w.id}>{JSON.stringify(w)}</li>
          ))}
        </ul>
        <Menu items={MenuItems}></Menu>
      </CenterOnContainer>
    </Background>
  );
}

export default App;
