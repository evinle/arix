import React from "react";

import type { MenuItemConfig } from "./MenuItem.type";

type MenuItemProps = {
  config: MenuItemConfig;
};

const MenuItem: React.FC<MenuItemProps> = ({ config }) => {
  return (
    <div
      className={`
        flex min-w-1/3 cursor-pointer items-center
        justify-center rounded-xl bg-blue-900 px-2 py-1
        transition select-none
        active:hover:translate-0.5 active:hover:scale-95
        active:hover:bg-gray-900 active:hover:outline-1
        active:hover:outline-white
      `}
      onClick={config.onClick}
    >
      {config.label}
    </div>
  );
};

export default MenuItem;
