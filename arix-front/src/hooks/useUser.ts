import { useQuery } from "@tanstack/react-query";
import { useLocalStorage } from "./useLocalStorage";
import type { UserProperties } from "../stores/userStore";

export const useUser = (): {
  isLoading: boolean;
  isError: boolean;
  user: UserProperties;
} => {
  const jwtInLocalStorage = useLocalStorage("jwt");

  const { data, isLoading, isError } = useQuery<{
    id: string;
    email: string;
    userName: string;
    picture: string;
  }>({
    queryKey: ["me", jwtInLocalStorage],
    queryFn: async () =>
      (
        await fetch("http://localhost:5115/me", {
          headers: {
            Authorization: "Bearer " + jwtInLocalStorage
          }
        })
      ).json()
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
