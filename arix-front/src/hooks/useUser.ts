import { useQuery } from "@tanstack/react-query";
import { useLocalStorage } from "./useLocalStorage";
import type { UserProperties } from "../stores/userStore";

export const useUser = (): {
  isLoading: boolean;
  isError: boolean;
  user: UserProperties;
} => {
  const { value: jwtInLocalStorage, unset } =
    useLocalStorage<string>("jwt");

  const { data, isLoading, isError } = useQuery<{
    id: string;
    email: string;
    userName: string;
    picture: string;
  }>({
    queryKey: ["me", jwtInLocalStorage],
    queryFn: async () => {
      const meQueryResult = await fetch(
        "http://localhost:5115/me",
        {
          headers: {
            Authorization: "Bearer " + jwtInLocalStorage
          }
        }
      );

      if (
        jwtInLocalStorage != null &&
        meQueryResult.status == 401
      ) {
        // expired token
        unset();
      }

      return meQueryResult.json();
    },
    enabled: jwtInLocalStorage != null
  });

  return {
    isLoading,
    isError,
    user: {
      id: data?.id ?? "",
      email: data?.email ?? "",
      name: data?.userName ?? "",
      picture: data?.picture ?? ""
    }
  };
};
