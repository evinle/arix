import React from "react";
import type { MenuItemConfig } from "./MenuItem.type";

type MenuItemProps = {
  config: MenuItemConfig;
};
const MenuItem: React.FC<MenuItemProps> = ({ config }) => {
  return (
    <div
      className="bg-blue-900 rounded-xl cursor-pointer px-2 py-1 transition min-w-1/3 
      flex justify-center items-center select-none
    active:hover:bg-gray-900 focus active:hover:outline-white active:hover:outline-1 
      active:hover:scale-95 active:hover:translate-0.5
      "
      onClick={config.onClick}
    >
      {config.label}
    </div>
  );
};

export default MenuItem;
