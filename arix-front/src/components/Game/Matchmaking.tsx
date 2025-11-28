import { useEffect, useState } from "react";
import useWebSocket, {
  ReadyState
} from "react-use-websocket";
import { ARIX_SERVER_ORIGIN } from "../../helpers/queryBuilder";
import MenuItem from "../Menu/MenuItem";
import { useNavigate } from "react-router";

const wsUrl = `wss://${ARIX_SERVER_ORIGIN}/Websocket/ws`;
const Matchmaking = () => {
  const { sendMessage, lastMessage, readyState } =
    useWebSocket(wsUrl);

  const [messages, setMessages] = useState([]);

  useEffect(() => {
    setMessages((m) => [...m, lastMessage] as any);
  }, [lastMessage]);

  const connectionStatus = {
    [ReadyState.CONNECTING]: "Connecting",
    [ReadyState.OPEN]: "Open",
    [ReadyState.CLOSING]: "Closing",
    [ReadyState.CLOSED]: "Closed",
    [ReadyState.UNINSTANTIATED]: "Uninstantiated"
  }[readyState];

  const navigate = useNavigate();
  return (
    <div>
      <span>Status: {connectionStatus}</span>
      <ul>
        {messages.map((m) => (
          <li key={m}>{m}</li>
        ))}
      </ul>

      <MenuItem
        config={{
          id: "Send",
          label: "Send",
          onClick: () =>
            sendMessage(JSON.stringify({ message: "hi" }))
        }}
      ></MenuItem>
      <MenuItem
        config={{
          id: "back",
          label: "Back",
          onClick: () => navigate("/")
        }}
      ></MenuItem>
    </div>
  );
};

export default Matchmaking;
