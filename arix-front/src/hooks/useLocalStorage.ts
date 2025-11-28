import { useCallback, useEffect } from "react";
import { create } from "zustand";

type LocalStorageState = {
  keyValueMap: Record<string, any>;
  setItem: <T>(key: string, value: T) => void;
  clearItem: (key: string) => void;
};

const essentialStorageItems = ["jwt"];
const fetchFromStorage = (keys: string[]) => {
  return Object.fromEntries(
    keys.map((key) => {
      try {
        const parsedObj: { value: any } = JSON.parse(
          String(localStorage.getItem(key))
        );
        return [key, parsedObj?.value];
      } catch (e) {
        console.error(
          `Failed to get item "${key}" from local storage`,
          e
        );
        return [key, null];
      }
    })
  );
};

const useLocalStorageStore = create<LocalStorageState>(
  (set) => ({
    keyValueMap: fetchFromStorage(essentialStorageItems),
    setItem: (key, value) => {
      set((prev) => ({
        keyValueMap: { ...prev, [key]: value }
      }));
    },
    clearItem: (key) =>
      set((prev) => {
        delete prev.keyValueMap[key];
        return prev;
      })
  })
);

const UNSET_FLAG = "__UNSET_LOCAL_STORAGE_VALUE__" as const;

export const useLocalStorage = <T>(
  key: string,
  value?: T
) => {
  const state = useLocalStorageStore(
    (s) => s.keyValueMap[key] as T
  );
  const setItem = useLocalStorageStore((s) => s.setItem);
  const setState = useCallback(
    (value: T) => setItem(key, value),
    [key, setItem]
  );

  const unsetState = useCallback(
    () => setItem(key, UNSET_FLAG),
    [key, setItem]
  );

  useEffect(() => {
    if (value !== undefined) setState(value);
  }, [key, value, setState]);

  useEffect(() => {
    if (state == UNSET_FLAG) {
      localStorage.removeItem(key);
      setState(undefined as T);
      return;
    }

    localStorage.setItem(
      key,
      JSON.stringify({ value: state })
    );
  }, [key, state, setState]);

  return {
    value: state,
    set: setState,
    unset: unsetState
  };
};
