import { Fragment, useLayoutEffect, useRef, useState } from "react";
import { formatMessageTime } from "../helpers/formatMessageTime";
import type { Chat } from "../types/chat";
import type { Message } from "../types/message";
import type { User } from "../types/user";
import MessageInput from "./MessageInput";
import "../styles/ChatInterface.css";
import ChatInfo from "./ChatInfo";
import { formatMessageDate } from "../helpers/formatMessageDate";
import type { UserInChatInfoMemberList } from "../types/userInChatInfoMemberList";
import MessageMenuInterface from "./MessageMenuInterface";
import { deleteMessage } from "../api/deleteMessage";
import { sendMessage } from "../services/signalrService";

function ChatInterface({ currentUser, selectedChat, chatMembers, messages, onMemberRemoved, onMemberDemoted, onMemberPromoted, onRemoveChatById, onMessageSent, onAddUsers, onMessageDeleted, onBack }:
    {
        currentUser: User; selectedChat: Chat | null; chatMembers: UserInChatInfoMemberList[]; messages: Message[]; onMemberRemoved: (user: UserInChatInfoMemberList) => void;
        onMemberPromoted: (user: UserInChatInfoMemberList) => void; onMemberDemoted: (user: UserInChatInfoMemberList) => void; onRemoveChatById: (chatId: string) => void; onMessageSent: (messageSent: Message) => void;
        onAddUsers: (usersToAdd: UserInChatInfoMemberList[]) => void; onMessageDeleted: (messageId: string) => void;
        onBack: () => void;
    }) {

    const [activeMessageMenuId, setActiveMessageMenuId] = useState<string | null>(null);

    const messageListRef = useRef<HTMLUListElement>(null);

    const currentUserIsAdmin = chatMembers.some(
        member => member.userId === currentUser.userId && member.isAdmin
    );

    useLayoutEffect(() => { // useLayoutEffect statt useEffect => wird vorm Zeichnen im Browser ausgeführt
        if (!messageListRef.current) return;

        messageListRef.current.scrollTop = messageListRef.current.scrollHeight; // Ganz nach unten
    }, [messages, selectedChat?.chatId]);

    function handleSendButtonClicked(content: string) {
        const tempId = crypto.randomUUID(); // RandomId auf dem Client => wird nachher bei ReceiveMessage mit der richtigen Id ausgetauscht

        const messageSent: Message = {
            chatId: selectedChat!.chatId,
            messageId: tempId,
            content: content,
            senderId: currentUser.userId,
            senderName: `${currentUser.firstName} ${currentUser.lastName}`,
            sentAt: new Date().toISOString(),
            isSystemMessage: false,
            isDeleted: false,
            tempId: tempId
        };

        sendMessage(selectedChat!.chatId, content, tempId);
        onMessageSent(messageSent);
    }

    async function handleDeleteMessage(messageId: string) {
        try {
            await deleteMessage(messageId);

            onMessageDeleted(messageId);

            setActiveMessageMenuId(null);
        }
        catch {
            // Wird in fetchWrapper gehandelt // Für weitere optionale Ausgabe der Fehlermeldung
        }
    }

    return (
        <div className="chatInterfaceWrapper">
            {selectedChat ?
                <>
                    <ChatInfo currentUser={currentUser} currentUserIsAdmin={currentUserIsAdmin} selectedChat={selectedChat} chatMembers={chatMembers} onMemberRemoved={onMemberRemoved}
                        onMemberPromoted={(user: UserInChatInfoMemberList) => onMemberPromoted(user)} 
                        onMemberDemoted={(user: UserInChatInfoMemberList) => onMemberDemoted(user)}
                        onRemoveChatById={(chatId: string) => onRemoveChatById(chatId)}
                        onAddUsers={(usersToAdd: UserInChatInfoMemberList[]) => onAddUsers(usersToAdd)}
                        onBack={onBack} />

                    <ul ref={messageListRef} className="messageList">
                        {
                            messages.map((message, index) => {
                                const messageDate = new Date(message.sentAt);
                                const prevMessageDate = new Date(messages[index - 1]?.sentAt);
                                const showNewDayBanner = messageDate.toDateString() !== prevMessageDate.toDateString();

                                return (
                                    <Fragment key={message.messageId}> {/* Übergreifendes Element braucht einen Key => Fragment gewährleistet das, ohne ein neues Element einzufügen => da in ul nur li rein sollten */}
                                        {showNewDayBanner ? <li className="newDayBanner">{formatMessageDate(messageDate)}</li> : ""}

                                        <li className={`messageElement ${message.senderId === currentUser.userId ? "own" : ""} ${message.isSystemMessage ? "systemMessage" : ""}`}>
                                            {message.messageId === activeMessageMenuId && <MessageMenuInterface onClose={() => setActiveMessageMenuId(null)} onDelete={() => handleDeleteMessage(message.messageId)} />}
                                            <div className="messageFirstLineWrapper">
                                                <div className="messageSenderName" hidden={message.senderId === currentUser.userId || message.isDeleted}>{message.senderName}</div>
                                                {!message.isSystemMessage && !message.isDeleted && (selectedChat.chatType !== "Direct" && currentUserIsAdmin || message.senderId === currentUser.userId) && !message.tempId
                                                    && <button className="messageMenuButton" type="button" onClick={() => setActiveMessageMenuId(message.messageId)}>⋮</button>}
                                            </div>
                                            <div className="messageContent">{message.content}</div>
                                            <div className="messageSentAt">{message.tempId ? "✓" : "✓✓"} {formatMessageTime(message.sentAt)}</div>
                                        </li>
                                    </Fragment>
                                );
                            }
                            )
                        }
                    </ul>

                    <div className="messageInputArea">
                        <MessageInput onSendMessage={(content) => handleSendButtonClicked(content)} />
                    </div>
                </>
                : <p>Chat auswählen</p>
            }
        </div >
    );
}

export default ChatInterface;
