import { useLocalStorage } from "../../hooks/useLocalStorage";
import { useUser } from "../../hooks/useUser";

type TopBarProps = {};
const TopBar: React.FC<TopBarProps> = () => {
  const { user } = useUser();
  const { value: jwtToken, unset } =
    useLocalStorage<string>("jwt");
  const isLoggedIn = user != null && jwtToken != null;

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
      {/* <button
        className={`
          aspect-square w-14 cursor-pointer rounded-full
          bg-gray-500 outline-2 outline-red-900
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
      </button> */}
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
        <h1 className="flex gap-2 text-4xl">
          a r i <span className="font-bold">X</span>
        </h1>
      </section>

      {/* middle */}
      <section className="w-1/3"></section>

      {/* right */}
      <section className="w-1/3">
        <div
          className={`flex w-full items-center justify-end`}
        >
          {isLoggedIn ? (
            <>
              {UserAvatar}
              <button
                className={`
                  ml-2 flex cursor-pointer items-center
                  justify-center rounded-xl bg-blue-900 p-2
                  font-bold uppercase transition select-none
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
            <button
              className={`
                ml-2 flex cursor-pointer items-center
                justify-center rounded-xl bg-blue-900 p-2
                font-bold uppercase transition select-none
                active:hover:translate-0.5
                active:hover:scale-95
                active:hover:bg-gray-900
                active:hover:outline-1
                active:hover:outline-white
              `}
              onClick={() =>
                (window.location.href =
                  "http://localhost:5115/oauth")
              }
            >
              Login
            </button>
          )}
        </div>
      </section>
    </nav>
  );
};

export default TopBar;
