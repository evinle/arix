import { useCallback, useEffect, useRef } from "react";

function useInnerLocalStorage<T>(key: string): T;
function useInnerLocalStorage<T>(
  key: string,
  value: T,
  shouldSet: (k: typeof key, v: typeof value) => boolean
): {
  set: (key: string, data: T) => boolean;
  get: (key: string) => T;
  value: T;
};
function useInnerLocalStorage<T>(
  key: string,
  value?: T,
  shouldSet?: (k: typeof key, v: typeof value) => boolean
) {
  const prevVal = useRef<T | null>(null);

  const set = useCallback(
    (key: string, data: T): boolean => {
      try {
        localStorage.setItem(
          key,
          JSON.stringify({ value: data })
        );
      } catch (e) {
        console.error(
          `Error while setting local storage for ${key}: ${e}`
        );
        return false;
      }

      return true;
    },
    []
  );

  const get = useCallback((key: string): T => {
    return JSON.parse(String(localStorage.getItem(key)))
      ?.value satisfies T;
  }, []);

  const inReadonlyMode = !(value && shouldSet);
  useEffect(() => {
    if (inReadonlyMode) return;
    if (prevVal.current != null && prevVal.current == value)
      return;
    if (!shouldSet(key, value)) return;
    set(key, value);
    prevVal.current = value;
  }, [key, value, set, shouldSet, inReadonlyMode]);

  if (inReadonlyMode) return get(key);

  return { set, get, value: get(key) };
}
export const useLocalStorage = useInnerLocalStorage;
