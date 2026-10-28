import { useState } from "react";
import ChatInfoInterface from "./ChatInfoInterface";
import type { Chat } from "../types/chat";
import type { User } from "../types/user";
import type { UserInChatInfoMemberList } from "../types/userInChatInfoMemberList";
import "../styles/ChatInfo.css";

function ChatInfo({ currentUser, currentUserIsAdmin, selectedChat, chatMembers, onMemberRemoved, onMemberPromoted, onMemberDemoted, onRemoveChatById, onAddUsers, onBack }:
    {
        currentUser: User; currentUserIsAdmin: boolean; selectedChat: Chat; chatMembers: UserInChatInfoMemberList[]; onMemberRemoved: (user: UserInChatInfoMemberList) => void;
        onMemberPromoted: (user: UserInChatInfoMemberList) => void; onMemberDemoted: (user: UserInChatInfoMemberList) => void; onRemoveChatById: (chatId: string) => void;
        onAddUsers: (usersToAdd: UserInChatInfoMemberList[]) => void;
        onBack: () => void;
    }) {
    const [showChatInfo, setShowChatInfo] = useState<boolean>(false);

    return (
        <>
            <div className="chatTopBar" onClick={() => setShowChatInfo(true)}>
                <div className="chatTopBarContent">
                    <button
                        type="button"
                        className="chatBackButton"
                        onClick={(e) => {
                            e.stopPropagation(); // OnClick steigt nicht zur Eltern-Komponente hoch
                            onBack();
                        }}
                    >←</button>
                    <div className="chatTopBarText">
                        <h3 className="chatTopBarTitle">{selectedChat.name}</h3>
                        <span className="chatTopBarSubtitle">
                            {selectedChat.chatType === "Groupchat" ? `${chatMembers.length} Mitglieder` : "Direktchat"}
                        </span>
                    </div>
                </div>
            </div>

            {
                showChatInfo && <ChatInfoInterface currentUser={currentUser} currentUserIsAdmin={currentUserIsAdmin} selectedChat={selectedChat} chatMembers={chatMembers} onMemberRemoved={onMemberRemoved}
                    onMemberPromoted={(user: UserInChatInfoMemberList) => onMemberPromoted(user)}
                    onMemberDemoted={(user: UserInChatInfoMemberList) => onMemberDemoted(user)}
                    onReturnButtonClicked={() => setShowChatInfo(false)} onRemoveChatById={(chatId: string) => onRemoveChatById(chatId)}
                    onAddUsers={(usersToAdd: UserInChatInfoMemberList[]) => onAddUsers(usersToAdd)} />
            }
        </>
    );
}

export default ChatInfo;