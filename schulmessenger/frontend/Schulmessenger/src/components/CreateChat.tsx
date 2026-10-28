import { useState } from "react";
import CreateChatInterface from "./CreateChatInterface.tsx";
import "../styles/CreateChat.css";
import type { User } from "../types/user.ts";

export function CreateChat({ currentUser }: { currentUser: User; }) {
    const [showCreateInterface, setShowCreateInterface] = useState<boolean>(false);

    return (
        <div className="createChatWrapper">
            <input className="createChatButton" type="button" value={"Neuer Chat"} onClick={() => setShowCreateInterface(true)} />
            {
                showCreateInterface && <CreateChatInterface currentUser={currentUser} onCancelButtonClicked={() => setShowCreateInterface(false)} />
            }
        </div>
    );
}

export default CreateChat;