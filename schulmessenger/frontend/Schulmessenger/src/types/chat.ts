export type ChatType = "Direct" | "Groupchat"

export type ChatInfoType = "Organize" | "AddUser"

export interface Chat{
    chatId: string,
    name: string | null,
    description: string | null,
    chatType: ChatType,
    lastMessageId: string | null,
    lastMessageContent: string | null,
    lastMessageSenderName: string | null,
    lastMessageSentAt: string | null
}