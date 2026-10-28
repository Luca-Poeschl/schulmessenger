import type { ChatType } from "./chat.ts";
import type { UserOnChatCreate } from "./userOnChatCreate.ts";

export interface ChatCreate {
    name: string | null,
    description: string | null,
    users: UserOnChatCreate[],
    chatType: ChatType;
}