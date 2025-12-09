import { useEffect, useState } from "react";
import useWebSocket, {
  ReadyState
} from "react-use-websocket";
import MenuItem from "../Menu/MenuItem";
import { useNavigate } from "react-router";
import { useLocalStorage } from "../../hooks/useLocalStorage";

const wsUrl = `ws://localhost:5115/Websocket/ws`;
const Matchmaking = () => {
  const { value: jwt } = useLocalStorage<string>("jwt");
  const { sendMessage, lastMessage, readyState } =
    useWebSocket(wsUrl, {
      protocols: [`${jwt}`],
      onClose(event) {
        console.log("closing", event);
      }
    });

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
