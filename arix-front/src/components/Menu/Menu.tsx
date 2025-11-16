import React from "react";
import MenuItem from "./MenuItem";
import type { MenuItemConfig } from "./MenuItem.type";

type MenuProps = (
  | {
      items: MenuItemConfig[];
    }
  | { children: React.ReactNode }
) & { className?: string };

const Menu: React.FC<MenuProps> = (props) => {
  return (
    <div
      className={`
        flex h-1/4 min-h-fit min-w-1/2 flex-col items-center
        justify-between gap-2 rounded-xl border-2
        border-blue-200 p-3
        ${props.className ?? ""}
      `}
    >
      {"items" in props
        ? props.items.map((itemConfig) => (
            <MenuItem
              key={itemConfig.id}
              config={itemConfig}
            ></MenuItem>
          ))
        : props.children}
    </div>
  );
};

export default Menu;
