import { formatMessageTime } from "../helpers/formatMessageTime";
import "../styles/ChatSidebar.css";
import type { Chat } from "../types/chat";
import type { User } from "../types/user";
import CreateChat from "./CreateChat";

function ChatSidebar({ currentUser, chats, selectedChat, onSelectChat }: { currentUser: User, chats: Chat[], selectedChat: Chat | null, onSelectChat: (chat: Chat) => void; }) {

    return (
        <aside className="chatSidebarWrapper">
            <CreateChat currentUser={currentUser} />
            <ul className="chatList">
                {
                    chats.map(chat =>
                        <li key={chat.chatId} className={`chatElement ${selectedChat?.chatId === chat.chatId ? "active" : ""}`}
                            onClick={() => onSelectChat(chat)}>
                            <div className="chatNameArea">{chat.name}</div>
                            <div className="chatLastMessageSenderArea">{chat.lastMessageSenderName !== null && chat.lastMessageSenderName !== "System" ? `${chat.lastMessageSenderName}:` : ""}</div>
                            <div className="chatLastMessageContentArea">{chat.lastMessageContent}</div>
                            <div className="chatLastMessageSentAtArea">{formatMessageTime(chat.lastMessageSentAt)}</div>
                        </li>)
                }
            </ul>
        </aside>
    );
}

export default ChatSidebar;