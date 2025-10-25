import React from "react";
import type { MenuItemConfig } from "./MenuItem.type";

type MenuItemProps = {
  config: MenuItemConfig;
};
const MenuItem: React.FC<MenuItemProps> = ({ config }) => {
  return (
    <div
      className="bg-blue-900 rounded-md cursor-pointer active:hover:bg-blue-400 
      active:hover:scale-95 active:hover:translate-0.5 px-2 py-1 transition"
      onClick={config.onClick}
    >
      {config.label}
    </div>
  );
};

export default MenuItem;
