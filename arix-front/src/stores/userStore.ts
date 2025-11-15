import { create } from "zustand";

export type UserProperties = {
  id: string | typeof UNKNOWN_ID;
  email: string | typeof UNKNOWN_EMAIL;
  picture: string | typeof UNKNOWN_PICTURE;
  name: string | typeof UNKNOWN_NAME;
};
export type UserState = UserProperties & {
  setUser: (user: UserProperties) => void;
  // TODO
  // character: Character
};

const UNKNOWN_ID = "unknown-id" as const;
const UNKNOWN_EMAIL = "unknown-email" as const;
const UNKNOWN_PICTURE = "unknown-picture" as const;
const UNKNOWN_NAME = "unknown-name" as const;

export const useUserStore = create<UserState>((set) => ({
  id: UNKNOWN_ID,
  email: UNKNOWN_EMAIL,
  picture: UNKNOWN_PICTURE,
  name: UNKNOWN_NAME,
  setUser: (user) => {
    set({ ...user });
  }
}));
