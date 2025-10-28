import Background from "./components/Containers/Background";
import CenterOnContainer from "./components/Containers/CenterOnContainer";
import Menu from "./components/Menu/Menu";
import type { MenuItemConfig } from "./components/Menu/MenuItem.type";

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
    id: "exit",
    label: "Exit",
    onClick: () => console.log("Exiting")
  }
];
function App() {
  return (
    <>
      <Background className="font-poppins">
        <CenterOnContainer className="flex-col">
          <Menu items={MenuItems}></Menu>
        </CenterOnContainer>
      </Background>
    </>
  );
}

export default App;
