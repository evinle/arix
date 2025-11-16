import { useForm, useWatch } from "react-hook-form";
import CenterOnContainer from "../Containers/CenterOnContainer";
import AxInput from "../Form/AxInput";
import Menu from "../Menu/Menu";
import { useQuery } from "@tanstack/react-query";
import { useLocalStorage } from "../../hooks/useLocalStorage";
import { useEffect } from "react";
import { useNavigate } from "react-router";

type LoginProps = {};
type LoginFormInput = {
  username: string;
  password: string;
};

const useLoginQuery = ({
  username,
  password
}: {
  username: string;
  password: string;
}) =>
  useQuery({
    queryKey: [username, password],
    queryFn: async () => {
      return await fetch(
        "http://localhost:5115/LoginController/Login",
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json"
          },
          body: JSON.stringify({
            username: username,
            password: password,
            email: ""
          })
        }
      );
    },
    enabled: false
  });
const Login: React.FC<LoginProps> = () => {
  const {
    handleSubmit,
    control,
    watch,
    clearErrors,
    formState: {
      errors: { root: formTopLevelErrors }
    }
  } = useForm<LoginFormInput>();
  const navigate = useNavigate();
  const [username, password] = useWatch({
    control,
    name: ["username", "password"]
  });

  const { refetch: login, data: loginState } =
    useLoginQuery({
      username: username,
      password: password
    });
  useEffect(() => {
    clearErrors();
  }, [username, password, clearErrors]);

  useEffect(() => {
    if (!loginState) return;

    if (loginState.ok) navigate("/");

    if (loginState.status == 401)
      control.setError("root", {
        message: "Username or Password Incorrect"
      });
  }, [loginState, navigate, control]);

  return (
    <CenterOnContainer>
      <Menu
        className={`
          justify-evenly gap-6
          ${formTopLevelErrors ? `relative outline-red-600` : ""}
        `}
      >
        {formTopLevelErrors && (
          <sub
            className={`
              absolute top-3 h-fit max-h-max text-sm
              text-red-700
            `}
          >
            {formTopLevelErrors.message}
          </sub>
        )}
        <form
          className="contents"
          onSubmit={handleSubmit(() => login())}
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
