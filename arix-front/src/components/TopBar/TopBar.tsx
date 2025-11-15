type TopBarProps = {};
const TopBar: React.FC<TopBarProps> = () => {
  return (
    <nav className="h-1/12 min-h-20 flex justify-start items-center bg-blue-600 px-3">
      {/* left */}
      <section className="w-1/3">
        <h1 className="text-4xl flex gap-2">
          a r i <span className="font-bold">X</span>
        </h1>
      </section>

      {/* middle */}
      <section className="w-1/3"></section>

      {/* right */}
      <section className="w-1/3">
        <div className="w-full flex justify-end items-center">
          <button className="rounded-full w-14 aspect-square bg-gray-500">
            X
          </button>
        </div>
      </section>
    </nav>
  );
};

export default TopBar;
