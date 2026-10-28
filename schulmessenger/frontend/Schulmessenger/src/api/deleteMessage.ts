import type { Message } from "../types/message";
import { fetchWrapper } from "./fetchWrapper";

export async function deleteMessage(messageId: string): Promise<Message> { // Nach Auth keine senderId mehr
    return await fetchWrapper(`/api/Message/${messageId}`, {
        method: "DELETE",
    });
}