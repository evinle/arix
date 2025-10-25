import React from "react";

type BackgroundProps = {
  children?: React.ReactNode;
};
const Background: React.FC<BackgroundProps> = ({ children }) => {
  return (
    <div className="min-h-dvh h-dvh bg-gray-900 text-white">{children}</div>
  );
};

export default Background;
