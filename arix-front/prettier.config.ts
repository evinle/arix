import { type Config } from "prettier";

const config: Config = {
  trailingComma: "none",
  printWidth: 60,
  plugins: ["prettier-plugin-tailwindcss", "prettier-plugin-organize-imports"],
};

export default config;
