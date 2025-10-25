import React from "react";

type CenterOnContainerProps = {
  className?: string;
  children?: React.ReactNode;
};

const CenterOnContainer: React.FC<CenterOnContainerProps> = ({
  className,
  children,
}) => {
  return (
    <div
      className={
        "w-full h-full flex justify-center items-center flex-1" + className
      }
    >
      {children}
    </div>
  );
};

export default CenterOnContainer;
