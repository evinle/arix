import React from "react";
import MenuItem from "./MenuItem";
import type { MenuItemConfig } from "./MenuItem.type";

type MenuProps = {
  items: MenuItemConfig[];
};

const Menu: React.FC<MenuProps> = ({ items }) => {
  return (
    <div className="flex flex-col justify-between items-center h-1/4 min-h-36 min-w-1/2 rounded-xl border-2 border-blue-200 p-3 ">
      {items.map((itemConfig) => (
        <MenuItem key={itemConfig.id} config={itemConfig}></MenuItem>
      ))}
    </div>
  );
};

export default Menu;
