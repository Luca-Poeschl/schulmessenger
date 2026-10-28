export interface Message {
    chatId: string
    messageId: string,
    content: string,
    senderId: string | null,
    senderName: string | null,
    sentAt: string,
    isSystemMessage: boolean,
    isDeleted: boolean,
    tempId?: string | null
};