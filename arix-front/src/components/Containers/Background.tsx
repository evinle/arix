import React from "react";

type BackgroundProps = {
  children?: React.ReactNode;
  className?: string;
};
const Background: React.FC<BackgroundProps> = ({
  children,
  className
}) => {
  return (
    <div
      className={`
        h-dvh min-h-dvh bg-gray-900 text-white
        ${className}
      `}
    >
      {children}
    </div>
  );
};

export default Background;
