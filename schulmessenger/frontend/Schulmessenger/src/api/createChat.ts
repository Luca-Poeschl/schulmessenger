import { fetchWrapper } from "./fetchWrapper";
import type { ChatCreate } from "../types/chatCreate";

export async function createChat(newChat: ChatCreate) { // Nach Auth keine creatorId mehr
    return await fetchWrapper(`/api/Chat`, {
        method: "POST",
        body: JSON.stringify(newChat)
    });
}