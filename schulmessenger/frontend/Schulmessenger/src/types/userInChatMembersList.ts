import type { UserRole } from "./user";

export interface UserInChatMemberList {
    userId: string,
    firstName: string,
    lastName: string,
    role: UserRole,
    className: string | null
}