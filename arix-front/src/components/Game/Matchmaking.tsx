import { useEffect, useState } from "react";
import useWebSocket, {
  ReadyState
} from "react-use-websocket";
import { ARIX_SERVER_ORIGIN } from "../../helpers/queryBuilder";
import MenuItem from "../Menu/MenuItem";
import { useNavigate } from "react-router";
import { useLocalStorage } from "../../hooks/useLocalStorage";
import AxInput from "../Form/AxInput";
import { useForm, useWatch } from "react-hook-form";

const Matchmaking = () => {
  const { value: jwt } = useLocalStorage<string>("jwt");
  const wsUrl = `${ARIX_SERVER_ORIGIN.replace(/^http/, "ws")}/Websocket/ws?access_token=${jwt}`;

  const [messages, setMessages] = useState([]);
  const { control, setValue } = useForm<{
    message: string | null;
  }>();
  const [messageToSend] = useWatch({
    control,
    name: ["message"]
  });

  const { sendMessage, readyState } = useWebSocket(wsUrl, {
    shouldReconnect: () => true,
    onMessage: (m) => {
      setMessages((prev) => [...prev, m.data] as any);
    }
  });

  const connectionStatus = {
    [ReadyState.CONNECTING]: "Connecting",
    [ReadyState.OPEN]: "Open",
    [ReadyState.CLOSING]: "Closing",
    [ReadyState.CLOSED]: "Closed",
    [ReadyState.UNINSTANTIATED]: "Uninstantiated"
  }[readyState];

  const send = () => {
    sendMessage(String(messageToSend));
    setValue("message", "");
  };

  const navigate = useNavigate();
  return (
    <div>
      <span>Status: {connectionStatus}</span>
      <ul>
        {messages.map((m, i) => (
          <li key={`${m}-${i}`}>{JSON.stringify(m)}</li>
        ))}
      </ul>
      <form
        onSubmit={(e) => {
          e.preventDefault();
          send();
        }}
      >
        <AxInput
          control={control}
          name="message"
          label="Message"
        ></AxInput>
        <MenuItem
          config={{
            id: "Send",
            label: "Send",
            onClick: send
          }}
        ></MenuItem>
      </form>
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
