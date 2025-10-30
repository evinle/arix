import React from "react";

type CenterOnContainerProps = {
  className?: string;
  children?: React.ReactNode;
};

const CenterOnContainer: React.FC<
  CenterOnContainerProps
> = ({ className, children }) => {
  return (
    <div
      className={
        `
          flex h-full w-full flex-1 items-center
          justify-center
        ` + className
      }
    >
      {children}
    </div>
  );
};

export default CenterOnContainer;
