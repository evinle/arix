import { useUser } from "../../hooks/useUser";

type TopBarProps = {};
const TopBar: React.FC<TopBarProps> = () => {
  const { user } = useUser();

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
          <button
            className={`
              aspect-square w-14 cursor-pointer rounded-full
              bg-gray-500
              active:translate-0.5 active:scale-95
              active:bg-gray-900 active:outline-1
              active:outline-white
            `}
          >
            {user ? (
              <img
                src={user.picture}
                className={`aspect-square w-14 rounded`}
              />
            ) : (
              "X"
            )}
          </button>
        </div>
      </section>
    </nav>
  );
};

export default TopBar;
