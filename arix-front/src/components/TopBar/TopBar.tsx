import { useLocation, useNavigate } from "react-router";
import { useLocalStorage } from "../../hooks/useLocalStorage";
import { useUser } from "../../hooks/useUser";

type TopBarProps = {};
const TopBar: React.FC<TopBarProps> = () => {
  const { user } = useUser();
  const { value: jwtToken, unset } =
    useLocalStorage<string>("jwt");
  const isLoggedIn = user != null && jwtToken != null;

  const navigate = useNavigate();
  const location = useLocation();

  const UserAvatar = (
    <div
      className={`
        aspect-square w-14 cursor-pointer rounded-full
        bg-gray-500 outline-2 outline-pink-300
        active:translate-0.5 active:scale-95
        active:bg-gray-900 active:outline-1
        active:outline-white
      `}
    >
      {user.picture && (
        <img
          src={user.picture}
          className={`
            aspect-square w-14 rounded-full object-fill
          `}
        />
      )}
    </div>
  );
  return (
    <nav
      className={`
        absolute flex h-1/20 min-h-16 w-full items-center
        justify-start bg-blue-600 px-3
      `}
    >
      {/* left */}
      <section className="w-1/3">
        <h1
          className="flex cursor-pointer gap-2 text-4xl"
          onClick={() => navigate("/")}
        >
          a r i <span className="font-bold">X</span>
        </h1>
      </section>

      {/* middle */}
      <section className="w-1/3"></section>

      {/* right */}
      <section className="w-1/3">
        {!["/login", "/signup"].includes(
          location.pathname
        ) && (
          <div
            className={`
              flex w-full items-center justify-end
            `}
          >
            {isLoggedIn ? (
              <>
                {UserAvatar}
                <button
                  className={`
                    ml-2 flex cursor-pointer items-center
                    justify-center rounded-xl bg-blue-900
                    p-2 font-bold uppercase transition
                    select-none
                    active:hover:translate-0.5
                    active:hover:scale-95
                    active:hover:bg-gray-900
                    active:hover:outline-1
                    active:hover:outline-white
                  `}
                  onClick={() => {
                    unset();
                  }}
                >
                  Log Out
                </button>
              </>
            ) : (
              <>
                <button
                  className={`
                    ml-2 flex cursor-pointer items-center
                    justify-center rounded-xl bg-blue-900
                    p-2 font-bold uppercase transition
                    select-none
                    active:hover:translate-0.5
                    active:hover:scale-95
                    active:hover:bg-gray-900
                    active:hover:outline-1
                    active:hover:outline-white
                  `}
                  onClick={() => navigate("/login")}
                >
                  Login
                </button>
                <button
                  className={`
                    ml-2 flex cursor-pointer items-center
                    justify-center rounded-xl bg-none p-2
                    font-bold text-pink-200 uppercase
                    outline-1 outline-pink-200 transition
                    select-none
                    active:hover:translate-0.5
                    active:hover:scale-95
                    active:hover:bg-gray-900
                    active:hover:outline-1
                    active:hover:outline-white
                  `}
                  onClick={() => navigate("/signup")}
                >
                  Sign Up
                </button>
              </>
            )}
          </div>
        )}
      </section>
    </nav>
  );
};

export default TopBar;
