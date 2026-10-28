import type { UserOnChatCreate } from "../types/userOnChatCreate";
import { fetchWrapper } from "./fetchWrapper";

export async function addUsers(chatId: string, usersToAdd: UserOnChatCreate[]) { // Nach Auth keine creatorId mehr
    return await fetchWrapper(`/api/Chat/${chatId}/add`, {
        method: "POST",
        body: JSON.stringify({ userIds: usersToAdd.map(u => u.userId) })
    });
}