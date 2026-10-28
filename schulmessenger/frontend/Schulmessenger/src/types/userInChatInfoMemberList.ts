import type { UserRole } from "./user";

export interface UserInChatInfoMemberList {
    userId: string,
    firstName: string,
    lastName: string,
    role: UserRole,
    isAdmin: boolean,
    className: string | null
}