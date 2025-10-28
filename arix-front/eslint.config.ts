import pluginReact from "eslint-plugin-react";
import tseslint from "typescript-eslint";
import js from "@eslint/js";
import eslintParserTypeScript from "@typescript-eslint/parser";
import eslintPluginBetterTailwindcss from "eslint-plugin-better-tailwindcss";
import reactRefresh from "eslint-plugin-react-refresh";
import reactHooks from "eslint-plugin-react-hooks";
import { defineConfig } from "eslint/config";

import globals from "globals";

export default defineConfig([
  {
    files: ["**/*.{ts,tsx,cts,mts}"],
    extends: [
      reactRefresh.configs.vite,
      reactHooks.configs["recommended-latest"]
    ],
    languageOptions: {
      parser: eslintParserTypeScript
    },
    rules: {
      "no-unused-vars": "warn"
    }
  },

  {
    files: ["**/*.{jsx,tsx}"],
    languageOptions: {
      parserOptions: {
        ecmaFeatures: {
          jsx: true
        }
      }
    },
    plugins: {
      "better-tailwindcss": eslintPluginBetterTailwindcss
    },

    rules: {
      // enable all recommended rules to report a warning
      ...eslintPluginBetterTailwindcss.configs[
        "recommended-warn"
      ].rules,
      // enable all recommended rules to report an error
      ...eslintPluginBetterTailwindcss.configs[
        "recommended-error"
      ].rules,
      // or configure rules individually
      "better-tailwindcss/enforce-consistent-line-wrapping":
        [
          "warn",
          {
            printWidth: 60,
            lineBreakStyle: "windows"
          }
        ]
    },

    settings: {
      "better-tailwindcss": {
        entryPoint: "src/main.css"
      }
    }
  },
  {
    files: ["**/*.{js,mjs,cjs,ts,mts,cts,jsx,tsx}"],
    plugins: { js },
    extends: ["js/recommended"],
    languageOptions: { globals: globals.browser }
  },
  // tseslint.configs.recommended,
  {
    ...tseslint.configs.eslintRecommended
  },
  pluginReact.configs.flat.recommended
]);
