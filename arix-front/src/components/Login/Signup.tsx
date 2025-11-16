import React, { useEffect } from "react";
import { useForm, useWatch } from "react-hook-form";
import CenterOnContainer from "../Containers/CenterOnContainer";
import Menu from "../Menu/Menu";
import AxInput from "../Form/AxInput";
import { useQuery } from "@tanstack/react-query";
import { Link, useNavigate } from "react-router";

type SignupFormInput = {
  username: string;
  password: string;
  email: string;
};

const useSignupQuery = ({
  username,
  password,
  email
}: {
  username: string;
  password: string;
  email: string;
}) =>
  useQuery({
    queryKey: [username, password, email],
    queryFn: async () => {
      return await fetch(
        "http://localhost:5115/LoginController/Register",
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json"
          },
          body: JSON.stringify({
            username: username,
            password: password,
            email: email
          })
        }
      );
    },
    enabled: false
  });
const Signup: React.FC = () => {
  const signupForm = useForm<SignupFormInput>();
  const { control, handleSubmit, setError } = signupForm;
  const [username, password, email] = useWatch({
    control,
    name: ["username", "password", "email"]
  });
  const navigate = useNavigate();

  const formTopLevelErrors =
    signupForm.formState.errors.root;
  const { refetch: signup, data: signupData } =
    useSignupQuery({
      username,
      password,
      email
    });

  useEffect(() => {
    if (!signupData) return;
    if (signupData.ok) navigate("/login");
    if (signupData.status >= 500)
      setError("root", {
        message: `Something went wrong with the signup ${signupData.statusText}`
      });
  }, [signupData, navigate, setError]);

  return (
    <CenterOnContainer>
      <Menu
        className={`
          justify-evenly gap-6
          ${formTopLevelErrors ? `relative outline-red-600` : ""}
        `}
      >
        <form
          className="contents w-full"
          onSubmit={handleSubmit(() => signup())}
        >
          <div className="grid w-1/3 grid-cols-1 gap-4">
            <AxInput
              control={control}
              name={"username"}
              label="Username"
              rules={{
                required: "Username is required",
                minLength: {
                  message: "Username must be longer than 3",
                  value: 3
                }
              }}
            />
            <AxInput
              control={control}
              name={"password"}
              label="Password"
              rules={{ required: "Password is required" }}
            />
            <AxInput
              control={control}
              name={"email"}
              label="Email"
              type="email"
              rules={{ required: "Email is required" }}
            />
          </div>

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
              Sign Up
            </span>
          </button>
          <div className="flex gap-2 text-sm">
            Already have an account?
            <Link to={"/login"} className="underline">
              Login
            </Link>
          </div>
        </form>
      </Menu>
    </CenterOnContainer>
  );
};

export default Signup;
