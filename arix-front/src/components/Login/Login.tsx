import { useState } from "react";
import CenterOnContainer from "../Containers/CenterOnContainer";
import Menu from "../Menu/Menu";
import MenuItem from "../Menu/MenuItem";
import { Controller, useForm } from "react-hook-form";
import AxInput from "../Form/AxInput";

type LoginProps = {};
type LoginFormInput = {
  username: string;
  password: string;
};

const className = {
  input:
    "rounded-md text-lg outline-1 outline-pink-300 px-2",
  textfieldContainer:
    "flex justify-start items-center gap-4"
};

const Login: React.FC<LoginProps> = () => {
  const { handleSubmit, control } =
    useForm<LoginFormInput>();

  return (
    <CenterOnContainer>
      <Menu className="justify-evenly gap-6">
        <form
          className="contents"
          onSubmit={handleSubmit(({ username, password }) =>
            window.alert(`${username} ${password}`)
          )}
        >
          <AxInput
            key={"username"}
            control={control}
            name="username"
            rules={{
              required: "Username is required",
              minLength: {
                message:
                  "Username has to be more than 3 characters",
                value: 3
              }
            }}
          />
          <AxInput
            key={"password"}
            control={control}
            name="password"
            type={"password"}
            rules={{ required: "Password is required" }}
          />

          <button
            type="submit"
            className={`
              flex min-w-1/3 cursor-pointer items-center
              justify-center rounded-xl bg-blue-900 p-2
              text-4xl font-bold uppercase transition
              select-none
              active:hover:translate-0.5
              active:hover:scale-95 active:hover:bg-gray-900
              active:hover:outline-1
              active:hover:outline-white
            `}
          >
            <span
              className={`
                transition
                active:scale-110
              `}
            >
              Login
            </span>
          </button>
        </form>
      </Menu>
    </CenterOnContainer>
  );
};

export default Login;
