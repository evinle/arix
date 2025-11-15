import { create } from "zustand";

export type UserProperties = {
  id: string | typeof UNKNOWN_ID;
  email: string | typeof UNKNOWN_EMAIL;
};
export type UserState = UserProperties & {
  setUser: (user: UserProperties) => void;
  // TODO
  // character: Character
};

const UNKNOWN_ID = "unknown-id" as const;
const UNKNOWN_EMAIL = "unknown-email" as const;

export const useUserStore = create<UserState>((set) => ({
  id: UNKNOWN_ID,
  email: UNKNOWN_EMAIL,
  setUser: (user) => {
    set({ ...user });
  }
}));
