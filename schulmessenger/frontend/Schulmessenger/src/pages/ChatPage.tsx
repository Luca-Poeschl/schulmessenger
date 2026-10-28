import { useState, useEffect } from 'react';
import { getSignalRConnection, joinChat, leaveChat } from '../services/signalrService.ts';
import { fetchWrapper } from '../api/fetchWrapper.ts';
import ChatSidebar from "../components/ChatSidebar.tsx";
import ChatInterface from "../components/ChatInterface.tsx";
import "../styles/ChatPage.css";
import type { User } from "../types/user";
import type { Chat } from "../types/chat";
import type { Message } from '../types/message.ts';
import type { UserInChatInfoMemberList } from "../types/userInChatInfoMemberList";
import { sortChatMembers } from '../helpers/sortChatMembers.ts';

function ChatPage({ currentUser }: { currentUser: User; }) {

    const [selectedChat, setSelectedChat] = useState<Chat | null>(null);
    const [chatMembers, setChatMembers] = useState<UserInChatInfoMemberList[]>([]);
    const [chats, setChats] = useState<Chat[]>([]);
    const [messages, setMessages] = useState<Message[]>([]);

    useEffect(() => {
        fetchWrapper("/api/Chat")
            .then(data => setChats(data));
    }, []);

    useEffect(() => {
        if (selectedChat?.chatId) {
            fetchWrapper(`/api/Chat/${selectedChat.chatId}/users`)
                .then(data => setChatMembers(data));
        }
    }, [selectedChat?.chatId]);

    useEffect(() => {
        const connection = getSignalRConnection();

        const handleReceiveNewChat = async (incomingNewChat: Chat, creatorId: string) => {
            await joinChat(incomingNewChat.chatId);

            setChats(prevChats => [incomingNewChat, ...prevChats]);

            if (currentUser.userId === creatorId)
                setSelectedChat(incomingNewChat);
        };

        connection.on("ReceiveNewChat", handleReceiveNewChat);

        return () => {
            connection.off("ReceiveNewChat", handleReceiveNewChat);
        };
    }, [currentUser.userId]);

    useEffect(() => {
        const connection = getSignalRConnection();

        const handleReceiveUserLeftChat = async (chatId: string, userId: string) => {
            if (userId === currentUser.userId) {
                await leaveChat(chatId);

                setChats((prev) => prev.filter(c => c.chatId !== chatId));

                if (selectedChat?.chatId === chatId)
                    setSelectedChat(null);

                return;
            }

            if (selectedChat?.chatId !== chatId)
                return;

            setChatMembers((prev) => prev.filter(m => m.userId !== userId));
        };

        connection.on("ReceiveUserLeftChat", handleReceiveUserLeftChat);
        connection.on("ReceiveUserRemovedFromChat", handleReceiveUserLeftChat);

        return () => {
            connection.off("ReceiveUserLeftChat", handleReceiveUserLeftChat);
            connection.off("ReceiveUserRemovedFromChat", handleReceiveUserLeftChat);
        };

    }, [selectedChat?.chatId, currentUser.userId]);

    useEffect(() => {
        const connection = getSignalRConnection();

        const handleReceiveChatDeleted = (chatId: string) => {
            if (selectedChat?.chatId === chatId)
                setSelectedChat(null);

            setChats((prev) => prev.filter(c => c.chatId !== chatId));
        };

        connection.on("ReceiveChatDeleted", handleReceiveChatDeleted);

        return () => {
            connection.off("ReceiveChatDeleted", handleReceiveChatDeleted);
        };

    }, [selectedChat?.chatId]);

    useEffect(() => {
        const connection = getSignalRConnection();

        const handleReceiveUserPromoted = (chatId: string, userId: string) => {
            if (selectedChat?.chatId !== chatId)
                return;

            setChatMembers((prev) => prev.map(m => {
                if (m.userId === userId)
                    return { ...m, isAdmin: true };

                return m;
            }));
        };

        connection.on("ReceiveUserPromoted", handleReceiveUserPromoted);

        return () => {
            connection.off("ReceiveUserPromoted", handleReceiveUserPromoted);
        };

    }, [selectedChat?.chatId]);

    useEffect(() => {
        const connection = getSignalRConnection();

        const handleReceiveUserDemoted = (chatId: string, userId: string) => {
            if (selectedChat?.chatId !== chatId)
                return;

            setChatMembers((prev) => prev.map(m => {
                if (m.userId === userId)
                    return { ...m, isAdmin: false };

                return m;
            }));
        };

        connection.on("ReceiveUserDemoted", handleReceiveUserDemoted);

        return () => {
            connection.off("ReceiveUserDemoted", handleReceiveUserDemoted);
        };

    }, [selectedChat?.chatId]);

    useEffect(() => {
        if (selectedChat?.chatId) {
            fetchWrapper("/api/Message/" + selectedChat.chatId)
                .then(data => setMessages(data))
                .catch(() => {
                    setMessages([]);
                });
        }
    }, [selectedChat?.chatId]);

    useEffect(() => {
        const connection = getSignalRConnection();

        const handleReceiveMessage = (incomingMessage: Message) => {
            if (incomingMessage.chatId === selectedChat?.chatId) {
                setMessages(prev => {
                    if (incomingMessage.tempId) {
                        const isOwnPending = (prev.some(m => m.tempId === incomingMessage.tempId));

                        if (isOwnPending) {
                            return prev.map(m => m.tempId === incomingMessage.tempId ? { ...incomingMessage, tempId: null } : m);
                        }

                        return [...prev, { ...incomingMessage, tempId: null }];
                    }

                    return [...prev, incomingMessage];
                });
            }

            setChats(prevChats => {
                const chat = prevChats.find(c => c.chatId === incomingMessage.chatId);

                if (!chat) return prevChats;

                const updatedChat = {
                    ...chat,
                    lastMessageId: incomingMessage.messageId,
                    lastMessageContent: incomingMessage.content,
                    lastMessageSenderName: incomingMessage.senderName,
                    lastMessageSentAt: incomingMessage.sentAt
                };

                return [updatedChat, ...prevChats.filter(c => c.chatId != chat.chatId)];
            }
            );
        };

        connection.on("ReceiveMessage", handleReceiveMessage);

        return () => {
            connection.off("ReceiveMessage", handleReceiveMessage);
            // Schließen der WebSocket-Verbindung wenn die Komponente gelöscht wird
        };
    }, [selectedChat?.chatId]);

    useEffect(() => {
        const connection = getSignalRConnection();

        const handleReceiveDeletedMessage = (deletedMessage: Message) => {
            if (deletedMessage.chatId === selectedChat?.chatId) {
                setMessages(prev => prev.map((m) => m.messageId === deletedMessage.messageId ? deletedMessage : m));
            }

            setChats(prev =>
                prev.map(c => c.lastMessageId === deletedMessage.messageId ?
                    {
                        ...c,
                        lastMessageContent: deletedMessage.content,
                        lastMessageSenderName: deletedMessage.senderName
                    } : c));
        };

        connection.on("ReceiveDeletedMessage", handleReceiveDeletedMessage);

        return () => connection.off("ReceiveDeletedMessage", handleReceiveDeletedMessage);
    }, [selectedChat?.chatId]);

    useEffect(() => {
        const connection = getSignalRConnection();

        const handleReceiveUserAdded = (chatId: string, userToAdd: UserInChatInfoMemberList) => {
            if (chatId === selectedChat?.chatId)
                setChatMembers(prev => sortChatMembers([...prev, userToAdd], currentUser.userId));
        };

        connection.on("ReceiveUserAdded", handleReceiveUserAdded);

        return () => connection.off("ReceiveUserAdded", handleReceiveUserAdded);
    }, [currentUser.userId, selectedChat?.chatId]);

    function handleMessageSent(messageSent: Message) {
        if (messageSent.chatId !== selectedChat?.chatId)
            return;

        setMessages(prev => [...prev, messageSent]);

        setChats(prevChats => {
            const chat = prevChats.find(c => c.chatId === messageSent.chatId);

            if (!chat) return prevChats;

            return [{
                ...chat, lastMessageId: messageSent.messageId, lastMessageContent: messageSent.content,
                lastMessageSenderName: messageSent.senderName, lastMessageSentAt: messageSent.sentAt
            },
            ...prevChats.filter(c => c.chatId != chat.chatId)];
        });
    }

    function handleMessageDeleted(messageId: string) {
        setMessages(prev => prev.map(
            (m) => m.messageId === messageId
                ? { ...m, isDeleted: true, content: "Diese Nachricht wurde gelöscht." } : m)
        );

        if (messages.length > 0 && messages[messages.length - 1].messageId == messageId) {
            setChats(prev => prev.map((c) => {
                if (c.chatId !== selectedChat?.chatId) return c;

                return { ...c, lastMessageContent: "Diese Nachricht wurde gelöscht.", lastMessageSenderName: null };
            }
            ));
        }
    }

    return (
        <>
            <div className='chatPageArea'>
                <div className='chatSidebarArea'>
                    <ChatSidebar currentUser={currentUser} chats={chats} selectedChat={selectedChat} onSelectChat={(chat: Chat) => setSelectedChat(chat)} />
                </div>
                <div className='chatInterfaceArea'>
                    <ChatInterface currentUser={currentUser} chatMembers={chatMembers} messages={messages} onMemberRemoved={((user: UserInChatInfoMemberList) => setChatMembers(prev => prev.filter(m => m.userId !== user.userId)))}
                        onMemberPromoted={(user: UserInChatInfoMemberList) => setChatMembers(prev => prev.map(m => { if (m.userId === user.userId) { return { ...m, isAdmin: true }; } return m; }))}
                        onMemberDemoted={(user: UserInChatInfoMemberList) => setChatMembers(prev => prev.map(m => { if (m.userId === user.userId) { return { ...m, isAdmin: false }; } return m; }))}
                        selectedChat={selectedChat} onRemoveChatById={(chatId: string) => { if (chatId === selectedChat?.chatId) { setSelectedChat(null); } setChats(prev => prev.filter(c => c.chatId !== chatId)); }} onMessageSent={(messageSent: Message) => handleMessageSent(messageSent)}
                        onAddUsers={(usersToAdd: UserInChatInfoMemberList[]) => { setChatMembers(prev => sortChatMembers([...prev, ...usersToAdd], currentUser.userId)); }}
                        onMessageDeleted={handleMessageDeleted}
                        onBack={() => setSelectedChat(null)} />
                </div>
            </div>
        </>
    );
}

export default ChatPage;