import type { UserInChatInfoMemberList } from "../types/userInChatInfoMemberList";

export function sortChatMembers(users: UserInChatInfoMemberList[], currentUserId: string) {
    return users.sort((a, b) => {
        const aIsMe = a.userId === currentUserId ? 1 : 0;
        const bIsMe = b.userId === currentUserId ? 1 : 0;
        if (aIsMe !== bIsMe) return bIsMe - aIsMe; // > 0 => a vor b; < 0 => b vor a

        const aIsAdmin = a.isAdmin ? 1 : 0;
        const bIsAdmin = b.isAdmin ? 1 : 0;
        if (aIsAdmin !== bIsAdmin) return bIsAdmin - aIsAdmin;

        return a.lastName.localeCompare(b.lastName, "de");
    });
}