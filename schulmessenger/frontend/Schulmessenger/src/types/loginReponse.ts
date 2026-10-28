import type { User } from "./user";

export interface LoginResponse extends User {
    accessToken: string,
    refreshToken: string
}