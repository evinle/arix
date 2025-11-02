import { create } from "zustand";
import type { MenuItemConfig } from "../components/Menu/MenuItem.type";

type Path = string;

const HOME_PAGE_MENU_ITEMS: MenuItemConfig[] = [
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
    onClick: () => console.log("Debugging Weapons")
  },
  {
    id: "exit",
    label: "Exit",
    onClick: () => console.log("Exiting")
  }
];

type MenuItemsConfigState = {
  items: MenuItemConfig[];
  onPageUpdate: (path: Path) => void;
};

export const useMenuItems = create<MenuItemsConfigState>(
  (set) => ({
    items: [],
    onPageUpdate: (path: Path) => {
      const itemsForPath = getMenuItemsForPath(path);
      return set(() => ({ items: itemsForPath }));
    }
  })
);

function getMenuItemsForPath(path: Path): MenuItemConfig[] {
  switch (path) {
    case "/":
    default:
      return HOME_PAGE_MENU_ITEMS;
  }
}
