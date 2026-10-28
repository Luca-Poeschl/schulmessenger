import { useState } from "react";
import { createChat } from "../api/createChat";
import "../styles/CreateInterface.css";
import type { ChatType } from "../types/chat";
import type { ChatCreate } from "../types/chatCreate";
import type { UserOnChatCreate } from "../types/userOnChatCreate";
import type { User } from "../types/user";
import UserSearch from "./UserSearch";

export function CreateChatInterface({ currentUser, onCancelButtonClicked }: { currentUser: User, onCancelButtonClicked: () => void; }) {
    const [chatType, setChatType] = useState<ChatType>("Direct");
    const [chatName, setChatName] = useState<string>("");
    const [chatDescription, setChatDescription] = useState<string>("");
    const [chatMembers, setChatMembers] = useState<UserOnChatCreate[]>([]);

    const [chatNameError, setChatNameError] = useState<string | null>(null);
    const [chatDescriptionError, setChatDescriptionError] = useState<string | null>(null);
    const [chatMemberError, setChatMemberError] = useState<string | null>(null);

    async function handleCreateChatButtonClick() {
        const trimmedName = chatName.trim();
        const trimmedDescription = chatDescription.trim();

        if (chatType === "Groupchat") {
            if (!trimmedName) {
                setChatNameError("Es muss ein Gruppenname vergeben werden!");
                return;
            }
            if (trimmedName.length > 50) {
                setChatNameError("Der Gruppenname darf maximal 50 Zeichen haben!");
                return;
            }
            if (trimmedDescription.length > 250) {
                setChatDescriptionError("Die Gruppenbeschreibung darf maximal 250 Zeichen haben!");
                return;
            }
        }

        const newChat: ChatCreate = {
            name: trimmedName,
            description: trimmedDescription,
            chatType: chatType,
            users: chatMembers
        };

        if (newChat.users.length === 0) {
            setChatMemberError("Es muss mindestens ein anderes Mitglied angegeben werden!");
            return;
        }

        try {
            await createChat(newChat);
            onCancelButtonClicked();
        }
        catch {
            // Wird in fetchWrapper gehandelt // Für weitere optionale Ausgabe der Fehlermeldung
        }
    }

    function handleChatTypeSwitch(chatTypeToSwitch: ChatType) {
        if (chatType === "Groupchat" && chatTypeToSwitch === "Direct" && chatMembers.length > 1)
            setChatMembers([]);

        setChatType(chatTypeToSwitch);
        setChatNameError(null);
        setChatDescriptionError(null);
        setChatMemberError(null);
    }

    return (
        <>
            <div className="createChatInterfaceBackground" onClick={onCancelButtonClicked}>
                <div className="createChatInterfaceWrapper" onClick={(e) => e.stopPropagation()}>
                    <h1>Neuen Chat erstellen</h1>

                    <div className="chatTypeSwitcher">
                        <div className={`pillGlider ${chatType === "Groupchat" ? "isGroup" : ""}`}></div>
                        <input type="button" value="Direktchat" className={`pillButton ${chatType === "Direct" ? "active" : ""}`} onClick={() => handleChatTypeSwitch("Direct")} />
                        <input type="button" value="Gruppenchat" className={`pillButton ${chatType === "Groupchat" ? "active" : ""}`} onClick={() => handleChatTypeSwitch("Groupchat")} />
                    </div>

                    <div className={`groupchatInputWrapper ${chatType === "Groupchat" ? "expanded" : ""}`}>
                        <div className="groupchatInner">
                            <div className="groupchatInputComponent">
                                <p>*Name:</p>
                                <input type="text" value={chatName} placeholder="Gruppenname..." onChange={(e) => { setChatName(e.target.value); setChatNameError(null); }} />
                                <p className={`createChatError ${chatNameError ? "visible" : ""}`}>{chatNameError}</p>
                            </div>
                            <div className="groupchatInputComponent">
                                <p>Beschreibung:</p>
                                <input type="text" value={chatDescription} placeholder="Gruppenbeschreibung..." onChange={(e) => { setChatDescription(e.target.value); setChatDescriptionError(null); }} />
                                <p className={`createChatError ${chatDescriptionError ? "visible" : ""}`}>{chatDescriptionError}</p>
                            </div>
                        </div>
                    </div>

                    <UserSearch currentUser={currentUser} chatMembers={chatMembers} setChatMembers={setChatMembers}
                        onSetChatError={(error: string | null) => setChatMemberError(error)} chatType={chatType} selectedChat={null} />
                    <p className={`createChatError ${chatMemberError ? "visible" : ""}`}>{chatMemberError}</p>

                    <div className="">
                        <input className="createChatButton" type="button" value="Erstellen" onClick={handleCreateChatButtonClick} />
                        <input className="exitCreateChatInterface" type="button" value="Abbrechen" onClick={onCancelButtonClicked} />
                    </div>
                </div>
            </div>
        </>
    );
}

export default CreateChatInterface;